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

// 晕3D：罕见牌（阴 / 技能）。抽牌，将 1 张晕眩加入抽牌堆；升级 1→0 费。
[RegisterCard(typeof(BellaCardPool))]
public sealed class Dizzy3D : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const int DazedCount = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/dizzy3d.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3)
    ];

    // 晕眩悬浮提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<Dazed>()
    ];

    // 阴阳属性：晕3D为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public Dizzy3D() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 抽 3 张牌。
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, base.Owner);

        // 1 张晕眩加入抽牌堆。
        List<Dazed> dazed = new();
        for (int i = 0; i < DazedCount; i++)
            dazed.Add(base.CombatState!.CreateCard<Dazed>(base.Owner));

        IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(
            dazed, PileType.Draw, base.Owner, CardPilePosition.Random);
        if (LocalContext.IsMe(base.Owner))
            CardCmd.PreviewCardPileAdd(results);
    }

    protected override void OnUpgrade()
    {
        // 费用 1 → 0。
        base.EnergyCost.UpgradeBy(-1);
    }
}
