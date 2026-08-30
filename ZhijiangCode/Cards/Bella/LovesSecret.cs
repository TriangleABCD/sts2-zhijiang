using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Keywords;
using Zhijiang.ZhijiangCode.Characters.Bella;
using Zhijiang.ZhijiangCode.SecondResource;

namespace Zhijiang.ZhijiangCode.Cards.Bella;

// 恋爱心事（Love's Secret）：稀有牌（阴 / 技能）。抽 3 张牌；若队伍中有乃琳，你与乃琳一同回复生命。
[RegisterCard(typeof(BellaCardPool))]
public sealed class LovesSecret : ModCardTemplate
{
    private const int BaseEnergyCost = 0;
    private const int HeartWallCost = 20;
    private const CardType CardKind = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTarget = TargetType.Self;
    private const bool ShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Entry.ResPath}/images/cards/Bella/loves_secret.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new DynamicVar("HealAmount", 7m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BellaYinYangService.YinKeywordId.GetModCardKeyword()
    ];

    public LovesSecret() : base(BaseEnergyCost, CardKind, CardRarityValue, CardTarget, ShowInCardLibrary)
    {
        this.SecondaryCosts().Set(HeartWall.HeartWallId, HeartWallCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, base.Owner);

        // 多人联动：队伍中有乃琳时，你与乃琳各回复 HealAmount 点生命。
        // TODO: 乃琳角色加入后，把 hasPrincess 改为
        //   hasPrincess = FindCharacterTeammate(base.Owner, "nailin") != null;
        bool hasPrincess = false;
        if (hasPrincess)
        {
            Player? princess = FindCharacterTeammate(base.Owner, "nailin");
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
