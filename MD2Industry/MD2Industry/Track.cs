using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace MD2
{
    public class Track : Building
    {
        public static readonly SettingsDef Config = DefDatabase<SettingsDef>.GetNamed("MD2CartSettings");

        private List<Track> connectedTracks = new List<Track>();

        public override void Draw()
        {
            base.Draw();

            if (Config.drawTrackDebug)
            {
                foreach (var track in connectedTracks)
                {
                    GenDraw.DrawTargetHighlight(track);
                }
            }
        }

        public int ConnectedTracksCount
        {
            get
            {
                return connectedTracks.Count;
            }
        }

        public List<Track> ConnectedTracks
        {
            get
            {
                return this.connectedTracks;
            }
        }

        public bool NextTrackInDirection(Direction dir, out Track track)
        {
            IntVec3 pos = Position.AdjacentInDirection(dir);
            if (pos.InBounds() && !pos.InNoBuildEdgeArea())
            {
                foreach (var t in connectedTracks)
                {
                    if (t.Position == pos)
                    {
                        track = t;
                        return true;
                    }
                }
            }
            track = null;
            return false;
        }

        public bool SetDirectionRelativeTo(Track track, out Direction dir)
        {
            if (track.Position.AdjacentToCardinal(this.Position))
            {
                if ((track.Position + IntVec3.North) == Position)
                {
                    dir = Direction.Down;
                    return true;
                }
                else if ((track.Position + IntVec3.South) == Position)
                {
                    dir = Direction.Up;
                    return true;
                }
                else if ((track.Position + IntVec3.East) == Position)
                {
                    dir = Direction.Left;
                    return true;
                }
                else if ((track.Position + IntVec3.West) == Position)
                {
                    dir = Direction.Right;
                    return true;
                }
                dir = Direction.Any;
                return false;
            }
            dir = Direction.Any;
            return false;
        }

        public bool NextTrack(Direction preferredDir, out Track result, Track previousTrack = null)
        {
            Track t;
            if (preferredDir != Direction.Any)
            {
                if (NextTrackInDirection(preferredDir, out t))
                {
                    result = t;
                    return true;
                }
                result = NextTrackRandom(previousTrack);
                return result != null;
            }
            else
            {
                result = NextTrackRandom(previousTrack);
                return result != null;
            }
        }

        public bool NextTrack(out Track result, Track previousTrack = null)
        {
            return NextTrack(Direction.Any, out result, previousTrack);
        }

        public Track NextTrackRandom(Track previousTrack = null)
        {
            if (connectedTracks.Count > 0)
            {
                List<Track> list;
                if(previousTrack!=null&&connectedTracks.Count>1)
                {
                    list = connectedTracks.Where(t => t != previousTrack).ToList();
                }
                else
                {
                    list = connectedTracks;
                }
                return list.RandomElement();
            }
            return null;
        }

        public virtual bool ShouldStopCart
        {
            get 
            { 
                return false;
            }
        }

        public bool HasTrackLeft
        {
            get
            {
                Track t;
                return NextTrackInDirection(Direction.Left, out t);
            }
        }

        public bool HasTrackRight
        {
            get
            {
                Track t;
                return NextTrackInDirection(Direction.Right, out t);
            }
        }

        public bool HasTrackUp
        {
            get
            {
                Track t;
                return NextTrackInDirection(Direction.Up, out t);
            }
        }

        public bool HasTrackDown
        {
            get
            {
                Track t;
                return NextTrackInDirection(Direction.Down, out t);
            }
        }

        public bool HasValidPath
        {
            get
            {
                return this.connectedTracks.Count >= 0;
            }
        }

        public bool IsAdjacentTo(Track track)
        {
            return this.connectedTracks.Contains(track);
        }

        public void ReloadConnected(bool spawnSetup = false)
        {
            connectedTracks = new List<Track>();
            foreach (var cell in GenAdj.CellsAdjacentCardinal(this))
            {
                foreach (var thing in Find.ThingGrid.ThingsAt(cell))
                {
                    if (thing is Track && !(thing.Destroyed || !thing.SpawnedInWorld) && thing != this)
                    {
                        Track t = (Track)thing;
                        connectedTracks.Add(t);
                        if (spawnSetup)
                        {
                            t.ReloadConnected();
                        }
                    }
                }
            }
        }

        public override void SpawnSetup()
        {
            //this.renderer = new Track_Renderer(this);
            base.SpawnSetup();
            ReloadConnected(true);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            foreach (var track in connectedTracks)
            {
                track.ReloadConnected();
            }
        }
    }
}
