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
        new DamageVar(12, ValueProp.Move),
        // 战斗内实时显示攻击段数：基础 1 次 + 每 10 点心之壁 1 次。
        // ⚠️ 本牌有自己的心之壁费用（15），费用在 OnPlay 之前就已被扣除（RitsuLib 在 CardModel.SpendResources 里结算），
        // 所以这里要按「扣费后的心之壁」算，否则卡面预览会比实际多出 1~2 段。
        new CalculationBaseVar(1m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedHits").WithMultiplier((card, _) => ExtraHits(card))
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
        // 与卡面 {CalculatedHits} 同源，保证预览与实际一致。
        int hitCount = Math.Max(1, (int)((CalculatedVar)DynamicVars["CalculatedHits"]).Calculate(cardPlay.Target));

        foreach (Creature enemy in base.CombatState?.HittableEnemies ?? Array.Empty<Creature>())
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(hitCount)
                .FromCard(this)
                .Targeting(enemy)
                .Execute(choiceContext);
        }
    }

    /// <summary>额外攻击段数 = 扣掉本牌心之壁费用后的心之壁 ÷ 10。</summary>
    private static decimal ExtraHits(CardModel card)
    {
        int heartWall = SecondaryResourceCmd.Get(card.Owner, HeartWall.HeartWallId);
        return Math.Max(0, (heartWall - HeartWallCost) / HeartWallPerExtraHit);
    }

    protected override void OnUpgrade()
    {
        // 伤害 9 → 12。
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}