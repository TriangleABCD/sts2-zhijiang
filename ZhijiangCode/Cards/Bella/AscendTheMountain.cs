using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 上山：稀有牌（阴 / 技能）。耗 20 心之壁；消耗抽牌堆中最多数张牌，每消耗一张回复生命。
[RegisterCard(typeof(BellaCardPool))]
public sealed class AscendTheMountain : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const int HeartWallCost = 20;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图待补：占位路径指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/ascend_the_mountain.png");

    // 可消耗张数（3 → 5）、每张回复生命（2 → 3）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("ExhaustCount", 3m),
        new DynamicVar("HealPerCard", 2m)
    ];

    // 阴阳属性：上山为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public AscendTheMountain() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
        // 第二费用：消耗 20 心之壁。
        this.SecondaryCosts().Set(HeartWall.HeartWallId, HeartWallCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 抽牌堆为空时没有可选的牌，直接跳过选牌界面（避免空选牌界面卡住）。
        if (PileType.Draw.GetPile(base.Owner).Cards.Count == 0)
            return;

        // 从抽牌堆自选 0~N 张消耗（同原版「净化」Cleanse 的选牌 UI，可少于上限）。
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 0,
            DynamicVars["ExhaustCount"].IntValue);
        IEnumerable<CardModel> selected = await CardSelectCmd.FromCombatPile(
            choiceContext, PileType.Draw.GetPile(base.Owner), base.Owner, prefs);

        int exhausted = 0;
        foreach (CardModel card in selected.ToList())
        {
            await CardCmd.Exhaust(choiceContext, card);
            exhausted++;
        }

        // 每消耗一张回复生命。
        if (exhausted > 0)
            await CreatureCmd.Heal(base.Owner.Creature, exhausted * DynamicVars["HealPerCard"].IntValue);
    }

    protected override void OnUpgrade()
    {
        // 可消耗张数 3 → 5，每张回复 2 → 3。
        DynamicVars["ExhaustCount"].UpgradeValueBy(2m);
        DynamicVars["HealPerCard"].UpgradeValueBy(1m);
    }
}
