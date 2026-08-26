using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Cards.Status;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 短裙（Short Skirt）：稀有牌（阳 / 技能）。获得格挡；将高雅加入手牌。
[RegisterCard(typeof(BellaCardPool))]
public sealed class ShortSkirt : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/short_skirt.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10, ValueProp.Move),
        new DynamicVar("ElegantCount", 2m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromCard<Elegant>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public ShortSkirt() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, DynamicVars.Block, cardPlay);

        IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(
            Elegant.Create(base.Owner, DynamicVars["ElegantCount"].IntValue, base.CombatState!),
            PileType.Hand, base.Owner);
        if (LocalContext.IsMe(base.Owner))
            CardCmd.PreviewCardPileAdd(results);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
        DynamicVars["ElegantCount"].UpgradeValueBy(1m);
    }
}
