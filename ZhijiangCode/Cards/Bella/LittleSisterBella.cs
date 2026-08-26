using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 妹宝：罕见牌（阳 / 技能）。对所有敌人施加 1→2 层虚弱。
[RegisterCard(typeof(BellaCardPool))]
public sealed class LittleSisterBella : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/little_sister_bella.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("WeakAmount", 1m)
    ];

    // 阴阳属性：妹宝为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public LittleSisterBella() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int amount = DynamicVars["WeakAmount"].IntValue;
        foreach (Creature enemy in base.CombatState?.HittableEnemies ?? Array.Empty<Creature>())
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, amount, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 虚弱 1 → 2。
        DynamicVars["WeakAmount"].UpgradeValueBy(1m);
    }
}