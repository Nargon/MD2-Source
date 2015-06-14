using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace MD2
{
    public class Building_RepairStation : Building
    {
        private IRepairable repairable;
        private List<CompRepairStationSupplier> suppliers = new List<CompRepairStationSupplier>();

        public CompPowerTrader Power
        {
            get
            {
                return GetComp<CompPowerTrader>();
            }
        }

        public ThingDef_RepairStation Def
        {
            get
            {
                return this.def as ThingDef_RepairStation;
            }
        }

        public IEnumerable<Thing> AllAvailableResources
        {
            get
            {
                if(suppliers.Count>0)
                {
                    foreach(var s in suppliers)
                    {
                        foreach(var t in s.AvailableResources)
                        {
                            yield return t;
                        }
                    }
                }
            }
        }

        public bool HasEnoughOf(ThingDef def, int amount)
        {
            int amountAvailable = 0;
            foreach(var t in AllAvailableResources)
            {
                if (t.def == def)
                {
                    //Log.Message("Found some");
                    amountAvailable += t.stackCount;
                }
            }
            return amountAvailable >= amount ? true : false;
        }

        public bool TakeSomeOf(ThingDef def, int amount)
        {
            if (HasEnoughOf(def, amount))
            {
                int remaining = amount;
                while (remaining > 0)
                {
                    foreach (var t in AllAvailableResources)
                    {
                        if (t.def == def)
                        {
                            int num = Mathf.Min(t.stackCount, remaining);
                            remaining -= num;
                            t.stackCount -= num;
                            if (t.stackCount <= 0)
                                t.Destroy();
                            //Log.Message("Took some");
                        }
                        if (remaining <= 0)
                            break;
                    }
                }
                if (remaining <= 0)
                {
                    //Log.Message("Took all");
                    return true;
                }
                return false;
            }
            return false;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            Notify_RepairStationDespawned();
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            Notify_RepairStationSpawned();
        }

        public void Notify_SupplierSpawned(CompRepairStationSupplier supplier)
        {
            if (!suppliers.Contains(supplier))
                suppliers.Add(supplier);
        }

        public void Notify_SupplierDespawned(CompRepairStationSupplier supplier)
        {
            if (suppliers.Contains(supplier))
                suppliers.Remove(supplier);
        }

        private void Notify_RepairStationSpawned()
        {
            foreach(var c in GenAdj.CellsAdjacentCardinal(this))
            {
                Building b = Find.ThingGrid.ThingAt<Building>(c);
                if(b!=null)
                {
                    CompRepairStationSupplier supplier = b.GetComp<CompRepairStationSupplier>();
                    if(supplier!=null)
                    {
                        supplier.Notify_RepairStationSpawned(this);
                    }
                }
            }
        }

        private void Notify_RepairStationDespawned()
        {
            if(suppliers.Count>0)
            {
                foreach(var s in suppliers)
                {
                    s.Notify_RepairStationDespawned(this);
                }
            }
        }

        public void RegisterRepairee(IRepairable d)
        {
            this.repairable = d;
            d.RepairStation = this;
        }

        public void DeregisterRepairee(IRepairable d)
        {
            d.RepairStation = null;
            repairable = null;
        }

        public bool IsAvailable(Pawn p)
        {
            if(Power!=null)
            {
                return Power.PowerOn && CanUse(p);
            }
            return CanUse(p);
        }

        private bool CanUse(Pawn p)
        {
            return (p is IRepairable) && repairable == null || repairable == p;
        }
    }
}
