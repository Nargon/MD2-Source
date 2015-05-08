using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MD2
{
    public class Track : Building
    {
        public IEnumerable<Track> ConnectedTracks()
        {
            return ConnectedTracks(null);
        }

        public IEnumerable<Track> ConnectedTracks(Track track)
        {
            foreach (var cell in GenAdj.CellsAdjacentCardinal(this))
            {
                if (cell.InBounds() && !cell.InNoBuildEdgeArea())
                {
                    Track t = Find.ThingGrid.ThingAt<Track>(cell);
                    if(track!=null&&t==track)
                    {
                        continue;
                    }
                    if (t != null && t != track)
                    {
                        yield return t;
                    }
                }
            }
        }

        public virtual bool TrackInDirection(Direction dir, out Track track)
        {
            if (HasConnectedTrack())
            {
                IntVec3 pos = Position.AdjacentInDirection(dir);
                if (pos.InBounds())
                {
                    Track t = Find.ThingGrid.ThingAt<Track>(pos);
                    if (t != null)
                    {
                        track = t;
                        return true;
                    }
                }
            }
            track = null;
            return false;
        }

        public virtual bool HasConnectedTrack(Track queryingTrack)
        {
            List<Track> list = ConnectedTracks(queryingTrack).ToList();
            if (list.Count == 0)
                return false;
            return true;
        }

        public virtual bool HasConnectedTrack()
        {
            return HasConnectedTrack(null);
        }
    }
}
