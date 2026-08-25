using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 情感障碍能力：白拉时每回合开始获得（心之壁 ÷ 10）点格挡；
/// 升级后（HeartWallGain=1）每回合额外获得 1 点心之壁。
/// </summary>
[RegisterPower]
public sealed class EmotionalDisorderPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => true;

    // 图标占位：暂用白拉图标。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/bai_la_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/bai_la_power_256x256.png");

    /// <summary>升级后每回合额外获得的心之壁数量（0/1）。</summary>
    public void SetHeartWallGain(decimal gain)
    {
        AssertMutable();
        base.DynamicVars["HeartWallGain"].BaseValue = gain;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side)
            return;
        if (base.Owner.Player is not { } player)
            return;
        if (!BellaYinYangService.IsBaiLa(player))
            return;

        // 获得（心之壁 ÷ 10）点格挡。
        int heartWall = SecondaryResourceCmd.Get(player, HeartWall.HeartWallId);
        int block = heartWall / 10;
        if (block > 0)
            await CreatureCmd.GainBlock(base.Owner, block, ValueProp.Move, null);

        // 升级后每回合额外获得 1 心之壁。
        if (DynamicVars["HeartWallGain"].IntValue > 0)
            await SecondaryResourceCmd.Gain(player, HeartWall.HeartWallId, 1, this);
    }
}
