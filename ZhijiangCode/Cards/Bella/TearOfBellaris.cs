using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

[RegisterCard(typeof(BellaCardPool))]
public sealed class TearOfBellaris : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Ancient;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/tear_of_bellaris.png");

    // 消耗。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        // 阴阳属性：贝极星的眼泪为阴牌。
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];


    // 伤害与每 10 点心之壁的力量增益。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(17, ValueProp.Move),
        new DynamicVar("StrengthPer10", 1)
    ];

    // 力量悬浮提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public TearOfBellaris() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 对所有敌人造成伤害。
        foreach (var enemy in base.CombatState?.HittableEnemies ?? Array.Empty<Creature>())
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(enemy)
                .Execute(choiceContext);
        }

        // 消耗所有心之壁。
        int heartWall = SecondaryResourceCmd.Get(base.Owner, HeartWall.HeartWallId);
        if (heartWall > 0)
        {
            await SecondaryResourceCmd.Lose(base.Owner, HeartWall.HeartWallId, heartWall, this);
        }

        // 每消耗 10 点心之壁，获得力量。
        int strengthGain = heartWall / 10 * DynamicVars["StrengthPer10"].IntValue;
        if (strengthGain > 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature,
                strengthGain, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPer10"].UpgradeValueBy(1m);
    }
}
