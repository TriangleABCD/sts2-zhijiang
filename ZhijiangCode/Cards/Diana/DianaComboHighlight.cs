using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Scaffolding.Cards.HandOutline;
using STS2RitsuLib.Scaffolding.Content;

namespace Zhijiang.ZhijiangCode.Cards.Diana;

/// <summary>手牌组合高亮要显示哪一族。</summary>
public enum DianaComboView
{
    /// <summary>不显示任何高亮（默认状态）。</summary>
    None = 0,
    SameRank,
    Straight,
    /// <summary>两族都显示（组号在两族之间连续编号），留给以后可能的"全部显示"入口。</summary>
    All,
}

/// <summary>
/// 手牌组合高亮状态：谁被点亮、点第几组、用什么强度色。
///
/// 数据一律来自 <see cref="DianaPokerCombos.Evaluate" />；描边（RitsuLib hand outline）与
/// 卡面角标（DianaComboBadgePatch）都只读这里，保证"高亮的"和"结算的"是同一份判定。
/// 手牌每次变化（抽牌/出牌/弃牌/生成）都会重算 —— 由 DianaComboHandRefreshPatch 触发。
/// </summary>
public static class DianaComboHighlight
{
    /// <summary>描边 / 角标的强度档位：0=金（弱）、1=红（中）、2=青（强）。</summary>
    public readonly record struct Entry(int GroupIndex, int Tier);

    private static readonly Dictionary<CardModel, Entry> Entries = [];

    /// <summary>当前显示哪一族。默认什么都不亮，点按钮才亮。</summary>
    public static DianaComboView View { get; private set; } = DianaComboView.None;

    /// <summary>最近一次判定的结果（供后续增益结算复用）。</summary>
    public static ComboEvaluation LastEvaluation { get; private set; } = new([], []);

    public static void SetView(DianaComboView view)
    {
        View = view;
        Recompute();
    }

    /// <summary>点已激活的按钮 = 取消高亮；点未激活的 = 切换到该族（两族互斥）。</summary>
    public static void Toggle(DianaComboView view)
    {
        SetView(View == view ? DianaComboView.None : view);
    }

    public static bool TryGet(CardModel card, out Entry entry)
    {
        return Entries.TryGetValue(card, out entry);
    }

    /// <summary>按当前手牌重算高亮集合。手牌不在战斗里时清空。</summary>
    public static void Recompute()
    {
        Entries.Clear();

        NPlayerHand? hand = NPlayerHand.Instance;
        if (hand is null)
        {
            LastEvaluation = new ComboEvaluation([], []);
            return;
        }

        var cards = new List<CardModel>();
        foreach (var holder in hand.ActiveHolders)
            if (holder.CardModel is { } model)
                cards.Add(model);

        ComboEvaluation evaluation = DianaPokerCombos.Evaluate(cards);
        LastEvaluation = evaluation;

        var groupIndex = 0;

        if (View is DianaComboView.All or DianaComboView.SameRank)
        {
            foreach (SameRankGroup group in evaluation.SameRankGroups)
            {
                groupIndex++;
                int tier = group.Count switch
                {
                    <= 2 => 0,
                    3 => 1,
                    _ => 2,
                };
                foreach (CardModel card in group.Cards)
                    Entries[card] = new Entry(groupIndex, tier);
            }
        }

        if (View is DianaComboView.All or DianaComboView.Straight)
        {
            foreach (StraightRun run in evaluation.Straights)
            {
                groupIndex++;
                int tier = run.Length switch
                {
                    <= 4 => 0,
                    5 => 1,
                    _ => 2,
                };
                foreach (CardModel card in run.Cards)
                    Entries[card] = new Entry(groupIndex, tier);
            }
        }
    }

    /// <summary>强度档位 → 手牌描边颜色（游戏只提供这三种）。</summary>
    public static Color TierColor(int tier)
    {
        return tier switch
        {
            1 => NCardHighlight.red,
            2 => NCardHighlight.playableColor,
            _ => NCardHighlight.gold,
        };
    }

    /// <summary>注册手牌描边规则（一次覆盖本 mod 全部卡牌）。</summary>
    public static void RegisterOutline()
    {
        ModCardHandOutlineRegistry.Register<ModCardTemplate>(
            ModCardHandOutlineRules.Switch<ModCardTemplate>(
                card => TryGet(card, out Entry entry) ? TierColor(entry.Tier) : null,
                priority: 1,
                visibleWhenUnplayable: true,
                refreshEveryFrame: true));
    }
}
