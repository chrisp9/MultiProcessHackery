using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Host
{
    public class ChildWindowHost : HwndHost
    {
        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateWindowEx(int dwExStyle,
                                                      string lpszClassName,
                                                      string lpszWindowName,
                                                      int style,
                                                      int x, int y,
                                                      int width, int height,
                                                      IntPtr hwndParent,
                                                      IntPtr hMenu,
                                                      IntPtr hInst,
                                                      [MarshalAs(UnmanagedType.AsAny)] object pvParam);


        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        private static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        [DllImport("user32.dll")]
        static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        public IntPtr ChildWindowHandle { get; private set; }

        public HandleRef ChildWindowHandleRef { get; private set; }

        public ChildWindowHost()
        {
            this.MessageHook += ChildWindowHost_MessageHook;
        }

        private IntPtr ChildWindowHost_MessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = true;
            return new IntPtr(1);
        }

        // This helper static method is required because the 32-bit version of user32.dll does not contain this API
        // (on any versions of Windows), so linking the method will fail at run-time. The bridge dispatches the request
        // to the correct function (GetWindowLong in 32-bit mode and GetWindowLongPtr in 64-bit mode)
        public IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(new HandleRef(this, hWnd), nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(new HandleRef(this, hWnd), nIndex, dwNewLong.ToInt32()));
        }



        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

     //   [DllImport("user32.dll")]
     //   static extern IntPtr DefWindowProc(IntPtr hWnd, WM uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam);

        private WndProcDelegate _newDelegate;
        private IntPtr _oldDelegate;

        private IntPtr SetWindowProc(IntPtr hWnd, WndProcDelegate newWndProc)
        {
            IntPtr newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
            IntPtr oldWndProcPtr;

            oldWndProcPtr = SetWindowLongPtr(hWnd, -4, newWndProcPtr);
            var g = Marshal.GetLastWin32Error();
            return oldWndProcPtr;
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var process = System.Diagnostics.Process.Start(@"G:\src\OutOfProcessArchitecture\MultiProcessArchitecture\Client\bin\Debug\Client.exe");
            while(process.MainWindowHandle == IntPtr.Zero)
            {
            }

            ChildWindowHandle = process.MainWindowHandle;

            long style = GetWindowLong(process.MainWindowHandle, Constants.GWL_STYLE);
            style = style & ~((int)Constants.WS_CAPTION) & ~((int)Constants.WS_THICKFRAME); // Removes Caption bar and the sizing border
            style |= ((int)Constants.WS_CHILD); // Must be a child window to be hosted
            style |= (Constants.WS_CLIPCHILDREN);

            SetWindowLongPtr(process.MainWindowHandle, Constants.GWL_STYLE, new IntPtr(style));
            SetWindowLongPtr(process.MainWindowHandle, Constants.GWL_HWNDPARENT, process.MainWindowHandle);

            InvalidateVisual();

            return new HandleRef(this, base.Handle);
        }


         [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        //private IntPtr OverrideWndProc(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam)
        //{
        //    if (!hWnd.Equals(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle))
        //    {
        //        // _oldDelegate(hWnd, message, wParam, lParam);
        //        PostMessage(hWnd, (uint)message, wParam, lParam);
        //        return new IntPtr(1);
        //
        //    } else
        //    {
        //        Task.Run(() =>
        //        {
        //            CallWindowProc(_oldDelegate, hWnd, (uint)message, wParam, lParam);
        //        }).Wait();
        //        
        //        //PostMessage(hWnd, (uint)message, wParam, lParam);
        //        return new IntPtr(0);
        //    }
        //}
    
        protected override void DestroyWindowCore(HandleRef hwnd)
        {
        }
    }
}
