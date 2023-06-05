using BannerKings.Managers.Skills;
using BannerKings.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;
using static TaleWorlds.Core.ItemObject;

namespace BannerKings._CUSTOM
{
    public static class RobHeroUpdater
    {
        private static Lazy<SkillObject[]> _allSkills = new Lazy<SkillObject[]>(() => new SkillObject[]
        {
            DefaultSkills.Tactics,
            DefaultSkills.Scouting,
            DefaultSkills.Athletics,
            DefaultSkills.Charm,
            DefaultSkills.Steward,
            DefaultSkills.Riding,
            DefaultSkills.Bow,
            DefaultSkills.Crafting,
            DefaultSkills.Crossbow,
            DefaultSkills.Engineering,
            DefaultSkills.Leadership,
            DefaultSkills.Medicine,
            DefaultSkills.OneHanded,
            DefaultSkills.Polearm,
            DefaultSkills.Roguery,
            DefaultSkills.Throwing,
            DefaultSkills.Trade,
            DefaultSkills.TwoHanded,
            BKSkills.Instance.Lordship,
            BKSkills.Instance.Theology,
            BKSkills.Instance.Scholarship,
        });

        ////private static int _fixCounter = 0;
        ////private static readonly EquipmentIndex[] CheckSlots =
        ////{
        ////    EquipmentIndex.Body,
        ////    EquipmentIndex.Head,
        ////    EquipmentIndex.Leg,
        ////};
        ////private static readonly HashSet<string> heroChecked = new();
        public static void MaybeTryFixNakedHeros(Hero hero)
        {
            RobNullFixer.FixNullRoster();
            ////RobNullFixer.FixNullSettlementLastAttackerParty();
            if (hero == null) return;
            if (hero.Clan == null) return;
            if (hero.IsDead) return;
            if (hero.BattleEquipment == null) return;
            //if (heroChecked.Contains(hero.StringId)) return;
            //heroChecked.Add(hero.StringId);

            if (hero.BattleEquipment[EquipmentIndex.Body].IsEmpty)
            {
                RobMod.Log($"Hero({hero.StringId}) is missing Body - trying to fix");
                FillHeroEquipment(hero);
            }
            //else if (hero.BattleEquipment[EquipmentIndex.Body].Item.IsCivilian)
            //{
            //    Log($"Hero({hero.StringId}) has civilian Body battle equipment - clearing slot");
            //    //hero.BattleEquipment[EquipmentIndex.Body].Clear();
            //    SetBattleEquipment(hero, EquipmentIndex.Body);
            //}

            if (hero.BattleEquipment[EquipmentIndex.Leg].IsEmpty)
            {
                RobMod.Log($"Hero({hero.StringId}) is missing Leg - trying to fix");
                FillHeroEquipment2(hero, EquipmentIndex.Leg);
            }
            //else if (hero.BattleEquipment[EquipmentIndex.Leg].Item.IsCivilian)
            //{
            //    Log($"Hero({hero.StringId}) has civilian Leg battle equipment - clearing slot");
            //    //hero.BattleEquipment[EquipmentIndex.Leg].Clear();
            //    SetBattleEquipment(hero, EquipmentIndex.Leg);
            //}

            if (hero.BattleEquipment[EquipmentIndex.Head].IsEmpty)
            {
                RobMod.Log($"Hero({hero.StringId}) is missing Head - trying to fix");
                FillHeroEquipment2(hero, EquipmentIndex.Head);
            }
            //else if (hero.BattleEquipment[EquipmentIndex.Head].Item.IsCivilian)
            //{
            //    Log($"Hero({hero.StringId}) has civilian Head battle equipment - clearing slot");
            //    //hero.BattleEquipment[EquipmentIndex.Head].Clear();
            //    SetBattleEquipment(hero, EquipmentIndex.Head);
            //}
            if (hero.Clan != null && Hero.MainHero.Clan != hero.Clan && !hero.IsFactionLeader)
            {
                // 0.1% chance
                const float chancePerDayPerSlot = 0.001f;
                if (MBRandom.RandomFloat < chancePerDayPerSlot)
                    MaybeChangeGear(hero, EquipmentIndex.Head);
                if (MBRandom.RandomFloat < chancePerDayPerSlot)
                    MaybeChangeGear(hero, EquipmentIndex.Body);
                if (MBRandom.RandomFloat < chancePerDayPerSlot)
                    MaybeChangeGear(hero, EquipmentIndex.Gloves);
                if (MBRandom.RandomFloat < chancePerDayPerSlot)
                    MaybeChangeGear(hero, EquipmentIndex.Leg);
                if (MBRandom.RandomFloat < chancePerDayPerSlot)
                    MaybeChangeGear(hero, EquipmentIndex.Cape);

                if (MBRandom.RandomFloat < chancePerDayPerSlot)
                    MaybeChangeGear(hero, EquipmentIndex.NumAllWeaponSlots);
            }
        }

