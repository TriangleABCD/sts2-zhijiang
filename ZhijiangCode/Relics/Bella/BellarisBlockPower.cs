using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 贝极星遗物白拉技能格挡能力：处于白拉状态（阳多于阴）时，每打出 1 张技能牌获得 Amount 点格挡（层数即格挡值）。
/// 贝极星施加 3 层（+3），闪耀贝极星施加 6 层（+6）。
/// </summary>
public sealed class BellarisBlockPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 遗物能力不显示图标。
    protected override bool IsVisibleInternal => false;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 仅自己打出的技能牌触发。
        if (cardPlay.Card.Owner.Creature != base.Owner)
            return;

        if (cardPlay.Card.Type != CardType.Skill)
            return;

        // 仅白拉状态生效。
        if (base.Owner.Player is not { } player || !BellaYinYangService.IsBaiLa(player))
            return;

        if (base.Amount <= 0)
            return;

        // 快速获得格挡，避免多张技能牌触发时动画堆积。
        await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null, fast: true);
    }
}
