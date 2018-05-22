using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageRotator.Models
{
    public enum FirstPointPosition
    {
        LeftUp,
        LeftDown,
        RightUp,
        RightDown
    }

    public static class Position
    {
        public static FirstPointPosition Get(Point pointA, Point pointB)
        {
            if (pointA.X - pointB.X < 0 && pointA.Y - pointB.Y < 0)
            {
                return FirstPointPosition.LeftUp;
            }
            else
            {
                if (pointA.X - pointB.X < 0 && pointA.Y - pointB.Y > 0)
                {
                    return FirstPointPosition.LeftDown;
                }
                else
                {
                    if (pointA.X - pointB.X > 0 && pointA.Y - pointB.Y < 0)
                    {
                        return FirstPointPosition.RightUp;
                    }
                }
            }
            return FirstPointPosition.RightDown;
        }
    }
}
