using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Entities.Players;
using Zhijiang.ZhijiangCode.Keywords;

namespace Zhijiang.ZhijiangCode.Characters.Diana;

/// <summary>
/// 嘉心糖的「王牌花色」状态：每场战斗开始时由玩家四选一，本场只有该花色的增益生效。
/// 状态只存在本体内存里（不进存档），战斗开始时由遗物重置。
/// </summary>
public static class DianaTrumpSuit
{
    /// <summary>本场战斗选定的王牌花色；未选定为 null。</summary>
    public static PokerSuit? Current { get; private set; }

    /// <summary>触发门槛（每 N 张同花色牌触发一次）：基础遗物 3，升级遗物 2。</summary>
    public static int Threshold { get; private set; } = 3;

    public static void Reset()
    {
        Current = null;
        Threshold = 3;
    }

    public static void Set(PokerSuit suit)
    {
        Current = suit;
    }

    public static void SetThreshold(int threshold)
    {
        Threshold = threshold < 1 ? 1 : threshold;
    }

    /// <summary>数手牌里指定花色的张数（大小王没有花色，天然不计入）。</summary>
    public static int CountInHand(Player player, PokerSuit suit)
    {
        var count = 0;
        foreach (var card in PileType.Hand.GetPile(player).Cards)
            if (DianaPokerKeywords.TryGetFace(card, out var face) && face.Suit == suit)
                count++;

        return count;
    }

    /// <summary>本回合该花色能触发几次（张数 ÷ 门槛）。</summary>
    public static int GroupsInHand(Player player, PokerSuit suit)
    {
        return CountInHand(player, suit) / Threshold;
    }

    /// <summary>
    /// 所选王牌花色的效果说明（用于遗物描述追加）。没有选定时返回空串。
    /// 文案直接复用选择卡片上的那两条本地化，保证两处永远一致。
    /// </summary>
    public static string DescribeEffect(PokerSuit suit)
    {
        string? suffix = suit switch
        {
            PokerSuit.Spade => "spade",
            PokerSuit.Heart => "heart",
            PokerSuit.Diamond => "diamond",
            PokerSuit.Club => "club",
            _ => null,
        };

        if (suffix is null)
            return string.Empty;

        string title = new LocString("relics", $"ZHIJIANG_RELIC_JIA_XIN_TANG.suit_{suffix}.title")
            .GetFormattedText();

        var description = new LocString("relics", $"ZHIJIANG_RELIC_JIA_XIN_TANG.suit_{suffix}.description");
        description.Add("Count", Threshold);

        return $"\n\n[gold]{title}[/gold]：{description.GetFormattedText()}";
    }
}
