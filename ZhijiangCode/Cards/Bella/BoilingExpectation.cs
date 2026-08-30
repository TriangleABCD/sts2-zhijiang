using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 沸腾期待：罕见牌（阴 / 攻击）。造成伤害；你每有 5 点心之壁，此牌伤害 +1→+2。
[RegisterCard(typeof(BellaCardPool))]
public sealed class BoilingExpectation : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const int HeartWallPerBonus = 5;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/boiling_expectation.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar("DamagePerHeartWall", 1m)
    ];

    // 阴阳属性：沸腾期待为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public BoilingExpectation() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        int heartWall = SecondaryResourceCmd.Get(base.Owner, HeartWall.HeartWallId);
        int bonus = (heartWall / HeartWallPerBonus) * DynamicVars["DamagePerHeartWall"].IntValue;

        await DamageCmd.Attack(DynamicVars.Damage.IntValue + bonus)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 伤害 6 → 9，每 5 心壁伤害加成 1 → 2。
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["DamagePerHeartWall"].UpgradeValueBy(1m);
    }
}
