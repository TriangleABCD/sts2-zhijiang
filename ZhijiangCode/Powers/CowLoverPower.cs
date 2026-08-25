using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Cards.Status;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 牛牛民能力：每当你消耗一张牛批，获得 {Amount} 点心之壁。
/// </summary>
[RegisterPower]
public sealed class CowLoverPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override bool IsVisibleInternal => true;

    // 图标占位：暂用黑贝拉图标。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/evil_bella_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/evil_bella_power_256x256.png");

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        // 仅自己消耗「牛批」时触发。
        if (card.Owner?.Creature != base.Owner)
            return;
        if (card is not Np)
            return;
        if (base.Owner.Player is not { } player)
            return;

        await SecondaryResourceCmd.Gain(player, HeartWall.HeartWallId, Amount, null);
    }
}
