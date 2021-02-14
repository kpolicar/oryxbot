using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Bot;
using OryxBot.Client.Windows.Native;

namespace OryxBot.Client.Windows
{
    public partial class MainForm : Form
    {
        private Process? _pAlbion;
        private IntPtr _hWndDocked;
        private IntPtr _oldWndHandle;
        public Panel GameWindowPanel => gameWindowPanel;


        public MainForm() {
            InitializeComponent();
            // Load += MainWindow_OnLoaded;
            // Closing += MainWindow_OnClosing;
        }

        private void MainWindow_OnLoaded(object sender, EventArgs e) {
            BuildServices();
            
            _pAlbion = Process.GetProcessesByName("Albion-Online").FirstOrDefault();
            _oldWndHandle = DockProcess();

            RunRouteThread = new Thread(() => {
                while (IsHandleCreated) {
                    if (Running) 
                        Recenter();
                    Thread.Sleep(200);
                }
            });
            RunRouteThread.Start();
        }

        private void BuildServices() {
        }

        private void OnF1KeyPress() {
            Recording = !Recording;
        }

        private void OnEscapeKeyPress() {
            Running = !Running;
        }

        public bool Recording { get; set; }
        public bool Running { get; set; }

        public Thread RunRouteThread { get; set; }
        public Thread RecordRouteThread { get; set; }

        private void Recenter() {
            var rect = new User32.Rect();
            User32.GetWindowRect(_pAlbion.MainWindowHandle, ref rect);
            Invoke(new MethodInvoker(() => {
                var gameRect = gameWindowPanel.RectangleToScreen(new Rectangle(rect.Left, rect.Top-60, rect.Right - rect.Left,
                    rect.Bottom - rect.Top-60));
                
                // var center = new Point(gameRect.Width/2, gameRect.Height/2);
                // var targetX = center.X + MoveRequestHandler.MoveMouseTo.x * 200;
                // var targetY = center.Y + MoveRequestHandler.MoveMouseTo.y * 200;
                //
                //
                // var newCursorPos = gameWindowPanel.PointToScreen(new Point((int) targetX, (int) targetY));
                // User32.SetCursorPos(newCursorPos.X, newCursorPos.Y);
                // cursorIndicator.Location = new Point(x, y);
                
                // User32.PostMessage(_pAlbion.MainWindowHandle, User32.WM_LBUTTONDOWN, 0,
                //     User32.MakeLParam(x, y));
                // User32.PostMessage(_pAlbion.MainWindowHandle, User32.WM_LBUTTONUP, 0,
                //     User32.MakeLParam(x, y));
            }));
            
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e) {
            if (_pAlbion == null)
                return;
            UnDockProcess();
        }

        private void UnDockProcess() {
            if (_pAlbion == null)
                return;
            
            User32.SetParent(_pAlbion.MainWindowHandle, _oldWndHandle);
        }

        public IntPtr DockProcess() {
            if (_pAlbion == null)
                return IntPtr.Zero;
            while (_hWndDocked == IntPtr.Zero) {
                _pAlbion.WaitForInputIdle(1000);
                _pAlbion.Refresh();
                if (_pAlbion.HasExited)
                    return IntPtr.Zero;
                _hWndDocked = _pAlbion.MainWindowHandle;
            }

            var oldParentHandle = User32.SetParent(_hWndDocked, gameWindowPanel.Handle);

            EventHandler moveEventHandler = (sender, e) =>
                User32.MoveWindow(_hWndDocked, 0, 0, Width, Height, true);
            SizeChanged += moveEventHandler;
            moveEventHandler(null, EventArgs.Empty);

            return oldParentHandle;
        }

        public static void RemoveWindowBorders(IntPtr window) {
            var style = User32.GetWindowLong(window, User32.GWL_STYLE);
            User32.SetWindowLong(window, User32.GWL_STYLE, style & ~User32.WS_CAPTION);
        }

        private void OnMainFormClosing(object sender, CancelEventArgs e) {
            trayIcon.Dispose();
        }
    }
}
