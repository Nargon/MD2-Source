using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MD2
{
    public class CartPather : Saveable
    {
        private int cellsPerSecond = 8;
        private Cart cart;
        private Direction direction;
        private Track curTrack;
        protected bool moving = false;

        public CartPather(Cart cart)
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

        private bool HasTrackToMoveTo(Direction movingDir)
        {
            Track t;
            if(curTrack!=null && curTrack.TrackInDirection(movingDir, out t))
            {
                return true;
            }
            return false;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue(ref moving, "moving");
        }
    }
}
