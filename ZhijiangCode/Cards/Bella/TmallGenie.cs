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

// 天猫精灵：罕见牌（阴 / 攻击）。对所有敌人造成伤害；若处于黑拉，对随机一名敌人再造成伤害。
[RegisterCard(typeof(BellaCardPool))]
public sealed class TmallGenie : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/tmall_genie.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar("HeiLaExtra", 6m)
    ];

    // 阴阳属性：天猫精灵为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public TmallGenie() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 对所有敌人造成伤害。
        foreach (Creature enemy in base.CombatState?.HittableEnemies ?? Array.Empty<Creature>())
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(enemy)
                .Execute(choiceContext);
        }

        // 黑拉时：对随机一名敌人再造成伤害。
        if (BellaYinYangService.IsHeiLa(base.Owner))
        {
            IReadOnlyList<Creature> enemies = base.CombatState?.HittableEnemies ?? Array.Empty<Creature>();
            if (enemies.Count > 0)
            {
                Creature? target = base.Owner.RunState.Rng.CombatTargets.NextItem(enemies);
                if (target != null)
                {
                    await DamageCmd.Attack(DynamicVars["HeiLaExtra"].BaseValue)
                        .FromCard(this)
                        .Targeting(target)
                        .Execute(choiceContext);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害 6 → 9，黑拉追加 4 → 6。
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["HeiLaExtra"].UpgradeValueBy(2m);
    }
}