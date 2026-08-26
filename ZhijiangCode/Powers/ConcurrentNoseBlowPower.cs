using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 9000同接擤鼻涕能力：本回合结束时失去 {Amount} 点心之壁，随后自动移除。
/// 技能牌产生的间接效果，不显示图标。
/// </summary>
public sealed class ConcurrentNoseBlowPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    // 技能牌产生的间接效果，不显示图标。
    protected override bool IsVisibleInternal => false;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 仅拥有者回合结束时扣心之壁。
        if (side != Owner.Side)
            return;
        if (base.Owner.Player is not { } player)
            return;

        int loss = Amount;
        if (loss > 0)
            await SecondaryResourceCmd.Lose(player, HeartWall.HeartWallId, loss, this);

        Flash();
        await PowerCmd.Remove(this);
    }
}
