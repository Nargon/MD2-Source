using RimWorld;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using Verse;


namespace MD2
{
    public class Droid : Pawn, InternalCharge, RepairableDroid
    {
        public DroidUIOverlay overlay;

        private readonly Texture2D SDIcon = ContentFinder<Texture2D>.Get("UI/Commands/SelfDestructIcon");
        private readonly Texture2D StartIcon = ContentFinder<Texture2D>.Get("UI/Commands/BeginUI");
        private readonly Texture2D StopIcon = ContentFinder<Texture2D>.Get("UI/Commands/PauseUI");
        public Graphic bodyGraphic;
        public Graphic headGraphic;
        private float totalCharge = 1000f;
        public bool ChargingNow = false;
        public bool RequiresPower = true;
        private bool active = true;

        private bool beingRepaired = false;
        private Building_DroidRepairStation repairStation;

        public override void SpawnSetup()
        {
            ListerDroids.RegisterDroid(this);
            base.SpawnSetup();
            this.bodyGraphic = GraphicDatabase.Get<Graphic_Multi>(KindDef.standardBodyGraphicPath, ShaderDatabase.Cutout, IntVec2.One, Color.white);
            if (!KindDef.headGraphicPath.NullOrEmpty())
                this.headGraphic = GraphicDatabase.Get<Graphic_Multi>(KindDef.headGraphicPath, ShaderDatabase.Cutout, IntVec2.One, Color.white);
            DoGraphicChanges();
        }

        public float MaxEnergy
        {
            get
            {
                return KindDef.maxEnergy;
            }
        }

        public override void Tick()
        {
            RefillNeeds();
            CureDiseases();
            CheckPowerRemaining();
            ResetDrafter();
            base.Tick();
            if (!Active || BeingRepaired || (KindDef.disableOnSolarFlare && Find.MapConditionManager.ConditionIsActive(MapConditionDefOf.SolarFlare)))
            {
                Inactive();
            }
            else
            {
                if (!ChargingNow && !BeingRepaired)
                {
                    Deplete(KindDef.EnergyUseRate);
                }
            }
        }

        private void ResetDrafter()
        {
            if(this.playerController.Drafted)
            {
                if(Find.TickManager.TicksGame%4800==0)
                {
                    AutoUndrafter undrafter = (AutoUndrafter)typeof(Drafter).GetField("autoUndrafter", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(this.playerController.drafter);
                    if(undrafter!=null)
                    {
                        typeof(AutoUndrafter).GetField("lastNonWaitingTick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(undrafter, Find.TickManager.TicksGame);
                        //Log.Message("Reset ticks " + ((int)typeof(AutoUndrafter).GetField("lastNonWaitingTick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(undrafter)).ToString());
                    }
                }
            }
        }

        private void Inactive()
        {
            if (pather.Moving)
                pather.StopDead();
            if (!BeingRepaired && jobs.curJob.def != Job_DroidDeactivated.Def)
            {
                jobs.StopAll();
                jobs.StartJob(new Verse.AI.Job(Job_DroidDeactivated.Def));
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
            str.AppendLine(string.Format("Current energy: {0}W/{1}Wd", TotalCharge.ToString("0.0"), MaxEnergy));
            return str.ToString();
        }

        public override TipSignal GetTooltip()
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine(this.LabelCap + " " + this.kindDef.label);
            s.AppendLine("Current energy: " + this.TotalCharge.ToString("0.0") + "W/" + this.MaxEnergy.ToString() + "Wd");
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
                if (!active)
                {
                    jobs.StopAll();
                    jobs.StartJob(new Verse.AI.Job(Job_DroidDeactivated.Def));
                }
                else
                {
                    jobs.StopAll();
                }
            };
            yield return com;

            Command_Action a = new Command_Action();
            a.action = () =>
            {
                Find.LayerStack.Add(new Dialog_Confirm("DroidSelfDestructPrompt".Translate(), delegate
                    {
                        this.Destroy(DestroyMode.Kill);
                    }));
            };
            a.activateSound = SoundDefOf.Click;
            a.defaultDesc = "Click this button to cause the droid to self destruct";
            a.defaultLabel = "Self Destruct";
            a.disabled = false;
            a.groupKey = 313740004;
            a.icon = this.SDIcon;
            yield return a;
        }

        public SettingsDef Settings
        {
            get
            {
                return ((DroidKindDef)kindDef).Settings;
            }
        }

        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
            overlay.DrawGUIOverlay();
        }

