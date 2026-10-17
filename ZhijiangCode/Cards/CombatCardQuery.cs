using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Zhijiang.ZhijiangCode.Cards;

/// <summary>
/// 卡牌/牌堆查询收口。游戏把 PlayerCombatState、CardModel.Pile 标成可空，
/// 判空在这里统一处理，避免每张"清点状态牌"的卡各写一遍 ! 断言。
/// </summary>
internal static class CombatCardQuery
{
    /// <summary>
    /// 该玩家本场战斗中仍在循环里的牌：抽牌堆 + 手牌 + 弃牌堆 + 打出堆，不含已消耗的牌。
    /// </summary>
    public static IEnumerable<CardModel> CardsInCycle(this Player player)
        => player.PlayerCombatState?.AllCards.Where(c => c.Pile is { Type: not PileType.Exhaust }) ?? [];
}
