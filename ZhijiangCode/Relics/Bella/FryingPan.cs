using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Cards.Bella;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 平底锅：拾起时，牌组中所有「防御」获得重放 1 与[gold]虚无[/gold]。
/// 直接修改卡牌自身（BaseReplayCount / Ethereal 关键字），此后每次打出都生效。
/// </summary>
[RegisterRelic(typeof(BellaRelicPool))]
public sealed class FryingPan : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic)
    ];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/frying_pan_85x85.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/frying_pan_85x85.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/frying_pan_256x256.png");

    public override Task AfterObtained()
    {
        foreach (var card in PileType.Deck.GetPile(base.Owner).Cards.ToList())
        {
            if (card is not BellaDefend)
                continue;

            card.BaseReplayCount += 1;
            card.AddKeyword(CardKeyword.Ethereal);
        }
        return Task.CompletedTask;
    }
}
