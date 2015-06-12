using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MD2
{
    public interface RepairableDroid
    {
        bool BeingRepaired { get; set; }
        Building_DroidRepairStation RepairStation { get; set; }
        void RepairTick();
        bool ShouldGetRepairs { get; }
        Pawn Pawn { get; }
        int RepairsNeededCount { get; }
    }
}
