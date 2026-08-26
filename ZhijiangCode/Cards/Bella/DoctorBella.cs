using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 贝拉博士（Doctor Bella）：罕见牌（阴 / 攻击）。造成伤害；若黑拉，额外造成伤害。
[RegisterCard(typeof(BellaCardPool))]
public sealed class DoctorBella : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/doctor_bella.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5, ValueProp.Move),
        new DynamicVar("HeiLaBonus", 2m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public DoctorBella() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        int damage = DynamicVars.Damage.IntValue;
        if (BellaYinYangService.IsHeiLa(base.Owner))
            damage += DynamicVars["HeiLaBonus"].IntValue;
        await DamageCmd.Attack(damage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["HeiLaBonus"].UpgradeValueBy(1m);
    }
}
