using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;


namespace MD2
{
    public class Droid : Pawn, InternalCharge
    {
        private readonly Texture2D SDIcon = ContentFinder<Texture2D>.Get("UI/Commands/SelfDestructIcon");
        private readonly Texture2D StartIcon = ContentFinder<Texture2D>.Get("UI/Commands/BeginUI");
        private readonly Texture2D StopIcon = ContentFinder<Texture2D>.Get("UI/Commands/PauseUI");
        public Graphic bodyGraphic;
        public Graphic headGraphic;
        private float totalCharge = 1000f;
        public float maxEnergy = 1000;
        public float EnergyUseRate = 150f;
        public bool ChargingNow = false;
        public bool RequiresPower = true;
        private bool active = true;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            if (((DroidKindDef)this.kindDef).maxEnergy > 0)
                maxEnergy = ((DroidKindDef)this.kindDef).maxEnergy;
            else
                Log.Error("Max energy for " + this.ToString() + " was zero or below");
            this.bodyGraphic = GraphicDatabase.Get<Graphic_Multi>(((DroidKindDef)this.kindDef).standardBodyGraphicPath, ShaderDatabase.Cutout, IntVec2.One, Color.white);
            this.headGraphic = GraphicDatabase.Get<Graphic_Multi>(((DroidKindDef)this.kindDef).headGraphicPath, ShaderDatabase.Cutout,IntVec2.One,Color.white);
            DoGraphicChanges();
        }

        public override void Tick()
        {
            RefillNeeds();
            CureDiseases();
            HealDamages();
            CheckPowerRemaining();
            base.Tick();
            if (!Active)
            {
                if (pather.Moving)
                    pather.StopDead();
                if(jobs.curJob.def!=DroidDeactivatedJob.Def)
                {
                    jobs.StopAll();
                    jobs.StartJob(new Verse.AI.Job(DroidDeactivatedJob.Def));
                }
            }
            else
            {
                if (!ChargingNow)
                {
                    Deplete(EnergyUseRate);
                }
            }
        }

