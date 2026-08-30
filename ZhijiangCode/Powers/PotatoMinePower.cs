using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 小土豆雷能力：3 回合倒计时（Amount 显示剩余回合数），玩家侧回合结束时递减；
/// 倒数到 1 时引爆，对当前血量最多的敌人造成固定伤害（不受力量修正），随后移除。
/// 参考原版 TheBombPower。
/// ⚠️ 按项目「能力图标规则」非能力牌产生的间接能力本应 IsVisibleInternal=false，
/// 但倒计时是本卡的核心可玩信息（同原版炸弹的倒计时图标），故破例显示；
/// 如要遵循统一规则可改为 false（卡牌描述已写明回合数）。
/// </summary>
[RegisterPower]
public sealed class PotatoMinePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 每颗雷是独立实例：多张雷各自倒计时，而不是把回合数加到一起（参考原版 TheBombPower）。
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override bool IsVisibleInternal => true;

    // 专属炸弹图标：potato_mine_power_64x64.png / potato_mine_power_256x256.png（待替换为成品图）。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/potato_mine_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/potato_mine_power_256x256.png");

    // 爆炸伤害（固定值、不受力量）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(17m, ValueProp.Unpowered)
    ];

    /// <summary>设定本实例的爆炸伤害（Amount 用于倒计时，伤害需单独存储，同 TheBombPower.SetDamage）。</summary>
    public void SetDamage(decimal damage)
    {
        AssertMutable();
        base.DynamicVars.Damage.BaseValue = damage;
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(base.Owner))
            return;

        if (base.Amount > 1m)
        {
            await PowerCmd.Decrement(this);
            return;
        }

        // 倒计时结束：引爆，攻击当前血量最多的敌人。
        Flash();
        Creature? target = base.CombatState?.HittableEnemies
            .OrderByDescending(e => e.CurrentHp)
            .FirstOrDefault();
        if (target != null)
        {
            await CreatureCmd.Damage(choiceContext, target, base.DynamicVars.Damage, base.Owner);
        }
        await PowerCmd.Remove(this);
    }
}
