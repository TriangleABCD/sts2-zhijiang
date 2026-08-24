using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Powers;
using Zhijiang.ZhijiangCode.Relics;

namespace Zhijiang.ZhijiangCode.Characters.Bella;

/// <summary>
/// 贝拉阴阳状态计算服务。
/// 状态由玩家当前拥有的全部卡牌的阴阳构成实时推导（派生状态，不缓存），
/// 因此不存在更新时机不同步的问题。卡组几十张牌，遍历开销可忽略。
/// </summary>
public static class BellaYinYangService
{
    // 阳/阴关键词 id（与 BellaYinYangKeywords 注册的 stem 对应）。
    public const string YangKeywordId = "ZHIJIANG_KEYWORD_YANG";
    public const string YinKeywordId = "ZHIJIANG_KEYWORD_YIN";

    // 本回合各贝拉玩家已打出的反差牌计数（打出瞬间按当时状态判定；玩家回合开始时清零）。
    private static readonly Dictionary<Player, int> ContrastPlaysThisTurn = new();

    // 本回合各贝拉玩家已打出的技能牌数（Replay 多段只算第一段）。
    private static readonly Dictionary<Player, int> SkillPlaysThisTurn = new();

    // 本回合各贝拉玩家已打出的阳/阴牌数（Replay 多段只算第一段）。
    private static readonly Dictionary<Player, int> YangPlaysThisTurn = new();
    private static readonly Dictionary<Player, int> YinPlaysThisTurn = new();

    /// <summary>
    /// 注册战斗状态同步，在 Entry.Initialize 中调用：
    /// 1. 战斗开始时为贝拉玩家施加可见状态标记（白拉/黑拉），并同步贝极星的阴阳代价。
    /// 2. 卡牌在牌堆间移动（打出、消耗、生成等）会改变阴阳计数，状态翻转时
    ///    同步替换状态标记并切换贝极星的阴阳代价。
    ///    贝极星的 buff（技能格挡/攻击伤害）在每次出牌时实时判状态，无需同步。
    /// </summary>
    public static void RegisterCombatStateSync()
    {
        RitsuLibFramework.SubscribeLifecycle<CombatStartingEvent>(async evt =>
        {
            if (evt.CombatState is not { } combat)
                return;

            // 新战斗清空各回合计数。
            ContrastPlaysThisTurn.Clear();
            SkillPlaysThisTurn.Clear();
            YangPlaysThisTurn.Clear();
            YinPlaysThisTurn.Clear();

            foreach (var player in combat.Players)
            {
                if (player.Character is not BellaCharacter)
                    continue;
                await SyncStateEffects(player);
            }
        });

        RitsuLibFramework.SubscribeLifecycle<CardMovedBetweenPilesEvent>(async evt =>
        {
            if (evt.CombatState is not { } combat || !combat.IsLiveCombat())
                return;

            if (evt.Card.Owner is not { } player || player.Character is not BellaCharacter)
                return;

            await SyncStateEffects(player);
        });

        // 反差牌计数：每张牌的第一段打出瞬间（CardPlayingEvent 早于 OnPlay）按当时状态判定。
        // 仅计 IsFirstInSeries，Replay 类多段重复打出只算一张牌。
        RitsuLibFramework.SubscribeLifecycle<CardPlayingEvent>(evt =>
        {
            if (!evt.CardPlay.IsFirstInSeries)
                return;

            if (evt.CardPlay.Card.Owner is not { } player || player.Character is not BellaCharacter)
                return;

            // 技能牌计数。
            if (evt.CardPlay.Card.Type == CardType.Skill)
            {
                SkillPlaysThisTurn.TryGetValue(player, out int skillCount);
                SkillPlaysThisTurn[player] = skillCount + 1;
            }

            // 阳/阴牌计数。
            if (evt.CardPlay.Card.Keywords.Contains(YangKeywordId.GetModCardKeyword()))
            {
                YangPlaysThisTurn.TryGetValue(player, out int yangCount);
                YangPlaysThisTurn[player] = yangCount + 1;
            }
            else if (evt.CardPlay.Card.Keywords.Contains(YinKeywordId.GetModCardKeyword()))
            {
                YinPlaysThisTurn.TryGetValue(player, out int yinCount);
                YinPlaysThisTurn[player] = yinCount + 1;
            }

            if (!IsContrastCard(player, evt.CardPlay.Card))
                return;

            ContrastPlaysThisTurn.TryGetValue(player, out int count);
            ContrastPlaysThisTurn[player] = count + 1;
        });

        // 玩家侧回合开始时清零本回合计数。
        RitsuLibFramework.SubscribeLifecycle<SideTurnStartedEvent>(evt =>
        {
            if (evt.Side != CombatSide.Player)
                return;

            foreach (var player in evt.CombatState.Players)
            {
                if (player.Character is BellaCharacter)
                {
                    ContrastPlaysThisTurn[player] = 0;
                    SkillPlaysThisTurn[player] = 0;
                    YangPlaysThisTurn[player] = 0;
                    YinPlaysThisTurn[player] = 0;
                }
            }
        });

        // 战斗结束释放引用。
        RitsuLibFramework.SubscribeLifecycle<CombatEndedEvent>(_ =>
        {
            ContrastPlaysThisTurn.Clear();
            SkillPlaysThisTurn.Clear();
            YangPlaysThisTurn.Clear();
            YinPlaysThisTurn.Clear();
        });
    }