        private void RefillNeeds()
        {
            if (Find.TickManager.TicksGame % 180 == 0)
            {
                foreach (Need n in this.needs.AllNeeds)
                {
                    if (n.CurLevel < 90)
                    {
                        n.CurLevel = 100f;
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<bool>(ref this.ChargingNow, "chargingNow");
            Scribe_Values.LookValue<float>(ref this.totalCharge, "TotalCharge");
            Scribe_Values.LookValue<bool>(ref this.active, "active");
        }

        public override string GetInspectString()
        {
            StringBuilder str = new StringBuilder();
            //str.AppendLine(jobs.curJob.def.reportString);
            str.Append(base.GetInspectString());
            str.AppendLine(string.Format("Current energy: {0}W/{1}Wd", TotalCharge.ToString("0.0"), maxEnergy));
            return str.ToString();
        }

        public override TipSignal GetTooltip()
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine(this.LabelCap + " " + this.kindDef.label);
            s.AppendLine("Current energy: " + this.TotalCharge.ToString("0.0") + "W/" + this.maxEnergy.ToString() + "Wd");
            if (this.equipment != null && this.equipment.Primary != null)
            {
                s.AppendLine(this.equipment.Primary.LabelCap);
            }
            s.AppendLine(HealthUtility.GetGeneralConditionLabel(this));
            return new TipSignal(s.ToString().TrimEnd(new char[]
    {
        '\n'
    }), this.thingIDNumber * 152317, TooltipPriority.Pawn);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (base.GetGizmos() != null)
            {
                foreach (Gizmo c in base.GetGizmos())
                {
                    yield return c;
                }
            }
            Command_Action com = new Command_Action();
            com.activateSound = SoundDefOf.Click;
            if (this.active)
            {
                com.defaultDesc = "Click to deactivate this droid and save power.";
                com.defaultLabel = "Deactivate";
                com.icon = this.StopIcon;
            }
            else
            {
                com.defaultDesc = "Click to activate this droid.";
                com.defaultLabel = "Activate";
                com.icon = this.StartIcon;
            }
            com.disabled = false;
            com.groupKey = 313740008;
            com.hotKey = Keys.DeactivateDroid;
            com.action = () =>
            {
                this.active = !this.active;
                if(!active)
                {
                    jobs.StopAll();
                    jobs.StartJob(new Verse.AI.Job(DroidDeactivatedJob.Def));
                }
                else
                {
                    jobs.StopAll();
                }
            };
            yield return com;
        }
        //    /* Command_Action a = new Command_Action();
        //     a.action = () =>
        //     {
        //         this.Destroy(DestroyMode.Kill);
        //     };
        //     a.activateSound = SoundDefOf.Click;
        //     a.defaultDesc = "Click this button to cause the droid to self destruct";
        //     a.defaultLabel = "Self Destruct";
        //     a.disabled = false;
        //     a.groupKey = 313740004;
        //     a.icon = this.SDIcon;
        //     yield return a;*/
        //}

        public SettingsDef Settings
        {
            get
            {
                return ((DroidKindDef)kindDef).Settings;
            }
        }

        public virtual void DoGraphicChanges()
        {
            this.drawer.renderer.graphics.ResolveGraphics();
            this.drawer.renderer.graphics.headGraphic = this.headGraphic;
            this.drawer.renderer.graphics.nakedGraphic = this.bodyGraphic;
            this.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved", true);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            if (mode == DestroyMode.Kill)
                GenExplosion.DoExplosion(this.Position, 1.9f, DamageDefOf.Bomb, this);

        }

        public virtual void CheckPowerRemaining()
        {
            if (TotalCharge < 1f || this.Downed)
            {
                this.Destroy();
            }
        }

        public bool Active
        {
            get
            {
                return this.active;
            }
        }

        private void CureDiseases()
        {
            IEnumerable<Hediff_Staged> diseases = this.health.hediffSet.GetDiseases();
            if (diseases != null)
            {
                foreach (Hediff_Staged d in diseases)
                {
                    d.DirectHeal(1000f);
                }
            }
        }

        public SettingsDef Config
        {
            get
            {
                return ((DroidKindDef)this.kindDef).Settings;
            }
        }

        private void HealDamages()
        {
            if (Find.TickManager.TicksGame % Settings.healDelayTicks == 0)
            {
                IEnumerable<Hediff_Injury> treatableInjuries = this.health.hediffSet.GetInjuriesLocalTreatable();
                foreach (var current in treatableInjuries)
                {
                    if (!current.IsTreatedAndHealing())
                    {
                        current.DirectHeal(1000000f);
                        break;
                    }
                }
            }
        }

        public float TotalCharge
        {
            get { return this.totalCharge; }
            set { this.totalCharge = value; }
        }

        public bool AddPowerDirect(float amount)
        {
            TotalCharge += amount;
            if (TotalCharge > maxEnergy)
            {
                TotalCharge = maxEnergy;
                return false;
            }
            return true;
        }

        public bool RemovePowerDirect(float amount)
        {
            TotalCharge -= amount;
            if (TotalCharge < 0)
            {
                TotalCharge = 0f;
                return false;
            }
            return true;
        }

        public bool Charge(float rate)
        {

            if (TotalCharge < maxEnergy)
            {
                TotalCharge += (rate * CompPower.WattsToWattDaysPerTick * 2);
                if (TotalCharge > maxEnergy)
                    TotalCharge = maxEnergy;
                return true;
            }
            return false;

        }

        public bool Deplete(float rate)
        {
            if (TotalCharge > 0)
            {
                TotalCharge -= (rate * CompPower.WattsToWattDaysPerTick);
                if (TotalCharge < 0)
                    TotalCharge = 0;
                return true;
            }
            return false;
        }

        public bool DesiresCharge()
        {
            return this.TotalCharge < maxEnergy;
        }
    }
}
