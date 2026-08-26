using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 挠头：罕见牌（阳 / 技能）。获得格挡；若本回合未打出过攻击牌，额外获得格挡。
[RegisterCard(typeof(BellaCardPool))]
public sealed class ScratchHead : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/scratch_head.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8, ValueProp.Move),
        new DynamicVar("BonusBlock", 4m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public ScratchHead() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int block = DynamicVars.Block.IntValue;
        if (BellaYinYangService.GetAttackPlaysThisTurn(base.Owner) == 0)
            block += DynamicVars["BonusBlock"].IntValue;
        await CreatureCmd.GainBlock(base.Owner.Creature, block, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars["BonusBlock"].UpgradeValueBy(1m);
    }
}
