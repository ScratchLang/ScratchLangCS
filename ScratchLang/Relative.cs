using System.Runtime.InteropServices;

namespace ScratchLang
{
    public static partial class Relative
    {
        public static object[] Position(int offsetX = 0, int offsetY = 0)
        {
            // Returns:
            //
            // The X position of the mouse cursor relative to the console,
            // The Y position of the mouse cursor relative to the console,
            // A boolean that is true if the mouse pointer is in the console bounds,
            // The console height,
            // And the console width
            Rect dimensions = new();
            GetWindowRect(GetForegroundWindow(), ref dimensions);
            dimensions.Left += 8;
            dimensions.Top += 8 + offsetY;
            dimensions.Right += 8;
            dimensions.Bottom += 8;
            int width = dimensions.Right - dimensions.Left - 16 - offsetX;
            int height = dimensions.Bottom - dimensions.Top - 16;
            int relativeMX = Cursor.Position.X - dimensions.Left;
            int relativeMY = Cursor.Position.Y - dimensions.Top;
            bool inWindow = true;
            if (relativeMX < 0)
            {
                relativeMX = 0;
                inWindow = false;
            }
            if (relativeMX > width)
            {
                relativeMX = width;
                inWindow = false;
            }
            if (relativeMY < 0)
            {
                relativeMY = 0;
                inWindow = false;
            }
            if (relativeMY > height)
            {
                relativeMY = height;
                inWindow = false;
            }
            return new object[] { relativeMX, relativeMY, inWindow, height, width };
        }

        public static object[] CharacterPosition(int offsetX = 0, int offsetY = 0)
        {
            // Returns:
            //
            // The character X position of the mouse cursor relative to the console,
            // The character Y position of the mouse cursor relative to the console,
            // A boolean that is true if the mouse pointer is in the console bounds,
            // The console height,
            // And the console width
            object[] i = Position(offsetX, offsetY);
            double rY = Math.Floor(int.Parse(i[1].ToString()) / Math.Round((float)int.Parse(i[3].ToString()) / Console.WindowHeight));
            if (rY == Console.WindowHeight)
            {
                rY--;
            }
            return new object[] { Math.Floor(int.Parse(i[0].ToString()) / Math.Round((float)int.Parse(i[4].ToString()) / Console.WindowWidth)), rY, i[2], i[3], i[4] };
        }

        public static void Calibrate()
        {
            // todo
        }

        [LibraryImport("user32.dll")]
        private static partial IntPtr GetForegroundWindow();

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}