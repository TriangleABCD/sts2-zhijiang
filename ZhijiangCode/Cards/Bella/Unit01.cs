using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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

// 初号机：稀有牌（阴 / 攻击）。对所有敌人造成伤害；并按当前心之壁每 10 点，再次造成伤害。
[RegisterCard(typeof(BellaCardPool))]
public sealed class Unit01 : ModCardTemplate
{
    private const int BaseEnergyCost = 3;
    private const int HeartWallCost = 15;
    private const int HeartWallPerExtraHit = 10;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：暂用通用攻击牌卡图。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/unit01.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12, ValueProp.Move)
    ];

    // 阴阳属性：初号机为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public Unit01() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
        // 第二费用：消耗 15 心之壁。
        this.SecondaryCosts().Set(HeartWall.HeartWallId, HeartWallCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int heartWall = SecondaryResourceCmd.Get(base.Owner, HeartWall.HeartWallId);
        int extraHits = Math.Max(0, heartWall / HeartWallPerExtraHit);
        int hitCount = 1 + extraHits;

        foreach (Creature enemy in base.CombatState?.HittableEnemies ?? Array.Empty<Creature>())
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(hitCount)
                .FromCard(this)
                .Targeting(enemy)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害 9 → 12。
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}