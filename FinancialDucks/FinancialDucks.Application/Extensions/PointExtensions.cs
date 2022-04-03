using System.Drawing;

namespace FinancialDucks.Application.Extensions
{
    public static class PointExtensions
    {
        public static double GetAngleTo(this Point center, Point pt)
        {
            var vector = new Point(pt.X-center.X, pt.Y-center.Y);
            var angle = Math.Atan2(vector.X, vector.Y) * (180.0/Math.PI);

            if (angle < 0)
                angle += 360;

            return angle;
        }

        public static double DistanceTo(this Point p1, Point p2)
        {
            var x = p1.X - p2.X;
            var y = p1.Y - p2.Y;
            return Math.Sqrt(x*x + y*y);
        }
    }
}
