﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MD2
{
    public class Cart_Pather : Saveable
    {
        private Cart cart;
        private Direction direction = Direction.Up.Random();
        private Track destination = null;
        private Track previousTrack = null;
        private int ticksUntilMove = 0;
        private int totalMoveDuration = 1;
        protected bool moving = false;

        public Cart_Pather(Cart cart)
        {
            this.cart = cart;
        }

        public bool Moving
        {
            get
            {
                return moving;
            }
        }

        public Track Destination
        {
            get
            {
                return destination;
            }
        }

        public Direction Direction
        {
            get
            {
                return this.direction;
            }
        }

        public int TicksUntilMove
        {
            get
            {
                return this.ticksUntilMove;
            }
            set
            {
                this.ticksUntilMove = value;
            }
        }

        public int TotalMoveDuration
        {
            get
            {
                return this.totalMoveDuration;
            }
        }

        public Track CurTrack
        {
            get
            {
                return Find.ThingGrid.ThingAt<Track>(cart.Position);
            }
        }

        public bool DestinationIsValid
        {
            get
            {
                bool flag = true;

                if (destination == null)
                    flag = false;

                if (destination.Destroyed)
                    flag = false;

                if (!destination.IsAdjacentTo(CurTrack))
                    flag = false;

                if (!(destination.Position.InBounds()))
                    flag = false;

                return flag;
            }
        }

        public virtual void Tick()
        {
            if (CurTrack == null)
            {
                return;
            }
            if (!Moving)
            {
                StartNewMove();
            }
            else
            {
                MovementTick();
            }

        }

        private void MoveTo(Track track)
        {
            previousTrack = CurTrack;
            cart.Position = track.Position;
            moving = false;
            ticksUntilMove = 0;
        }

        private void MovementTick()
        {
            if(TicksUntilMove>0)
            {
                TicksUntilMove--;
                return;
            }
            if(Moving)
            {
                MoveTo(destination);
            }
        }

        private void StartNewMove()
        {
            if (CurTrack.HasValidPath && !CurTrack.ShouldStopCart)
            {
                SetupNewMove();
                CurTrack.NextTrack(direction, out destination, previousTrack);
                CurTrack.SetDirectionRelativeTo(destination, out direction);
                moving = true;
            }
        }

        private void SetupNewMove()
        {
            int ticks = cart.TicksPerMove();
            if(ticks>450)
            {
                ticks = 450;
            }
            totalMoveDuration = ticks;
            ticksUntilMove = ticks;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref moving, "moving");
            Scribe_Values.LookValue(ref this.direction, "direction", Direction.Any);
            Scribe_References.LookReference(ref this.previousTrack, "previousTrack");
        }
    }
}
