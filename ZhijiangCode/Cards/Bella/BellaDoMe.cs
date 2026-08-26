using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 老公x我（Bella do me）：罕见牌（阳 / 技能）。获得心之壁；若阴阳平衡，抽牌。
[RegisterCard(typeof(BellaCardPool))]
public sealed class BellaDoMe : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/bella_do_me.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HeartWallGain", 2m),
        new DynamicVar("Draw", 2m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public BellaDoMe() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SecondaryResourceCmd.Gain(base.Owner, HeartWall.HeartWallId, DynamicVars["HeartWallGain"].IntValue, this);
        if (BellaYinYangService.ComputeDiff(base.Owner) == 0)
            await CardPileCmd.Draw(choiceContext, DynamicVars["Draw"].BaseValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Draw"].UpgradeValueBy(1m);
    }
}
