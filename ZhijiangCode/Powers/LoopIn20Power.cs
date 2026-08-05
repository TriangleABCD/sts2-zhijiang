using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 20号循环能力：你打出的下一张攻击牌会额外打出 20 次。
/// 触发一次后消耗该层能力。
/// </summary>
public sealed class LoopIn20Power : PowerModel
{
    private const int ExtraPlays = 20;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    // 非能力牌产生的间接效果，不显示图标。
    protected override bool IsVisibleInternal => false;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        // 只影响自己打出的攻击牌。
        if (card.Owner.Creature != base.Owner)
            return playCount;

        if (card.Type != CardType.Attack)
            return playCount;

        return playCount + ExtraPlays;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        // 已生效，消耗 1 层能力。
        await PowerCmd.Decrement(this);
    }
}
