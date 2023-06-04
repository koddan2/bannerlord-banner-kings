using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings._CUSTOM
{
    public static class RobNullFixer
    {
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
