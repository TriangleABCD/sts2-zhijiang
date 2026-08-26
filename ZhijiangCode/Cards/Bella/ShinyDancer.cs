using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
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

// 闪光弹：稀有牌（阴 / 攻击）。造成 17→20 伤害；若击杀敌人，击晕其他所有敌人。
[RegisterCard(typeof(BellaCardPool))]
public sealed class ShinyDancer : ModCardTemplate
{
    private const int BaseEnergyCost = 3;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AnyEnemy;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/shiny_dancer.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(17, ValueProp.Move)
    ];

    // 阴阳属性：闪光弹为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public ShinyDancer() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        AttackCommand? attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 若击杀敌人，击晕其他所有敌人。
        if (attack != null && attack.Results.SelectMany(list => list).Any(r => r.WasTargetKilled))
        {
            foreach (Creature enemy in base.CombatState?.HittableEnemies ?? Array.Empty<Creature>())
            {
                if (enemy == cardPlay.Target)
                    continue;
                await CreatureCmd.Stun(enemy);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害 17 → 20。
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
