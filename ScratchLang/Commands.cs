namespace ScratchLang
{
    public static class SL
    {
        public static void Compile()
        {
            Compiler.Compiler.Compile();
        }

        public static void Decompile(string[] arguments, bool argumentBool, string realCWD)
        {
            Decompiler.Decompiler.Decompile(arguments, argumentBool, realCWD);
        }

        public static void Edit(string rc, string e = "")
        {
            Editor.Editor.MainEditor(rc, e);
        }
    }

    public static class Functions
    {
        private const string RED = "\u001b[31m";
        private const string NC = "\u001b[0m";

        public static void Error(string message)
        {
            Console.WriteLine($"{RED}Error: {message}{NC}");
        }

        public static string GetInput()
        {
            return Console.ReadKey(true).KeyChar.ToString();
        }

        public static void WriteToFile(string fileName, string value)
        {
            if (File.Exists(fileName))
            {
                File.AppendAllText(fileName, $"\n{value}");
            }
            else
            {
                File.AppendAllText(fileName, value);
            }
        }
    }

    public static class GlobalVariables
    {
        public const string RED = "\u001b[31m";
        public const string NC = "\u001b[0m";
        public const string P = "\u001b[0;35m";

        public static Dictionary<byte, string> AsciiTable { get; set; } = new() // I had to type this ENTIRE DICTIONARY by hand. It doesn't seem that bad but it took a hell of a long time...
            {
                { 1, "Escape" },
                { 2, "1" },
                { 3, "2" },
                { 4, "3" },
                { 5, "4" },
                { 6, "5" },
                { 7, "6" },
                { 8, "7" },
                { 9, "8" },
                { 10, "9" },
                { 11, "0" },
                { 12, "-" },
                { 13, "=" },
                { 14, "Backspace" },
                { 15, "Tab" },
                { 16, "q" },
                { 17, "w" },
                { 18, "e" },
                { 19, "r" },
                { 20, "t" },
                { 21, "y" },
                { 22, "u" },
                { 23, "i" },
                { 24, "o" },
                { 25, "p" },
                { 26, "[" },
                { 27, "]" },
                { 28, "Enter" },
                { 29, "Control" },
                { 30, "a" },
                { 31, "s" },
                { 32, "d" },
                { 33, "f" },
                { 34, "g" },
                { 35, "h" },
                { 36, "j" },
                { 37, "k" },
                { 38, "l" },
                { 39, ";" },
                { 40, "'" },
                { 41, "`" },
                { 42, "LeftShift" },
                { 43, "\\" },
                { 44, "z" },
                { 45, "x" },
                { 46, "c" },
                { 47, "v" },
                { 48, "b" },
                { 49, "n" },
                { 50, "m" },
                { 51, "," },
                { 52, "." },
                { 53, "/" },
                { 54, "RightShift" },
                { 55, "PrtScn" },
                { 56, "Alt" },
                { 57, "Spacebar" },
                { 58, "CapsLock" },
                { 59, "F1" },
                { 60, "F2" },
                { 61, "F3" },
                { 62, "F4" },
                { 63, "F5" },
                { 64, "F6" },
                { 65, "F7" },
                { 66, "F8" },
                { 67, "F9" },
                { 68, "F10" },
                { 69, "NumLock" },
                { 70, "ScrlLock" },
                { 71, "Home" },
                { 72, "UpArrow" },
                { 73, "PageUp" },
                { 74, "NumHyphen" },
                { 75, "LeftArrow" },
                { 76, "NumFive" },
                { 77, "RightArrow" },
                { 78, "NumPlus" },
                { 79, "End" },
                { 80, "DownArrow" },
                { 81, "PageDown" },
                { 82, "Insert" },
                { 83, "Delete" },
                { 87, "F11" },
                { 88, "F12" },
                { 93, "Menu" }
            };
    }
}