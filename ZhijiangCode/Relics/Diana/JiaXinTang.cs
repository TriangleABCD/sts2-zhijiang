using STS2RitsuLib.Interop.AutoRegistration;
using Zhijiang.ZhijiangCode.Characters.Diana;

namespace Zhijiang.ZhijiangCode.Relics.Diana;

// 嘉然的初始遗物「嘉心糖」。
// 每场战斗开始时四选一「王牌花色」，本场只有该花色的增益生效；手牌每 3 张该花色触发一次。
[RegisterRelic(typeof(DianaRelicPool))]
[RegisterCharacterStarterRelic(typeof(DianaCharacter))]
[RegisterTouchOfOrobasRefinement(typeof(SweetestJiaXinTang))]
public sealed class JiaXinTang : DianaStarterRelicTemplate
{
    protected override int SuitThreshold => 3;
}
