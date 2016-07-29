using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace Host
{
    public class WindowsFormsHostSubclass : WindowsFormsHost
    {
        public IntPtr ChildWindowHandle { get; set; }
        public Action Render { get; set; }
        private int i = 0;

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            ++i;

            if (i % 50 == 0)
                Console.WriteLine(i);

       //     Render();
            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
        //    Render();
            base.OnRender(drawingContext);
        }
    }

 }
