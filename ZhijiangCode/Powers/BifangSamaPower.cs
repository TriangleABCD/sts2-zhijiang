using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Cards.Bella;
using Zhijiang.ZhijiangCode.Characters.Bella;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 毕方大人能力：白拉时每回合开始，获得 {Amount} 点临时力量（本回合）。
/// </summary>
[RegisterPower]
public sealed class BifangSamaPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => true;

    // 专属能力图标：bifang_sama_power_64x64.png / bifang_sama_power_256x256.png（待补成品图）。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/bifang_sama_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/bifang_sama_power_256x256.png");

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side)
            return;
        if (base.Owner.Player is not { } player)
            return;
        if (!BellaYinYangService.IsBaiLa(player))
            return;

        await PowerCmd.Apply<BifangSamaTemporaryStrengthPower>(new ThrowingPlayerChoiceContext(), base.Owner, Amount, base.Owner, null);
    }
}

/// <summary>
/// 毕方大人施加的临时力量（本回合），作为 TemporaryStrengthPower 的具体实现。
/// </summary>
public sealed class BifangSamaTemporaryStrengthPower : TemporaryStrengthPower
{
    public override MegaCrit.Sts2.Core.Models.AbstractModel OriginModel => ModelDb.Card<BifangSama>();

    protected override bool IsPositive => true;

    protected override bool IsVisibleInternal => true;
}