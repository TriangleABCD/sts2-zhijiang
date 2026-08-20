using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 绝无拉我能力：每次损失生命值时，获得 {Amount} 点心之壁。
/// 层数即每次获得的心之壁数值（未升级=3，升级后=4）。
/// 触发判定同 EvilBellaPower：仅自身受到未格挡的伤害时触发。
/// </summary>
[RegisterPower]
public sealed class NeverForgivePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => true;

    // 专属能力图标：never_forgive_power_64x64.png / never_forgive_power_256x256.png（待替换为成品图）。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/never_forgive_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/never_forgive_power_256x256.png");

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 仅自身受到未格挡的伤害时触发。
        if (target != base.Owner || result.UnblockedDamage <= 0)
            return;

        if (base.Owner.Player is not { } player)
            return;

        await SecondaryResourceCmd.Gain(player, HeartWall.HeartWallId, base.Amount, null);
    }
}
