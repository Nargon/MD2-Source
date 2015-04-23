using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace MD2
{
    public static class CorpseFinderUtility
    {
        public static Thing FindClosestCorpse(Pawn getter)
        {
            Predicate<Thing> predicate = (Thing c) => (c is Corpse) && !((Corpse)c).innerPawn.RaceProps.Animal && !((Corpse)c).innerPawn.RaceProps.mechanoid && !c.IsBuried() && !c.IsForbidden(Faction.OfColony) && getter.AwareOf(c) && getter.CanReserve(c);
            ThingRequest thingReq = ThingRequest.ForGroup(ThingRequestGroup.Corpse);
            return GenClosest.ClosestThingReachable(getter.Position, thingReq, PathMode.ClosestTouch, TraverseParms.For(getter), 9999f, predicate, null);
        }
    }
}