    /// <summary>
    /// 同步贝拉玩家与阴阳状态绑定的战斗效果：可见状态标记（白拉/黑拉）+ 贝极星的阴阳代价（若已施加）。
    /// 幂等：状态未变时不做任何事。
    /// </summary>
    public static async Task SyncStateEffects(Player player)
    {
        PlayerChoiceContext ctx = new ThrowingPlayerChoiceContext();
        Creature creature = player.Creature;
        bool isBaiLa = IsBaiLa(player);

        // 可见状态标记：状态翻转时替换（白拉 ⇄ 黑拉）。
        bool hasBaiLa = creature.HasPower<BaiLaPower>();
        bool hasHeiLa = creature.HasPower<HeiLaPower>();
        if (isBaiLa && !hasBaiLa)
        {
            if (hasHeiLa)
                await PowerCmd.Remove<HeiLaPower>(creature);
            await PowerCmd.Apply<BaiLaPower>(ctx, creature, 1, creature, null);
        }
        else if (!isBaiLa && !hasHeiLa)
        {
            if (hasBaiLa)
                await PowerCmd.Remove<BaiLaPower>(creature);
            await PowerCmd.Apply<HeiLaPower>(ctx, creature, 1, creature, null);
        }

        // 贝极星阴阳代价：仅当贝极星已施加代价控制器时同步（闪耀贝极星无代价）。
        if (creature.GetPower<BellarisYinYangDebuffPower>() is { } debuff)
            await debuff.Sync(ctx);
    }

    /// <summary>
    /// 计算阴阳差值 d = 阳牌数 − 阴牌数。
    /// d &gt; 0 白拉、d &lt; 0 黑拉、d == 0 白拉。
    /// </summary>
    public static int ComputeDiff(Player player)
    {
        int yang = 0, yin = 0;
        foreach (var card in GetOwnedCards(player))
        {
            if (card.Keywords.Contains(YangKeywordId.GetModCardKeyword()))
                yang++;
            else if (card.Keywords.Contains(YinKeywordId.GetModCardKeyword()))
                yin++;
        }
        return yang - yin;
    }

    /// <summary>是否处于白拉状态（阳 ≥ 阴；被「了转反」翻转时取反）。</summary>
    public static bool IsBaiLa(Player player) => (ComputeDiff(player) >= 0) != IsInverted(player);

    /// <summary>是否处于黑拉状态（阴 &gt; 阳；被「了转反」翻转时取反）。</summary>
    public static bool IsHeiLa(Player player) => (ComputeDiff(player) < 0) != IsInverted(player);

    /// <summary>本场战斗内是否持有「了转反」判定翻转标记。</summary>
    public static bool IsInverted(Player player) => player.Creature.HasPower<TurnOverPower>();

    /// <summary>
    /// 阴阳差修正幅度：|d| ÷ 3（d = 阳牌数 − 阴牌数）。
    /// 贝极星的馈赠与代价均在此幅度上加 1。
    /// </summary>
    public static int ComputeMagnitude(Player player)
    {
        return Math.Abs(ComputeDiff(player)) / 3;
    }

    /// <summary>
    /// 是否反差牌：与当前状态阴阳相反的牌（白拉时的阴牌、黑拉时的阳牌）。
    /// 无阴阳标签的中立牌恒为 false。
    /// </summary>
    public static bool IsContrastCard(Player player, CardModel card)
    {
        if (card.Keywords.Contains(YangKeywordId.GetModCardKeyword()))
            return !IsBaiLa(player);
        if (card.Keywords.Contains(YinKeywordId.GetModCardKeyword()))
            return IsBaiLa(player);
        return false;
    }

    /// <summary>
    /// 本回合此前已打出的反差牌数量。
    /// 注意：当前卡牌打出瞬间自身已被计数（CardPlayingEvent 早于 OnPlay），
    /// 卡牌读取时若自身是反差牌需自行扣 1。
    /// </summary>
    public static int GetContrastPlaysThisTurn(Player player)
    {
        return ContrastPlaysThisTurn.TryGetValue(player, out int count) ? count : 0;
    }

    /// <summary>本回合此前已打出的技能牌数量。</summary>
    public static int GetSkillPlaysThisTurn(Player player)
    {
        return SkillPlaysThisTurn.TryGetValue(player, out int count) ? count : 0;
    }

    /// <summary>本回合此前已打出的阳牌数量。</summary>
    public static int GetYangPlaysThisTurn(Player player)
    {
        return YangPlaysThisTurn.TryGetValue(player, out int count) ? count : 0;
    }

    /// <summary>本回合此前已打出的阴牌数量。</summary>
    public static int GetYinPlaysThisTurn(Player player)
    {
        return YinPlaysThisTurn.TryGetValue(player, out int count) ? count : 0;
    }

    /// <summary>本回合阴阳打出差（阳 − 阴）。</summary>
    public static int GetYinYangBalanceThisTurn(Player player)
    {
        return GetYangPlaysThisTurn(player) - GetYinPlaysThisTurn(player);
    }

    /// <summary>
    /// 玩家当前参与状态计数的卡牌。
    /// 战斗内排除两类"已离场"的牌：
    /// 1. 消耗堆——消耗掉的阴阳牌不参与计数；
    /// 2. 打出堆中的能力牌——能力牌打出后停留在打出堆、本场战斗不再回到循环，视同离场。
    ///    打出堆中的非能力牌只是"正在打出"的瞬态，仍然参与计数，避免打牌瞬间状态反复翻转。
    /// 战斗外为主卡组（能力牌在卡组中正常计数）。
    /// </summary>
    private static IEnumerable<CardModel> GetOwnedCards(Player player)
    {
        if (player.PlayerCombatState is { } combat)
        {
            return combat.AllPiles
                .Where(p => p.Type != PileType.Exhaust)
                .SelectMany(p => p.Type == PileType.Play
                    ? p.Cards.Where(c => c.Type != CardType.Power)
                    : p.Cards);
        }
        return player.Deck.Cards;
    }
}
