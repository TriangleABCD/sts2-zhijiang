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
        IReadOnlyDictionary<CardKeyword, string> SuitSymbols,
        IReadOnlyDictionary<CardKeyword, bool> SuitIsRed,
        IReadOnlyDictionary<CardKeyword, string> RankTexts,
        CardKeyword BigJokerValue,
        CardKeyword SmallJokerValue);

    // 关键词在注册完成（mod init）后才能取铸造值，所以延迟到第一次渲染时构建。
    private static readonly Lazy<KeywordLookup> Lookup = new(BuildLookup);

    private static KeywordLookup BuildLookup()
    {
        return new KeywordLookup(
            new Dictionary<CardKeyword, string>
            {
                [SuitSpadeId.GetModCardKeyword()] = "♠",
                [SuitHeartId.GetModCardKeyword()] = "♥",
                [SuitDiamondId.GetModCardKeyword()] = "♦",
                [SuitClubId.GetModCardKeyword()] = "♣",
            },
            new Dictionary<CardKeyword, bool>
            {
                [SuitSpadeId.GetModCardKeyword()] = false,
                [SuitHeartId.GetModCardKeyword()] = true,
                [SuitDiamondId.GetModCardKeyword()] = true,
                [SuitClubId.GetModCardKeyword()] = false,
            },
            new Dictionary<CardKeyword, string>
            {
                [RankAId.GetModCardKeyword()] = "A",
                [Rank2Id.GetModCardKeyword()] = "2",
                [Rank3Id.GetModCardKeyword()] = "3",
                [Rank4Id.GetModCardKeyword()] = "4",
                [Rank5Id.GetModCardKeyword()] = "5",
                [Rank6Id.GetModCardKeyword()] = "6",
                [Rank7Id.GetModCardKeyword()] = "7",
                [Rank8Id.GetModCardKeyword()] = "8",
                [Rank9Id.GetModCardKeyword()] = "9",
                [Rank10Id.GetModCardKeyword()] = "10",
                [RankJId.GetModCardKeyword()] = "J",
                [RankQId.GetModCardKeyword()] = "Q",
                [RankKId.GetModCardKeyword()] = "K",
            },
            BigJokerId.GetModCardKeyword(),
            SmallJokerId.GetModCardKeyword());
    }

    /// <summary>
    /// 把卡牌上的花色/点数关键词合成一行卡面文字（如 <c>[gold]♠3[/gold]</c>）。
    /// 没有点数关键词的牌（非嘉然卡、状态牌、诅咒等）返回 null，调用方不注入任何内容。
    /// </summary>
    public static string? BuildLabel(CardModel card)
    {
        KeywordLookup lookup = Lookup.Value;

        string? suitSymbol = null;
        var suitIsRed = false;
        string? rankText = null;

        foreach (CardKeyword keyword in card.Keywords)
        {
            if (lookup.SuitSymbols.TryGetValue(keyword, out string? symbol))
            {
                suitSymbol = symbol;
                suitIsRed = lookup.SuitIsRed[keyword];
            }
            else if (lookup.RankTexts.TryGetValue(keyword, out string? text))
            {
                rankText = text;
            }
            else if (keyword == lookup.BigJokerValue)
            {
                // 大小王没有花色：牌面就是它自己。
                return RedOpen + "大王" + RedClose;
            }
            else if (keyword == lookup.SmallJokerValue)
            {
                return GoldOpen + "小王" + GoldClose;
            }
        }

        if (rankText is null)
            return null;

        if (suitSymbol is null)
            return GoldOpen + rankText + GoldClose;

        return suitIsRed
            ? RedOpen + suitSymbol + RedClose + GoldOpen + rankText + GoldClose
            : GoldOpen + suitSymbol + rankText + GoldClose;
    }
}
