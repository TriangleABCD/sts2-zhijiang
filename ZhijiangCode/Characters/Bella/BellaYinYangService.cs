using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
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

    /// <summary>
    /// 战斗开始时为贝拉玩家施加差值修正能力与可见状态标记（白拉/黑拉）。
    /// 在 Entry.Initialize 中订阅 CombatStartingEvent 调用。
    /// </summary>
    public static void RegisterCombatCorrection()
    {
        RitsuLibFramework.SubscribeLifecycle<CombatStartingEvent>(async evt =>
        {
            if (evt.CombatState is not { } combat)
                return;

            foreach (var player in combat.Players)
            {
                if (player.Character is not BellaCharacter)
                    continue;

                // 施加差值修正能力：层数恒为 1，修正幅度在能力内部按当前差值计算。
                await PowerCmd.Apply<BellaYinYangCorrectionPower>(
                    new ThrowingPlayerChoiceContext(), player.Creature, 1, player.Creature, null);

                // 施加当前状态的可见标记。
                var ctx = new ThrowingPlayerChoiceContext();
                if (IsBaiLa(player))
                    await PowerCmd.Apply<BaiLaPower>(ctx, player.Creature, 1, player.Creature, null);
                else
                    await PowerCmd.Apply<HeiLaPower>(ctx, player.Creature, 1, player.Creature, null);
            }
        });
    }

    /// <summary>
    /// 计算阴阳差值 d = 阳牌数 − 阴牌数。
    /// d &gt; 0 白拉、d &lt; 0 黑拉、d == 0 白拉（无修正）。
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

    /// <summary>是否处于白拉状态（阳 ≥ 阴）。</summary>
    public static bool IsBaiLa(Player player) => ComputeDiff(player) >= 0;

    /// <summary>是否处于黑拉状态（阴 &gt; 阳）。</summary>
    public static bool IsHeiLa(Player player) => ComputeDiff(player) < 0;

    /// <summary>
    /// 计算一张卡牌与当前状态相符时获得的修正幅度（正整数）。
    /// 相符：阳牌在白拉 / 阴牌在黑拉；相反则为负修正（由调用方决定方向）。
    /// 公式：|d| ÷ 3，d = 阳牌数 − 阴牌数。
    /// </summary>
    public static int ComputeMagnitude(Player player)
    {
        return Math.Abs(ComputeDiff(player)) / 3;
    }

    /// <summary>
    /// 判断某张卡牌当前是否与状态相符（获得加成而非削弱）。
    /// </summary>
    public static bool IsAligned(Player player, CardModel card)
    {
        bool isYang = card.Keywords.Contains(YangKeywordId.GetModCardKeyword());
        return isYang == IsBaiLa(player);
    }

    /// <summary>
    /// 玩家当前参与状态计数的卡牌。
    /// 战斗内为除消耗堆外的所有牌堆（抽牌/手牌/弃牌/打出等）——消耗掉的阴阳牌不参与计数；
    /// 战斗外为主卡组。
    /// </summary>
    private static IEnumerable<CardModel> GetOwnedCards(Player player)
    {
        if (player.PlayerCombatState is { } combat)
            return combat.AllPiles.Where(p => p.Type != PileType.Exhaust).SelectMany(p => p.Cards);
        return player.Deck.Cards;
    }
}
