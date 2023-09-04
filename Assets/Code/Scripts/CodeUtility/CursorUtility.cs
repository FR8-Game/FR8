using System.Drawing;
using System.Runtime.InteropServices;

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

        public static (int lastCursorX, int lastCursorY) GetPosition()
        {
            GetCursorPos(out var p);
            return (p.X, p.Y);
        }
    }
}