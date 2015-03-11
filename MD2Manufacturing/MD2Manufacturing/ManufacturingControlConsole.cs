﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MD2
{
    public class ManufacturingControlConsole : Building
    {

        public override void SpawnSetup()
        {
            base.SpawnSetup();
        }

        public override void Tick()
        {
            if(Power!=null)
            {
                Power.powerOutput = MPmanager.manager.PowerUsage;
            }
            base.Tick();
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptionsFor(Pawn myPawn)
        {
            foreach(var m in base.GetFloatMenuOptionsFor(myPawn))
            {
                yield return m;
            }
            FloatMenuOption option = new FloatMenuOption();
            option.action = () =>
            {
                Find.LayerStack.Add(new Page_ManufacturingPlantMainUI());
            };
            option.label = "Open manager";
            option.priority = MenuOptionPriority.High;
            option.autoTakeable = true;
            yield return option;
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
            com.action = () =>
                {
                    Find.LayerStack.Add(new Page_ManufacturingPlantMainUI());
                };
            com.defaultDesc = "Opens the manufacturing manager user interface";
            com.defaultLabel = "Open manufacturing manager";
            com.disabled = false;
            com.groupKey = 123456789;
            yield return com;
        }

        public CompPowerTrader Power
        {
            get
            {
                return base.GetComp<CompPowerTrader>();
            }
        }
    }
}
