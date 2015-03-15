﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace MD2
{
    public class WorkGiver_CollectSand : WorkGiver
    {
        public override PathMode PathMode
        {
            get
            {
                return PathMode.Touch;
            }
        }

        public WorkGiver_CollectSand(WorkGiverDef giverDef):base(giverDef)
        {
        }

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            List<IntVec3> cells = new List<IntVec3>();
            foreach(var designation in Find.DesignationManager.DesignationsOfDef(DefDatabase<DesignationDef>.GetNamed("MD2CollectSand")))
            {
                IntVec3 cell = designation.target.Cell;
                if(cell!=null && cell.InBounds()&&pawn.CanReserveAndReach(designation.target,ReservationType.Total,PathMode.OnCell,Danger.Some))
                {
                   cells.Add( designation.target.Cell); 
                }
            }
            Log.Message(cells.Count.ToString());
            return cells.AsEnumerable();
        }

        public override bool ShouldSkip(Pawn pawn)
        {
            return Find.DesignationManager.DesignationsOfDef(DefDatabase<DesignationDef>.GetNamed("MD2CollectSand")).Count() ==0;
        }

        public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
        {
            return pawn.Faction == Faction.OfColony && Find.DesignationManager.DesignationAt(c, DefDatabase<DesignationDef>.GetNamed("MD2CollectSand")) != null && pawn.CanReserveAndReach(c, ReservationType.Total, Verse.AI.PathMode.OnCell, Danger.Some);
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 cell)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("MD2CollectSandJob"), new TargetInfo(cell));
        }
    }
}
