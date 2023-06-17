using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Skills;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Models.Vanilla
{
    public class BKBattleSimulationModel : DefaultCombatSimulationModel
    {
        public override int SimulateHit(CharacterObject strikerTroop, CharacterObject struckTroop, PartyBase strikerParty,
            PartyBase struckParty, float strikerAdvantage, MapEvent battle)
        {
            var result = base.SimulateHit(strikerTroop, struckTroop, strikerParty, struckParty, strikerAdvantage, battle);
            var leader = strikerParty.LeaderHero;
            if (leader != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                if (data.HasPerk(BKPerks.Instance.SiegePlanner) && strikerParty.SiegeEvent != null &&
                    strikerTroop.IsInfantry && strikerTroop.IsRanged)
                {
                    result = (int)(result * 1.15f);
                }

                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(leader, DefaultDivinities.Instance.AmraSecondary1))
                {
                    var faceTerrainType = Campaign.Current.MapSceneWrapper
                                                  .GetFaceTerrainType(strikerParty.MobileParty.CurrentNavigationFace);
                    if (faceTerrainType == TerrainType.Forest)
                    {
                        result = (int)(result * 1.08f);
                    }
                }
            }

            if (battle.IsSiegeAssault)
            {
                var siegeFactor = 1f;
                if (battle.MapEventSettlement != null)
                {
                    siegeFactor = this.GetSettlementAdvantage(battle.MapEventSettlement) / 10;
                }
                siegeFactor = MBMath.ClampFloat(siegeFactor, 0.1f, 0.9f);
                if (strikerParty.MapEventSide == battle.AttackerSide)
                {
                    result = (int)(result * (1f - siegeFactor));
                }
                else if (strikerParty.MapEventSide == battle.DefenderSide)
                {
                    result = (int)(result * (1f + siegeFactor));
                }
            }

            {
                var extraStrikerAdvantage = (strikerParty.LeaderHero?.GetSkillValue(DefaultSkills.Tactics) ?? 0)
                    - (struckParty.LeaderHero?.GetSkillValue(DefaultSkills.Tactics) ?? 0);
                if (extraStrikerAdvantage != 0)
                {
                    var extraFactor = extraStrikerAdvantage / 600f;
                    result = (int)(result * (1 + extraFactor));
                }
            }

            {
                var extraStrikerAdvantage = (strikerTroop.Tier - struckTroop.Tier) * 3;
                if (extraStrikerAdvantage != 0)
                {
                    var extraFactor = extraStrikerAdvantage / 100f;
                    result = (int)(result * (1 + extraFactor));
                }
            }

            if (strikerParty.LeaderHero == null)
            {
                if (struckTroop.Tier > 4)
                {
                    result = (int)(result / 2);
                }
            }

            ////result = Math.Max(1, result);

            return result;
        }
    }
}