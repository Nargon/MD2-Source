using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MD2
{
    public static class DroidGenerator
    {
        public static Droid GenerateDroid(DroidKindDef kindDef, Faction faction)
        {
            //Log.Message("1");
            Droid droid = (Droid)ThingMaker.MakeThing(kindDef.race, null);
            droid.SetFactionDirect(faction);
            droid.kindDef = kindDef;
            droid.RaceProps.corpseDef = ThingDef.Named("MD2DroidCorpse");
            droid.thinker = new Pawn_Thinker(droid);
            droid.TotalCharge = (((DroidKindDef)kindDef).maxEnergy * 0.3f);

            droid.playerController = new Pawn_PlayerController(droid);
            droid.outfits = new Pawn_OutfitTracker(droid);
            droid.inventory = new Pawn_InventoryTracker(droid);
            droid.pather = new Pawn_PathFollower(droid);
            droid.jobs = new Pawn_JobTracker(droid);
            droid.health = new Pawn_HealthTracker(droid);
            droid.ageTracker = new Pawn_AgeTracker(droid);
            droid.filth = new Pawn_FilthTracker(droid);
            droid.mindState = new Pawn_MindState(droid);
            droid.equipment = new Pawn_EquipmentTracker(droid);
            droid.natives = new Pawn_NativeVerbs(droid);
            droid.meleeVerbs = new Pawn_MeleeVerbs(droid);
            droid.carryHands = new Pawn_CarryHands(droid);
            droid.apparel = new Pawn_ApparelTracker(droid);
            droid.ownership = new Pawn_Ownership(droid);
            droid.skills = new Pawn_SkillTracker(droid);
            //droid.talker = new Pawn_TalkTracker(droid);
            droid.story = new Pawn_StoryTracker(droid);
            droid.workSettings = new Pawn_WorkSettings(droid);
            droid.guest = new Pawn_GuestTracker(droid);
            droid.needs = new Pawn_NeedsTracker(droid);
            droid.timetable = new Pawn_TimetableTracker();
            for (int i = 0; i < droid.timetable.times.Count; i++)
            {
                droid.timetable.SetAssignment(i, TimeAssignment.Work);
            }
            if (droid.RaceProps.hasGenders)
            {
                droid.gender = Gender.Male;
            }
            else
            {
                droid.gender = Gender.None;
            }
            droid.ageTracker.SetChronologicalBirthDate(GenDate.CurrentYear, GenDate.DayOfMonth);

            //Log.Message("5");
            droid.story.skinColor = PawnSkinColors.PaleWhiteSkin;
            droid.story.crownType = CrownType.Narrow;
            droid.story.headGraphicPath = GraphicDatabaseHeadRecords.GetHeadRandom(droid.gender, droid.story.skinColor, droid.story.crownType).GraphicPath;
            droid.story.hairColor = PawnHairColors.RandomHairColor(droid.story.skinColor, droid.ageTracker.AgeBiologicalYears);
            droid.backstoryKey = kindDef.backstoryName;
            droid.story.childhood = DroidBackstories.GetBackstoryFor(kindDef.backstoryName);
            droid.story.adulthood = DroidBackstories.GetBackstoryFor(kindDef.backstoryName);
            droid.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved", true);
            PawnName name = DroidBS.DroidName(kindDef.label);
            droid.story.name = name;

            foreach (SkillRecord sk in droid.skills.skills)
            {
                sk.level = (droid.Config.skillLevel > 20 && !(droid.Config.skillLevel < 0)) ? 20 : droid.Config.skillLevel;
                sk.passion = droid.Config.passion;
            }
            PawnInventoryGenerator.GenerateInventoryFor(droid);
            //PawnInventoryGenerator.GiveAppropriateKeysTo(droid);
            //droid.AdAddAndRemoveComponentsAsAppropriate();
            return droid;
        }

        public static Droid GenerateDroid(DroidKindDef kindDef)
        {
            return GenerateDroid(kindDef, Faction.OfColony);
        }


        public static void SpawnDroid(DroidKindDef kindDef, IntVec3 pos)
        {
            DroidBS.AddBs(DroidBackstories.GetBackstoryFor(kindDef.backstoryName));
            GenSpawn.Spawn(DroidGenerator.GenerateDroid(kindDef), pos);
            DroidBS.RemoveBs(DroidBackstories.GetBackstoryFor(kindDef.backstoryName));
        }
    }
}
