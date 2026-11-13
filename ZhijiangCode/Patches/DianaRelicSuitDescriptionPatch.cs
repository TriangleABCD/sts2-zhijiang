using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Patching.Models;
using Zhijiang.ZhijiangCode.Characters.Diana;
using Zhijiang.ZhijiangCode.Relics.Diana;

namespace Zhijiang.ZhijiangCode.Patches;

/// <summary>
/// 嘉心糖 / 最甜甜甜的嘉心糖：在遗物描述末尾追加「本场已选王牌花色」的效果说明。
///
/// 原版遗物的描述是私有字段拼出来的 LocString，没有可覆写的钩子，
/// 所以在 <see cref="RelicModel.DynamicDescription" /> 的 Postfix 里补一个变量，
/// 本地化文本用 {SuitEffect} 占位；没进战斗（或还没选）时该变量是空串。
/// </summary>
public sealed class DianaRelicSuitDescriptionPatch : IPatchMethod
{
    public static string PatchId => "zhijiang_diana_relic_suit_description";

    public static string Description => "Append the chosen trump suit effect to Diana's starter relic description";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(RelicModel), nameof(RelicModel.DynamicDescription), MethodType.Getter)];
    }

    public static void Postfix(RelicModel __instance, ref LocString __result)
    {
        if (__instance is not DianaStarterRelicTemplate || __result is null)
            return;

        // 只在战斗内显示本场选择（离开战斗后不残留上一次的花色）。
        bool inCombat = NCombatRoom.Instance is not null;

        __result.Add("SuitEffect",
            inCombat && DianaTrumpSuit.Current is { } suit
                ? DianaTrumpSuit.DescribeEffect(suit)
                : string.Empty);
    }
}
