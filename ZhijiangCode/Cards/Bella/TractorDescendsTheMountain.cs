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
using Zhijiang.ZhijiangCode.Audio;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 拖拉机下山：稀有牌（阳 / 攻击）。对所有敌人造成伤害，并随机击晕一名敌人。
[RegisterCard(typeof(BellaCardPool))]
public sealed class TractorDescendsTheMountain : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;

    // 卡图待补：占位路径指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/tractor_descends_the_mountain.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13, ValueProp.Move)
    ];

    // 阴阳属性：拖拉机下山为阳牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public TractorDescendsTheMountain() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 打出音效：Zhijiang/audios/tractor_descends_the_mountain.ogg。
        BellaAudio.PlayCardSfx("tractor_descends_the_mountain.ogg");

        // 对所有敌人造成伤害。
        foreach (Creature enemy in base.CombatState?.HittableEnemies ?? Array.Empty<Creature>())
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(enemy)
                .Execute(choiceContext);
        }

        // 随机击晕一名敌人：优先挑还没被击晕的，避免把击晕浪费在已经晕住的敌人身上。
        IReadOnlyList<Creature> enemies = base.CombatState?.HittableEnemies ?? Array.Empty<Creature>();
        List<Creature> candidates = enemies.Where(e => !e.IsStunned).ToList();
        if (candidates.Count == 0)
            return;

        Creature? target = base.Owner.RunState.Rng.CombatTargets.NextItem(candidates);
        if (target != null)
            await CreatureCmd.Stun(target);
    }

    protected override void OnUpgrade()
    {
        // 伤害 13 → 15。
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
