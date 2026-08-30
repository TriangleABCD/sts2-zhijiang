using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 我以灵魂注视你的心能力：每回合开始时获得 1 张灵魂（入手）。
/// </summary>
[RegisterPower]
public sealed class SoulGazePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 每张牌独立生效：多张时每回合各获得 1 张灵魂。
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override bool IsVisibleInternal => true;

    // 专属能力图标：soul_gaze_power_64x64.png / soul_gaze_power_256x256.png（待补成品图）。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/soul_gaze_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/soul_gaze_power_256x256.png");

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side)
            return;
        if (base.Owner.Player is not { } player)
            return;

        List<Soul> souls = Soul.Create(player, 1, combatState).ToList();
        IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(
            souls, PileType.Hand, player);
        if (LocalContext.IsMe(player))
            CardCmd.PreviewCardPileAdd(results);
    }
}
