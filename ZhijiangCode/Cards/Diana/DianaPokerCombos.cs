using MegaCrit.Sts2.Core.Models;
using Zhijiang.ZhijiangCode.Keywords;

namespace Zhijiang.ZhijiangCode.Cards.Diana;

/// <summary>同点组：手牌里点数相同的 2 张及以上（对子 / 三条 / 炸弹 / 五同点 …）。</summary>
public sealed record SameRankGroup(int Rank, IReadOnlyList<CardModel> Cards)
{
    public int Count => Cards.Count;
}

/// <summary>顺子：手牌里 4 张及以上连续点数（3～A，2 不参与）。</summary>
public sealed record StraightRun(int StartRank, int EndRank, IReadOnlyList<CardModel> Cards)
{
    public int Length => EndRank - StartRank + 1;
}

/// <summary>一次手牌组合判定结果（同点组与顺子各自独立，互不排斥）。</summary>
public sealed record ComboEvaluation(
    IReadOnlyList<SameRankGroup> SameRankGroups,
    IReadOnlyList<StraightRun> Straights)
{
    public bool IsEmpty => SameRankGroups.Count == 0 && Straights.Count == 0;
}

/// <summary>
/// 嘉然手牌扑克组合识别。UI 高亮与所有"若手牌有 N 同点 / 顺子"的增益判定都必须走这里，
/// 避免出现"高亮亮 4 张、增益按 3 张结算"这类不一致。
///
/// 规则（2026-10 定稿）：
/// - 同点：按手牌张数计（重复卡计入），2 张起成立，无上限；
/// - 顺子：4 张起成立，点数区间 3～A（2 不参与，A 只在高位）；同一手牌里不重叠，
///   把可用点数排序后按"最长连续段"切分（3,4,5,6,7,8 是一段 6 张，不是两段 4 张）；
/// - 重复点数进顺子时只取其中任意一张；
/// - 大小王不参与同点也不参与顺子；
/// - 同点组与顺子可以同时存在（例如 3,3,4,5,6 同时有"2 同点 3"和"顺子 3-4-5-6"）。
/// </summary>
public static class DianaPokerCombos
{
    /// <summary>同点成立的最小张数。</summary>
    public const int MinSameRankCount = 2;

    /// <summary>顺子成立的最小张数。</summary>
    public const int MinStraightLength = 4;

    /// <summary>识别手牌里的同点组与顺子（均按优先级排好序）。</summary>
    public static ComboEvaluation Evaluate(IReadOnlyList<CardModel> hand)
    {
        var entries = new List<(CardModel Item, PokerFace Face)>(hand.Count);
        foreach (CardModel card in hand)
            if (DianaPokerKeywords.TryGetFace(card, out PokerFace face))
                entries.Add((card, face));

        SameRankGroup[] groups = GroupSameRank(entries)
            .Select(group => new SameRankGroup(group.Rank, group.Cards))
            .ToArray();

        StraightRun[] straights = FindStraights(entries)
            .Select(run => new StraightRun(run.Start, run.End, run.Cards))
            .ToArray();

        return new ComboEvaluation(groups, straights);
    }

    private sealed record RankGroup<T>(int Rank, List<T> Cards);

    private sealed record StraightGroup<T>(int Start, int End, List<T> Cards);

    /// <summary>同点组：按张数降序、点数降序排列。</summary>
    private static List<RankGroup<T>> GroupSameRank<T>(IReadOnlyList<(T Item, PokerFace Face)> entries)
    {
        var byRank = new Dictionary<int, List<T>>();

        foreach ((T item, PokerFace face) in entries)
        {
            if (!face.CountsForSameRank)
                continue;

            if (!byRank.TryGetValue(face.Rank, out List<T>? cards))
                byRank[face.Rank] = cards = [];

            cards.Add(item);
        }

        return byRank
            .Where(pair => pair.Value.Count >= MinSameRankCount)
            .Select(pair => new RankGroup<T>(pair.Key, pair.Value))
            .OrderByDescending(group => group.Cards.Count)
            .ThenByDescending(group => group.Rank)
            .ToList();
    }

