using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 日程表能力：本回合内每消耗 1 点心之壁，获得 1 点格挡。
/// 技能牌产生的间接效果，不显示图标，回合结束（下一回合开始）自动移除。
/// </summary>
public sealed class SchedulePower : PowerModel, ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 每张日程表独立生效：多张时每次消耗心之壁每张各提供 1 格挡。
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    // 技能牌产生的间接效果，不显示图标。
    protected override bool IsVisibleInternal => false;

    public async Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {
        // 仅自己、只关心心之壁减少。
        if (context.Player.Creature != base.Owner)
            return;
        if (context.Definition.Id != HeartWall.HeartWallId)
            return;
        if (context.Delta >= 0)
            return;

        int loss = -context.Delta;
        if (loss <= 0)
            return;

        await CreatureCmd.GainBlock(base.Owner, loss, ValueProp.Move, null);
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 拥有者下一回合开始时移除。
        if (side != Owner.Side)
            return;

        Flash();
        await PowerCmd.Remove(this);
    }
}
