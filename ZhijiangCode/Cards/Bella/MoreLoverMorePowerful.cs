using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 情人越多越气派：造成伤害；本回合每打出过 1 张反差牌（与当前状态阴阳相反的牌），额外造成 1 次伤害。
[RegisterCard(typeof(BellaCardPool))]
public sealed class MoreLoverMorePowerful : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    // 卡图待补：占位路径指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/more_lover_more_powerful.png");

    // 伤害值：10 → 11（升级 +1）。
    // CalculatedHits：命中次数 = 基础 1 次 + 本回合此前的反差牌数（每张 1 次）。
    // ⚠️ CalculatedVar 的公式是 CalculationBase + CalculationExtra × multiplier，
    // 所以 multiplier 只能返回「额外次数」，基础那 1 次由 CalculationBase 提供，否则会重复计数。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move),
        new CalculationBaseVar(1m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedHits").WithMultiplier((CardModel card, Creature? _) => CountExtraHits(card))
    ];

    // 阴阳属性：情人越多越气派为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public MoreLoverMorePowerful() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        // 命中次数与卡面 {CalculatedHits} 同源，保证预览与实际一致。
        int hitCount = (int)((CalculatedVar)DynamicVars["CalculatedHits"]).Calculate(cardPlay.Target);
        for (int i = 0; i < hitCount; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害 10 → 14（命中公式不变）。
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    /// <summary>额外攻击次数 = 本回合此前打出的反差牌数（基础 1 次由 CalculationBaseVar 提供）；
    /// 本卡在打出瞬间已被计入反差牌数，需扣除自身。</summary>
    private static decimal CountExtraHits(CardModel card)
    {
        int contrastPlays = BellaYinYangService.GetContrastPlaysThisTurn(card.Owner);
        if (BellaYinYangService.IsContrastCard(card.Owner, card))
            contrastPlays--;

        return Math.Max(contrastPlays, 0);
    }
}
