using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 贝极星遗物：所有攻击牌造成伤害 +2，升级后的攻击牌额外 +3 伤害（共 +5）。
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

        // 基础加成：所有攻击牌 +2 伤害
        decimal bonus = 2m;

        // 升级加成：升级后的攻击牌额外 +3 伤害（共 +5）
        if (cardSource != null && cardSource.IsUpgraded)
            bonus += 3m;

        return bonus;
    }
}
