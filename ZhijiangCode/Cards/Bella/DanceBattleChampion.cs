using System;
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

// 舞斗会冠军：罕见牌（阳 / 攻击）。造成伤害 2 次，每次命中获得格挡。
[RegisterCard(typeof(BellaCardPool))]
public sealed class DanceBattleChampion : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const int HitCount = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/dance_battle_champion.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new DynamicVar("BlockPerHit", 1m)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public DanceBattleChampion() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        for (int i = 0; i < HitCount; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
            await CreatureCmd.GainBlock(base.Owner.Creature, DynamicVars["BlockPerHit"].IntValue, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars["BlockPerHit"].UpgradeValueBy(1m);
    }
}
