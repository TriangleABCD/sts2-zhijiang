using STS2RitsuLib;
using STS2RitsuLib.Data;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;

namespace Zhijiang.ZhijiangCode.Settings;

/// <summary>
/// 枝江 Mod 设置页：注册持久化数据 + 页面 UI。
/// 文案走原版 <c>settings_ui</c> 文本表——游戏会按原版的表文件名扫描各 mod 的
/// <c>res://{ModId}/localization/{语言}/settings_ui.json</c> 并合并进来，所以本 mod 不需要新表。
/// </summary>
internal static class ZhijiangSettingsPage
{
    private const string PageId = "settings";
    private const string LocTable = "settings_ui";

    internal static void Register()
    {
        // 必须先注册 DataStore，设置页绑定（Read/Write）才能找到这个 key。
        ModDataStore.For(Entry.ModId).Register<ZhijiangSettings>(
            key: ZhijiangSettingsAccess.DataKey,
            fileName: "settings.json",
            scope: SaveScope.Global,
            defaultFactory: static () => new ZhijiangSettings(),
            autoCreateIfMissing: true);

        RitsuLibFramework.RegisterModSettings(
            Entry.ModId,
            page => page
                .WithModDisplayName(Loc("ZHIJIANG_SETTINGS_MOD_NAME.title", "Zhijiang"))
                .WithTitle(Loc("ZHIJIANG_SETTINGS_PAGE.title", "Settings"))
                .AddSection("audio", section => section
                    .WithTitle(Loc("ZHIJIANG_SETTINGS_SECTION_AUDIO.title", "Audio"))
                    .AddToggle(
                        "card_sfx",
                        Loc("ZHIJIANG_SETTINGS_CARD_SFX.title", "Custom card sound effects"),
                        ZhijiangSettingsAccess.CardSfxEnabledBinding,
                        Loc("ZHIJIANG_SETTINGS_CARD_SFX.description",
                            "When off, Bella's cards no longer play custom sound clips (vanilla card sounds are unaffected)."))),
            PageId);
    }

    private static ModSettingsText Loc(string key, string fallback)
    {
        return ModSettingsText.LocString(LocTable, key, fallback);
    }
}
