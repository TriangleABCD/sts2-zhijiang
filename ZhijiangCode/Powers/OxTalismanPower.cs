using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Zhijiang.ZhijiangCode.Cards.Bella;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 牛符咒能力：本回合获得 {Amount} 点力量，玩家侧回合结束时移除并扣回同等力量。
/// 临时力量正数版（对照 BpknPower 的负数版）。
/// </summary>
public class OxTalismanPower : MegaCrit.Sts2.Core.Models.Powers.TemporaryStrengthPower
{
    public override MegaCrit.Sts2.Core.Models.AbstractModel OriginModel => ModelDb.Card<OxTalisman>();

    // 施加正力量（IsPositive 默认即为 true，此处显式声明）。
    protected override bool IsPositive => true;

    // 技能牌产生的间接能力，不显示图标（同 BpknPower；力量变化直接体现在力量图标上）。
    protected override bool IsVisibleInternal => false;
}
