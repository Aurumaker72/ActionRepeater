﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ActionRepeater.Win32.Input;

namespace ActionRepeater.Win32;

public static partial class PInvoke
{
    public static class Helpers
    {
        /// <summary>
        /// Synthesize a mouse event.
        /// </summary>
        /// <param name="eventFlags">The mouse event to send.</param>
        /// <param name="data">
        /// If <paramref name="eventFlags"/> is <see cref="MOUSEEVENTF.WHEEL"/>, then <paramref name="data"/> specifies the amount of wheel movement (positive value -> forward). One wheel click is <i>120</i> (use <see cref="MOUSEINPUT.CalculateWheelDeltaUInt32"/> to calculate wheel movement).<br/><br/>
        /// <b>Windows Vista</b>: If <paramref name="eventFlags"/> contains <see cref="MOUSEEVENTF.HWHEEL"/>, then <paramref name="data"/> specifies the amount of horizontal wheel movement (positive value -> right).<br/><br/>
        /// If <paramref name="eventFlags"/> does not contain <see cref="MOUSEEVENTF.WHEEL"/>, <see cref="MOUSEEVENTF.XDOWN"/>, or <see cref="MOUSEEVENTF.XUP"/>, then <paramref name="data"/> should be <i>0</i> (zero).<br/><br/>
        /// If <paramref name="eventFlags"/> contains <see cref="MOUSEEVENTF.XDOWN"/> or <see cref="MOUSEEVENTF.MOUSEEVENTF_XUP"/>, then <paramref name="data"/> specifies which X buttons (mouse side buttons) were pressed or released (<see cref="MouseEventData"/>).
        /// <paramref name="data"/> may be any combination of <see cref="MOUSEINPUT.XBUTTON1"/> and <see cref="MOUSEINPUT.XBUTTON2"/>.
        /// </param>
        /// <returns>
        /// <typeparamref name="true"/> if the input was successfully sent, otherwise <typeparamref name="false"/>.
        /// </returns>
        public static bool SendMouseEvent(MOUSEEVENTF eventFlags, uint data = 0, int dx = 0, int dy = 0)
        {
            INPUT[] input = new[]
            {
                new INPUT
                {
                    type = INPUT.TYPE.MOUSE,
                    union = new INPUT.UNION
                    {
                        mi = new MOUSEINPUT
                        {
                            dx = dx,
                            dy = dy,
                            mouseData = data,
                            dwFlags = eventFlags,
                            time = 0
                        }
                    }
                }
            };

            return PInvoke.SendInput(input) == 1;
        }

        /// <summary>
        /// Synthesize a key event.
        /// </summary>
        /// <param name="eventFlags">Specifies various aspects of a keystroke. This can be certain combinations of <see cref="KEYEVENTF"/>.</param>
        /// <param name="scanCode">
        /// A hardware scan code for the key. If <paramref name="eventFlags"/> specifies <see cref="KEYEVENTF.UNICODE"/>, <paramref name="scanCode"/> specifies a Unicode character which is to be sent to the foreground application.<br/>
        /// This can be a <see cref="ScanCode"/> (enum), or the value returned by <see cref="PInvoke.MapVirtualKey"/>.
        /// </param>
        /// <returns>
        /// <typeparamref name="true"/> if the input was successfully sent, otherwise <typeparamref name="false"/>.
        /// </returns>
        public static bool SendKeyEvent(KEYEVENTF eventFlags, ushort scanCode)
        {
            INPUT[] input = new[]
            {
                new INPUT
                {
                    type = INPUT.TYPE.KEYBOARD,
                    union = new INPUT.UNION
                    {
                        ki = new KEYBDINPUT
                        {
                            wScan = scanCode,
                            dwFlags = eventFlags,
                            time = 0
                        }
                    }
                }
            };

            return PInvoke.SendInput(input) == 1;
        }

        /// <summary>
        /// Retrieves the position of the mouse cursor, in screen coordinates.
        /// </summary>
        /// <returns>The screen coordinates of the cursor.</returns>
        public static unsafe POINT GetCursorPos()
        {
            POINT result;
            if (!PInvoke.GetCursorPos(&result))
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            }
            return result;
        }

        [System.Runtime.Versioning.SupportedOSPlatform("windows6.0.6000")]
        public static unsafe void SetWindowImmersiveDarkMode(IntPtr hWnd, bool enabled)
        {
            const uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
            int isEnabled = enabled ? 1 : 0;
            int result = PInvoke.DwmSetWindowAttribute(hWnd, DWMWA_USE_IMMERSIVE_DARK_MODE, &isEnabled, sizeof(int));
            if (result != 0) throw new Win32Exception(result);
        }
    }
}
