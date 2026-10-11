using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 提线木偶：普通牌（阴 / 技能）。0 费，获得 30→50 点心之壁与 6→9 点格挡。
[RegisterCard(typeof(BellaCardPool))]
public sealed class Marionette : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：暂用通用技能牌卡图。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/marionette.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Borrow", 30m),
        new BlockVar(6, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    // 阴阳属性：提线木偶为阴牌（应要求，非中立）。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public Marionette() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 直接获得心之壁，再获得格挡。
        await SecondaryResourceCmd.Gain(base.Owner, HeartWall.HeartWallId, DynamicVars["Borrow"].IntValue, this);
        await CreatureCmd.GainBlock(base.Owner.Creature, DynamicVars.Block.IntValue, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        // 心之壁 30 → 50，格挡 6 → 9。
        DynamicVars["Borrow"].UpgradeValueBy(20m);
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
