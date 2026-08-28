using System.Collections.Generic;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 贝拉初始遗物系共用基类：在贝极星/闪耀贝极星图标上显示当前阳/阴牌数双角标。
/// 左下角=阳（白），右下角=阴（暗红）。
/// </summary>
public abstract class BellaStarterRelicTemplate : ModStarterRelicTemplate,
    IRelicExtraIconAmountLabelSpecsProvider
{
    /// <summary>
    /// 图标双数字：左下角=当前阳牌数，右下角=当前阴牌数。
    /// 战斗外读卡组、战斗内读当前牌堆，与 <see cref="BellaYinYangService"/> 状态判定口径一致。
    /// </summary>
    public IReadOnlyList<ExtraIconAmountLabelSpec> GetRelicExtraIconAmountLabelSpecs()
    {
        var (yang, yin) = BellaYinYangService.GetYinYangCounts(Owner);
        return
        [
            ExtraIconAmountLabelSpec.RichText(
                ExtraIconAmountLabelCorner.BottomLeft,
                $"[color=#ffffff]{yang}[/color]"),
            ExtraIconAmountLabelSpec.RichText(
                ExtraIconAmountLabelCorner.BottomRight,
                $"[color=#8b0000]{yin}[/color]"),
        ];
    }

    /// <summary>
    /// 由 <see cref="BellaYinYangService"/> 在卡组/牌堆变动、战斗开始/结束后调用，刷新图标角标。
    /// </summary>
    public void NotifyYinYangCountChanged()
    {
        InvokeDisplayAmountChanged();
    }
}
