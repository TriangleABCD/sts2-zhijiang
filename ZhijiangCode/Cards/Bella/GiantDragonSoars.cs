using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
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

// 巨龙腾飞：稀有牌（阴 / 攻击）。造成伤害；若击杀敌人，所有玩家获得金币。
[RegisterCard(typeof(BellaCardPool))]
public sealed class GiantDragonSoars : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const int HeartWallCost = 10;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/giant_dragon_soars.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13, ValueProp.Move),
        new DynamicVar("GoldGain", 30m)
    ];

    // 消耗 + 阳。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public GiantDragonSoars() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
        // 第二费用：消耗 10 心之壁。
        this.SecondaryCosts().Set(HeartWall.HeartWallId, HeartWallCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        AttackCommand? attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 若击杀敌人，所有玩家获得金币。
        if (attack != null && attack.Results.SelectMany(list => list).Any(r => r.WasTargetKilled))
        {
            int gold = DynamicVars["GoldGain"].IntValue;
            foreach (Player player in base.CombatState?.Players ?? Array.Empty<Player>())
                await PlayerCmd.GainGold(gold, player);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害 13 → 15，金币 30 → 40。
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["GoldGain"].UpgradeValueBy(10m);
    }
}