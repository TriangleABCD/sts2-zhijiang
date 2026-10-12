using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 对抗路情侣能力：每回合开始时获得 {Amount} 点心之壁。
/// 层数即每回合获得量（未升级=10，升级后=20），多张叠加。
/// 可见能力，本地化见 powers.json。
/// </summary>
[RegisterPower]
public sealed class CoupleOnTheClashLanePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => true;

    // 图标待补：占位路径指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺图标的能力）。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/couple_on_the_clash_lane_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/couple_on_the_clash_lane_power_256x256.png");

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != Owner.Side)
            return;

        if (base.Owner.Player is not { } player)
            return;

        await SecondaryResourceCmd.Gain(player, HeartWall.HeartWallId, base.Amount, this);
    }
}
