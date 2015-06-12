using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MD2
{
    class Building_DroidChargePad : Building
    {
        public CompDroidCharger Charger
        {
            get
            {
                return this.GetComp<CompDroidCharger>();
            }
        }
        public bool IsAvailable(Pawn p)
        {
            CompPowerTrader power = this.GetComp<CompPowerTrader>();
            return power != null && power.PowerOn && Charger != null && Charger.CanUse(p);
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            if (Charger != null)
                Charger.Destroy();
        }
    }
}
