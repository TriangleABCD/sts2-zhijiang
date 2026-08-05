using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 黑拉能力：每次损失生命值时，生成黑暗充能球。
/// 层数决定每次生成的充能球数量（未升级=1，升级后=2）。
/// 栏位已满时先激发最前面的球再放入新球。
/// </summary>
public sealed class EvilBellaPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => true;

    // 跟踪当前栏位中的充能球数量。
    private int _orbCount;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 仅自身受到未格挡的伤害时触发。
        if (target != base.Owner || result.UnblockedDamage <= 0)
            return;

        if (base.Owner.Player is not { } player)
            return;

        // Amount=1（未升级）→ 1 栏位；Amount=2（升级）→ 2 栏位。
        int capacity = Amount;
        int toChannel = Amount;
        for (int i = 0; i < toChannel; i++)
        {
            // 栏位已满时先激发最靠前的球，为新球腾出位置。
            if (_orbCount >= capacity)
            {
                await OrbCmd.EvokeNext(choiceContext, player);
                _orbCount--;
            }
            await OrbCmd.Channel<DarkOrb>(choiceContext, player);
            _orbCount++;
        }
    }
}