    /// <summary>顺子：按长度降序、起始点数降序排列；每个点数只取一张牌。</summary>
    private static List<StraightGroup<T>> FindStraights<T>(IReadOnlyList<(T Item, PokerFace Face)> entries)
    {
        var firstByRank = new Dictionary<int, T>();

        foreach ((T item, PokerFace face) in entries)
            if (face.CountsForStraight)
                firstByRank.TryAdd(face.Rank, item);

        List<int> ranks = firstByRank.Keys.OrderBy(rank => rank).ToList();
        var runs = new List<StraightGroup<T>>();

        var start = 0;
        while (start < ranks.Count)
        {
            var end = start;
            while (end + 1 < ranks.Count && ranks[end + 1] == ranks[end] + 1)
                end++;

            int length = end - start + 1;
            if (length >= MinStraightLength)
            {
                var cards = new List<T>(length);
                for (var i = start; i <= end; i++)
                    cards.Add(firstByRank[ranks[i]]);

                runs.Add(new StraightGroup<T>(ranks[start], ranks[end], cards));
            }

            start = end + 1;
        }

        return runs
            .OrderByDescending(run => run.Cards.Count)
            .ThenByDescending(run => run.Start)
            .ToList();
    }

    // ---------------- 自检 ----------------
    // 组合识别是全部扑克机制的公共地基，这里用无卡牌依赖的合成牌面跑一组断言，
    // 把贪心、重叠、井号这些都钉住；启动时由 Entry 调一次（开销可忽略）。

    public static bool RunSelfCheck(out string failure)
    {
        failure = string.Empty;

        var cases = new (string Name, PokerFace[] Hand, string Expected)[]
        {
            ("对子", [S(3), D(3)], "G[3:2]S[]"),
            ("四张同点只算一个组", [S(3), H(3), D(3), C(3)], "G[3:4]S[]"),
            ("三连不算顺子", [S(3), S(4), S(5)], "G[]S[]"),
            ("最小顺子 4 张", [S(3), S(4), S(5), S(6)], "G[]S[3-6]"),
            ("六连只报一段", [S(3), S(4), S(5), S(6), S(7), S(8)], "G[]S[3-8]"),
            ("两段独立顺子", [S(3), S(4), S(5), S(6), S(8), S(9), S(10), S(11)], "G[]S[8-11,3-6]"),
            ("2 不参与顺子", [S(2), S(3), S(4), S(5), S(6)], "G[]S[3-6]"),
            ("A 只在高位", [S(10), S(11), S(12), S(13), A()], "G[]S[10-14]"),
            ("大小王不参与", [BJ(), SJ(), S(3), S(4), S(5), S(6)], "G[]S[3-6]"),
            ("重复点数只占一张顺子", [S(3), H(3), S(4), S(5), S(6)], "G[3:2]S[3-6]"),
            ("两组同点按张数再点数排序", [S(3), H(3), D(3), S(7), H(7), D(7)], "G[7:3,3:3]S[]"),
            ("对子按点数降序", [S(3), H(3), S(4), H(4)], "G[4:2,3:2]S[]"),
        };

        foreach ((string name, PokerFace[] hand, string expected) in cases)
        {
            string actual = Signature(hand);
            if (actual != expected)
            {
                failure = $"{name}: 期望 {expected}，实际 {actual}";
                return false;
            }
        }

        return true;
    }

    private static string Signature(PokerFace[] hand)
    {
        var entries = hand.Select(face => (Item: face, Face: face)).ToList();
        List<RankGroup<PokerFace>> groups = GroupSameRank(entries);
        List<StraightGroup<PokerFace>> straights = FindStraights(entries);

        return "G[" + string.Join(",", groups.Select(g => $"{g.Rank}:{g.Cards.Count}")) + "]S[" +
               string.Join(",", straights.Select(s => $"{s.Start}-{s.End}")) + "]";
    }

    private static PokerFace S(int rank) => new(PokerSuit.Spade, rank);
    private static PokerFace H(int rank) => new(PokerSuit.Heart, rank);
    private static PokerFace D(int rank) => new(PokerSuit.Diamond, rank);
    private static PokerFace C(int rank) => new(PokerSuit.Club, rank);
    private static PokerFace A() => new(PokerSuit.Spade, PokerFace.AceRank);
    private static PokerFace SJ() => new(PokerSuit.None, PokerFace.SmallJokerRank);
    private static PokerFace BJ() => new(PokerSuit.None, PokerFace.BigJokerRank);
}
