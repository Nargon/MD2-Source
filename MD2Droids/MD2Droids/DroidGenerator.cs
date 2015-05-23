using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Backstories;

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

            droid.story.skinColor = PawnSkinColors.PaleWhiteSkin;
            droid.story.crownType = CrownType.Narrow;
            droid.story.headGraphicPath = GraphicDatabaseHeadRecords.GetHeadRandom(droid.gender, droid.story.skinColor, droid.story.crownType).GraphicPath;
            droid.story.hairColor = PawnHairColors.RandomHairColor(droid.story.skinColor, droid.ageTracker.AgeBiologicalYears);
            droid.story.childhood = BackstoryDatabase.GetWithKey(kindDef.backstoryDef.UniqueSaveKeyFor());
            droid.story.adulthood = BackstoryDatabase.GetWithKey(kindDef.backstoryDef.UniqueSaveKeyFor());
            droid.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved", true);
            droid.drawer.renderer.graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(droid.story.hairDef.graphicPath, ShaderDatabase.Cutout, IntVec2.One, droid.story.hairColor);
            PawnName name = new PawnName()
            {
                first = "Droid",
                last = "Droid",
                nick = BackstoryDatabase.GetWithKey(kindDef.backstoryDef.UniqueSaveKeyFor()).titleShort
            };
            droid.story.name = name;

            foreach (SkillRecord sk in droid.skills.skills)
            {
                sk.level = (droid.Config.skillLevel > 20 && !(droid.Config.skillLevel < 0)) ? 20 : droid.Config.skillLevel;
                sk.passion = droid.Config.passion;
            }
            droid.workSettings.InitialSetupFromSkills();
            PawnInventoryGenerator.GenerateInventoryFor(droid);
            return droid;
        }

        public static Droid GenerateDroid(DroidKindDef kindDef)
        {
            return GenerateDroid(kindDef, Faction.OfColony);
        }


        public static void SpawnDroid(DroidKindDef kindDef, IntVec3 pos)
        {
            GenSpawn.Spawn(DroidGenerator.GenerateDroid(kindDef), pos);
        }
    }
}
