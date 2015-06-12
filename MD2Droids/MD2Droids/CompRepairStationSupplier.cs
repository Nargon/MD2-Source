using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MD2
{
    public class CompRepairStationSupplier : ThingComp
    {
        private List<Building_DroidRepairStation> repairStations = new List<Building_DroidRepairStation>();
        
        public IEnumerable<Thing> AvailableResources
        {
            get
            {
                Building_Storage b = (Building_Storage)this.parent;
                return b.slotGroup.HeldThings;
            }
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            Notify_SupplierSpawned();
        }

        public override void PostDestroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDestroy(mode);
            Notify_SupplierDespawned();
        }

        public void Notify_RepairStationDespawned(Building_DroidRepairStation rps)
        {
            if (repairStations.Contains(rps))
            {
                repairStations.Remove(rps);
            }
        }

        public void Notify_RepairStationSpawned(Building_DroidRepairStation rps)
        {
            if(!repairStations.Contains(rps))
            {
                repairStations.Add(rps);
            }
        }

        private void Notify_SupplierSpawned()
        {
            if (Find.ListerBuildings.AllBuildingsColonistOfClass<Building_DroidRepairStation>() != null)
            {
                foreach (var c in GenAdj.CellsAdjacentCardinal(this.parent))
                {
                    Building_DroidRepairStation rps = Find.ThingGrid.ThingAt<Building_DroidRepairStation>(c);
                    if(rps!=null)
                    {
                        rps.Notify_SupplierSpawned(this);
                        repairStations.Add(rps);
                    }
                }
            }
        }

        private void Notify_SupplierDespawned()
        {
            if(repairStations.Count>0)
            {
                foreach(var rps in repairStations)
                {
                    rps.Notify_SupplierDespawned(this);
                }
            }
        }
    }
}
