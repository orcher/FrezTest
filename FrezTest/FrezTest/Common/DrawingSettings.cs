using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrezTest.Common
{
    class DrawingSettings
    {
        public enum Shape
        {
            Rectangle,
            Circle
        }
        public static bool enable = false;
        public static Shape shape = Shape.Rectangle;
        public static int rectWidth = 10;
        public static int rectHeight = 10;
        public static int circleRadius = 5;
    }

    class GlobalSettings
    {
        public static int frezRadius = 4;
        public static int defaultLayoutWidth = 500;
        public static int defaultLayoutHeight = 500;
    }
}
