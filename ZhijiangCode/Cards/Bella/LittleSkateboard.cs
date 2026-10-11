using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 小滑板：普通牌（阴 / 技能）。抽 1 张牌；若抽到的是阴牌，再抽 1→2 张牌（升级只提升额外抽牌数）。
[RegisterCard(typeof(BellaCardPool))]
public sealed class LittleSkateboard : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：暂用通用技能牌卡图。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/little_skateboard.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new DynamicVar("ExtraDraw", 1m)
    ];

    // 阴阳属性：小滑板为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public LittleSkateboard() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 抽 1 张牌。
        List<CardModel> drawn = (await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, base.Owner)).ToList();

        // 若抽到的是阴牌，再抽 1→2 张。
        if (drawn.Any(c => c.Keywords.Contains(BellaYinYangService.YinKeywordId.GetModCardKeyword())))
            await CardPileCmd.Draw(choiceContext, DynamicVars["ExtraDraw"].BaseValue, base.Owner);
    }

    protected override void OnUpgrade()
    {
        // 额外抽牌 1 → 2（基础抽牌数不变）。
        DynamicVars["ExtraDraw"].UpgradeValueBy(1m);
    }
}
