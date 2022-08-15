﻿using ManagedShell.Common.Helpers;
using ManagedShell.Interop;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ModernBar.Controls
{
    /// <summary>
    /// Interaction logic for FloatingStartButton.xaml
    /// </summary>
    public partial class FloatingStartButton : Window
    {
        private WindowInteropHelper helper;
        private IntPtr handle;

        public FloatingStartButton(StartButton mainButton, Point position, Size size)
        {
            Owner = mainButton.Host;
            DataContext = mainButton;

            InitializeComponent();

            SetPosition(position, size);

            // Render the existing start button control as the ViewRect fill
            VisualBrush visualBrush = new VisualBrush(mainButton.Start);
            ViewRect.Fill = visualBrush;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // set up helper and get handle
            helper = new WindowInteropHelper(this);
            handle = helper.Handle;

            // set up window procedure
            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(WndProc);

            // Makes click-through by adding transparent style
            NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE, NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE) | (int)NativeMethods.ExtendedWindowStyles.WS_EX_TOOLWINDOW | (int)NativeMethods.ExtendedWindowStyles.WS_EX_TRANSPARENT);

            WindowHelper.ExcludeWindowFromPeek(helper.Handle);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Make transparent to hit tests
            if (msg == (int)NativeMethods.WM.NCHITTEST)
            {
                handled = true;
                return (IntPtr)(-1);
            }

            handled = false;
            return IntPtr.Zero;
        }

        internal void SetPosition(Point position, Size size)
        {
            Visibility = Visibility.Hidden;

            if (FlowDirection == FlowDirection.LeftToRight)
            {
                Left = position.X;
            }
            else
            {
                Left = position.X - size.Width;
            }
            
            Top = position.Y;
            Width = size.Width;
            Height = size.Height;

            Visibility = Visibility.Visible;
        }
    }
}
