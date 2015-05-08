using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace MD2
{
    public class Cart : ThingWithComps
    {
        public CartPather pather;
        public CartDrawer drawer;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.LookDeep(ref this.pather, "pather", this);
            Scribe_Deep.LookDeep(ref this.drawer, "drawer", this);
        }

        public override Vector3 DrawPos
        {
            get
            {
                return this.drawer.DrawPos;
            }
        }

        public override void DrawAt(Vector3 pos)
        {
            drawer.DrawAt(pos);
        }

        public int TicksPerMove(bool diagonal = false)
        {
            float cellsPerSecond = this.GetStatValue(StatDefOf.MoveSpeed, false);
            float num = cellsPerSecond / 60f;
            float num2 = 1f / num;
            if (!Find.RoofGrid.Roofed(base.Position))
            {
                num2 /= Find.WeatherManager.CurMoveSpeedMultiplier;
            }
            int value = Mathf.RoundToInt(num2);
            return Mathf.Clamp(value, 1, 450);
        }
    }
}
