using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 贝拉气晕能力：每当敌人的攻击被自己的格挡完全挡下（未穿透），击晕该敌人。
/// 只认攻击伤害（ValueProp.Move）且来源是存活敌人；已处于击晕状态的敌人不重复击晕。
/// 可见能力，本地化见 powers.json。
/// </summary>
[RegisterPower]
public sealed class FaintedByBellaPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override bool IsVisibleInternal => true;

    // 图标待补：占位路径指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺图标的能力）。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/fainted_by_bella_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/fainted_by_bella_power_256x256.png");

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 只处理自己挨打。
        if (target != base.Owner)
            return;

        // 必须被格挡完全挡下（没穿透）。
        if (!result.WasFullyBlocked)
            return;

        // 只认攻击伤害：怪物能力/中毒等非攻击伤害不算「突破格挡」。
        if (!props.HasFlag(ValueProp.Move))
            return;

        if (dealer == null || !dealer.IsMonster || dealer.IsDead || dealer.IsStunned)
            return;

        await CreatureCmd.Stun(dealer);
    }
}
