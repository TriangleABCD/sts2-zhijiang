using System;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 贝拉阴阳差值修正能力：根据当前阴阳差值 d = |阳牌数 − 阴牌数|，
/// 对参与修正的卡牌按相符/相反状态施加 `±(d ÷ 3)` 修正。
/// - 相符（阳牌在白拉 / 阴牌在黑拉）：效果值 +d÷3
/// - 相反（阳牌在黑拉 / 阴牌在白拉）：效果值 -d÷3，修正后至少保留 1 点
/// 只对实现了 <see cref="IBellaYinYangCorrectionCard" /> 且声明对应数值类型的卡牌生效，
/// 其余卡牌天然豁免。战斗开始时由贝拉角色施加。
/// </summary>
public sealed class BellaYinYangCorrectionPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 角色机制能力不显示图标。
    protected override bool IsVisibleInternal => false;

    // 伤害修正：仅对声明参与伤害修正的卡牌生效。
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (base.Owner != dealer)
            return 0m;

        if (!props.IsPoweredAttack())
            return 0m;

        if (cardSource is not IBellaYinYangCorrectionCard card || !card.CorrectDamage)
            return 0m;

        return CorrectionFor(cardSource, amount);
    }

    // 格挡修正：仅对声明参与格挡修正的卡牌生效。
    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (base.Owner != target)
            return 0m;

        if (cardSource is not IBellaYinYangCorrectionCard card || !card.CorrectBlock)
            return 0m;

        return CorrectionFor(cardSource, block);
    }

    // 计算单张卡的修正值。amount/block 为当前效果值，用于保底（修正后至少 1）。
    private decimal CorrectionFor(CardModel cardSource, decimal currentValue)
    {
        if (base.Owner.Player is not { } player)
            return 0m;

        int diff = BellaYinYangService.ComputeDiff(player);
        if (diff == 0)
            return 0m;

        // 修正幅度取差值绝对值：黑拉时 diff 为负，若直接 diff/3 会得到负数导致修正被跳过。
        int magnitude = Math.Abs(diff) / 3;
        if (magnitude <= 0)
            return 0m;

        bool isYang = cardSource.Keywords.Contains(BellaYinYangService.YangKeywordId.GetModCardKeyword());
        bool isBaiLa = BellaYinYangService.IsBaiLa(player);

        // 相符：阳牌在白拉 或 阴牌在黑拉；否则为相反。
        bool aligned = isYang == isBaiLa;

        decimal correction = aligned ? magnitude : -magnitude;

        // 保底：相反时修正后效果至少保留 1 点。
        if (correction < 0)
            correction = Math.Max(correction, 1 - currentValue);

        return correction;
    }
}
