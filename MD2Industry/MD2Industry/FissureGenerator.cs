using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace MD2
{
    public class FissureGenerator : Building
    {
        //Some working variables
        private FissureSize fissureSize = FissureSize.Small;
        public int ticksRemaining = 0;
        public int tickAmountToGen = 0;
        private const float CheckFrequency = 10f;
        public bool started = false;
        public bool running = false;
        public bool initialised = false;
        private int count = 0;
        CompPowerTrader powerTrader;
        private static Texture2D beginIcon;
        private static Texture2D pauseIcon;
        private static Texture2D cycleButton;

        //Tick method
        public override void Tick()
        {
            base.Tick();
            if (this.powerTrader != null && !this.powerTrader.PowerOn)
            {
                //If this has a powerTrader, and the power is off, do nothing
                return;
            }
            //Check if this has started digging
            if (!this.started)
            {
                this.powerTrader.powerOutputInt = -1f;
                return;
            }
            if (running && (Find.TickManager.TicksGame % 100) == 0 && Rand.Value < 20f / GenDate.TicksPerDay)
            {
                this.TakeDamage(new DamageInfo(DamageDefOf.Blunt, Rand.Range(1, 50), this));
            }
            if (this.running)
            {
                //Set the power usage according to the fissure size being generated
                this.powerTrader.powerOutputInt = powerUsage(this.fissureSize);
                //Check if this has initialised, if not then initialise. This is done only once, when the fissure to produce has been chosen.
                if (!this.initialised)
                {
                    this.tickAmountToGen = randomDigTime(this.fissureSize);
                    this.ticksRemaining = this.tickAmountToGen;
                    if (Game.GodMode)
                    {
                        this.tickAmountToGen = 100;
                        this.ticksRemaining = 100;
                    }
                    this.initialised = true;
                }
                //Tick down until the time has elapsed
                ticksRemaining--;
                if (ticksRemaining <= 0)
                {
                    //Spawn the fissure and destroy the generator
                    SoundDef.Named("ChunkRock_Drop").PlayOneShot(this);
                    makeAndSpawnFissure(fissureSize);
                    this.Destroy(DestroyMode.Vanish);
                }
            }
            else
            {
                //Stop using power if it is paused
                this.powerTrader.powerOutputInt = 0f;
            }
        }

        //Spawn set up
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.powerTrader = base.GetComp<CompPowerTrader>();
            FissureGenerator.beginIcon = ContentFinder<Texture2D>.Get("UI/Commands/BeginUI", true);
            FissureGenerator.pauseIcon = ContentFinder<Texture2D>.Get("UI/Commands/PauseUI", true);
            FissureGenerator.cycleButton = ContentFinder<Texture2D>.Get("Terrain/Fissure", true);
        }

        //Expose data
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.tickAmountToGen, "tickAmountToGen");
            Scribe_Values.LookValue<int>(ref this.ticksRemaining, "ticksRemaining");
            Scribe_Values.LookValue<bool>(ref this.started, "started");
            Scribe_Values.LookValue<FissureSize>(ref this.fissureSize, "fissureSize");
            Scribe_Values.LookValue<bool>(ref this.initialised, "initialised");
            Scribe_Values.LookValue<bool>(ref this.running, "running");
            Scribe_Values.LookValue(ref this.count, "count");
        }

        //Fissure Generator Commands
        public override IEnumerable<Gizmo> GetGizmos()
        {

            //Start/Stop
            var command = new Command_Action();
            if (!this.started)
            {
                command.defaultLabel = "Begin";
                string desc = "Click here to begin the drilling process once you have chosen your desired fissure size";
                command.defaultDesc = desc;
                command.icon = FissureGenerator.beginIcon;
            }
            else
            {
                if (this.running)
                {
                    command.defaultLabel = "Pause";
                    string desc2 = "Click here to pause the drilling process";
                    command.defaultDesc = desc2;
                    command.icon = FissureGenerator.pauseIcon;
                }
                else
                {
                    command.defaultLabel = "Resume";
                    string desc3 = "Click here to resume the drilling process";
                    command.defaultDesc = desc3;
                    command.icon = FissureGenerator.beginIcon;
                }
            }
            command.hotKey = Keys.BuildingOnOffToggle;
            command.activateSound = SoundDef.Named("Click");
            command.action = new Action(toggleStarted);
            command.disabled = false;
            command.groupKey = 313740003;
            yield return command;
            //Cycle through the fissure sizes to generate
            command = new Command_Action
            {
                icon = FissureGenerator.cycleButton,
                defaultLabel = "Choose Fissure",
                defaultDesc = "Choose Fissure",
                hotKey = Keys.FissureGeneratorChangeFissure,
                activateSound = SoundDef.Named("Click"),
                action = new Action(cycleThroughFissuresToGenerate),
                disabled = false,
                groupKey = 313740004
            };
            if (!this.started)
            {
                yield return command;
            }
            if (base.GetGizmos() != null)
            {
                foreach (var c in base.GetGizmos())
                    yield return c;
            }
        }


        //Inspect string, displays selected size and progress
        public override string GetInspectString()
        {
            var str = new StringBuilder();
            str.AppendLine(base.GetInspectString());
            if (this.fissureSize == FissureSize.SteamGeyser)
            {
                str.AppendLine("Selected size: Steam Geyser");
            }
            else
            {
                str.AppendLine("Selected size: " + this.fissureSize.ToString());
            }
            if (!this.started)
            {
                str.AppendLine("Operation has not begun");
            }
            else
            {
                str.AppendLine("Progress: " + (((float)this.ticksRemaining / (float)this.tickAmountToGen) * 100f).ToString("0.0") + "%");
            }
            return str.ToString();
        }

        //Method to get a random time based on the fissure type
        public int randomDigTime(FissureSize size)
        {
            if (size == FissureSize.Small)
            {
                return (int)Rand.Range(35000, 45000);
            }
            if (size == FissureSize.Medium)
            {
                return (int)Rand.Range(45000, 55000);
            }
            if (size == FissureSize.Large)
            {
                return (int)Rand.Range(60000, 75000);
            }
            if (size == FissureSize.SteamGeyser)
            {
                return (int)Rand.Range(60000, 90000);
            }
            return (int)Rand.Range(25000, 55000);
        }
        //Method to spawn fissure
        public void makeAndSpawnFissure(FissureSize size, IntVec3 loc)
        {
            if (size == FissureSize.SteamGeyser)
            {
                GenSpawn.Spawn(ThingDef.Named("SteamGeyser"), loc);
            }
            else
            {
                FissureClass fis = (FissureClass)ThingMaker.MakeThing(ThingDef.Named("mipFissure"));
                fis.size = size;
                GenSpawn.Spawn(fis, loc);
            }
        }
        //Overload to use generator base location as location
        public void makeAndSpawnFissure(FissureSize size)
        {
            makeAndSpawnFissure(size, base.Position);
        }

        public void toggleStarted()
        {
            if (!this.started)
            {
                this.started = true;
            }
            this.running = !this.running;
        }

        private void cycleThroughFissuresToGenerate()
        {
            count++;
            if (this.count > 3)
            {
                this.count = 0;
            }
            if (this.count == 0)
            {
                this.fissureSize = FissureSize.Small;
            }
            if (this.count == 1)
            {
                this.fissureSize = FissureSize.Medium;
            }
            if (this.count == 2)
            {
                this.fissureSize = FissureSize.Large;
            }
            if (this.count == 3)
            {
                this.fissureSize = FissureSize.SteamGeyser;
            }
        }

        private float powerUsage(FissureSize size)
        {
            switch (size)
            {
                case FissureSize.SteamGeyser:
                    return -1200f;
                case FissureSize.Large:
                    return -1000f;
                case FissureSize.Medium:
                    return -750f;
                case FissureSize.Small:
                    return -550f;
                default:
                    return -this.powerTrader.props.basePowerConsumption;
            }

        }
    }
}
