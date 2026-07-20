using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

[RegisterCard(typeof(BellaCardPool))]
[RegisterCharacterStarterCard(typeof(BellaCharacter), 1)]
public sealed class Heartfelt : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Basic;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/Characters/Bella/Bella_energy_text.png");

    // 规范值：去除格挡量。
    // {StripAmount} 显示去除的格挡值。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("StripAmount", 5m)
    ];

    // 格挡悬浮提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    public Heartfelt() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
        // 消耗 10 点心之壁。
        this.SecondaryCosts().Set(HeartWall.HeartWallId, 10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int stripAmount = DynamicVars["StripAmount"].IntValue;

        // 遍历所有可攻击敌人，去除格挡（最多 stripAmount 点）。
        foreach (var enemy in base.CombatState?.HittableEnemies ?? Array.Empty<Creature>())
        {
            if (enemy.Block > 0)
            {
                int amountToRemove = Math.Min(enemy.Block, stripAmount);
                await CreatureCmd.LoseBlock(enemy, amountToRemove);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StripAmount"].UpgradeValueBy(3m);
    }
}
