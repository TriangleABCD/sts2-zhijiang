using Godot;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace Zhijiang.ZhijiangCode.Characters.Diana;

// 实现 IModColorfulPhilosophersCardPool，让「色彩哲学家」事件把嘉然卡池作为奖励颜色之一。
public sealed class DianaCardPool : TypeListCardPoolModel, IModColorfulPhilosophersCardPool
{
    // 卡框色：#E799B0 的 HSV（H=0.951 S=0.338 V=0.906）。
    private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateHsvShaderMaterial(0.951f, 0.338f, 0.906f);
    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    // Title 和 EnergyColorName 是池子的稳定标识，不是玩家看到的角色名。
    public override string Title => "Diana";
    public override string EnergyColorName => "Diana";

    public override string? BigEnergyIconPath => $"{Entry.ResPath}/images/characters/Diana/Diana_energy_big.png";
    public override string? TextEnergyIconPath => $"{Entry.ResPath}/images/characters/Diana/Diana_energy_text.png";

    public override Color DeckEntryCardColor => DianaCharacter.ThemeColor;
    public override Color EnergyOutlineColor => new(0.24f, 0.09f, 0.15f);
    public override bool IsColorless => false;
}
