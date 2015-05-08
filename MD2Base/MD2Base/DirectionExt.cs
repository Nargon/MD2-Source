using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MD2
{
    public static class DirectionExt
    {
        public static Direction Opposite(this Direction dir)
        {
            switch (dir)
            {
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
