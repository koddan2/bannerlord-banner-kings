using BannerKings._CUSTOM;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Settings;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKTroopUpgradeModel : DefaultPartyTroopUpgradeModel
    {
        public override bool CanTroopGainXp(PartyBase owner, CharacterObject character)
        {
            if (character == null) return false;
            if (character.UpgradeTargets == null)
            {
                var value = Array.Empty<CharacterObject>();
                REFLECTIONHELPER.SetPrivatePropertyValue(character, nameof(character.UpgradeTargets), value);
            }
            for (int i = 0; i < character.UpgradeTargets.Length; i++)
            {
                CharacterObject upgradeTarget = character.UpgradeTargets[i];
                if (upgradeTarget == null) return false;
                int index = owner.MemberRoster.FindIndexOfTroop(character);
                int elementNumber = owner.MemberRoster.GetElementNumber(index);
                int elementXp = owner.MemberRoster.GetElementXp(index);
                int upgradeXpCost = character.GetUpgradeXpCost(owner, i);
                bool flag = elementXp >= upgradeXpCost * elementNumber;
                PerkObject perkObject;
                if (this.DoesPartyHaveRequiredPerksForUpgrade(owner, character, upgradeTarget, out perkObject) && !flag)
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetXpCostForUpgrade(PartyBase party, CharacterObject characterObject,
            CharacterObject upgradeTarget)
        {
            var result = base.GetXpCostForUpgrade(party, characterObject, upgradeTarget) * BannerKingsSettings.Instance.TroopUpgradeXp;
            if (party != null && party.MobileParty != null && party.MobileParty.LeaderHero != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(party.MobileParty.LeaderHero);
                if (education.Lifestyle != null && education.Lifestyle.Equals(DefaultLifestyles.Instance.Cataphract))
                {
                    result *= 1.25f;
                }
            }

            return (int)result;
        }
    }
}