using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace MD2
{
    static class DroidChargerFinder
    {
        public static Thing ClosestThing_Global_Reachable(IntVec3 center, IEnumerable<Thing> searchSet, PathMode pathMode, TraverseParms traverseParams, float maxDistance, Pawn pawn)
		{
			if (searchSet == null)
			{
				return null;
			}
			int num = 0;
			int num2 = 0;
			Thing result = null;
			int num3 = -2147483648;
			int num4 = 0;
			float num5 = maxDistance * maxDistance;
			float num6 = 2.14748365E+09f;
			foreach (Thing current in searchSet)
			{
				num2++;
				float lengthHorizontalSquared = (center - current.Position).LengthHorizontalSquared;
				if (lengthHorizontalSquared <= num5)
				{
					if (num4 > num3 || lengthHorizontalSquared < num6)
					{
						if (center.CanReach(current, pathMode, traverseParams))
						{
							if (current.SpawnedInWorld)
							{
                                
                                if(((Building_DroidChargePad)current).IsAvailable(pawn))
                                {
									result = current;
									num6 = lengthHorizontalSquared;
									num3 = num4;
									num++;
								}
							}
						}
					}
				}
			}
			return result;
		}
	}
    
}
