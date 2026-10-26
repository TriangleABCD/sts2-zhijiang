using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace Zhijiang.ZhijiangCode.Characters.Diana;

public sealed class DianaRelicPool : TypeListRelicPoolModel
{
    public override string EnergyColorName => "Diana";
    public override Color LabOutlineColor => DianaCharacter.ThemeColor;

    public override string? BigEnergyIconPath => $"{Entry.ResPath}/images/characters/Diana/Diana_energy_big.png";
    public override string? TextEnergyIconPath => $"{Entry.ResPath}/images/characters/Diana/Diana_energy_text.png";
}
