using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Zhijiang.ZhijiangCode.Relics.Diana;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>王牌花色·黑桃：回合开始时按手牌黑桃数量给的「本回合 +力量」，回合结束自动消失。</summary>
public sealed class DianaTempStrengthPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Relic<JiaXinTang>();

    protected override bool IsPositive => true;

    // 回合内的临时加成，不显示图标（避免角色下方刷屏）。
    protected override bool IsVisibleInternal => false;
}
