using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

[RegisterCard(typeof(BellaCardPool))]
public sealed class ASoulIsComing : ModCardTemplate
{
    private const int BaseEnergyCost = 1;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/a_soul_is_coming.png");

    // 灵魂卡悬浮提示。
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<Soul>()
    ];

    // 消耗。
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        // 阴阳属性：一个魂来咯为阳牌。
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public ASoulIsComing() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 收集所有玩家（自己 + 联机队友），排除重复。
        List<Player> players = [base.Owner];
        players.AddRange(
            from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
            where c != null && c.IsAlive && c.IsPlayer && c.Player != base.Owner
            select c.Player);

        foreach (Player player in players)
        {
            List<Soul> souls = Soul.Create(player, 3, base.CombatState).ToList();

            // 抽牌堆 +1
            IReadOnlyList<CardPileAddResult> drawResults = await CardPileCmd.AddGeneratedCardsToCombat(
                new[] { souls[0] }, PileType.Draw, player, CardPilePosition.Random);

            // 手牌 +1
            await CardPileCmd.AddGeneratedCardsToCombat(
                new[] { souls[1] }, PileType.Hand, player);

            // 弃牌堆 +1
            IReadOnlyList<CardPileAddResult> discardResults = await CardPileCmd.AddGeneratedCardsToCombat(
                new[] { souls[2] }, PileType.Discard, player, CardPilePosition.Random);

            // 触发本地玩家的 UI 刷新。
            if (LocalContext.IsMe(player))
            {
                CardCmd.PreviewCardPileAdd(drawResults);
                CardCmd.PreviewCardPileAdd(discardResults);
            }
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
