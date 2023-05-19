using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace FileOrganizerUI.CodeBehind
{
    public class CategoryIconGenerator
    {
        public static Bitmap Make(int argb)
        {
            return Make(Color.FromArgb(argb));
        }

        public static Bitmap Make(Color color)
        {
            var icon = new Bitmap(12, 12);

            using (Graphics g = Graphics.FromImage(icon)) {
                g.Clear(color);
                g.DrawImage(
                    icon,
                    new Rectangle(Point.Empty, icon.Size),
                    0, 0, icon.Width, icon.Height,
                    GraphicsUnit.Pixel);
            }

            return icon;
        }
    }
}