        private static void MaybeChangeGear(Hero hero, EquipmentIndex equipmentIndex)
        {
            if (hero == null) return;
            var settlement = (hero.HomeSettlement ?? hero.LastKnownClosestSettlement ?? hero.BornSettlement);
            if (settlement == null) return;
            var cult = settlement.Culture;
            if (cult == null) return;
            if (hero.BattleEquipment == null) return;

            ItemTiers minTier = ItemTiers.Tier4;

            var initpool = GetBattleEquipmentPool(cult);
            var iswep = false;

            if (equipmentIndex == EquipmentIndex.NumAllWeaponSlots)
            {
                iswep = true;
                equipmentIndex = EquipmentIndex.Weapon0;
            }

            var pool = initpool?
                .WhereQ(it => it != null && (iswep || !it.IsCivilian) && !it[equipmentIndex].IsEmpty && it[equipmentIndex].Item.Tier >= minTier)
                .ToArray();
            if (pool != null && pool.Any())
            {
                var newGear = pool[MBRandom.RandomInt(0, pool.Length)];
                if (iswep)
                {
                    void SetEq(EquipmentIndex idx)
                    {
                        if (hero.BattleEquipment[idx].Item?.Name != newGear[idx].Item?.Name)
                        {
                            RobMod.Log($"[R813]Hero({hero.StringId}):\t[{hero.BattleEquipment[idx].Item?.Name}] -> [{newGear[idx].Item?.Name}]");
                            hero.BattleEquipment[idx] = newGear[idx];
                        }
                    }
                    SetEq(EquipmentIndex.Weapon0);
                    SetEq(EquipmentIndex.Weapon1);
                    SetEq(EquipmentIndex.Weapon2);
                    SetEq(EquipmentIndex.Weapon3);
                }
                else
                {
                    var current = hero.BattleEquipment[equipmentIndex];
                    if (newGear == null || newGear[equipmentIndex].Item == null
                        || current.Item?.Name == newGear[equipmentIndex].Item?.Name)
                    {
                        return;
                    }
                    hero.BattleEquipment[equipmentIndex] = newGear[equipmentIndex];
                    RobMod.Log($"[R813]Hero({hero.StringId}):\t[{current.Item?.Name}] -> [{newGear[equipmentIndex].Item?.Name}]");
                }
            }
        }

        private static readonly Dictionary<string, IEnumerable<Equipment>> cachedBattleEquipmentsByCult = new();
        private static IEnumerable<Equipment> GetBattleEquipmentPool(CultureObject cult)
        {
            var id = cult.StringId;
            IEnumerable<Equipment> pool;
            if (cachedBattleEquipmentsByCult.TryGetValue(id, out var cached))
            {
                pool = cached;
            }
            else
            {
                pool = EnumerateBattleEquipment(cult);
                cachedBattleEquipmentsByCult[id] = pool;
            }

            return pool;
        }

        private static IEnumerable<Equipment> EnumerateBattleEquipment(CultureObject cult)
        {
            IEnumerable<Equipment> r1;
            IEnumerable<Equipment> r2;
            r1 = EnumerateBattleEquipmentFromTroopLine(cult.BasicTroop);
            r2 = EnumerateBattleEquipmentFromTroopLine(cult.EliteBasicTroop);
            return r1.Concat(r2);
        }

        private static IEnumerable<Equipment> EnumerateBattleEquipmentFromTroopLine(CharacterObject troop)
        {
            List<CharacterObject> allTroops = new();
            List<Equipment> r = new();
            void Rec(CharacterObject co)
            {
                allTroops.Add(co);
                foreach (var sub in co.UpgradeTargets)
                {
                    Rec(sub);
                }
            }
            Rec(troop);
            foreach (var charobj in allTroops)
            {
                r.AddRange(charobj.BattleEquipments);
            }
            return r;
        }

        private static void SetBattleEquipment(Hero hero, EquipmentIndex equipmentIndex)
        {
            var settlement = (hero.HomeSettlement ?? hero.LastKnownClosestSettlement ?? hero.BornSettlement);
            if (settlement == null) return;
            var cult = settlement.Culture;
            var equipments = cult.EliteBasicTroop?.AllEquipments?.Where(y => !y.IsCivilian && !y.GetEquipmentFromSlot(equipmentIndex).IsEmpty).ToArray();
            if (equipments == null || !equipments.Any()) return;
            var eq = equipments.GetRandomElement();
            if (eq == null) return;
            hero.BattleEquipment[equipmentIndex] = eq[equipmentIndex];
        }

        private static void FillHeroEquipment2(Hero hero, EquipmentIndex equipmentIndex)
        {
            var settlement = (hero.HomeSettlement ?? hero.LastKnownClosestSettlement ?? hero.BornSettlement);
            if (settlement == null) return;
            var cult = settlement.Culture;
            var equipments = cult.EliteBasicTroop?.AllEquipments?.Where(y => !y.IsCivilian && !y.GetEquipmentFromSlot(equipmentIndex).IsEmpty).ToArray();
            if (equipments == null || !equipments.Any()) return;
            hero.BattleEquipment.FillFrom(equipments.GetRandomElement());
            RobMod.Log($"\tHero({hero.StringId}) fixed: {hero.Name}");
        }

        private static void FillHeroEquipment(Hero hero)
        {
            var settlement = (hero.HomeSettlement ?? hero.LastKnownClosestSettlement ?? hero.BornSettlement);
            if (settlement == null) return;
            var cult = settlement.Culture;
            var templates = cult.LordTemplates;
            if (templates == null || !templates.Any()) return;
            var templ = templates.GetRandomElement();
            hero.BattleEquipment.FillFrom(templ.Equipment);
            RobMod.Log($"\tHero({hero.StringId}) fixed: {hero.Name}");
        }

        public static void ApplyGenericTutoring(Hero hero)
        {
            var companions = hero.CompanionsInParty;
            if (companions == null || !companions.Any())
            {
                return;
            }
            var topThreeSkills = _allSkills.Value.OrderByDescending(sk => hero.GetSkillValue(sk))
                .Take(3).ToArray();

            if (topThreeSkills.Any())
            {
                var skill = topThreeSkills[MBRandom.RandomInt(0, topThreeSkills.Length - 1)];
                foreach (var companion in companions)
                {
                    companion.AddSkillXp(skill, MBRandom.RandomFloatRanged(1, 4));
                }
            }
        }
    }
}
