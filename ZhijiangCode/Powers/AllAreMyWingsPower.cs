using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 都是我的翅膀能力：若玩家本回合打出的阳牌数等于阴牌数，下回合获得 {Amount} 点能量。
/// Amount 显示奖励能量数（3，升级后 4）。
/// 阴阳平衡判定复用 BellaYinYangService 的本回合阳/阴打出计数。
/// </summary>
[RegisterPower]
public sealed class AllAreMyWingsPower : ModPowerTemplate
{
    private int _pendingReward;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => true;

    // 图标占位：暂用白拉图标。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/bai_la_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/bai_la_power_256x256.png");

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 仅拥有者回合结束时结算。
        if (side == Owner.Side && base.Owner.Player is { } player
            && BellaYinYangService.GetYinYangBalanceThisTurn(player) == 0)
        {
            _pendingReward = Amount;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side)
            return;
        if (base.Owner.Player is not { } player)
            return;

        if (_pendingReward > 0)
        {
            await PlayerCmd.GainEnergy(_pendingReward, player);
            _pendingReward = 0;
        }
    }
}
