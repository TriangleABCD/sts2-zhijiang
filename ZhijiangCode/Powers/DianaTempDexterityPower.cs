using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Zhijiang.ZhijiangCode.Relics.Diana;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>王牌花色·红桃：回合开始时按手牌红桃数量给的「本回合 +敏捷」，回合结束自动消失。</summary>
public sealed class DianaTempDexterityPower : TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Relic<JiaXinTang>();

    protected override bool IsPositive => true;

    protected override bool IsVisibleInternal => false;
}
