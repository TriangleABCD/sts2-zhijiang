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

// 大咪（Great Kitty）：罕见牌（阴 / 技能）。抽 2→3 张牌；若抽到的牌中有阳牌，回复 1→2 点生命。
[RegisterCard(typeof(BellaCardPool))]
public sealed class GreatKitty : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/great_kitty.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new DynamicVar("HealAmount", 1m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public GreatKitty() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 抽 2→3 张牌。
        List<CardModel> drawn = (await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, base.Owner)).ToList();

        // 若抽到的牌中有阳牌，回复 1→2 点生命。
        if (drawn.Any(c => c.Keywords.Contains(BellaYinYangService.YangKeywordId.GetModCardKeyword())))
            await CreatureCmd.Heal(base.Owner.Creature, DynamicVars["HealAmount"].IntValue);
    }

    protected override void OnUpgrade()
    {
        // 抽牌 2 → 3，回复 1 → 2。
        DynamicVars.Cards.UpgradeValueBy(1m);
        DynamicVars["HealAmount"].UpgradeValueBy(1m);
    }
}
