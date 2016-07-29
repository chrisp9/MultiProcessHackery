using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

       // [DllImport("user32.dll")]
        //static extern IntPtr DefWindowProc(IntPtr hWnd, WM uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);

        private WndProcDelegate _newDelegate;
        private WndProcDelegate _oldDelegate;

        private bool hasFailed = false;
        private IntPtr OverrideWndProc(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam)
        {
            return CallWindowProc(_ptr, hWnd, message, wParam, lParam);
        }

       //private IntPtr SetWindowProc(IntPtr hWnd, WndProcDelegate newWndProc)
       //{
       //    IntPtr newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
       //    IntPtr oldWndProcPtr;
       //
       //    oldWndProcPtr = SetWindowLongPtr(hWnd, -4, newWndProcPtr);
       //    var g = Marshal.GetLastWin32Error();
       //    return oldWndProcPtr;
       //}
       //// This helper static method is required because the 32-bit version of user32.dll does not contain this API
       //// (on any versions of Windows), so linking the method will fail at run-time. The bridge dispatches the request
       //// to the correct function (GetWindowLong in 32-bit mode and GetWindowLongPtr in 64-bit mode)
       //public IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
       //{
       //    if (IntPtr.Size == 8)
       //        return SetWindowLongPtr64(new HandleRef(this, hWnd), nIndex, dwNewLong);
       //    else
       //        return new IntPtr(SetWindowLong32(new HandleRef(this, hWnd), nIndex, dwNewLong.ToInt32()));
       //}
       //


        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < 200; i++)
            {
                var btn = new Button();

                btn.Click += (_, __) =>
                {
                    while (true)
                    {

                    }
                };
                Panel.Children.Add(btn);
            }

        }

        private WndProcDelegate _delegate;
        private IntPtr _ptr;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void OnDispatcher(Action a)
        {
            Application.Current.Dispatcher.BeginInvoke(a);
        }
    }
}
