﻿using EarTrumpet.Services;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EarTrumpet.Extensions
{
    internal static class WindowExtensions
    {
        public static void ShowwithAnimation(this Window window, Action completed)
        {
            const int animationOffset = 25;

            var onCompleted = new EventHandler((s, e) =>
            {
                window.Opacity = 1;
                window.Topmost = true;
                window.Focus();
                completed();
            });

            window.Topmost = false;
            window.Activate();

            if (!SystemParameters.MenuAnimation)
            {
                window.Visibility = Visibility.Visible;
                onCompleted(null, null);
                return;
            }

            var moveAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(266)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                From = 0,
                To = 1
            };
            Storyboard.SetTarget(fadeAnimation, window);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

            var taskbarPosition = TaskbarService.GetWinTaskbarState().TaskbarPosition;

            switch (taskbarPosition)
            {
                case TaskbarPosition.Left:
                    moveAnimation.To = window.Left;
                    window.Left -= animationOffset;
                    break;
                case TaskbarPosition.Right:
                    moveAnimation.To = window.Left;
                    window.Left += animationOffset;
                    break;
                case TaskbarPosition.Top:
                    moveAnimation.To = window.Top;
                    window.Top -= animationOffset;
                    break;
                case TaskbarPosition.Bottom:
                default:
                    moveAnimation.To = window.Top;
                    window.Top += animationOffset;
                    break;
            }

            if (taskbarPosition == TaskbarPosition.Left || taskbarPosition == TaskbarPosition.Right)
            {
                Storyboard.SetTarget(moveAnimation, window);
                Storyboard.SetTargetProperty(moveAnimation, new PropertyPath(Window.LeftProperty));
                moveAnimation.From = window.Left;
            }
            else
            {
                Storyboard.SetTarget(moveAnimation, window);
                Storyboard.SetTargetProperty(moveAnimation, new PropertyPath(Window.TopProperty));
                moveAnimation.From = window.Top;
            }

            //window.Opacity = 0;
            window.Visibility = Visibility.Visible;

            var storyboard = new Storyboard();
            storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Children.Add(moveAnimation);
            //storyboard.Children.Add(fadeAnimation);
            storyboard.Completed += onCompleted;
            storyboard.Begin(window);
        }

        public static Matrix CalculateDpiFactors(this Window window)
        {
            var mainWindowPresentationSource = PresentationSource.FromVisual(window);
            return mainWindowPresentationSource == null ? new Matrix() { M11 = 1, M22 = 1 } : mainWindowPresentationSource.CompositionTarget.TransformToDevice;
        }

        public static double DpiHeightFactor(this Window window)
        {
            var m = CalculateDpiFactors(window);
            return m.M22;
        }

        public static double DpiWidthFactor(this Window window)
        {
            var m = CalculateDpiFactors(window);
            return m.M11;
        }

        static class Interop
        {
            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(
                IntPtr hWnd,
                IntPtr hWndInsertAfter,
                int X,
                int Y,
                int cx,
                int cy,
                uint uFlags);

            public const UInt32 SWP_NOSIZE = 0x0001;
            public const UInt32 SWP_NOMOVE = 0x0002;
            public const UInt32 SWP_NOZORDER = 0x0004;
        }

        public static void Move(this Window window, double top, double left, double height, double width)
        {
            Interop.SetWindowPos(new WindowInteropHelper(window).Handle, IntPtr.Zero, (int)left, (int)top, (int)width, (int)height, Interop.SWP_NOZORDER);
        }
    }
}
