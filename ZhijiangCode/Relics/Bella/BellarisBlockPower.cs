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
/// 贝极星遗物白拉技能格挡能力：处于白拉状态（阳 ≥ 阴）时，每打出 1 张技能牌
/// 获得 1+|d|÷3 点格挡（固定值，不受敏捷修正）。数值在每次打出技能牌时按当前阴阳差实时计算。
/// 贝极星与闪耀贝极星共用本能力。
/// </summary>
public sealed class BellarisBlockPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

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

        int block = BellaYinYangService.ComputeMagnitude(player) + 1;

        // 快速获得格挡，避免多张技能牌触发时动画堆积。
        await CreatureCmd.GainBlock(base.Owner, block, ValueProp.Unpowered, null, fast: true);
    }
}
