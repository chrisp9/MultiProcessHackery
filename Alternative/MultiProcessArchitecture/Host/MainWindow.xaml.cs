using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;

namespace Host
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        private Dispatcher _dispatcher;
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
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam);

        private IntPtr _handle;
        private WindowsFormsHostSubclass Host;

        /// <summary>
        ///     Installs an application-defined hook procedure into a hook chain. You would install a hook procedure to monitor the
        ///     system for certain types of events. These events are associated either with a specific thread or with all threads
        ///     in the same desktop as the calling thread.
        ///     <para>See https://msdn.microsoft.com/en-us/library/windows/desktop/ms644990%28v=vs.85%29.aspx for more information</para>
        /// </summary>
        /// <param name="hookType">
        ///     C++ ( idHook [in]. Type: int )<br />The type of hook procedure to be installed. This parameter can be one of the
        ///     following values.
        ///     <list type="table">
        ///     <listheader>
        ///         <term>Possible Hook Types</term>
        ///     </listheader>
        ///     <item>
        ///         <term>WH_CALLWNDPROC (4)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors messages before the system sends them to the
        ///         destination window procedure. For more information, see the CallWndProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_CALLWNDPROCRET (12)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors messages after they have been processed by the
        ///         destination window procedure. For more information, see the CallWndRetProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_CBT (5)</term>
        ///         <description>
        ///         Installs a hook procedure that receives notifications useful to a CBT application. For more
        ///         information, see the CBTProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_DEBUG (9)</term>
        ///         <description>
        ///         Installs a hook procedure useful for debugging other hook procedures. For more information,
        ///         see the DebugProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_FOREGROUNDIDLE (11)</term>
        ///         <description>
        ///         Installs a hook procedure that will be called when the application's foreground thread is
        ///         about to become idle. This hook is useful for performing low priority tasks during idle time. For more
        ///         information, see the ForegroundIdleProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_GETMESSAGE (3)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors messages posted to a message queue. For more
        ///         information, see the GetMsgProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_JOURNALPLAYBACK (1)</term>
        ///         <description>
        ///         Installs a hook procedure that posts messages previously recorded by a WH_JOURNALRECORD hook
        ///         procedure. For more information, see the JournalPlaybackProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_JOURNALRECORD (0)</term>
        ///         <description>
        ///         Installs a hook procedure that records input messages posted to the system message queue. This
        ///         hook is useful for recording macros. For more information, see the JournalRecordProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_KEYBOARD (2)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors keystroke messages. For more information, see the
        ///         KeyboardProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_KEYBOARD_LL (13)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors low-level keyboard input events. For more information,
        ///         see the LowLevelKeyboardProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_MOUSE (7)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors mouse messages. For more information, see the
        ///         MouseProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_MOUSE_LL (14)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors low-level mouse input events. For more information,
        ///         see the LowLevelMouseProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_MSGFILTER (-1)</term>
        ///         <description>
        ///         Installs a hook procedure that monitors messages generated as a result of an input event in a
        ///         dialog box, message box, menu, or scroll bar. For more information, see the MessageProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_SHELL (10)</term>
        ///         <description>
        ///         Installs a hook procedure that receives notifications useful to shell applications. For more
        ///         information, see the ShellProc hook procedure.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>WH_SYSMSGFILTER (6)</term><description></description>
        ///     </item>
        ///     </list>
        /// </param>
        /// <param name="lpfn">
        ///     C++ ( lpfn [in]. Type: HOOKPROC )<br />A pointer to the hook procedure. If the dwThreadId parameter
        ///     is zero or specifies the identifier of a thread created by a different process, the lpfn parameter must point to a
        ///     hook procedure in a DLL. Otherwise, lpfn can point to a hook procedure in the code associated with the current
        ///     process.
        /// </param>
        /// <param name="hMod">
        ///     C++ ( hMod [in]. Type: HINSTANCE )<br />A handle to the DLL containing the hook procedure pointed to
        ///     by the lpfn parameter. The hMod parameter must be set to NULL if the dwThreadId parameter specifies a thread
        ///     created by the current process and if the hook procedure is within the code associated with the current process.
        /// </param>
        /// <param name="dwThreadId">
        ///     C++ ( dwThreadId [in]. Type: DWORD )<br />The identifier of the thread with which the hook
        ///     procedure is to be associated. For desktop apps, if this parameter is zero, the hook procedure is associated with
        ///     all existing threads running in the same desktop as the calling thread. For Windows Store apps, see the Remarks
        ///     section.
        /// </param>
        /// <returns>
        ///     C++ ( Type: HHOOK )<br />If the function succeeds, the return value is the handle to the hook procedure. If
        ///     the function fails, the return value is NULL.
        ///     <para>To get extended error information, call GetLastError.</para>
        /// </returns>
        /// <remarks>
        ///     <para>
        ///     SetWindowsHookEx can be used to inject a DLL into another process. A 32-bit DLL cannot be injected into a
        ///     64-bit process, and a 64-bit DLL cannot be injected into a 32-bit process. If an application requires the use
        ///     of hooks in other processes, it is required that a 32-bit application call SetWindowsHookEx to inject a 32-bit
        ///     DLL into 32-bit processes, and a 64-bit application call SetWindowsHookEx to inject a 64-bit DLL into 64-bit
        ///     processes. The 32-bit and 64-bit DLLs must have different names.
        ///     </para>
        ///     <para>
        ///     Because hooks run in the context of an application, they must match the "bitness" of the application. If a
        ///     32-bit application installs a global hook on 64-bit Windows, the 32-bit hook is injected into each 32-bit
        ///     process (the usual security boundaries apply). In a 64-bit process, the threads are still marked as "hooked."
        ///     However, because a 32-bit application must run the hook code, the system executes the hook in the hooking app's
        ///     context; specifically, on the thread that called SetWindowsHookEx. This means that the hooking application must
        ///     continue to pump messages or it might block the normal functioning of the 64-bit processes.
        ///     </para>
        ///     <para>
        ///     If a 64-bit application installs a global hook on 64-bit Windows, the 64-bit hook is injected into each
        ///     64-bit process, while all 32-bit processes use a callback to the hooking application.
        ///     </para>
        ///     <para>
        ///     To hook all applications on the desktop of a 64-bit Windows installation, install a 32-bit global hook and a
        ///     64-bit global hook, each from appropriate processes, and be sure to keep pumping messages in the hooking
        ///     application to avoid blocking normal functioning. If you already have a 32-bit global hooking application and
        ///     it doesn't need to run in each application's context, you may not need to create a 64-bit version.
        ///     </para>
        ///     <para>
        ///     An error may occur if the hMod parameter is NULL and the dwThreadId parameter is zero or specifies the
        ///     identifier of a thread created by another process.
        ///     </para>
        ///     <para>
        ///     Calling the CallNextHookEx function to chain to the next hook procedure is optional, but it is highly
        ///     recommended; otherwise, other applications that have installed hooks will not receive hook notifications and
        ///     may behave incorrectly as a result. You should call CallNextHookEx unless you absolutely need to prevent the
        ///     notification from being seen by other applications.
        ///     </para>
        ///     <para>
        ///     Before terminating, an application must call the UnhookWindowsHookEx function to free system resources
        ///     associated with the hook.
        ///     </para>
        ///     <para>
        ///     The scope of a hook depends on the hook type. Some hooks can be set only with global scope; others can also
        ///     be set for only a specific thread, as shown in the following table.
        ///     </para>
        ///     <list type="table">
        ///     <listheader>
        ///         <term>Possible Hook Types</term>
        ///     </listheader>
        ///     <item>
        ///         <term>WH_CALLWNDPROC (4)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_CALLWNDPROCRET (12)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_CBT (5)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_DEBUG (9)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_FOREGROUNDIDLE (11)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_GETMESSAGE (3)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_JOURNALPLAYBACK (1)</term>
        ///         <description>Global only</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_JOURNALRECORD (0)</term>
        ///         <description>Global only</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_KEYBOARD (2)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_KEYBOARD_LL (13)</term>
        ///         <description>Global only</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_MOUSE (7)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_MOUSE_LL (14)</term>
        ///         <description>Global only</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_MSGFILTER (-1)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_SHELL (10)</term>
        ///         <description>Thread or global</description>
        ///     </item>
        ///     <item>
        ///         <term>WH_SYSMSGFILTER (6)</term>
        ///         <description>Global only</description>
        ///     </item>
        ///     </list>
        ///     <para>
        ///     For a specified hook type, thread hooks are called first, then global hooks. Be aware that the WH_MOUSE,
        ///     WH_KEYBOARD, WH_JOURNAL*, WH_SHELL, and low-level hooks can be called on the thread that installed the hook
        ///     rather than the thread processing the hook. For these hooks, it is possible that both the 32-bit and 64-bit
        ///     hooks will be called if a 32-bit hook is ahead of a 64-bit hook in the hook chain.
        ///     </para>
        ///     <para>
        ///     The global hooks are a shared resource, and installing one affects all applications in the same desktop as
        ///     the calling thread. All global hook functions must be in libraries. Global hooks should be restricted to
        ///     special-purpose applications or to use as a development aid during application debugging. Libraries that no
        ///     longer need a hook should remove its hook procedure.
        ///     </para>
        ///     <para>
        ///     Windows Store app development If dwThreadId is zero, then window hook DLLs are not loaded in-process for the
        ///     Windows Store app processes and the Windows Runtime broker process unless they are installed by either UIAccess
        ///     processes (accessibility tools). The notification is delivered on the installer's thread for these hooks:
        ///     </para>
        ///     <list type="bullet">
        ///     <item>
        ///         <term>WH_JOURNALPLAYBACK</term>
        ///     </item>
        ///     <item>
        ///         <term>WH_JOURNALRECORD </term>
        ///     </item>
        ///     <item>
        ///         <term>WH_KEYBOARD </term>
        ///     </item>
        ///     <item>
        ///         <term>WH_KEYBOARD_LL </term>
        ///     </item>
        ///     <item>
        ///         <term>WH_MOUSE </term>
        ///     </item>
        ///     <item>
        ///         <term>WH_MOUSE_LL </term>
        ///     </item>
        ///     </list>
        ///     <para>
        ///     This behavior is similar to what happens when there is an architecture mismatch between the hook DLL and the
        ///     target application process, for example, when the hook DLL is 32-bit and the application process 64-bit.
        ///     </para>
        ///     <para>
        ///     For an example, see Installing and
        ///     <see
        ///         cref="!:https://msdn.microsoft.com/en-us/library/windows/desktop/ms644960%28v=vs.85%29.aspx#installing_releasing">
        ///         Releasing
        ///         Hook Procedures.
        ///     </see>
        ///     [
        ///     https://msdn.microsoft.com/en-us/library/windows/desktop/ms644960%28v=vs.85%29.aspx#installing_releasing ]
        ///     </para>
        /// </remarks>
        /// 
        delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);
        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct tagCWPSTRUCT
        {
            public IntPtr lParam;
            public IntPtr wParam;
            public uint message;
            public IntPtr hwnd;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }
        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError =true)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        private HookProc _delegate;

        private IntPtr MyHookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            var g =(tagCWPSTRUCT) Marshal.PtrToStructure(lParam, typeof(tagCWPSTRUCT));

            if (g.message == 70 || g.message == 71)
            {
                var h = (WINDOWPOS)Marshal.PtrToStructure(g.lParam, typeof(WINDOWPOS));
                //var ggg = SetWindowPos( _handle, System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle, h.x, h.y, h.cx, h.cy, SWP.NOMOVE | SWP.NOSIZE);

                if(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle == IntPtr.Zero)
                {
                    return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

                }

                var zorderPos = new WINDOWPOS
                {
                    hwnd = _handle,
                    hwndInsertAfter = h.hwndInsertAfter,
                    x = h.x,
                    y = h.y,
                    cx = h.cx,
                    cy = h.cy,
                    flags = h.flags
                };
                var allocPtr = Marshal.AllocHGlobal(Marshal.SizeOf(zorderPos));

                Point locationFromWindow = b.TranslatePoint(new Point(0, 0), this);

                Point locationFromScreen = b.PointToScreen(locationFromWindow);

                Marshal.StructureToPtr(zorderPos, allocPtr, false);
                Task.Run(() =>
                {
                    SetWindowPos(_handle, h.hwnd, (int)locationFromScreen.X, (int)locationFromScreen.Y, (int)b.ActualWidth, (int)b.ActualHeight, (int)h.flags);
                });
                    //  var hgg = SendMessage(_handle, g.message, IntPtr.Zero, allocPtr);
              //  var err = Marshal.GetLastWin32Error();
                //you need to call CallNextHookEx without further processing
                //and return the value returned by CallNextHookEx
                return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
            }
            // we can convert the 2nd parameter (the key code) to a System.Windows.Forms.Keys enum constant
            //return the value returned by CallNextHookEx
            return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        private Process p;


        public MainWindow()
        {
            InitializeComponent();
            var process = System.Diagnostics.Process.Start(@"G:\src\OutOfProcessArchitecture\MultiProcessArchitecture\Client\bin\Debug\Client.exe");
            while (process.MainWindowHandle == IntPtr.Zero)
            {

            }
            p = process;
            _delegate = MyHookProc;

            this.LocationChanged += MainWindow_LocationChanged;
            //this.Deactivated += MainWindow_Deactivated;
#pragma warning disable CS0618 // Type or member is obsolete
            var i =  SetWindowsHookEx(HookType.WH_CALLWNDPROC, _delegate, IntPtr.Zero, (uint)AppDomain.GetCurrentThreadId());
#pragma warning restore CS0618 // Type or member is obsolete
            var h = Marshal.GetLastWin32Error();
            //Host = new WindowsFormsHostSubclass();
            //this.AddChild(Host);
           // var panel = new System.Windows.Forms.Panel();
            //Host.Child = panel;
            //panel.BackColor = System.Drawing.Color.Red;

            _handle = process.MainWindowHandle;
          //  ChildWindowHandle = process.MainWindowHandle;

            long style = GetWindowLong(process.MainWindowHandle, Constants.GWL_STYLE);
            style = style & ~((int)Constants.WS_CAPTION) & ~((int)Constants.WS_THICKFRAME); // Removes Caption bar and the sizing border
            style |= ((int)Constants.WS_CHILD); // Must be a child window to be hosted
            style |= (Constants.WS_CLIPCHILDREN);

            SetWindowLongPtr(process.MainWindowHandle, Constants.GWL_STYLE, new IntPtr(style));
            //var g = SetWindowLongPtr(process.MainWindowHandle, Constants.GWL_HWNDPARENT, System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);

            InvalidateVisual();
        }

        //private void MainWindow_Deactivated(object sender, EventArgs e)
        //{
        //    Point locationFromWindow = Host.TranslatePoint(new Point(0, 0), this);
        //    Point locationFromScreen = Host.PointToScreen(locationFromWindow);
        //    //
        //    var io = new WindowInteropHelper(this);
        //
        //    var handle = io.Handle;
        //
        //    Task.Run(() =>
        //    {
        //        RECT rect;
        //
        //        GetWindowRect(handle, out rect);
        //        SetWindowPos(_handle, IntPtr.Zero, (int)Host.ActualWidth, (int)Host.ActualHeight, rect.Left, rect.Top, SWP.NOACTIVATE);
        //        SetWindowPos(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle, _handle, 0, 0, 0, 0, SWP.NOMOVE | SWP.NOSIZE);
        //    });
        //}

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            //Point locationFromWindow = Host.TranslatePoint(new Point(0, 0), this);
            //Point locationFromScreen = Host.PointToScreen(locationFromWindow);
            ////
            //var io = new WindowInteropHelper(this);
            //
            //var handle = io.Handle;
            //
            //Task.Run(() =>
            //{
            //    RECT rect;
            //
            //    GetWindowRect(handle, out rect);
            //    //SetWindowPos(_handle, IntPtr.Zero, (int)Host.ActualWidth, (int)Host.ActualHeight, rect.Left, rect.Top, SWP.NOACTIVATE);
            //    SetWindowPos(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle, _handle, 0, 0, 0, 0, SWP.NOMOVE | SWP.NOSIZE);
            //});
        }



        /// <summary>
        ///     Changes the size, position, and Z order of a child, pop-up, or top-level window. These windows are ordered
        ///     according to their appearance on the screen. The topmost window receives the highest rank and is the first window
        ///     in the Z order.
        ///     <para>See https://msdn.microsoft.com/en-us/library/windows/desktop/ms633545%28v=vs.85%29.aspx for more information.</para>
        /// </summary>
        /// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br />A handle to the window.</param>
        /// <param name="hWndInsertAfter">
        ///     C++ ( hWndInsertAfter [in, optional]. Type: HWND )<br />A handle to the window to precede the positioned window in
        ///     the Z order. This parameter must be a window handle or one of the following values.
        ///     <list type="table">
        ///     <itemheader>
        ///         <term>HWND placement</term><description>Window to precede placement</description>
        ///     </itemheader>
        ///     <item>
        ///         <term>HWND_BOTTOM ((HWND)1)</term>
        ///         <description>
        ///         Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost
        ///         window, the window loses its topmost status and is placed at the bottom of all other windows.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>HWND_NOTOPMOST ((HWND)-2)</term>
        ///         <description>
        ///         Places the window above all non-topmost windows (that is, behind all topmost windows). This
        ///         flag has no effect if the window is already a non-topmost window.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>HWND_TOP ((HWND)0)</term><description>Places the window at the top of the Z order.</description>
        ///     </item>
        ///     <item>
        ///         <term>HWND_TOPMOST ((HWND)-1)</term>
        ///         <description>
        ///         Places the window above all non-topmost windows. The window maintains its topmost position
        ///         even when it is deactivated.
        ///         </description>
        ///     </item>
        ///     </list>
        ///     <para>For more information about how this parameter is used, see the following Remarks section.</para>
        /// </param>
        /// <param name="X">C++ ( X [in]. Type: int )<br />The new position of the left side of the window, in client coordinates.</param>
        /// <param name="Y">C++ ( Y [in]. Type: int )<br />The new position of the top of the window, in client coordinates.</param>
        /// <param name="cx">C++ ( cx [in]. Type: int )<br />The new width of the window, in pixels.</param>
        /// <param name="cy">C++ ( cy [in]. Type: int )<br />The new height of the window, in pixels.</param>
        /// <param name="uFlags">
        ///     C++ ( uFlags [in]. Type: UINT )<br />The window sizing and positioning flags. This parameter can be a combination
        ///     of the following values.
        ///     <list type="table">
        ///     <itemheader>
        ///         <term>HWND sizing and positioning flags</term>
        ///         <description>Where to place and size window. Can be a combination of any</description>
        ///     </itemheader>
        ///     <item>
        ///         <term>SWP_ASYNCWINDOWPOS (0x4000)</term>
        ///         <description>
        ///         If the calling thread and the thread that owns the window are attached to different input
        ///         queues, the system posts the request to the thread that owns the window. This prevents the calling
        ///         thread from blocking its execution while other threads process the request.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_DEFERERASE (0x2000)</term>
        ///         <description>Prevents generation of the WM_SYNCPAINT message. </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_DRAWFRAME (0x0020)</term>
        ///         <description>Draws a frame (defined in the window's class description) around the window.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_FRAMECHANGED (0x0020)</term>
        ///         <description>
        ///         Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message
        ///         to the window, even if the window's size is not being changed. If this flag is not specified,
        ///         WM_NCCALCSIZE is sent only when the window's size is being changed
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_HIDEWINDOW (0x0080)</term><description>Hides the window.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOACTIVATE (0x0010)</term>
        ///         <description>
        ///         Does not activate the window. If this flag is not set, the window is activated and moved to
        ///         the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter
        ///         parameter).
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOCOPYBITS (0x0100)</term>
        ///         <description>
        ///         Discards the entire contents of the client area. If this flag is not specified, the valid
        ///         contents of the client area are saved and copied back into the client area after the window is sized or
        ///         repositioned.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOMOVE (0x0002)</term>
        ///         <description>Retains the current position (ignores X and Y parameters).</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOOWNERZORDER (0x0200)</term>
        ///         <description>Does not change the owner window's position in the Z order.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOREDRAW (0x0008)</term>
        ///         <description>
        ///         Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies
        ///         to the client area, the nonclient area (including the title bar and scroll bars), and any part of the
        ///         parent window uncovered as a result of the window being moved. When this flag is set, the application
        ///         must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOREPOSITION (0x0200)</term><description>Same as the SWP_NOOWNERZORDER flag.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOSENDCHANGING (0x0400)</term>
        ///         <description>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOSIZE (0x0001)</term>
        ///         <description>Retains the current size (ignores the cx and cy parameters).</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOZORDER (0x0004)</term>
        ///         <description>Retains the current Z order (ignores the hWndInsertAfter parameter).</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_SHOWWINDOW (0x0040)</term><description>Displays the window.</description>
        ///     </item>
        ///     </list>
        /// </param>
        /// <returns><c>true</c> or nonzero if the function succeeds, <c>false</c> or zero otherwise or if function fails.</returns>
        /// <remarks>
        ///     <para>
        ///     As part of the Vista re-architecture, all services were moved off the interactive desktop into Session 0.
        ///     hwnd and window manager operations are only effective inside a session and cross-session attempts to manipulate
        ///     the hwnd will fail. For more information, see The Windows Vista Developer Story: Application Compatibility
        ///     Cookbook.
        ///     </para>
        ///     <para>
        ///     If you have changed certain window data using SetWindowLong, you must call SetWindowPos for the changes to
        ///     take effect. Use the following combination for uFlags: SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER |
        ///     SWP_FRAMECHANGED.
        ///     </para>
        ///     <para>
        ///     A window can be made a topmost window either by setting the hWndInsertAfter parameter to HWND_TOPMOST and
        ///     ensuring that the SWP_NOZORDER flag is not set, or by setting a window's position in the Z order so that it is
        ///     above any existing topmost windows. When a non-topmost window is made topmost, its owned windows are also made
        ///     topmost. Its owners, however, are not changed.
        ///     </para>
        ///     <para>
        ///     If neither the SWP_NOACTIVATE nor SWP_NOZORDER flag is specified (that is, when the application requests that
        ///     a window be simultaneously activated and its position in the Z order changed), the value specified in
        ///     hWndInsertAfter is used only in the following circumstances.
        ///     </para>
        ///     <list type="bullet">
        ///     <item>Neither the HWND_TOPMOST nor HWND_NOTOPMOST flag is specified in hWndInsertAfter. </item>
        ///     <item>The window identified by hWnd is not the active window. </item>
        ///     </list>
        ///     <para>
        ///     An application cannot activate an inactive window without also bringing it to the top of the Z order.
        ///     Applications can change an activated window's position in the Z order without restrictions, or it can activate
        ///     a window and then move it to the top of the topmost or non-topmost windows.
        ///     </para>
        ///     <para>
        ///     If a topmost window is repositioned to the bottom (HWND_BOTTOM) of the Z order or after any non-topmost
        ///     window, it is no longer topmost. When a topmost window is made non-topmost, its owners and its owned windows
        ///     are also made non-topmost windows.
        ///     </para>
        ///     <para>
        ///     A non-topmost window can own a topmost window, but the reverse cannot occur. Any window (for example, a
        ///     dialog box) owned by a topmost window is itself made a topmost window, to ensure that all owned windows stay
        ///     above their owner.
        ///     </para>
        ///     <para>
        ///     If an application is not in the foreground, and should be in the foreground, it must call the
        ///     SetForegroundWindow function.
        ///     </para>
        ///     <para>
        ///     To use SetWindowPos to bring a window to the top, the process that owns the window must have
        ///     SetForegroundWindow permission.
        ///     </para>
        /// </remarks>

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
        public static class SWP
        {
            public static readonly int
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;
        }

        private void Window_LayoutUpdated(object sender, EventArgs e)
        {
            //Point locationFromWindow = Host.TranslatePoint(new Point(0, 0), this);
            //Point locationFromScreen = Host.PointToScreen(locationFromWindow);
            //
            //SetWindowPos(_handle, System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle, (int)Host.ActualWidth, (int)Host.ActualHeight, (int)locationFromScreen.X, (int)locationFromScreen.Y, 0);
        }
    }
}
