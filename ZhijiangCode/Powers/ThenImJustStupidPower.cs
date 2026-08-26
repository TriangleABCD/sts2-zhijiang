using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 那我就是笨嘛能力：每回合开始时失去 1 点敏捷，回复 {Amount} 点生命。
/// </summary>
[RegisterPower]
public sealed class ThenImJustStupidPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => true;

    // 图标占位：暂用黑贝拉图标。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/evil_bella_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/evil_bella_power_256x256.png");

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side)
            return;
        if (base.Owner is not { } owner)
            return;

        // 失去 1 点敏捷。
        await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), owner, -1m, owner, null);

        // 回复 {Amount} 点生命。
        if (Amount > 0)
            await CreatureCmd.Heal(owner, Amount);
    }
}
