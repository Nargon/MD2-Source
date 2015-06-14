using Backstories;
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
            Droid droid = (Droid)ThingMaker.MakeThing(kindDef.race, null);

            droid.SetFactionDirect(faction);
            droid.kindDef = kindDef;
            droid.RaceProps.corpseDef = ThingDef.Named("MD2DroidCorpse");
            droid.TotalCharge = kindDef.maxEnergy * 0.3f;

            droid.thinker = new Pawn_Thinker(droid);
            droid.playerController = new Pawn_PlayerController(droid);
            droid.inventory = new Pawn_InventoryTracker(droid);
            droid.pather = new Pawn_PathFollower(droid);
            droid.jobs = new Pawn_JobTracker(droid);
            droid.health = new Pawn_HealthTracker(droid);
            droid.ageTracker = new Pawn_AgeTracker(droid);
            droid.filth = new Pawn_FilthTracker(droid);
            droid.mindState = new Pawn_MindState(droid);
            droid.equipment = new Pawn_EquipmentTracker(droid);
            droid.apparel = new Pawn_ApparelTracker(droid);
            droid.natives = new Pawn_NativeVerbs(droid);
            droid.meleeVerbs = new Pawn_MeleeVerbs(droid);
            droid.carryHands = new Pawn_CarryHands(droid);
            droid.ownership = new Pawn_Ownership(droid);
            droid.skills = new Pawn_SkillTracker(droid);
            droid.story = new Pawn_StoryTracker(droid);
            droid.workSettings = new Pawn_WorkSettings(droid);
            droid.guest = new Pawn_GuestTracker(droid);
            droid.needs = new Pawn_NeedsTracker(droid);
            droid.stances = new Pawn_StanceTracker(droid);
            droid.overlay = new DroidUIOverlay(droid);

            if (droid.RaceProps.hasGenders)
            {
                droid.gender = Gender.Male;
            }
            else
            {
                droid.gender = Gender.None;
            }

            typeof(Pawn_NeedsTracker).GetMethod("AddNeed",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(droid.needs, new object[] { DefDatabase<NeedDef>.GetNamed("Mood") });

            droid.ageTracker.SetChronologicalBirthDate(GenDate.CurrentYear, GenDate.DayOfMonth);
            droid.story.skinColor = PawnSkinColors.PaleWhiteSkin;
            droid.story.crownType = CrownType.Narrow;
            droid.story.headGraphicPath = GraphicDatabaseHeadRecords.GetHeadRandom(Gender.Male, droid.story.skinColor, droid.story.crownType).GraphicPath;
            droid.story.hairColor = PawnHairColors.RandomHairColor(droid.story.skinColor, droid.ageTracker.AgeBiologicalYears);
            droid.story.childhood = BackstoryDatabase.GetWithKey(kindDef.backstoryDef.UniqueSaveKeyFor());
            droid.story.adulthood = BackstoryDatabase.GetWithKey(kindDef.backstoryDef.UniqueSaveKeyFor());
            droid.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved", true);
            droid.drawer.renderer.graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(droid.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, droid.story.hairColor);

            PawnName name = new PawnName()
            {
                first = "Droid",
                last = "Droid",
                nick = BackstoryDatabase.GetWithKey(kindDef.backstoryDef.UniqueSaveKeyFor()).titleShort
            };
            droid.story.name = name;

            foreach (SkillRecord sk in droid.skills.skills)
            {
                sk.level = (droid.Config.skillLevel > 20)  ? 20 : (droid.Config.skillLevel <= 0) ? 1 : droid.Config.skillLevel;
                sk.passion = droid.Config.passion;
            }
            droid.workSettings.EnableAndInitialize();

            WorkTypeDef maintenanceDef = DefDatabase<WorkTypeDef>.GetNamed("MD2Maintenance", false);
            foreach (var def in DefDatabase<WorkTypeDef>.AllDefs)
            {
                if (!droid.KindDef.allowedWorkTypeDefs.Contains(def) && def != maintenanceDef)
                {
                    droid.workSettings.SetPriority(def, 0);
                    droid.workSettings.Disable(def);
                }
                if (def == maintenanceDef)
                {
                    droid.workSettings.SetPriority(def, 4);
                }
            }

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
