using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 虚拟感能力：每打出 1 张技能牌，获得 {Amount} 点心之壁。
/// 层数即每次获得的心之壁数值（未升级=2，升级后=3）。
/// </summary>
[RegisterPower]
public sealed class VirtualSensePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => true;

    // 图标占位：暂用贝拉能量图标，后续可替换为专属能力图标。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/Bella_energy_big.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/Bella_energy_big.png");

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 仅自己打出的技能牌触发。
        if (cardPlay.Card.Owner.Creature != base.Owner)
            return;

        if (cardPlay.Card.Type != CardType.Skill)
            return;

        if (base.Owner.Player is not { } player)
            return;

        await SecondaryResourceCmd.Gain(player, HeartWall.HeartWallId, base.Amount, null);
    }
}
