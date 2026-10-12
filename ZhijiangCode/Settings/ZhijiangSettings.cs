using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;

namespace Zhijiang.ZhijiangCode.Settings;

/// <summary>
/// 枝江 Mod 的全局设置模型：<see cref="SaveScope.Global" />、本机所有存档共用，
/// 由 RitsuLib 的 ModDataStore 序列化到 mod_data 下的 settings.json。
/// </summary>
public sealed class ZhijiangSettings
{
    /// <summary>是否播放卡牌的自定义打出音效（默认开，保持加入该开关前的行为）。</summary>
    public bool CardSfxEnabled { get; set; } = true;
}

/// <summary>
/// 设置读取入口：设置页控件与游戏逻辑（<c>BellaAudio.PlayCardSfx</c> 的闸门）共用同一份绑定，
/// 避免出现第二个 DataStore 键或两份不一致的默认值。
/// </summary>
internal static class ZhijiangSettingsAccess
{
    internal const string DataKey = "settings";

    internal static readonly ModSettingsValueBinding<ZhijiangSettings, bool> CardSfxEnabledBinding = new(
        Entry.ModId,
        DataKey,
        SaveScope.Global,
        static settings => settings.CardSfxEnabled,
        static (settings, value) => settings.CardSfxEnabled = value);

    /// <summary>卡牌自定义音效总开关；在每次出牌播放音效时实时读取，因此改设置即时生效。</summary>
    internal static bool CardSfxEnabled => CardSfxEnabledBinding.Read();
}
