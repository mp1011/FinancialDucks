using FinancialDucks.Application.Extensions;
using System.Drawing;
using System.Linq;
using Xunit;

namespace FinancialDucks.Tests.ExtensionTests
{
    public class PointExtensionsTests
    {
        [Theory]
        [InlineData("100,100", "100,0", 180.0)]
        [InlineData("100,100", "0,100", 270.0)]
        [InlineData("100,100", "200,100", 90)]
        [InlineData("100,100", "100,200", 0)]

        public void TestAngleTo(string pt1, string pt2, double expectedAngle)
        {
            Point p1 = StringToPoint(pt1);
            Point p2 = StringToPoint(pt2);

            var angle = p1.GetAngleTo(p2);
            Assert.Equal(expectedAngle, angle,2);
        }

        private Point StringToPoint(string pt)
        {
            var xy = pt.Split(',')
                       .Select(c => int.Parse(c))
                       .ToArray();
            return new Point(xy[0], xy[1]);
        }
    }
}
