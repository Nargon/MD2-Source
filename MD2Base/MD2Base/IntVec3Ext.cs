using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MD2
{
    public static class IntVec3Ext
    {
        public static IntVec3 AdjacentInDirection(this IntVec3 pos, Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:
                    return pos += IntVec3.North;
                case Direction.Down:
                    return pos += IntVec3.South;
                case Direction.Left:
                    return pos += IntVec3.West;
                case Direction.Right:
                    return pos += IntVec3.East;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
