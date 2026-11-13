using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Zhijiang.ZhijiangCode.Characters.Diana;
using Zhijiang.ZhijiangCode.Keywords;
using Zhijiang.ZhijiangCode.Powers;
using Zhijiang.ZhijiangCode.Ui;

namespace Zhijiang.ZhijiangCode.Relics.Diana;

/// <summary>
/// 嘉心糖的能力本体：每回合开始（抽完牌后）按本场选定的王牌花色结算一次。
///
/// 为什么做成 Power 而不是直接写在遗物上：原版的回合开始钩子通过能力派发，
/// 而且这里的「选花色」是一个玩家选择，官方文档明确要求用 AfterPlayerTurnStart 触发玩家选择。
/// </summary>
public sealed class DianaTrumpSuitPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (base.Owner.Player is not { } owner || owner != player)
            return;

        // 每场战斗的第一回合先让玩家选花色（此时手牌已抽完）。
        if (DianaTrumpSuit.Current is null)
            await DianaSuitChoicePanel.ShowAsync(owner);

        if (DianaTrumpSuit.Current is not { } suit)
            return;

        int groups = DianaTrumpSuit.GroupsInHand(owner, suit);
        if (groups <= 0)
            return;

        // 先数完再结算：方块抽牌不会再次触发本效果。
        switch (suit)
        {
            case PokerSuit.Spade:
                await PowerCmd.Apply<DianaTempStrengthPower>(choiceContext, base.Owner, groups, base.Owner, null);
                break;

            case PokerSuit.Heart:
                await PowerCmd.Apply<DianaTempDexterityPower>(choiceContext, base.Owner, groups, base.Owner, null);
                break;

            case PokerSuit.Diamond:
                await CardPileCmd.Draw(choiceContext, groups * 2, owner);
                break;

            case PokerSuit.Club:
                await PlayerCmd.GainEnergy(groups, owner);
                break;
        }
    }
}
