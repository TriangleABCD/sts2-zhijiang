using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 勇气大爆发：稀有牌（阴 / 攻击）。对所有敌人造成伤害；若处于黑拉，对血量最少的敌人再造成伤害。
[RegisterCard(typeof(BellaCardPool))]
public sealed class CourageBurst : ModCardTemplate
{
    private const int BaseEnergyCost = 3;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/courage_burst.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move),
        new DynamicVar("LowestHpExtra", 14m)
    ];

    // 阴阳属性：勇气大爆发为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public CourageBurst() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IReadOnlyList<Creature> enemies = base.CombatState?.HittableEnemies ?? Array.Empty<Creature>();

        // 对所有敌人造成伤害。
        foreach (Creature enemy in enemies)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(enemy)
                .Execute(choiceContext);
        }

        // 黑拉时：对血量最少的敌人再造成伤害。
        if (BellaYinYangService.IsHeiLa(base.Owner) && enemies.Count > 0)
        {
            Creature target = enemies.OrderBy(e => e.CurrentHp).First();
            await DamageCmd.Attack(DynamicVars["LowestHpExtra"].BaseValue)
                .FromCard(this)
                .Targeting(target)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害 7 → 10，残血追加 10 → 15。
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["LowestHpExtra"].UpgradeValueBy(5m);
    }
}