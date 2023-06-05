using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings._CUSTOM
{
    public static class RobNullFixer
    {
        public static void FixNullRoster()
        {
            foreach (var party in MobileParty.All.ToArray())
            {
                if (party.MemberRoster == null)
                {
                    RobMod.Log($"[{nameof(RobNullFixer)}.{nameof(FixNullRoster)}]: (1) Fixing {party}");
                    party.RemoveParty();
                }
                else if (party.MemberRoster.TotalManCount < 1)
                {
                    RobMod.Log($"[{nameof(RobNullFixer)}.{nameof(FixNullRoster)}]: (2) Fixing {party}");
                    party.RemoveParty();
                }
            }
        }

        public static void FixNullSettlementLastAttackerParty()
        {
            return;
            var settlements = Settlement.All;
            foreach (var settlement in settlements)
            {
                if (settlement.LastAttackerParty == null)
                {
                    RobMod.Log($"[{nameof(RobNullFixer)}.{nameof(FixNullSettlementLastAttackerParty)}]: Fixing {settlement}");
                    var party = MobileParty.All.GetRandomElement();
                    settlement.LastAttackerParty = party;
                }
            }
        }
    }
}
