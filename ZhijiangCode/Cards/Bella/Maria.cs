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

// Maria：稀有牌（阳 / 攻击）。造成伤害；若阴阳平衡，再造成伤害。
[RegisterCard(typeof(BellaCardPool))]
public sealed class Maria : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/maria.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move),
        new DynamicVar("BalanceBonus", 5m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public Maria() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);

        if (BellaYinYangService.ComputeDiff(base.Owner) == 0)
            await DamageCmd.Attack(DynamicVars["BalanceBonus"].BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["BalanceBonus"].UpgradeValueBy(2m);
    }
}
