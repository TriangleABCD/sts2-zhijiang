using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Zhijiang.ZhijiangCode.Keywords;

// 嘉然扑克牌面：花色与点数拆成两组独立关键词，大小王只有点数、没有花色。
//
// 注册时 CardDescriptionPlacement 刻意保持默认的 None —— RitsuLib 的注入器会让每个关键词各占一行、
// 且句尾补一个句点（"♠." 换行 "3."），做不出牌面 "♠3"。卡面文字改由
// Patches/DianaPokerCardTextPatch 在描述构建的最后一步把花色+点数合成一行插到描述上方。
//
// IncludeInCardHoverTip = false：扑克牌面没有需要解释的规则，卡牌 hover 提示里不再列出 ♠ / 3；
// 本地化里的 .description 因此只作为以后可能复用的备用文案。
//
// 生成的 id：ZHIJIANG_KEYWORD_POKER_SPADE / ..._POKER_HEART / ..._POKER_DIAMOND / ..._POKER_CLUB
//            ZHIJIANG_KEYWORD_POKER_A ... _POKER_10 / _POKER_J / _POKER_Q / _POKER_K
//            ZHIJIANG_KEYWORD_POKER_BIG_JOKER / _POKER_SMALL_JOKER
[RegisterOwnedCardKeyword("poker_spade", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_heart", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_diamond", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_club", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_a", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_2", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_3", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_4", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_5", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_6", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_7", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_8", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_9", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_10", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_j", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_q", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_k", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_big_joker", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("poker_small_joker", IncludeInCardHoverTip = false)]
public sealed class DianaPokerKeywords
{
    // ---- 花色（4） ----
    public const string SuitSpadeId = "ZHIJIANG_KEYWORD_POKER_SPADE";
    public const string SuitHeartId = "ZHIJIANG_KEYWORD_POKER_HEART";
    public const string SuitDiamondId = "ZHIJIANG_KEYWORD_POKER_DIAMOND";
    public const string SuitClubId = "ZHIJIANG_KEYWORD_POKER_CLUB";

    // ---- 点数（13 + 大小王） ----
    public const string RankAId = "ZHIJIANG_KEYWORD_POKER_A";
    public const string Rank2Id = "ZHIJIANG_KEYWORD_POKER_2";
    public const string Rank3Id = "ZHIJIANG_KEYWORD_POKER_3";
    public const string Rank4Id = "ZHIJIANG_KEYWORD_POKER_4";
    public const string Rank5Id = "ZHIJIANG_KEYWORD_POKER_5";
    public const string Rank6Id = "ZHIJIANG_KEYWORD_POKER_6";
    public const string Rank7Id = "ZHIJIANG_KEYWORD_POKER_7";
    public const string Rank8Id = "ZHIJIANG_KEYWORD_POKER_8";
    public const string Rank9Id = "ZHIJIANG_KEYWORD_POKER_9";
    public const string Rank10Id = "ZHIJIANG_KEYWORD_POKER_10";
    public const string RankJId = "ZHIJIANG_KEYWORD_POKER_J";
    public const string RankQId = "ZHIJIANG_KEYWORD_POKER_Q";
    public const string RankKId = "ZHIJIANG_KEYWORD_POKER_K";
    public const string BigJokerId = "ZHIJIANG_KEYWORD_POKER_BIG_JOKER";
    public const string SmallJokerId = "ZHIJIANG_KEYWORD_POKER_SMALL_JOKER";

    // ---- 卡牌侧挂载用（CanonicalKeywords 里返回这些） ----
    public static CardKeyword Spade => SuitSpadeId.GetModCardKeyword();
    public static CardKeyword Heart => SuitHeartId.GetModCardKeyword();
    public static CardKeyword Diamond => SuitDiamondId.GetModCardKeyword();
    public static CardKeyword Club => SuitClubId.GetModCardKeyword();

    public static CardKeyword RankA => RankAId.GetModCardKeyword();
    public static CardKeyword Rank2 => Rank2Id.GetModCardKeyword();
    public static CardKeyword Rank3 => Rank3Id.GetModCardKeyword();
    public static CardKeyword Rank4 => Rank4Id.GetModCardKeyword();
    public static CardKeyword Rank5 => Rank5Id.GetModCardKeyword();
    public static CardKeyword Rank6 => Rank6Id.GetModCardKeyword();
    public static CardKeyword Rank7 => Rank7Id.GetModCardKeyword();
    public static CardKeyword Rank8 => Rank8Id.GetModCardKeyword();
    public static CardKeyword Rank9 => Rank9Id.GetModCardKeyword();
    public static CardKeyword Rank10 => Rank10Id.GetModCardKeyword();
    public static CardKeyword RankJ => RankJId.GetModCardKeyword();
    public static CardKeyword RankQ => RankQId.GetModCardKeyword();
    public static CardKeyword RankK => RankKId.GetModCardKeyword();
    public static CardKeyword BigJoker => BigJokerId.GetModCardKeyword();
    public static CardKeyword SmallJoker => SmallJokerId.GetModCardKeyword();

    // ---- 卡面文字 ----
    // 红桃/方块/大王用红色，其余（黑桃/梅花/小王）跟随关键词默认的金色。
    private const string RedOpen = "[color=#ff5555]";
    private const string RedClose = "[/color]";
    private const string GoldOpen = "[gold]";
    private const string GoldClose = "[/gold]";

    private sealed record KeywordLookup(
        IReadOnlyDictionary<CardKeyword, PokerSuit> Suits,
        IReadOnlyDictionary<CardKeyword, int> Ranks);

    // 关键词在注册完成（mod init）后才能取铸造值，所以延迟到第一次使用时构建。
    private static readonly Lazy<KeywordLookup> Lookup = new(BuildLookup);

    private static KeywordLookup BuildLookup()
    {
        return new KeywordLookup(
            new Dictionary<CardKeyword, PokerSuit>
            {
                [SuitSpadeId.GetModCardKeyword()] = PokerSuit.Spade,
                [SuitHeartId.GetModCardKeyword()] = PokerSuit.Heart,
                [SuitDiamondId.GetModCardKeyword()] = PokerSuit.Diamond,
                [SuitClubId.GetModCardKeyword()] = PokerSuit.Club,
            },
            new Dictionary<CardKeyword, int>
            {
                [RankAId.GetModCardKeyword()] = PokerFace.AceRank,
                [Rank2Id.GetModCardKeyword()] = 2,
                [Rank3Id.GetModCardKeyword()] = 3,
                [Rank4Id.GetModCardKeyword()] = 4,
                [Rank5Id.GetModCardKeyword()] = 5,
                [Rank6Id.GetModCardKeyword()] = 6,
                [Rank7Id.GetModCardKeyword()] = 7,
                [Rank8Id.GetModCardKeyword()] = 8,
                [Rank9Id.GetModCardKeyword()] = 9,
                [Rank10Id.GetModCardKeyword()] = 10,
                [RankJId.GetModCardKeyword()] = 11,
                [RankQId.GetModCardKeyword()] = 12,
                [RankKId.GetModCardKeyword()] = 13,
                [SmallJokerId.GetModCardKeyword()] = PokerFace.SmallJokerRank,
                [BigJokerId.GetModCardKeyword()] = PokerFace.BigJokerRank,
            });
    }

    /// <summary>
    /// 读出一张牌的牌面。没有点数关键词的牌（非嘉然卡、状态牌、诅咒等）返回 false。
    /// 牌面是全部扑克相关判定（组合识别、卡面文字、以后的排序）的唯一入口。
    /// </summary>
    public static bool TryGetFace(CardModel card, out PokerFace face)
    {
        KeywordLookup lookup = Lookup.Value;

        var suit = PokerSuit.None;
        var rank = 0;

        foreach (CardKeyword keyword in card.Keywords)
        {
            if (lookup.Suits.TryGetValue(keyword, out PokerSuit cardSuit))
                suit = cardSuit;
            else if (lookup.Ranks.TryGetValue(keyword, out int cardRank))
                rank = cardRank;
        }

        if (rank == 0)
        {
            face = default;
            return false;
        }

        // 大小王没有花色：即使误挂了花色关键词也不认。
        face = new PokerFace(rank >= PokerFace.SmallJokerRank ? PokerSuit.None : suit, rank);
        return true;
    }

    /// <summary>
    /// 把卡牌上的牌面合成一行卡面文字（如 <c>[gold]♠3[/gold]</c>）。
    /// 没有牌面的牌返回 null，调用方不注入任何内容。
    /// </summary>
    public static string? BuildLabel(CardModel card)
    {
        return TryGetFace(card, out PokerFace face) ? BuildLabel(face) : null;
    }

    /// <summary>牌面 → 卡面文字（红桃/方块/大王红，其余金色）。</summary>
    public static string BuildLabel(PokerFace face)
    {
        if (face.Rank == PokerFace.BigJokerRank)
            return RedOpen + "大王" + RedClose;

        if (face.Rank == PokerFace.SmallJokerRank)
            return GoldOpen + "小王" + GoldClose;

        string rankText = RankText(face.Rank);

        if (face.Suit == PokerSuit.None)
            return GoldOpen + rankText + GoldClose;

        string symbol = SuitSymbol(face.Suit);
        return IsRedSuit(face.Suit)
            ? RedOpen + symbol + RedClose + GoldOpen + rankText + GoldClose
            : GoldOpen + symbol + rankText + GoldClose;
    }

    /// <summary>点数 → 显示文字（A/J/Q/K 用字母，其余用数字）。</summary>
    public static string RankText(int rank)
    {
        return rank switch
        {
            PokerFace.AceRank => "A",
            11 => "J",
            12 => "Q",
            13 => "K",
            PokerFace.SmallJokerRank => "小王",
            PokerFace.BigJokerRank => "大王",
            _ => rank.ToString(),
        };
    }

    private static string SuitSymbol(PokerSuit suit)
    {
        return suit switch
        {
            PokerSuit.Spade => "♠",
            PokerSuit.Heart => "♥",
            PokerSuit.Diamond => "♦",
            PokerSuit.Club => "♣",
            _ => string.Empty,
        };
    }

    private static bool IsRedSuit(PokerSuit suit)
    {
        return suit is PokerSuit.Heart or PokerSuit.Diamond;
    }
}
