using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 贝极星遗物攻击加成能力：所有攻击牌造成伤害 +Amount（层数即基础加成）。
/// 贝极星施加 1 层（+1），闪耀贝极星施加 3 层（+3）。
/// 升级后的攻击牌额外 +2 伤害。
/// 对连续攻击的每段均生效（由 ModifyDamageAdditive 按 ValueProp 逐段返回）。
/// </summary>
public sealed class BellarisStrengthPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 遗物能力不显示图标。
    protected override bool IsVisibleInternal => false;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (base.Owner != dealer)
            return 0m;

        if (!props.IsPoweredAttack())
            return 0m;

        // 基础加成：层数即基础加成（贝极星=1，闪耀贝极星=3）。
        decimal bonus = base.Amount;

        // 升级加成：升级后的攻击牌额外 +2 伤害。
        if (cardSource != null && cardSource.IsUpgraded)
            bonus += 2m;

        return bonus;
    }
}
