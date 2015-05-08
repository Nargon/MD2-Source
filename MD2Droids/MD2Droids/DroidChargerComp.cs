using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MD2
{
    class DroidChargerComp : ThingComp
    {
        private Pawn pawn;
        public bool CanUse(Pawn p)
        {
            return pawn == null || pawn == p;
        }

        public Pawn Droid
        {
            get
            {
                return this.pawn;
            }
        }

        public void Destroy()
        {
            if (pawn != null)
                pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.Incompletable);
        }

        public override void CompTick()
        {
            base.CompTick();
            //Checks if the pawn is not on this parent or is destroyed
            if (pawn != null && (pawn.Position != this.parent.Position || pawn.Destroyed))
            {
                pawn = null;
            }
            //Assigns the working var 'pawn' to the pawn found on the parent
            if (pawn == null)
            {
                pawn = Find.ThingGrid.ThingAt<Droid>(this.parent.Position);
            }
            //Checks if there is a powertrader comp on the parent
            CompPowerTrader power = this.parent.GetComp<CompPowerTrader>();
            if (power == null)
            {
                Log.Message(parent.def.defName + " needs PowerTrader to run");
                return;
            }
            float usageRate = 0.01f;
            //If there was a pawn found, get its current energy and see if it is below the max, if yes then start charging
            if (pawn != null && power.PowerOn)
            {
                usageRate = 1f;
                InternalCharge foundDroid = (InternalCharge)pawn;
                if (power.PowerOn && foundDroid.DesiresCharge())
                {
                    power.powerOutputInt = -usageRate * power.props.basePowerConsumption;
                    foundDroid.Charge(-power.powerOutputInt);
                }
            }
            else
            {
                power.powerOutputInt = -usageRate * power.props.basePowerConsumption;
            }
        }
    }
}
