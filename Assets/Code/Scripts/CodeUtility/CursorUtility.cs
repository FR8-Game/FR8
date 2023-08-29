using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FR8Runtime.CodeUtility
{
    public static class CursorUtility
    {
        public static void SetPosition(int x, int y)
        {
            SetCursorPos(x, y);
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);
    }
}