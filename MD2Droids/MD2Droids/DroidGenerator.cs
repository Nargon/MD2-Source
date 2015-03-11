﻿using System;
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

            droid.inventory = new Pawn_InventoryTracker(droid);
            droid.pather = new Pawn_PathFollower(droid);
            droid.jobs = new Pawn_JobTracker(droid);
            droid.healthTracker = new Pawn_HealthTracker(droid);
            droid.ageTracker = new Pawn_AgeTracker(droid);
            droid.filth = new Pawn_FilthTracker(droid);
            droid.mindState = new Pawn_MindState(droid);
            droid.needs = new Pawn_NeedsTracker(droid);

            if (droid.RaceProps.humanoid || droid.RaceProps.mechanoid)
            {
                droid.equipment = new Pawn_EquipmentTracker(droid);
                droid.carryHands = new Pawn_CarryHands(droid);
            }
            if (!droid.RaceProps.humanoid)
            {
                droid.caller = new Pawn_CallTracker(droid);
            }
            else
            {
                droid.apparel = new Pawn_ApparelTracker(droid);
                droid.ownership = new Pawn_Ownership(droid);
                droid.skills = new Pawn_SkillTracker(droid);
                droid.talker = new Pawn_TalkTracker(droid);
                droid.story = new Pawn_StoryTracker(droid);
                droid.workSettings = new Pawn_WorkSettings(droid);
                droid.guest = new Pawn_GuestTracker(droid);
                droid.needs.mood = new Need_Mood(droid);
                droid.needs.space = new Need_Space(droid);
                droid.needs.beauty = new Need_Beauty(droid);
            }
            if (droid.RaceProps.EatsFood)
            {
                droid.needs.food = new Need_Food(droid);
            }
            //Log.Message("4");
            if (droid.RaceProps.needsRest)
            {
                droid.needs.rest = new Need_Rest(droid);
            }
            if (droid.RaceProps.hasGenders)
            {
                droid.gender = Gender.Male;
            }
            else
            {
                droid.gender = Gender.None;
            }
            droid.ageTracker.SetBirthDate(GenDate.CurrentYear,GenDate.DayOfMonth);
            //Log.Message("5");
            if (droid.RaceProps.hasStory)
            {
                droid.story.skinColor = PawnSkinColors.PaleWhiteSkin;
                droid.story.crownType = CrownType.Narrow;
                droid.story.headGraphicPath = GraphicDatabaseHeadRecords.GetHeadRandom(droid.gender, droid.story.skinColor, droid.story.crownType).GraphicPath;
                droid.story.hairColor = PawnHairColors.RandomHairColor(droid.story.skinColor,droid.ageTracker.AgeBiologicalYears);
                droid.backstoryKey = kindDef.backstoryName;
                droid.story.childhood = DroidBackstories.GetBackstoryFor(kindDef.backstoryName);
                droid.story.adulthood = DroidBackstories.GetBackstoryFor(kindDef.backstoryName);
                droid.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved", true);
                PawnName name = DroidBS.DroidName(kindDef.label);
                droid.story.name = name;
            }
            foreach (SkillRecord sk in droid.skills.skills)
            {
                sk.level = 15;
                sk.passion = Passion.Major;
            }
            PawnInventoryGenerator.GenerateInventoryFor(droid);
            PawnInventoryGenerator.GiveAppropriateKeysTo(droid);
            droid.AddAndRemoveComponentsAsAppropriate();
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
