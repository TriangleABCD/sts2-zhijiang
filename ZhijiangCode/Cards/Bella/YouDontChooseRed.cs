using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Audio;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 你不选...红色：稀有牌（中立 / 技能）。消耗。从抽牌堆与弃牌堆随机取 3→5 张反差牌加入手牌并升级。
[RegisterCard(typeof(BellaCardPool))]
public sealed class YouDontChooseRed : ModCardTemplate
{
    private const int BaseEnergyCost = 2;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    // 卡图占位：暂用通用技能牌卡图。
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/you_dont_choose_red.png");

    // 随机取得的反差牌张数（3 → 5）。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CardCount", 3m)
    ];

    // 消耗。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    public YouDontChooseRed() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 打出音效：Zhijiang/audios/you_dont_choose_red.ogg。
        BellaAudio.PlayCardSfx("you_dont_choose_red.ogg", 1f);

        // 候选：抽牌堆 + 弃牌堆里的反差牌（与当前白拉/黑拉状态阴阳相反的牌）。
        List<CardModel> candidates = PileType.Draw.GetPile(base.Owner).Cards
            .Concat(PileType.Discard.GetPile(base.Owner).Cards)
            .Where(c => BellaYinYangService.IsContrastCard(base.Owner, c))
            .ToList();
        if (candidates.Count == 0)
            return;

        // 随机取 {CardCount} 张，不足则能拿多少拿多少。
        List<CardModel> picked = candidates
            .StableShuffle(base.Owner.RunState.Rng.Shuffle)
            .Take(DynamicVars["CardCount"].IntValue)
            .ToList();

        IReadOnlyList<CardPileAddResult> results = await CardPileCmd.Add(picked, PileType.Hand);
        if (LocalContext.IsMe(base.Owner))
            CardCmd.PreviewCardPileAdd(results);

        // 加入手牌的这些反差牌升级。
        CardCmd.Upgrade(picked, CardPreviewStyle.HorizontalLayout);
    }

    protected override void OnUpgrade()
    {
        // 张数 3 → 5。
        DynamicVars["CardCount"].UpgradeValueBy(2m);
    }
}
