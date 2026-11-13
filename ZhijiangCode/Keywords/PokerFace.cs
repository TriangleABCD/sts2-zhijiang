namespace Zhijiang.ZhijiangCode.Keywords;

/// <summary>
/// 花色。大小王没有花色，用 <see cref="None" />。
/// </summary>
public enum PokerSuit
{
    None = 0,
    Spade,
    Heart,
    Diamond,
    Club,
}

/// <summary>
/// 一张牌的扑克牌面。
/// Rank 的取值：2～10 就是点数本身；J=11、Q=12、K=13、A=14；小王=15、大王=16。
/// 大小王没有花色，且不参与同点与顺子。
/// </summary>
public readonly record struct PokerFace(PokerSuit Suit, int Rank)
{
    public const int AceRank = 14;
    public const int SmallJokerRank = 15;
    public const int BigJokerRank = 16;

    public bool IsJoker => Rank >= SmallJokerRank;

    /// <summary>能进顺子的点数：3 到 A（2 不参与顺子，A 只在高位）。</summary>
    public bool CountsForStraight => Rank is >= 3 and <= AceRank;

    /// <summary>能进同点组的点数：2 到 A（大小王不参与）。</summary>
    public bool CountsForSameRank => Rank is >= 2 and <= AceRank;
}
