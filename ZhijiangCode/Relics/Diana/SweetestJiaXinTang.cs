using STS2RitsuLib.Interop.AutoRegistration;
using Zhijiang.ZhijiangCode.Characters.Diana;

namespace Zhijiang.ZhijiangCode.Relics.Diana;

// 嘉心糖的升级版「最甜甜甜的嘉心糖」：只把门槛从每 3 张降到每 2 张，效果本身不加强。
[RegisterRelic(typeof(DianaRelicPool))]
public sealed class SweetestJiaXinTang : DianaStarterRelicTemplate
{
    protected override int SuitThreshold => 2;
}
