using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 贝极星遗物黑拉攻击加成能力：处于黑拉状态（阴多于阳）时，所有攻击牌造成伤害 +Amount（层数即加成值）。
/// 贝极星施加 2 层（+2），闪耀贝极星施加 4 层（+4）。
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

        // 仅黑拉状态生效。
        if (base.Owner.Player is not { } player || !BellaYinYangService.IsHeiLa(player))
            return 0m;

        // 层数即攻击加成（贝极星=2，闪耀贝极星=4）。
        return base.Amount;
    }
}
