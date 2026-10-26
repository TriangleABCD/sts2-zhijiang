using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace Zhijiang.ZhijiangCode.Characters.Diana;

// 药水池先建好结构，暂时没有专属药水；资源路径缺失时 RitsuLib 会输出诊断并回退。
public sealed class DianaPotionPool : TypeListPotionPoolModel
{
    public override string EnergyColorName => "Diana";
    public override Color LabOutlineColor => DianaCharacter.ThemeColor;

    public override string? BigEnergyIconPath => $"{Entry.ResPath}/images/characters/Diana/Diana_energy_big.png";
    public override string? TextEnergyIconPath => $"{Entry.ResPath}/images/characters/Diana/Diana_energy_text.png";
}
