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

// 大咪（Great Kitty）：罕见牌（阴 / 技能）。抽牌，弃 1 张；若弃的是阴牌，再抽 1 张。
[RegisterCard(typeof(BellaCardPool))]
public sealed class GreatKitty : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const int ExtraDraw = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/great_kitty.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public GreatKitty() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, base.Owner);

        CardModel? discarded = (await CardSelectCmd.FromHandForDiscard(
            choiceContext, base.Owner, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1), null, this))
            .FirstOrDefault();
        if (discarded == null) return;

        await CardCmd.Discard(choiceContext, discarded);
        if (discarded.Keywords.Contains(BellaYinYangService.YinKeywordId.GetModCardKeyword()))
            await CardPileCmd.Draw(choiceContext, ExtraDraw, base.Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
