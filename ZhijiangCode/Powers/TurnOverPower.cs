using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 了转反标记：本场战斗内黑白拉判定翻转的隐藏开关（仅作逻辑标记）。
/// BellaYinYangService.IsInverted 检测其存在；状态图标与贝极星代价由服务实时同步翻转。
/// </summary>
public sealed class TurnOverPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    // 隐藏：翻转效果通过白拉/黑拉状态图标与代价的实时变化体现。
    protected override bool IsVisibleInternal => false;
}
