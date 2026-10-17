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

// 贝拉求雨：普通牌（阳 / 攻击）。造成伤害；本回合每打出一张技能牌，此牌伤害 +1。
[RegisterCard(typeof(BellaCardPool))]
public sealed class BellaPraysForRain : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：暂用通用攻击牌卡图。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/bella_prays_for_rain.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar("BonusPerSkill", 1m),
        // 战斗内实时显示当前实际伤害（基础 + 已打出技能牌数 × 加成）。
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CurrentDamage").WithMultiplier((card, _) => CurrentDamage(card))
    ];

    // 阴阳属性：贝拉求雨为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public BellaPraysForRain() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
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
        int skills = BellaYinYangService.GetSkillPlaysThisTurn(card.Owner);
        return card.DynamicVars.Damage.IntValue + skills * card.DynamicVars["BonusPerSkill"].IntValue;
    }

    protected override void OnUpgrade()
    {
        // 伤害 6 → 8；每张技能牌的额外伤害 +1 → +2。
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["BonusPerSkill"].UpgradeValueBy(1m);
    }
}