using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 黑贝拉sama能力：每次损失生命值时，生成黑暗充能球。
/// 层数决定每次生成的充能球数量（未升级=1，升级后=2）。
/// 栏位已满时先激发最靠前的球，再放入新球。
/// 栏位占用数直接读取玩家真实充能球队列，与其他来源的充能球（如冰山美人）互不冲突。
/// 可见能力，本地化见 powers.json。
/// </summary>
[RegisterPower]
public sealed class EvilBellaPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => true;

    // 图标占位：暂用贝拉能量图标，后续可替换为专属能力图标。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/evil_bella_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/evil_bella_power_256x256.png");

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 仅自身受到未格挡的伤害时触发。
        if (target != base.Owner || result.UnblockedDamage <= 0)
            return;

        if (base.Owner.Player is not { } player)
            return;

        for (int i = 0; i < Amount; i++)
        {
            // 栏位已满时先激发最靠前的球，为新球腾出位置。
            if (player.PlayerCombatState?.OrbQueue is { } queue
                && queue.Orbs.Count >= queue.Capacity && queue.Capacity > 0)
            {
                await OrbCmd.EvokeNext(choiceContext, player);
            }
            await OrbCmd.Channel<DarkOrb>(choiceContext, player);
        }
    }
}
