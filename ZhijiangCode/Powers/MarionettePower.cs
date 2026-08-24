using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 提线木偶能力：下回合同等数量减少心之壁，随后自动移除。
/// 技能牌产生的间接效果，不显示图标。
/// </summary>
public sealed class MarionettePower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    // 技能牌产生的间接效果，不显示图标。
    protected override bool IsVisibleInternal => false;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 拥有者下一回合开始时还款。
        if (side != Owner.Side)
            return;
        if (base.Owner.Player is not { } player)
            return;

        int debt = Amount;
        if (debt > 0)
            await SecondaryResourceCmd.Lose(player, HeartWall.HeartWallId, debt, this);

        Flash();
        await PowerCmd.Remove(this);
    }
}
