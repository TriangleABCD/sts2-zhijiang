using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;
using Zhijiang.ZhijiangCode.Cards.Bella;

namespace Zhijiang.ZhijiangCode.Powers;

public class BpknPower : MegaCrit.Sts2.Core.Models.Powers.TemporaryStrengthPower, IModPowerAssetOverrides
{
    public override MegaCrit.Sts2.Core.Models.AbstractModel OriginModel => ModelDb.Card<Bpkn>();

    protected override bool IsPositive => false;

    // 保持可见：与玩家侧牛符咒的隐藏临时力量不同，这是对敌人施加的负面状态，
    // 需要被原版「不安油灯」等识别（参考原版 PiercingWailPower）。
    protected override bool IsVisibleInternal => true;

    // 专属状态图标：bpkn_64x64.png（敌人下方状态栏）/ bpkn_256x256.png（悬停大图）。
    // 本类继承原版 TemporaryStrengthPower（而非 ModPowerTemplate），没有 AssetProfile 可覆写，
    // 因此改用 RitsuLib 的 IModPowerAssetOverrides 让 PowerModel 的 IconPath/PackedIconPath/BigIcon 补丁生效。
    public PowerAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/characters/Bella/bpkn_64x64.png",
        BigIconPath: $"{Entry.ResPath}/images/characters/Bella/bpkn_256x256.png");

    public string? CustomIconPath => AssetProfile.IconPath;

    public string? CustomBigIconPath => AssetProfile.BigIconPath;
}
