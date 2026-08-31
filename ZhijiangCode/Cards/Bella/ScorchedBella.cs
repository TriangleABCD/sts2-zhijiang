using System;
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

// 焦拉：普通牌（阴 / 技能）。抽牌；若处于黑拉，抽到的阴牌本回合耗能 -1。
[RegisterCard(typeof(BellaCardPool))]
public sealed class ScorchedBella : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Common;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：指向尚不存在的资源（构建会有 RITSU013 警告，方便日后找出缺卡图的牌）。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/scorched_bella.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    // 阴阳属性：焦拉为阴牌。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public ScorchedBella() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 抽 2→3 张牌。
        List<CardModel> drawn = (await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, base.Owner)).ToList();

        // 黑拉时：抽到的阴牌本回合耗能 -1。
        if (BellaYinYangService.IsHeiLa(base.Owner))
        {
            foreach (CardModel card in drawn)
            {
                if (!card.Keywords.Contains(BellaYinYangService.YinKeywordId.GetModCardKeyword()))
                    continue;

                int current = card.EnergyCost.GetWithModifiers(CostModifiers.All);
                card.EnergyCost.SetThisTurn(Math.Max(0, current - 1), reduceOnly: true);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // 抽牌 2 → 3。
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
