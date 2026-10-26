using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;
using Zhijiang.ZhijiangCode.Keywords;

namespace Zhijiang.ZhijiangCode.Patches;

/// <summary>
/// 把嘉然卡牌的扑克牌面（花色 + 点数）合成一行，插到卡牌描述上方。
///
/// 不走 RitsuLib 的 mod 关键词注入器：它按"一个关键词一行、句尾补句点"的方式注入
/// （会变成 "♠." 换行 "3."），做不出 "♠3"。所以扑克关键词注册时 placement 设成 None，
/// 由本补丁在同一个原版描述方法（CardModel.GetDescriptionForPile）的 Postfix 里自己拼一行。
/// </summary>
public sealed class DianaPokerCardTextPatch : IPatchMethod
{
    public static string PatchId => "zhijiang_diana_poker_card_text";

    public static string Description => "Prepend Diana's poker face (suit + rank) to card descriptions as a single line";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        // CardModel.GetDescriptionForPile(PileType, DescriptionPreviewType, Creature)
        // DescriptionPreviewType 是 CardModel 的私有嵌套枚举，只能反射取。
        Type previewType = typeof(CardModel).GetNestedType("DescriptionPreviewType", BindingFlags.NonPublic)
            ?? throw new MissingMemberException(typeof(CardModel).FullName, "DescriptionPreviewType");

        return
        [
            new(typeof(CardModel), nameof(CardModel.GetDescriptionForPile),
                [typeof(PileType), previewType, typeof(Creature)])
        ];
    }

    public static void Postfix(CardModel __instance, ref string __result)
    {
        if (DianaPokerKeywords.BuildLabel(__instance) is not { } label)
            return;

        __result = string.IsNullOrEmpty(__result) ? label : label + "\n" + __result;
    }
}
