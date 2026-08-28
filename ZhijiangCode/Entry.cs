using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.Patches;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode;

[ModInitializer(nameof(Initialize))]
public partial class Entry
{
    // ModId 需要和 Zhijiang.json 里的 id 保持一致。
    // res://Zhijiang/... 里的 Zhijiang 是 PCK 资源目录，不是 C# namespace。
    public const string ModId = "Zhijiang";
    public const string ResPath = $"res://{ModId}";

    public static Logger Logger { get; } = new(ModId, LogType.Generic);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        // 以下示例默认已经在 Entry.Initialize() 中调用了
        // RitsuLibFramework.EnsureGodotScriptsRegistered(...) 和
        // ModTypeDiscoveryHub.RegisterModAssembly(...)，否则自动注册不会生效。
        //
        // Godot C# 脚本注册只负责让 pck 中的脚本类型能被 Godot 找到。
        // 这一步和 RitsuLib 的内容自动注册不是同一件事，两个都需要保留。
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

        // 自动注册扫描会读取当前程序集里的 RegisterCard/RegisterRelic 等 attribute。
        // 新增内容类后，只要 attribute 写对，通常不需要在入口里手动逐个注册。
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        HeartWall.Register();
        BellaYinYangService.RegisterCombatStateSync();

        // 去掉贝拉「阳/阴」关键词内联文本末尾的句号。
        // 注意：RitsuLib 的 RegisterPatch 只是登记，未调用 PatchAll 不会真正应用；该补丁按 AGENTS 约定暂缓，保持未应用。
        RitsuLibFramework.CreatePatcher(ModId, "KeywordPeriodRemoval")
            .RegisterPatch<ModKeywordPeriodRemovalPatch>();

        // 洗牌拖尾（弃牌堆 → 抽牌堆）染成贝拉应援色：
        // RitsuLib 的 TrailStyle 染色只覆盖"跟随节点是 NCard"的普通拖尾，
        // 洗牌特效（NCardFlyShuffleVfx）需要此补丁补上，否则仍是占位角色（战士）的颜色。
        // 之前只 RegisterPatch 而漏了 PatchAll，补丁实际从未生效，本次补上应用调用。
        var shuffleTrailPatcher = RitsuLibFramework.CreatePatcher(ModId, "BellaShuffleTrailStyle");
        shuffleTrailPatcher.RegisterPatch<BellaShuffleTrailStylePatch>();
        if (!shuffleTrailPatcher.PatchAll())
            throw new InvalidOperationException("Critical patches failed: BellaShuffleTrailStyle.");

        Logger.Info("Zhijiang initialized.");
    }
}
