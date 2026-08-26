using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 扁拉：罕见牌（阴 / 技能）。消耗。将 1→2 张原版无色攻击牌「压扁」加入弃牌堆，本场战斗内免费打出。
[RegisterCard(typeof(BellaCardPool))]
public sealed class FlattenedBella : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/flattened_bella.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("SquashCount", 1m)
    ];

    // 「压扁」悬浮提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<Squash>()
    ];

    // 消耗 + 阴。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public FlattenedBella() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 生成 1→2 张「压扁」，设为本场免费打出，加入弃牌堆。
        List<Squash> squashes = new();
        for (int i = 0; i < DynamicVars["SquashCount"].IntValue; i++)
        {
            Squash squash = base.CombatState!.CreateCard<Squash>(base.Owner);
            squash.SetToFreeThisCombat();
            squashes.Add(squash);
        }

        IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(
            squashes, PileType.Discard, base.Owner, CardPilePosition.Random);
        if (LocalContext.IsMe(base.Owner))
            CardCmd.PreviewCardPileAdd(results);
    }

    protected override void OnUpgrade()
    {
        // 压扁 1 → 2。
        DynamicVars["SquashCount"].UpgradeValueBy(1m);
    }
}