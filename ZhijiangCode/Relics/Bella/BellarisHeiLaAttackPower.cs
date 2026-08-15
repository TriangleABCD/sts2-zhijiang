using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Relics;

/// <summary>
/// 贝极星遗物黑拉攻击伤害能力：处于黑拉状态（阴 > 阳）时，每打出 1 张攻击牌
/// 对随机一名敌人造成 1+|d|÷3 点伤害（纯固定伤害，不受力量修正、不触发攻击反击）。
/// 数值在每次打出攻击牌时按当前阴阳差实时计算。贝极星与闪耀贝极星共用本能力。
/// </summary>
public sealed class BellarisHeiLaAttackPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    // 遗物能力不显示图标。
    protected override bool IsVisibleInternal => false;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 仅自己打出的攻击牌触发。
        if (cardPlay.Card.Owner.Creature != base.Owner)
            return;

        if (cardPlay.Card.Type != CardType.Attack)
            return;

        // 仅黑拉状态生效。
        if (base.Owner.Player is not { } player || !BellaYinYangService.IsHeiLa(player))
            return;

        IReadOnlyList<Creature> hittableEnemies =
            base.CombatState?.HittableEnemies ?? Array.Empty<Creature>();
        if (hittableEnemies.Count == 0)
            return;

        // 随机选取一名可攻击的敌人。
        Creature? target = player.RunState.Rng.CombatTargets.NextItem(hittableEnemies);
        if (target is null)
            return;

        int damage = BellaYinYangService.ComputeMagnitude(player) + 1;
        Flash();
        await CreatureCmd.Damage(choiceContext, target, damage, ValueProp.Unpowered, base.Owner, null);
    }
}
