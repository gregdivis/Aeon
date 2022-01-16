using System.Windows;
using System.Windows.Media;

namespace Aeon.Emulator.Launcher.Presentation
{
    /// <summary>
    /// Contains predefined geometries.
    /// </summary>
    public static class Geometries
    {
        /// <summary>
        /// Geometry of the default DOS mouse arrow.
        /// </summary>
        public static readonly Geometry Arrow = BuildArrow();

        /// <summary>
        /// Returns a new geometry containing the default DOS mouse arrow.
        /// </summary>
        /// <returns>New DOS mouse arrow geometry.</returns>
        private static StreamGeometry BuildArrow()
        {
            var g = new StreamGeometry();
            using (var context = g.Open())
            {
                context.BeginFigure(new Point(0, 0), true, true);
                context.LineTo(new Point(0, 14), true, true);
                context.LineTo(new Point(3, 11), true, true);
                context.LineTo(new Point(6, 15), true, true);
                context.LineTo(new Point(8, 14), true, true);
                context.LineTo(new Point(6, 10), true, true);
                context.LineTo(new Point(9, 9), true, true);
            }

            g.Freeze();
            return g;
        }
    }
}
