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
/// 锤子：拾起时，牌组中所有「打击」获得重放 1 与[gold]消耗[/gold]。
/// 直接修改卡牌自身（BaseReplayCount / Exhaust 关键字），此后每次打出都生效。
/// </summary>
[RegisterRelic(typeof(BellaRelicPool))]
public sealed class Hammer : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic)
    ];

    // 占位图标：复用贝极星素材，后续替换为专属图标。
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/hammer_85x85.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/hammer_85x85.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/hammer_256x256.png");

    public override Task AfterObtained()
    {
        foreach (var card in PileType.Deck.GetPile(base.Owner).Cards.ToList())
        {
            if (card is not BellaStrike)
                continue;

            card.BaseReplayCount += 1;
            card.AddKeyword(CardKeyword.Exhaust);
        }
        return Task.CompletedTask;
    }
}
