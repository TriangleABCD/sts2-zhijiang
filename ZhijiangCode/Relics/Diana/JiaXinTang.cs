using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Zhijiang.ZhijiangCode.Characters.Diana;

namespace Zhijiang.ZhijiangCode.Relics.Diana;

// 嘉然的初始遗物「嘉心糖」。
// 效果与描述暂时留空（本地化 description 也是空字符串），等玩法定稿再补。
[RegisterRelic(typeof(DianaRelicPool))]
[RegisterCharacterStarterRelic(typeof(DianaCharacter))]
public sealed class JiaXinTang : ModRelicTemplate
{
    // 初始遗物必须是 Starter，否则会被商店/奖励池当作普通遗物抽出来。
    public override RelicRarity Rarity => RelicRarity.Starter;

    // 初始遗物不在商店出现。
    public override bool IsAllowedInShops => false;

    // TODO(美术)：遗物图标待补，缺失时 RitsuLib 回退占位图。
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Entry.ResPath}/images/relics/JiaXinTang_85x85.png",
        IconOutlinePath: $"{Entry.ResPath}/images/relics/JiaXinTang_85x85.png",
        BigIconPath: $"{Entry.ResPath}/images/relics/JiaXinTang_256x256.png");
}