        public virtual void DoGraphicChanges()
        {
            overlay = new DroidUIOverlay(this);
            this.drawer.renderer.graphics.ResolveGraphics();
            this.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved", true);
            if (headGraphic != null)
            {
                this.drawer.renderer.graphics.headGraphic = this.headGraphic;
                this.drawer.renderer.graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(this.story.hairDef.graphicPath, ShaderDatabase.Cutout, IntVec2.One, this.story.hairColor);
            }
            this.drawer.renderer.graphics.nakedGraphic = this.bodyGraphic;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            ListerDroids.DeregisterDroid(this);

            base.Destroy(mode);
            if (mode == DestroyMode.Kill && this.KindDef.explodeOnDeath)
                GenExplosion.DoExplosion(this.Position, KindDef.explosionRadius, DamageDefOf.Bomb, this);

        }

        public virtual void CheckPowerRemaining()
        {
            if (TotalCharge < 1f || Downed)
            {
                this.Destroy(DestroyMode.Kill);
            }
        }

        public virtual DroidKindDef KindDef
        {
            get
            {
                return this.kindDef as DroidKindDef;
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

        public float TotalCharge
        {
            get { return this.totalCharge; }
            set { this.totalCharge = value; }
        }

        public bool AddPowerDirect(float amount)
        {
            TotalCharge += amount;
            if (TotalCharge > MaxEnergy)
            {
                TotalCharge = MaxEnergy;
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

            if (TotalCharge < MaxEnergy)
            {
                TotalCharge += (rate * CompPower.WattsToWattDaysPerTick * 2);
                if (TotalCharge > MaxEnergy)
                    TotalCharge = MaxEnergy;
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
            return this.TotalCharge < MaxEnergy;
        }

        public bool BeingRepaired
        {
            get
            {
                return beingRepaired;
            }
            set
            {
                beingRepaired = value;
            }
        }

        public Building_DroidRepairStation RepairStation
        {
            get
            {
                return repairStation;
            }
            set
            {
                repairStation = value;
            }
        }

        public void RepairTick()
        {
            if (RepairStation != null)
            {
                BodyPartRecord missingPart = null;
                if (health.hediffSet.GetHediffs<Hediff_MissingPart>().Count() > 0)
                {
                    missingPart = health.hediffSet.GetHediffs<Hediff_MissingPart>().First().Part;
                }

                Hediff_Injury injury = null;
                if (health.hediffSet.GetInjuriesLocalTreatable().Count() > 0)
                {
                    injury = health.hediffSet.GetInjuriesLocalTreatable().RandomElement();
                }  

                bool hasPartMissing = missingPart != null ? true : false;
                int repairAmount = RepairStation.Def.repairAmount;
                bool rpsHasResources = true;
                if (RepairStation.Def.replacePartsCostsResources)
                    rpsHasResources = RepairStation.HasEnoughOf(RepairStation.Def.repairThingDef, RepairStation.Def.repairCostAmount);

                if (hasPartMissing && rpsHasResources)
                {
                    //Log.Message("Part is missing!");
                    if (Rand.Value > 0.5f && injury != null)
                    {
                        injury.DirectHeal(repairAmount);
                    }
                    else if (missingPart != null)
                    {
                        if (RepairStation.TakeSomeOf(RepairStation.Def.repairThingDef, RepairStation.Def.repairCostAmount))
                        {
                            //Log.Message("Took the resources and restored the part");
                            health.hediffSet.RestorePart(missingPart);
                        }
                    }
                }
                else if (injury != null)
                {
                    health.hediffSet.GetInjuriesLocalTreatable().RandomElement().DirectHeal(repairAmount);
                }
            }
        }

        public bool ShouldGetRepairs
        {
            get
            {
                return health.hediffSet.HasTreatableInjury() ||
                    health.hediffSet.HasFreshMissingPartsCommonAncestor() ||
                    health.hediffSet.GetHediffs<Hediff_MissingPart>().Count() > 0 &&
                    ((this.Faction == Faction.OfColony && !this.IsPrisonerOfColony) || (this.guest != null && this.guest.doctorsCare));
            }
        }

        public Pawn Pawn
        {
            get
            {
                return this;
            }
        }

        public int RepairsNeededCount
        {
            get
            {
                return health.hediffSet.GetHediffs<Hediff_Injury>().Count() + health.hediffSet.GetHediffs<Hediff_MissingPart>().Count();
            }
        }
    }
}
