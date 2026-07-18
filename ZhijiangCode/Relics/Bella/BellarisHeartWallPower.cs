using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 贝极星遗物赋予的临时敏捷（心之壁换算）。回合结束时自动移除并扣回敏捷。
/// 每个角色需要自己的实现以绑定 OriginModel。
/// </summary>
public sealed class BellarisHeartWallPower : TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Relic<Bellaris>();

    // 隐藏能力图标，不在角色下方显示。
    protected override bool IsVisibleInternal => false;
}
