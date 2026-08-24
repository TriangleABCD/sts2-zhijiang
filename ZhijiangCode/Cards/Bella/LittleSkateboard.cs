using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
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

// 小滑板：普通牌（阴 / 技能）。抽 1→2 张牌；弃 1 张牌；若弃掉的是阴牌，额外抽 1→2 张牌。
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
        // 抽 1→2 张牌。
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, base.Owner);

        // 弃 1 张牌（玩家选择）。
        CardModel? discarded = (await CardSelectCmd.FromHandForDiscard(
            choiceContext, base.Owner, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1), null, this))
            .FirstOrDefault();
        if (discarded == null)
            return;

        await CardCmd.Discard(choiceContext, discarded);

        // 若弃掉的是阴牌，额外抽 1→2 张。
        if (discarded.Keywords.Contains(BellaYinYangService.YinKeywordId.GetModCardKeyword()))
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars["ExtraDraw"].BaseValue, base.Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // 抽牌 1 → 2，额外抽 1 → 2。
        DynamicVars.Cards.UpgradeValueBy(1m);
        DynamicVars["ExtraDraw"].UpgradeValueBy(1m);
    }
}