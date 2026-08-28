using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Zhijiang.ZhijiangCode.Characters.Bella;

/// <summary>
/// 阴阳状态翻转监听器：贝拉玩家的白拉/黑拉状态发生翻转时，由
/// <see cref="BellaYinYangService.SyncStateEffects"/> 通知实现本接口的模型（如遗物）。
/// 首次进入战斗（尚无之前状态）不会触发。
/// </summary>
public interface IBellaStateFlipListener
{
    Task OnBellaStateFlipped(Player player);
}
