using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Zhijiang.ZhijiangCode.Powers;

/// <summary>
/// 黑拉状态能力：贝拉当前处于黑拉状态（阴牌 &gt; 阳牌）的可见标记。
/// 仅用于展示当前状态，无额外游戏逻辑。战斗开始时由 BellaYinYangService 施加。
/// 若状态因卡组变化翻转，会替换为白拉能力。
/// </summary>
[RegisterPower]
public sealed class HeiLaPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    // 状态标记不叠加，Amount 恒为 1。
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => true;

    // 专属状态图标：hei_la_power_64x64.png / hei_la_power_256x256.png（待替换为成品图）。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/hei_la_power_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/hei_la_power_256x256.png");
}
