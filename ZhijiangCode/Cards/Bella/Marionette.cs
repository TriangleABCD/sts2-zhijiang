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
using Zhijiang.ZhijiangCode.Powers;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 提线木偶：普通牌（阴 / 技能）。预支 15→20 点心之壁，下回合同等数量减少。
[RegisterCard(typeof(BellaCardPool))]
public sealed class Marionette : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：暂用通用技能牌卡图。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/marionette.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Borrow", 15m)
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
        int borrow = DynamicVars["Borrow"].IntValue;

        // 预支心之壁。
        await SecondaryResourceCmd.Gain(base.Owner, HeartWall.HeartWallId, borrow, this);

        // 挂上下回合还款能力。
        await PowerCmd.Apply<MarionettePower>(choiceContext, base.Owner.Creature, borrow, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 预支 15 → 20。
        DynamicVars["Borrow"].UpgradeValueBy(5m);
    }
}