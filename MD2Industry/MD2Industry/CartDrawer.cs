using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace MD2
{
    public class CartDrawer : Saveable
    {
        private Cart cart;

        public CartDrawer(Cart cart)
        {
            this.cart = cart;
        }

        public void ExposeData()
        {
        }

        public Vector3 DrawPos
        {
            get;
            set;
        }

        public virtual void DrawAt(Vector3 pos)
        {

        }
    }
}
