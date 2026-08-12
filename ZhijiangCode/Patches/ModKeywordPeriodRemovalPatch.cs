using STS2RitsuLib.Keywords;
using STS2RitsuLib.Patching.Models;

namespace Zhijiang.ZhijiangCode.Patches;

/// <summary>
/// 去掉贝拉「阳/阴」关键词内联文本末尾的句号。
/// RitsuLib 的 <see cref="ModKeywordRegistry.GetCardText" /> 会在金色标题后拼接
/// card_keywords.PERIOD（句号），原版关键词（如"消耗"）沿用此样式，但贝拉的阴阳标签
/// 不需要句号。此补丁仅对 ZHIJIANG_KEYWORD_YANG / ZHIJIANG_KEYWORD_YIN 生效。
/// </summary>
public sealed class ModKeywordPeriodRemovalPatch : IPatchMethod
{
    public static string PatchId => "zhijiang_mod_keyword_period_removal";

    public static string Description => "Remove trailing period from Bella Yin/Yang keyword inline card text";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(ModKeywordRegistry), "GetCardText", [typeof(string)])];
    }

    public static void Postfix(string id, ref string __result)
    {
        if (id is not ("ZHIJIANG_KEYWORD_YANG" or "ZHIJIANG_KEYWORD_YIN"))
            return;

        __result = __result.TrimEnd('。', '.', ' ');
    }
}
