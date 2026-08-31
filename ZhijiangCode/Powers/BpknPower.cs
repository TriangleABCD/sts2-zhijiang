using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Zhijiang.ZhijiangCode.Cards.Bella;

namespace Zhijiang.ZhijiangCode.Powers;

public class BpknPower : MegaCrit.Sts2.Core.Models.Powers.TemporaryStrengthPower
{
    public override MegaCrit.Sts2.Core.Models.AbstractModel OriginModel => ModelDb.Card<Bpkn>();

    protected override bool IsPositive => false;

    // 保持可见：与玩家侧牛符咒的隐藏临时力量不同，这是对敌人施加的负面状态，
    // 需要被原版「不安油灯」等识别（参考原版 PiercingWailPower）。
    protected override bool IsVisibleInternal => true;
}
