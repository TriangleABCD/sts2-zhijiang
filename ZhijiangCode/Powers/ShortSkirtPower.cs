using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 短裙的临时效果：回合结束弃牌前，给手牌中所有阳牌施加单回合保留。
/// 回合结束后自动移除（Counter 层数递减），实现参考原版 WellLaidPlansPower 的
/// BeforeFlushLate + GiveSingleTurnRetain 机制。
/// </summary>
public sealed class ShortSkirtPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 技能牌产生的间接效果，不显示图标。
    protected override bool IsVisibleInternal => false;

    public override async Task BeforeFlushLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player)
            return;
        if (!Hook.ShouldFlush(player.Creature.CombatState, player))
            return;

        var yangCards = PileType.Hand.GetPile(player).Cards
            .Where(c => c.Keywords.Contains(BellaYinYangService.YangKeywordId.GetModCardKeyword()))
            .ToList();
        foreach (var card in yangCards)
        {
            if (!card.ShouldRetainThisTurn)
                card.GiveSingleTurnRetain();
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            await PowerCmd.Decrement(this);
        }
    }
}
