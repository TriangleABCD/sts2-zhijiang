using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Zhijiang.ZhijiangCode.Cards.Bella;

namespace Zhijiang.ZhijiangCode.Powers;

public class BpknPower : MegaCrit.Sts2.Core.Models.Powers.TemporaryStrengthPower
{
    public override MegaCrit.Sts2.Core.Models.AbstractModel OriginModel => ModelDb.Card<Bpkn>();

    protected override bool IsPositive => false;

    protected override bool IsVisibleInternal => false;
}
