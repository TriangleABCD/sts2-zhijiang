using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 队长的权力是无限的能力：每回合开始时，你可以丢弃一张牌；若弃的是技能牌，抽 {Amount} 张牌。
/// Amount 为抽牌数（1，升级后 2）。
/// </summary>
[RegisterPower]
public sealed class CaptainsPowerIsInfinitePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => true;

    // 专属能力图标：captains_power_is_infinite_power_64x64.png / captains_power_is_infinite_power_256x256.png（待补成品图）。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/captains_power_is_infinite_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/captains_power_is_infinite_power_256x256.png");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 仅自己。
        if (player.Creature != base.Owner)
            return;

        // 手牌为空时无事可弃。
        if (PileType.Hand.GetPile(player).Cards.Count == 0)
            return;

        // 可选弃牌：0~1 张，可取消。
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, 1)
        {
            Cancelable = true
        };
        var cards = (await CardSelectCmd.FromHandForDiscard(choiceContext, player, prefs, null, this)).ToList();
        if (cards.Count == 0)
            return;

        await CardCmd.Discard(choiceContext, cards);

        // 若弃的是技能牌，抽 {Amount} 张。
        if (cards[0].Type == CardType.Skill)
            await CardPileCmd.Draw(choiceContext, Amount, player);
    }
}
