using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 蓓蕾（Bud）：稀有牌（阳 / 攻击）。对所有敌人造成伤害 3 次；若队伍中有嘉然，你与嘉然一同回复生命。
[RegisterCard(typeof(BellaCardPool))]
public sealed class Bud : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const int HeartWallCost = 20;
    private const int HitCount = 3;
    private const CardType CardKind = CardType.Attack;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.AllEnemies;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/bud.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new DynamicVar("HealAmount", 5m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YangKeywordId.GetModCardKeyword()
    ];

    public Bud() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
        this.SecondaryCosts().Set(HeartWall.HeartWallId, HeartWallCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (Creature enemy in base.CombatState?.HittableEnemies ?? Array.Empty<Creature>())
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(HitCount)
                .FromCard(this)
                .Targeting(enemy)
                .Execute(choiceContext);
        }

        // 多人联动：队伍中有嘉然时，你与嘉然各回复 HealAmount 点生命。
        // TODO: 嘉然角色加入后，把 hasPrincess 改为
        //   hasPrincess = FindCharacterTeammate(base.Owner, "jiaran") != null;
        bool hasPrincess = false;
        if (hasPrincess)
        {
            Player? princess = FindCharacterTeammate(base.Owner, "jiaran");
            if (princess != null)
            {
                int heal = DynamicVars["HealAmount"].IntValue;
                await CreatureCmd.Heal(base.Owner.Creature, heal);
                await CreatureCmd.Heal(princess.Creature, heal);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HealAmount"].UpgradeValueBy(2m);
    }

    /// <summary>按角色 id 找队伍中的对应角色玩家（未实现动画/角色新增前的通用桩）。</summary>
    private static Player? FindCharacterTeammate(Player owner, string characterId)
    {
        foreach (var c in owner.Creature?.CombatState?.GetTeammatesOf(owner.Creature) ?? Array.Empty<Creature>())
        {
            if (c == null || !c.IsPlayer) continue;
            Player? other = c.Player;
            if (other == null || other == owner) continue;
            if (other.Character?.Id.Entry == characterId) return other;
        }
        return null;
    }
}
