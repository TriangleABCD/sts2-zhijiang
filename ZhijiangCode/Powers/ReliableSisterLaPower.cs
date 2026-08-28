using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 喔拉姐，可靠的拉姐能力：每当你失去心之壁，对随机一名敌人造成损失量 ÷ Amount 的伤害。
/// Amount 为除数（5，升级后 3）。
/// </summary>
[RegisterPower]
public sealed class ReliableSisterLaPower : ModPowerTemplate, ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => true;

    // 专属能力图标：reliable_sister_la_power_64x64.png / reliable_sister_la_power_256x256.png（待补成品图）。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/reliable_sister_la_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/reliable_sister_la_power_256x256.png");

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
        int divisor = Math.Max(1, Amount);
        int damage = loss / divisor;
        if (damage <= 0)
            return;

        IReadOnlyList<Creature> enemies = base.CombatState?.HittableEnemies ?? Array.Empty<Creature>();
        if (enemies.Count == 0)
            return;
        Creature? target = context.Player.RunState.Rng.CombatTargets.NextItem(enemies);
        if (target is null)
            return;

        Flash();
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, damage, ValueProp.Unpowered, base.Owner, null);
    }
}
