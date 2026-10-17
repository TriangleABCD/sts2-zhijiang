using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 回旋镖：罕见牌（阴 / 攻击）。造成伤害；本回合每打出过 1 张反差牌，伤害 +2。
[RegisterCard(typeof(BellaCardPool))]
public sealed class Boomerang : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：暂用通用攻击牌卡图。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/boomerang.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(11, ValueProp.Move),
        new DynamicVar("PerContrast", 4m),
        // 战斗内实时显示当前实际伤害（基础 + 反差牌加成）。
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CurrentDamage").WithMultiplier((card, _) => CurrentDamage(card))
    ];

    // 阴阳属性：回旋镖为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public Boomerang() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 与卡面 {CurrentDamage} 同源，保证预览与实际一致。
        int damage = (int)((CalculatedVar)DynamicVars["CurrentDamage"]).Calculate(cardPlay.Target);

        await DamageCmd.Attack(damage).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    private static decimal CurrentDamage(CardModel card)
    {
        int contrastPlays = BellaYinYangService.GetContrastPlaysThisTurn(card.Owner);
        if (BellaYinYangService.IsContrastCard(card.Owner, card))
            contrastPlays--;

        return card.DynamicVars.Damage.IntValue
               + Math.Max(contrastPlays, 0) * card.DynamicVars["PerContrast"].IntValue;
    }

    protected override void OnUpgrade()
    {
        // 伤害 8 → 12。
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}