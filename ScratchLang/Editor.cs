using ScratchLang;
using System.Runtime.InteropServices;
using System.Text;
using static PInvoke.Kernel32;
using static ScratchLang.Functions;
using static ScratchLang.GlobalVariables;

namespace Editor
{
    public static partial class Editor
    {
        public static void MainEditor(string realCWD, string path = "")
        {
            InitializeConsole();
            INPUT_RECORD inputRecord = new();
            Maximize();
            string MNC = NC;
            bool inEditor = true;
            bool mclick = false;
            bool breaking = false;
            string key = "";
            bool cursorBlink = false;
            int mx = 0;
            int my = 0;
            Directory.SetCurrentDirectory($"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\\data\\mainscripts");
            string currentWorkingDirectory = Directory.GetCurrentDirectory();
            int terminalHeight = Console.WindowHeight;
            int terminalWidth = Console.WindowWidth;

            void Increment()
            {
                cursorBlink = true;
                while (inEditor)
                {
                    cursorBlink = false;
                    Thread.Sleep(500);
                    cursorBlink = true;
                    Thread.Sleep(500);
                }
            }

            Console.WriteLine("\nSelect your project folder.\n");
            string folder = "";
            if (path == "")
            {
                Thread.Sleep(2000);
                FolderBrowserDialog dialog = new()
                {
                    InitialDirectory = $"{Directory.GetParent(currentWorkingDirectory)}\\projects\\"
                };
                dialog.ShowDialog();
                folder = dialog.SelectedPath;
            }
            else { folder = path; }
            if (folder == "")
            {
                Error("Folder not selected.");
                Environment.Exit(0);
            }
            string previousPath = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(folder);
            folder = Directory.GetCurrentDirectory();
            folder += "\\Stage";
            string realFolderPath = Directory.GetParent(folder).ToString();
            Directory.SetCurrentDirectory(previousPath);
            if (!File.Exists($"{realFolderPath}\\.maindir"))
            {
                Error($"Not a ScratchScript project ({realFolderPath}), or .maindir file was deleted.");
                Environment.Exit(0);
            }
            Directory.SetCurrentDirectory(realFolderPath);
            List<string> editorLines = new();
            Console.WriteLine($"Loading {realFolderPath}...\n");
            Directory.SetCurrentDirectory("Stage");
            string[] fileOpened = File.ReadAllLines("project.ss1");
            int fileOpenedLength = fileOpened.Length;
            int editorLinesCount;
            int progressBarLength = 55;
            int q = 0;
            float percent;
            foreach (string line in fileOpened)
            {
                q++;
                percent = (float)q / fileOpenedLength;
                Console.WriteLine($"\u001b[A[{new string('#', (int)Math.Round(progressBarLength * percent))}{new string(' ', (int)(progressBarLength - Math.Round(progressBarLength * percent)))}] {Math.Round(percent * 100)}%");
                editorLines.Add($"{line} ");
            }
            editorLinesCount = editorLines.Count;
            // add settings later
            int tabSize = 2;
            bool syntaxHighlightingEnabled = true;
            bool showCWD = true;
            string themeAnsi = "\u001b[48;2;35;37;41m";
            MNC += themeAnsi;
            int editorCurrentLine;
            int editorChar;
            int realLine;
            ConsoleKeyInfo keyInfo;

            void mouseClicks()
            {
                while (inEditor)
                {
                    mclick = Control.MouseButtons == MouseButtons.Left;
                    object[] objects = Relative.CharacterPosition(16, 42);
                    mx = (int)Math.Round((double)objects[0]);
                    my = (int)Math.Round((double)objects[1]);
                    if ((bool)objects[2] && mclick)
                    {
                        int prevecl = editorCurrentLine;
                        int prevec = editorChar;
                        editorCurrentLine = my + realLine - 1;
                        if (editorCurrentLine < realLine) { editorCurrentLine = prevecl; }
                        editorChar = mx - (5 + editorLinesCount.ToString().Length);
                        if (editorChar < 1) { editorChar = 1; }
                        if (editorCurrentLine == realLine + (terminalHeight - 2))
                        {
                            editorCurrentLine = prevecl;
                            if (editorChar + 4 + editorLinesCount.ToString().Length > 1)
                            {
                                if (editorChar + 4 + editorLinesCount.ToString().Length < 10)
                                {
                                    SendKeys.SendWait("^(s)");
                                }
                                else if (editorChar + 4 + editorLinesCount.ToString().Length < 23)
                                {
                                    SendKeys.SendWait("{F2}");
                                }
                                else if (editorChar + 4 + editorLinesCount.ToString().Length < 34)
                                {
                                    SendKeys.SendWait("{F1}");
                                }
                            }
                            editorChar = prevec;
                        }
                        if (editorCurrentLine > editorLinesCount) { editorCurrentLine = editorLinesCount; }
                        if (editorChar > editorLines[editorCurrentLine - 1].Length) { editorChar = editorLines[editorCurrentLine - 1].Length; }
                        cursorBlink = true;
                        breaking = true;
                    }
                }
            }

            Console.WriteLine("\nBuilding syntax highlighting...\n");
            q = 0;

            // ANSI color codes used for syntax highlighting
            Dictionary<string, string> colors = new()
            {
                { "c", "\u001b[0m" + themeAnsi},
                { "p", "\u001b[48;5;10m" },
                { "n", "\u001b[48;5;10m" },
                { "0", "\u001b[37m" },
                { "1", "\u001b[38;2;153;102;255m" },
                { "2", "\u001b[38;2;255;140;26m" },
                { "3", "\u001b[38;2;255;102;26m" },
                { "4", "\u001b[38;2;255;191;0m" },
                { "5", "\u001b[38;2;207;99;207m" },
                { "6", "\u001b[38;2;255;171;25m" },
                { "7", "\u001b[38;2;92;177;214m" },
                { "8", "\u001b[38;2;89;192;89m" },
                { "9", "\u001b[38;2;15;189;140m" },
                { "10", "\u001b[38;2;76;151;255m" },
            };
            Dictionary<string, string> backgroundColors = new()
            {
                {"c", "" },
                {"p", "\u001b[1;90m" },
                {"n", "\u001b[1.90m" },
                {"0", "" },
                { "1", "" },
                { "2", ""},
                { "3", ""},
                { "4", ""},
                { "5", "" },
                { "6", "" },
                { "7", "" },
                { "8", "" },
                { "9", "" },
                { "10", "" },
            };
            Dictionary<string, string> parenthesisColors = new()
            {
                { "0", "" },
                { "1", "\u001b[31m" },
                { "2", "\u001b[38;5;202m" },
                { "3", "\u001b[33m" },
                { "4", "\u001b[32m" },
                { "5", "\u001b[34m" },
                { "6", "\u001b[38;5;135m" },
                { "7", "\u001b[35m" },
                { "8", "\u001b[38;5;206m" },
            };
            string[] looks =
            {
                "switch backdrop to (",
                "next backdrop",
                "change [c",
                "change [f",
                "change [w",
                "change [pix",
                "change [m",
                "change [b",
                "change [g",
                "clear graphic effects",
                "(backdrop [",
                "set [c",
                "set [f",
                "set [w",
                "set [pix",
                "set [m",
                "set [b",
                "set [g",
                "say (",  // sprite blocks
                "think (",
                "switch costume to (",
                "next costume",
                "change size by (",
                "set size to (",
                "show",
                "hide",
                "go to [f",
                "go to [b",
                "go [f",
                "go [b",
                "(size)",
                "(costume [",
            };
            string[] looksFindType =
            {
                "le",
                "eq",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "eq",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "eq",
                "le",
                "le",
                "eq",
                "eq",
                "le",
                "le",
                "le",
                "le",
                "eq",
                "le",
            };
            string[] dataVar = {
                "var:",
                "] to (",
                "] by (",
                "show variable [",
                "hide variable [",
            };
            string[] dataVarFindType =
            {
                "le",
                "in",
                "in",
                "le",
                "le",
            };
            string[] dataList =
            {
                "list:",
                ") to [",
                ") by [",
                "delete all of [",
                "delete (",
                "insert (",
                "replace item (",
                "(item (",
                "(item # of (",
                "(length of [",
                "] contains (",
                "show list [",
                "hide list [",
            };
            string[] dataListFindType =
            {
                "le",
                "in",
                "in",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "in",
                "le",
                "le",
            };
            string[] events =
            {
                "broadcast [",
                "broadcast:",
                "when I receive [",
                "when [",
                "when flag clicked",
                "when backdrop switches to [",
                "when this sprite clicked",
            };
            string[] eventsFindType =
            {
                "le",
                "le",
                "le",
                "le",
                "eq",
                "le",
                "eq"
            };
            string[] sounds =
            {
                "play sound (",
                "start sound (",
                "change [pit",
                "change [pan",
                "set [pit",
                "set [pan",
                "stop all sounds",
                "(volume)",
                "clear sound effects",
                "change volume by (",
                "set volume to (",
            };
            string[] soundsFindType =
            {
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "eq",
                "eq",
                "eq",
                "le",
                "le",
            };
            string[] control =
            {
                "wait (",
                "wait until <",
                "repeat (",
                "repeat until <",
                "forever {",
                "if <",
                "} else {",
                "while <",
                "create a clone of (",
                "for [",
                "stop [",
                "when I start as a clone",
                "delete this clone",
            };
            string[] controlFindType =
            {
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "eq",
                "le",
                "le",
                "le",
                "le",
                "eq",
                "eq",
            };
            string[] sensing =
            {
                "ask (",
                "(answer)",
                "<key (",
                "<mouse down?>",
                "(mouse x)",
                "(mouse y)",
                "(loudness)",
                "(timer)",
                "reset timer",
                "([",
                "(current [",
                "(days since 2000)",
                "(username)",
                "<touching (",
                "<touching color (",
                "<color (",
                "(distance to (",
                "set drag mode [",
            };
            string[] sensingFindType =
            {
                "le",
                "eq",
                "le",
                "eq",
                "eq",
                "eq",
                "eq",
                "eq",
                "eq",
                "le",
                "le",
                "eq",
                "eq",
                "le",
                "le",
                "le",
                "le",
                "le",
            };
            string[] operators =
            {
                ") + (",
                ") - (",
                ") / (",
                ") * (",
                ") > (",
                ") < (",
                "(pick random (",
                ") = (",
                "> and <",
                "> or <",
                "<not <",
                "(join (",
                "(letter (",
                "(length of (",
                ") contains (",
                ") mod (",
                "(round (",
            };
            string[] operatorsFindType =
            {
                "in",
                "in",
                "in",
                "in",
                "in",
                "in",
                "le",
                "in",
                "in",
                "in",
                "le",
                "le",
                "le",
                "le",
                "in",
                "in",
                "le",
            };
            string[] motion =
            {
                "move (",
                "turn cw (",
                "turn ccw (",
                "go to (",
                "go to x: (",
                "glide (",
                "point in direction (",
                "point towards (",
                "change x by (",
                "set x to (",
                "change y by (",
                "set y to (",
                "if on edge, bounce",
                "set rotation style [",
                "(x position)",
                "(y position)",
                "(direction)",
            };
            string[] motionFindType =
            {
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "le",
                "eq",
                "le",
                "eq",
                "eq",
                "eq",
            };
            string[] pen =
            {
                "erase all",
                "stamp",
                "pen down",
                "pen up",
                "set pen color to (",
                "change pen (",
                "set pen (",
                "change pen size by (",
                "set pen size to (",
            };
            string[] penFindType =
            {
                "eq",
                "eq",
                "eq",
                "eq",
                "le",
                "le",
                "le",
                "le",
                "le"
            };
            string[] startParenthesis = { "(", "[", "{", "<" };
            string[] endParenthesis = { ")", "]", "}", ">" };
            string[] excludes = { "46;1", "38;5;8", "0", "37", "35", "7", "1" };
            int bracketCount = 0;
            string[] shabang =
            {
                "//!looks",
                "//!var",
                "//!list",
                "//!events",
                "//!sound",
                "//!control",
                "//!sensing",
                "//!operators",
                "//!pen",
                "//!motion",
            };
            string syntaxType;
            string syntaxLine;
            Dictionary<string, string[]> blocks = new()
            {
                { "8", operators },
                { "1", looks },
                { "2", dataVar },
                { "3", dataList },
                { "4", events },
                { "5", sounds },
                { "7", sensing },
                { "6", control },
                { "9", pen },
                { "10", motion },
            };
            Dictionary<string, string[]> blocksFindType = new()
            {
                { "8", operatorsFindType },
                { "1", looksFindType },
                { "2", dataVarFindType },
                { "3", dataListFindType },
                { "4", eventsFindType },
                { "5", soundsFindType },
                { "7", sensingFindType },
                { "6", controlFindType },
                { "9", penFindType },
                { "10", motionFindType },
            };
            int parenthesisCount;
            ControlKeyStates modifierKeys = new();
            bool shift = false;
            bool ctrl = false;
            string taskbar;

            void DTLoop(string line, string type)
            {
                int qt = -1;
                foreach (string i in blocks[type])
                {
                    qt++;
                    switch (blocksFindType[type][qt])
                    {
                        case "le":
                            if (line.TrimStart(' ').Length >= i.Length && i == line.TrimStart(' ')[..i.Length])
                            { syntaxType = type; }
                            break;

                        case "eq":
                            if (i == line.Trim(' ')) { syntaxType = type; }
                            break;

                        case "in":
                            if (line.TrimStart(' ').Contains(i)) { syntaxType = type; }
                            break;
                    }
                }
            }

            void DetermineType(string line)
            {
                syntaxType = "0";
                int syntaxTypeId = 0;
                DTLoop(line, "8");
                DTLoop(line, "1");
                DTLoop(line, "2");
                DTLoop(line, "3");
                DTLoop(line, "4");
                DTLoop(line, "5");
                DTLoop(line, "7");
                DTLoop(line, "6");
                DTLoop(line, "9");
                DTLoop(line, "10");
                foreach (string i in shabang)
                {
                    syntaxTypeId++;
                    if (line.TrimStart(' ').Contains(i)) { syntaxType = syntaxTypeId.ToString(); }
                }
                if (line.TrimStart(' ').Contains("\\nscript")) { syntaxType = "n"; }
                if (line.TrimStart(' ').Contains("\\prep")) { syntaxType = "p"; }
            }

            IEnumerable<string> SplitByLength(string str, int length)
            {
                for (int i = 0; i < str.Length; i += length) { yield return str.Substring(i, Math.Min(length, str.Length - i)); }
            }

            string AddSyntax(string line, bool progressBarEnabled = true)
            {
                syntaxLine = line;
                q++;
                percent = (float)q / fileOpenedLength;
                if (progressBarEnabled)
                {
                    Console.WriteLine($"\u001b[A[{new string('#', (int)Math.Round(progressBarLength * percent))}{new string(' ', (int)(progressBarLength - Math.Round(progressBarLength * percent)))}] {Math.Round(percent * 100)}%");
                    editorLines.Add($"{line} ");
                }
                DetermineType(line);
                int i = -1;
                StringBuilder buildedLine = new("");
                parenthesisCount = bracketCount;
                char character;
                while (true)
                {
                    i++;
                    try { character = syntaxLine[i]; }
                    catch (IndexOutOfRangeException) { break; }
                    buildedLine.Append(character);
                    if (startParenthesis.Contains(character.ToString()))
                    {
                        if (character != '{')
                        {
                            if (character == '<')
                            {
                                if (!(syntaxLine[i - 1] == ' ' && syntaxLine[i + 1] == ' '))
                                {
                                    parenthesisCount++;
                                    int c = 0;
                                    for (int z = 0; z < parenthesisCount; z++)
                                    {
                                        c++;
                                        if (c == 9) { c = 1; }
                                    }
                                    buildedLine = new(buildedLine.ToString().TrimEnd(buildedLine[^1]));
                                    buildedLine.Append($"{parenthesisColors[c.ToString()]}{character}{colors[syntaxType]}");
                                }
                            }
                            else
                            {
                                parenthesisCount++;
                                int c = 0;
                                for (int z = 0; z < parenthesisCount; z++)
                                {
                                    c++;
                                    if (c == 9) { c = 1; }
                                }
                                buildedLine = new(buildedLine.ToString().TrimEnd(buildedLine[^1]));
                                buildedLine.Append($"{parenthesisColors[c.ToString()]}{character}{colors[syntaxType]}");
                            }
                        }
                        else
                        {
                            bracketCount++;
                            int c = 0;
                            for (int z = 0; z < bracketCount; z++)
                            {
                                c++;
                                if (c == 9) { c = 1; }
                            }
                            buildedLine = new(buildedLine.ToString().TrimEnd(buildedLine[^1]));
                            buildedLine.Append($"{parenthesisColors[c.ToString()]}{character}{colors[syntaxType]}");
                        }
                    }
                    else if (endParenthesis.Contains(character.ToString()))
                    {
                        if (character != '}')
                        {
                            if (character == '<')
                            {
                                if (!(syntaxLine[i - 1] == ' ' && syntaxLine[i + 1] == ' ') && parenthesisCount > 0)
                                {
                                    int c = 0;
                                    for (int z = 0; z < parenthesisCount; z++)
                                    {
                                        c++;
                                        if (c == 9) { c = 1; }
                                    }
                                    buildedLine = new(buildedLine.ToString().TrimEnd(buildedLine[^1]));
                                    buildedLine.Append($"{parenthesisColors[c.ToString()]}{character}{colors[syntaxType]}");
                                    parenthesisCount--;
                                }
                            }
                            else if (parenthesisCount > 0)
                            {
                                int c = 0;
                                for (int z = 0; z < parenthesisCount; z++)
                                {
                                    c++;
                                    if (c == 9) { c = 1; }
                                }
                                buildedLine = new(buildedLine.ToString().TrimEnd(buildedLine[^1]));
                                buildedLine.Append($"{parenthesisColors[c.ToString()]}{character}{colors[syntaxType]}");
                                parenthesisCount--;
                            }
                        }
                        else if (bracketCount > 0)
                        {
                            int c = 0;
                            for (int z = 0; z < bracketCount; z++)
                            {
                                c++;
                                if (c == 9) { c = 1; }
                            }
                            buildedLine = new(buildedLine.ToString().TrimEnd(buildedLine[^1]));
                            buildedLine.Append($"{parenthesisColors[c.ToString()]}{character}{colors[syntaxType]}");
                            bracketCount--;
                        }
                    }
                    if (character == '"')
                    {
                        buildedLine = new(buildedLine.ToString().TrimEnd(buildedLine[^1]));
                        buildedLine.Append("\u001b[38;5;34m\"");
                        while (true)
                        {
                            i++;
                            try { character = syntaxLine[i]; }
                            catch (IndexOutOfRangeException) { break; }
                            buildedLine.Append(character);
                            if (character == '"')
                            {
                                buildedLine.Append(colors[syntaxType]);
                                break;
                            }
                        }
                    }
                    if (character == '\'')
                    {
                        buildedLine = new(buildedLine.ToString().TrimEnd(buildedLine[^1]));
                        buildedLine.Append("\u001b[38;5;34m'");
                        while (true)
                        {
                            i++;
                            try { character = syntaxLine[i]; }
                            catch (IndexOutOfRangeException) { break; }
                            buildedLine.Append(character);
                            if (character == '\'')
                            {
                                buildedLine.Append(colors[syntaxType]);
                                break;
                            }
                        }
                    }
                    if (character == '/')
                    {
                        try
                        {
                            i--;
                            if (syntaxLine[i] == '/')
                            {
                                i++;
                                buildedLine = new(buildedLine.ToString().TrimEnd("//".ToCharArray()));
                                buildedLine.Append("\u001b[38;5;8m//");
                                while (true)
                                {
                                    i++;
                                    try { character = syntaxLine[i]; }
                                    catch (IndexOutOfRangeException) { break; }
                                    buildedLine.Append(character);
                                }
                                break;
                            }
                            else { i++; }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            i++;
                        }
                    }
                }
                syntaxLine = buildedLine.ToString();
                string syntaxBuild = $"{backgroundColors[syntaxType]}{colors[syntaxType]}{syntaxLine}\u001b[0m{themeAnsi}";
                if (syntaxBuild == "") { syntaxBuild = line; }
                return syntaxBuild;
            }

            bool lineWrapWarning = false;
            List<string> editorLinesWithSyntax = new();
            foreach (string line in editorLines.ToArray())
            {
                if (line.Length > terminalWidth) { lineWrapWarning = true; }
                editorLinesWithSyntax.Add(AddSyntax(line));
            }
            if (lineWrapWarning)
            {
                Console.WriteLine("\nWARNING: Line wrap may occur and will make the editor glitch out. Instead of using this, you should use something like Notepad or VSCode to edit the ScratchScript file, as it's much better than this editor.");
                Thread.Sleep(2000);
            }
            editorCurrentLine = 1;
            editorChar = editorLines[0].Length;
            realLine = 1;
            bool quoteComplete = false;
            bool singleQuoteComplete = false;
            int parenthesisComplete = 0;
            bool capsLock = false;
            string taskbarMessage = "";
            string state = "edit";
            string[] validKeys =
            {
                "F1",
                "F2",
                "F3",
                "F4",
                "F5",
                "F6",
                "F7",
                "F8",
                "F9",
                "F10",
                "F11",
                "F12",
                "Enter",
                "Tab",
                "Backspace",
                "UpArrow",
                "DownArrow",
                "LeftArrow",
                "RightArrow",
                "Delete",
                "PageUp",
                "PageDown",
            };

            void OnKeyPress(string key)
            {
                key = (key == "Spacebar") ? " " : key;
                if (validKeys.Contains(key) || key.Length == 1)
                {
                    string transfer;
                    int leadings;
                    switch (key)
                    {
                        case "F1":
                            inEditor = false;
                            state = "tree";
                            break;

                        case "F2":
                            inEditor = false;
                            state = "new";
                            break;

                        case "UpArrow":
                            taskbarMessage = "Up Arrow";
                            editorCurrentLine--;
                            if (editorCurrentLine == realLine && realLine > 1) { realLine--; }
                            else if (editorCurrentLine == realLine - 1) { realLine -= 1 + ((editorCurrentLine != 1) ? 1 : 0); }
                            if (editorCurrentLine < 1) 
                            {
                                editorCurrentLine = 1; 
                                realLine = 1;
                            }
                            cursorBlink = true;
                            if (editorChar > editorLines[editorCurrentLine - 1].Length) { editorChar = editorLines[editorCurrentLine - 1].Length; }
                            break;

                        case "DownArrow":
                            taskbarMessage = "Down Arrow";
                            editorCurrentLine++;
                            if (editorCurrentLine == realLine + terminalHeight - 3 && (editorCurrentLine != editorLinesCount)) { realLine++; }
                            else if (editorCurrentLine == realLine + (terminalHeight - 2)) { realLine += 1 + ((editorCurrentLine != editorLinesCount) ? 1 : 0); }
                            cursorBlink = true;
                            if (editorCurrentLine > fileOpenedLength) { editorCurrentLine = fileOpenedLength; }
                            if (editorChar > editorLines[editorCurrentLine - 1].Length) { editorChar = editorLines[editorCurrentLine - 1].Length; }
                            break;

                        case "LeftArrow":
                            taskbarMessage = "Left Arrow";
                            if (editorChar > 1) { editorChar--; }
                            else if (editorCurrentLine > 1)
                            {
                                editorChar = editorLines[editorCurrentLine - 2].Length;
                                editorCurrentLine--;
                                if (editorCurrentLine == realLine && realLine > 1) { realLine--; }
                                else if (editorCurrentLine == realLine - 1) { realLine -= 1 + ((editorCurrentLine != 1) ? 1 : 0); }
                                if (editorChar > editorLines[editorCurrentLine - 1].Length) { editorChar = editorLines[editorCurrentLine - 1].Length; }
                            }
                            if (quoteComplete) { quoteComplete = false; }
                            if (singleQuoteComplete) { singleQuoteComplete = false; }
                            cursorBlink = true;
                            break;

                        case "RightArrow":
                            taskbarMessage = "Right Arrow";
                            if (editorChar < editorLines[editorCurrentLine - 1].Length) { editorChar++; }
                            else if (editorCurrentLine < editorLinesCount)
                            {
                                editorChar = 1;
                                editorCurrentLine++;
                                if (editorCurrentLine == realLine + terminalHeight - 3 && (editorCurrentLine != editorLinesCount)) { realLine++; }
                                else if (editorCurrentLine == realLine + (terminalHeight - 2)) { realLine += 1 + ((editorCurrentLine != editorLinesCount) ? 1 : 0); }
                                if (editorChar > editorLines[editorCurrentLine - 1].Length) { editorChar = editorLines[editorCurrentLine - 1].Length; }
                            }
                            if (quoteComplete) { quoteComplete = false; }
                            if (singleQuoteComplete) { singleQuoteComplete = false; }
                            cursorBlink = true;
                            break;

                        case "Backspace":
                            taskbarMessage = "Backspace";
                            if (editorChar == 1 && editorCurrentLine != 1)
                            {
                                editorChar = editorLines[editorCurrentLine - 2].Length;
                                transfer = $"{editorLines[editorCurrentLine - 2].TrimEnd(' ')}{editorLines[editorCurrentLine - 1]}";
                                editorLines[editorCurrentLine - 2] = transfer;
                                editorLinesWithSyntax[editorCurrentLine - 2] = AddSyntax(transfer, false);
                                editorLines.RemoveAt(editorCurrentLine - 1);
                                editorLinesWithSyntax.RemoveAt(editorCurrentLine - 1);
                                editorCurrentLine--;
                                if (realLine > 1 && editorCurrentLine == realLine && realLine > 1) { realLine--; }
                            }
                            else
                            {
                                try
                                {
                                    string backspaceSplit = editorLines[editorCurrentLine - 1].Remove(editorChar - 2, 1);
                                    editorLines[editorCurrentLine - 1] = backspaceSplit;
                                    editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(backspaceSplit, false);
                                    if (editorChar > 1) { editorChar--; }
                                    editorChar = (editorChar > editorLines[editorCurrentLine - 1].Length) ? editorLines[editorCurrentLine - 1].Length : editorChar;
                                    cursorBlink = true;
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    // Do nothing
                                }
                            }
                            break;

                        case "Delete":
                            taskbarMessage = "Delete";
                            if (editorChar + 1 != editorLines[editorCurrentLine - 1].Length)
                            {
                                string backspaceSplit = editorLines[editorCurrentLine - 1].Remove(editorChar - 1, 1);
                                editorLines[editorCurrentLine - 1] = backspaceSplit;
                                editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(backspaceSplit, false);
                            }
                            cursorBlink = true;
                            break;

                        case "Enter":
                            taskbarMessage = "Enter";
                            leadings = editorLines[editorCurrentLine - 1].Length - editorLines[editorCurrentLine - 1].TrimStart(' ').Length;
                            if (editorChar == editorLines[editorCurrentLine - 1].Length && editorLines[editorCurrentLine - 1].Length == editorLines[editorCurrentLine - 1].TrimStart(' ').Length)
                            {
                                leadings--;
                                if (editorLines[editorCurrentLine - 1].Trim() == "") { editorLines.Insert(editorCurrentLine, new string(' ', leadings + ((leadings == -1) ? 2 : 0))); }
                                else { editorLines.Insert(editorCurrentLine, new string(' ', leadings + ((leadings == -1) ? 2 : 1))); }
                                if (editorLines[editorCurrentLine - 1].Trim() == "") { editorLinesWithSyntax.Insert(editorCurrentLine, new string(' ', leadings + ((leadings == -1) ? 2 : 0))); }
                                else { editorLinesWithSyntax.Insert(editorCurrentLine, new string(' ', leadings + ((leadings == -1) ? 2 : 1))); }
                                editorChar = editorLines[editorCurrentLine].Length;
                            }
                            else
                            {
                                try
                                {
                                    if (editorLines[editorCurrentLine - 1][editorChar - 2] == '{')
                                    {
                                        leadings = editorLines[editorCurrentLine - 1].Length - editorLines[editorCurrentLine - 1].TrimStart(' ').Length;
                                        string lineInsert = $"{new string(' ', leadings)}{editorLines[editorCurrentLine - 1][(editorChar - 1)..]}";
                                        editorLines[editorCurrentLine - 1] = $"{editorLines[editorCurrentLine - 1].TrimEnd(lineInsert.ToCharArray())} ";
                                        editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(editorLines[editorCurrentLine - 1], false);
                                        editorLines.Insert(editorCurrentLine, new string(' ', leadings + tabSize + 1));
                                        editorLinesWithSyntax.Insert(editorCurrentLine, new string(' ', leadings + tabSize + 1));
                                        editorLines.Insert(editorCurrentLine + 1, lineInsert);
                                        editorLinesWithSyntax.Insert(editorCurrentLine + 1, AddSyntax(lineInsert, false));
                                        editorChar = leadings + tabSize + 1;
                                        if (editorCurrentLine == realLine + terminalHeight - 3) { realLine++; }
                                    }
                                    else
                                    {
                                        string lineInsert = editorLines[editorCurrentLine - 1][(editorChar - 1)..];
                                        editorLines[editorCurrentLine - 1] = $"{editorLines[editorCurrentLine - 1].TrimEnd(lineInsert.ToCharArray())} ";
                                        editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(editorLines[editorCurrentLine - 1], false);
                                        editorLines.Insert(editorCurrentLine, $"{new string(' ', leadings + ((leadings == -1) ? 2 : 0))}{lineInsert}");
                                        editorLinesWithSyntax.Insert(editorCurrentLine, AddSyntax($"{new string(' ', leadings + ((leadings == -1) ? 2 : 0))}{lineInsert}", false));
                                        editorChar = leadings + 1;
                                    }
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    string lineInsert = editorLines[editorCurrentLine - 1][(editorChar - 1)..];
                                    editorLines[editorCurrentLine - 1] = $"{editorLines[editorCurrentLine - 1].TrimEnd(lineInsert.ToCharArray())} ";
                                    editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(editorLines[editorCurrentLine - 1], false);
                                    editorLines.Insert(editorCurrentLine, $"{new string(' ', leadings + ((leadings == -1) ? 2 : 0))}{lineInsert}");
                                    editorLinesWithSyntax.Insert(editorCurrentLine, AddSyntax($"{new string(' ', leadings + ((leadings == -1) ? 2 : 0))}{lineInsert}", false));
                                    editorChar = leadings + 1;
                                }
                            }
                            editorCurrentLine++;
                            if (editorCurrentLine == realLine + terminalHeight - 2 || editorCurrentLine == realLine + terminalHeight - 3) { realLine++; }
                            cursorBlink = true;
                            break;

                        case "Tab":
                            taskbarMessage = "Tab";
                            string newLine = editorLines[editorCurrentLine - 1].Insert(editorChar - 1, new string(' ', tabSize));
                            editorLines[editorCurrentLine - 1] = newLine;
                            editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(newLine, false);
                            editorChar += tabSize;
                            cursorBlink = true;
                            break;

                        case "PageUp":
                            taskbarMessage = "Page Up";
                            realLine -= terminalHeight - 3;
                            if (realLine > editorLinesCount) { realLine = editorLinesCount; }
                            editorCurrentLine = realLine;
                            cursorBlink = true;
                            break;

                        case "PageDown":
                            taskbarMessage = "Page Down";
                            realLine += terminalHeight - 3;
                            if (realLine < 1) { realLine = 1; }
                            editorCurrentLine = realLine;
                            cursorBlink = true;
                            break;

                        default:
                            taskbarMessage = key;
                            if (key == "\"")
                            {
                                if (!quoteComplete)
                                {
                                    newLine = editorLines[editorCurrentLine - 1].Insert(editorChar - 1, key);
                                    editorLines[editorCurrentLine - 1] = newLine;
                                    editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(newLine, false);
                                    editorChar++;
                                    cursorBlink = true;
                                    try
                                    {
                                        if ((editorLines[editorCurrentLine - 1][editorChar - 3] == ' ' && (editorLines[editorCurrentLine - 1][editorChar - 1] == ' ')) || startParenthesis.Contains(editorLines[editorCurrentLine - 1][editorChar - 3].ToString()))
                                        {
                                            quoteComplete = true;
                                            newLine = editorLines[editorCurrentLine - 1].Insert(editorChar - 1, key);
                                            editorLines[editorCurrentLine - 1] = newLine;
                                            editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(newLine, false);
                                        }
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        quoteComplete = true;
                                        newLine = editorLines[editorCurrentLine - 1].Insert(editorChar - 1, key);
                                        editorLines[editorCurrentLine - 1] = newLine;
                                        editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(newLine, false);
                                    }
                                }
                                else
                                {
                                    editorChar++;
                                    cursorBlink = true;
                                    quoteComplete = false;
                                }
                            }
                            else if (key == "'")
                            {
                                if (!singleQuoteComplete)
                                {
                                    newLine = editorLines[editorCurrentLine - 1].Insert(editorChar - 1, key);
                                    editorLines[editorCurrentLine - 1] = newLine;
                                    editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(newLine, false);
                                    editorChar++;
                                    cursorBlink = true;
                                    try
                                    {
                                        if (editorLines[editorCurrentLine - 1][editorChar - 3] == ' ')
                                        {
                                            singleQuoteComplete = true;
                                            newLine = editorLines[editorCurrentLine - 1].Insert(editorChar - 1, key);
                                            editorLines[editorCurrentLine - 1] = newLine;
                                            editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(newLine, false);
                                        }
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        singleQuoteComplete = true;
                                        newLine = editorLines[editorCurrentLine - 1].Insert(editorChar - 1, key);
                                        editorLines[editorCurrentLine - 1] = newLine;
                                        editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(newLine, false);
                                    }
                                }
                                else
                                {
                                    editorChar++;
                                    cursorBlink = true;
                                    singleQuoteComplete = false;
                                }
                            }
                            else if (startParenthesis.Contains(key))
                            {
                                parenthesisComplete++;
                                string previousKey = key;
                                newLine = editorLines[editorCurrentLine - 1].Insert(editorChar - 1, key);
                                editorLines[editorCurrentLine - 1] = newLine;
                                editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(newLine, false);
                                editorChar++;
                                cursorBlink = true;
                                switch (key)
                                {
                                    case "(":
                                        key = ")";
                                        break;

                                    case "[":
                                        key = "]";
                                        break;

                                    case "{":
                                        key = "}";
                                        break;

                                    case "<":
                                        key = ">";
                                        break;
                                }
                                newLine = editorLines[editorCurrentLine - 1].Insert(editorChar - 1, key);
                                key = previousKey;
                                editorLines[editorCurrentLine - 1] = newLine;
                                editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(newLine, false);
                            }
                            else if (endParenthesis.Contains(key))
                            {
                                if (parenthesisComplete > 0)
                                {
                                    parenthesisComplete--;
                                    editorChar++;
                                    cursorBlink = true;
                                }
                                else
                                {
                                    newLine = editorLines[editorCurrentLine - 1].Insert(editorChar - 1, key);
                                    editorLines[editorCurrentLine - 1] = newLine;
                                    editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(newLine, false);
                                    editorChar++;
                                    cursorBlink = true;
                                }
                            }
                            else
                            {
                                if (ctrl)
                                {
                                    if (key == "/")
                                    {
                                        taskbarMessage = "CTRL+/";
                                        try
                                        {
                                            if (editorLines[editorCurrentLine - 1][..2] != "//")
                                            {
                                                newLine = $"// {editorLines[editorCurrentLine - 1]}";
                                                editorChar += 3;
                                            }
                                            else
                                            {
                                                newLine = editorLines[editorCurrentLine - 1][3..];
                                                editorChar -= 3;
                                            }
                                        }
                                        catch (ArgumentOutOfRangeException)
                                        {
                                            newLine = $"// {editorLines[editorCurrentLine - 1]}";
                                            editorChar += 3;
                                        }
                                        editorLines[editorCurrentLine - 1] = newLine;
                                        editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(newLine, false);
                                    }
                                    else if (key == "s")
                                    {
                                        string projectDir = $"{Directory.GetCurrentDirectory()}\\project.ss1";
                                        taskbarMessage = $"Saving to {projectDir}...";
                                        File.Delete(projectDir);
                                        foreach (string item in editorLines) { WriteToFile(projectDir, item.TrimEnd(' ')); }
                                        Thread.Sleep(100);
                                        taskbarMessage = "Done";
                                    }
                                }
                                else
                                {
                                    if (capsLock) { key = (shift) ? key.ToLower() : key.ToUpper(); }
                                    else if (shift) { key = key.ToUpper(); }
                                    newLine = editorLines[editorCurrentLine - 1].Insert(editorChar - 1, key);
                                    editorLines[editorCurrentLine - 1] = newLine;
                                    editorLinesWithSyntax[editorCurrentLine - 1] = AddSyntax(newLine, false);
                                    editorChar++;
                                    cursorBlink = true;
                                }
                            }
                            break;
                    }
                    if (editorChar < 1) { editorChar = 1; }
                    if (editorCurrentLine < 1)
                    {
                        editorCurrentLine = 1;
                        realLine = 1;
                    }
                    if (editorCurrentLine > editorLinesCount) { editorCurrentLine = editorLinesCount; }
                    if (editorChar > editorLines[editorCurrentLine - 1].Length) { editorChar = editorLines[editorCurrentLine - 1].Length; }
                }
            }

            void EditorPrint(int line)
            {
                int getLineCount = editorLinesCount.ToString().Length;
                string currentWorkingDirectoryString;
                StringBuilder editorBuffer;
                if (showCWD)
                {
                    currentWorkingDirectoryString = $"\u001b[48;2;56;113;228m\u001b[35;1m Current Working Directory: {folder}\\project.ss1";
                    if (" Current Working Directory: ".Length + $"{folder}\\project.ss1".Length > terminalWidth)
                    {
                        currentWorkingDirectoryString = $"{currentWorkingDirectoryString.TrimEnd(currentWorkingDirectoryString[^Math.Abs(terminalWidth - (" Current Working Directory: ".Length + $"{folder}\\project.ss1".Length) - 4)..].ToCharArray())}...";
                        editorBuffer = new($"\u001b[48;2;56;113;228m{new string(' ', terminalWidth)}\n\u001b[A{currentWorkingDirectoryString}\n{themeAnsi}");
                    }
                    else
                    {
                        editorBuffer = new($"{currentWorkingDirectoryString}{new string(' ', terminalWidth - (" Current Working Directory: ".Length + $"{folder}\\project.ss1".Length))}\u001b[0m\n{themeAnsi}");
                    }
                }
                else
                {
                    currentWorkingDirectoryString = "\u001b[48;2;56;113;228m\u001b[35;1m";
                    editorBuffer = new($"{currentWorkingDirectoryString}{new string(' ', terminalWidth)}\u001b[0m\n{themeAnsi}");
                }
                q = realLine - 1;
                string editorBufferLine = "";
                bool thist = false;
                for (int i = 0; i < line - 2; i++)
                {
                    q++;
                    string filler = "";
                    if (thist)
                    {
                        editorBufferLine = $"\u001b[38;5;8m ~{new string(' ', editorLinesCount.ToString().Length + 2)}\u001b[1;38;5;8m|\u001b[0m{themeAnsi}{new string(' ', terminalWidth - (editorLinesCount.ToString().Length + 5))}\n";
                    }
                    else
                    {
                        try
                        {
                            filler = new(' ', terminalWidth - (editorLines[q - 1].Length + editorLinesCount.ToString().Length + 5));
                            editorBufferLine = $"\u001b[38;5;8m{new string(' ', getLineCount - q.ToString().Length)}{((editorCurrentLine == q) ? "\u001b[93m" : "")}{q}\u001b[1;38;5;8m    |\u001b[0m{themeAnsi}{editorLinesWithSyntax[q - 1]}{filler}\n";
                            if (editorCurrentLine == q && cursorBlink)
                            {
                                string editorBufferBuffer = editorLines[q - 1];
                                DetermineType(editorBufferBuffer);
                                StringBuilder find = new(colors[syntaxType]);
                                parenthesisCount = 0;
                                bracketCount = 0;
                                int j = -1;
                                for (int k = 0; k < editorBufferBuffer.Length; k++)
                                {
                                    bool quote = false;
                                    bool comment = false;
                                    j++;
                                    if (editorChar == j + 1) { find.Append("\u001b[46;1m"); }
                                    try
                                    {
                                        switch (editorBufferBuffer[j])
                                        {
                                            case '"':
                                                find.Append("\u001b[38;5;34m\"");
                                                quote = true;
                                                break;

                                            case '\'':
                                                find.Append("\u001b[38;5;34m'");
                                                quote = true;
                                                break;

                                            case '/':
                                                j--;
                                                if (editorBufferBuffer[j] == '/')
                                                {
                                                    j++;
                                                    if (editorChar == j) { find.Append($"\u001b[46;1m\u001b[38;5;8m/\u001b[0m\u001b[38;5;8m{themeAnsi}/"); }
                                                    else if (editorChar == j + 1) { find.Append($"\u001b[0m{themeAnsi}\u001b[38;5;8m/\u001b[46;1m/\u001b[0m"); }
                                                    else { find.Append("\u001b[38;5;8m//"); }
                                                    comment = true;
                                                }
                                                else { j++; }
                                                break;

                                            default:
                                                find.Append(editorBufferBuffer[j]);
                                                break;
                                        }
                                    }
                                    catch (IndexOutOfRangeException) { j++; }
                                    if (j > editorBufferBuffer.Length - 1) { break; }
                                    char character = editorBufferBuffer[j];
                                    if (editorChar == j + 1) { find.Append($"\u001b[0m{themeAnsi}{colors[syntaxType]}"); }
                                    if (startParenthesis.Contains(character.ToString()))
                                    {
                                        if (character != '{')
                                        {
                                            if (character == '<')
                                            {
                                                if (!(editorBufferBuffer[j - 1] == ' ' || editorBufferBuffer[j + 1] == ' '))
                                                {
                                                    parenthesisCount++;
                                                    int c = 0;
                                                    for (int z = 0; z < parenthesisCount; z++)
                                                    {
                                                        c++;
                                                        if (c == 9) { c = 1; }
                                                    }
                                                    find = new(find.ToString().TrimEnd(find[^1]));
                                                    find.Append($"{parenthesisColors[c.ToString()]}{((editorChar != j + 1) ? character : "")}{colors[syntaxType]}");
                                                }
                                                else
                                                {
                                                    find = new(find.ToString().TrimEnd(find[^1]));
                                                    find.Append($"{((editorChar != j + 1) ? character : "")}{colors[syntaxType]}");
                                                }
                                            }
                                            else
                                            {
                                                parenthesisCount++;
                                                int c = 0;
                                                for (int z = 0; z < parenthesisCount; z++)
                                                {
                                                    c++;
                                                    if (c == 9) { c = 1; }
                                                }
                                                find = new(find.ToString().TrimEnd(find[^1]));
                                                find.Append($"{parenthesisColors[c.ToString()]}{((editorChar != j + 1) ? character : "")}{colors[syntaxType]}");
                                            }
                                        }
                                        else
                                        {
                                            bracketCount++;
                                            int c = 0;
                                            for (int z = 0; z < bracketCount; z++)
                                            {
                                                c++;
                                                if (c == 9) { c = 1; }
                                            }
                                            find = new(find.ToString().TrimEnd(find[^1]));
                                            find.Append($"{parenthesisColors[c.ToString()]}{((editorChar != j + 1) ? character : "")}{colors[syntaxType]}");
                                        }
                                    }
                                    else if (endParenthesis.Contains(character.ToString()))
                                    {
                                        if (character != '}')
                                        {
                                            if (character == '>')
                                            {
                                                if (!(editorBufferBuffer[j - 1] == ' ' && editorBufferBuffer[j + 1] == ' ') && parenthesisCount > 0)
                                                {
                                                    int c = 0;
                                                    for (int z = 0; z < parenthesisCount; z++)
                                                    {
                                                        c++;
                                                        if (c == 9) { c = 1; }
                                                    }
                                                    find = new(find.ToString().TrimEnd(find[^1]));
                                                    find.Append($"{parenthesisColors[c.ToString()]}{((editorChar != j + 1) ? character : "")}{colors[syntaxType]}");
                                                    parenthesisCount--;
                                                }
                                            }
                                            else if (parenthesisCount > 0)
                                            {
                                                int c = 0;
                                                for (int z = 0; z < parenthesisCount; z++)
                                                {
                                                    c++;
                                                    if (c == 9) { c = 1; }
                                                }
                                                find = new(find.ToString().TrimEnd(find[^1]));
                                                find.Append($"{parenthesisColors[c.ToString()]}{((editorChar != j + 1) ? character : "")}{colors[syntaxType]}");
                                                parenthesisCount--;
                                            }
                                        }
                                        else if (bracketCount > 0)
                                        {
                                            int c = 0;
                                            for (int z = 0; z < bracketCount; z++)
                                            {
                                                c++;
                                                if (c == 9) { c = 1; }
                                            }
                                            find = new(find.ToString().TrimEnd(find[^1]));
                                            find.Append($"{parenthesisColors[c.ToString()]}{((editorChar != j + 1) ? character : "")}{colors[syntaxType]}");
                                            bracketCount--;
                                        }
                                    }
                                    if (character == '"')
                                    {
                                        if (quote)
                                        {
                                            find.Append("\u001b[38;5;34m");
                                            while (true)
                                            {
                                                j++;
                                                try { character = editorBufferBuffer[j]; }
                                                catch (IndexOutOfRangeException) { break; }
                                                if (editorChar == j + 1) { find.Append("\u001b[46;1m"); }
                                                character = editorBufferBuffer[j];
                                                find.Append(character);
                                                if (editorChar == j + 1) { find.Append($"\u001b[0m{themeAnsi}\u001b[38;5;34m"); }
                                                if (character == '"')
                                                {
                                                    find.Append(colors[syntaxType]);
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            find = new(find.ToString().TrimEnd(find[^1]));
                                            find.Append("\u001b[38;5;34m\"");
                                            while (true)
                                            {
                                                j++;
                                                try { character = editorBufferBuffer[j]; }
                                                catch (IndexOutOfRangeException) { break; }
                                                find.Append(character);
                                                if (character == '"')
                                                {
                                                    find.Append(colors[syntaxType]);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (character == '\'')
                                    {
                                        if (quote)
                                        {
                                            find.Append("\u001b[38;5;34m");
                                            while (true)
                                            {
                                                j++;
                                                try { character = editorBufferBuffer[j]; }
                                                catch (IndexOutOfRangeException) { break; }
                                                if (editorChar == j + 1) { find.Append("\u001b[46;1m"); }
                                                character = editorBufferBuffer[j];
                                                find.Append(character);
                                                if (editorChar == j + 1) { find.Append($"\u001b[0m{themeAnsi}\u001b[38;5;34m"); }
                                                if (character == '\'')
                                                {
                                                    find.Append(colors[syntaxType]);
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            find = new(find.ToString().TrimEnd(find[^1]));
                                            find.Append("\u001b[38;5;34m'");
                                            while (true)
                                            {
                                                j++;
                                                try { character = editorBufferBuffer[j]; }
                                                catch (IndexOutOfRangeException) { break; }
                                                find.Append(character);
                                                if (character == '\'')
                                                {
                                                    find.Append(colors[syntaxType]);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    try
                                    {
                                        if (character == '/')
                                        {
                                            j--;
                                            if (editorBufferBuffer[j] == '/')
                                            {
                                                j++;
                                                if (comment)
                                                {
                                                    find.Append($"{themeAnsi}\u001b[38;5;8m");
                                                    while (true)
                                                    {
                                                        j++;
                                                        try { character = editorBufferBuffer[j]; }
                                                        catch (IndexOutOfRangeException) { break; }
                                                        if (editorChar == j + 1) { find.Append("\u001b[46;1m"); }
                                                        find.Append(character);
                                                        if (editorChar == j + 1) { find.Append($"\u001b[0m{themeAnsi}\u001b[38;5;8m"); }
                                                    }
                                                }
                                                else
                                                {
                                                    find = new(find.ToString().TrimEnd('/'));
                                                    if (editorChar == j) { find.Append($"\u001b[46;1m\u001b[38;5;8m/\u001b[0m\u001b[38;5;8m{themeAnsi}/"); }
                                                    else if (editorChar == j + 1) { find.Append("\u001b[38;5;8m/\u001b[46;1m/\u001b[0m"); }
                                                    else { find.Append("\u001b[38;5;8m//"); }
                                                    while (true)
                                                    {
                                                        j++;
                                                        try { character = editorBufferBuffer[j]; }
                                                        catch (IndexOutOfRangeException) { break; }
                                                        find.Append(character);
                                                    }
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                j++;
                                                if (editorBufferBuffer[j + 1] != '/')
                                                {
                                                    if (editorChar == j + 1) { find.Append("\u001b[46;1m/\u001b[0m"); }
                                                    else { find.Append(character); }
                                                }
                                            }
                                        }
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        j++;
                                        if (editorBufferBuffer[j + 1] != '/')
                                        {
                                            if (editorChar == j + 1) { find.Append("\u001b[46;1m/\u001b[0m"); }
                                            else { find.Append(character); }
                                        }
                                    }
                                }
                                editorBufferLine = $"\u001b[38;5;8m{new string(' ', getLineCount - editorCurrentLine.ToString().Length)}\u001b[93m{editorCurrentLine}\u001b[1;38;5;8m    |\u001b[0m{themeAnsi}{find}\u001b[0m{themeAnsi}{filler}\u001b[0m{themeAnsi}\n";
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            thist = true;
                            editorBufferLine = $"\u001b[38;5;8m ~{new string(' ', editorLinesCount.ToString().Length + 2)}\u001b[1;38;5;8m|\u001b[0m{themeAnsi}{new string(' ', terminalWidth - (editorLinesCount.ToString().Length + 5))}\n";
                        }
                    }
                    editorBuffer.Append(editorBufferLine);
                }
                Console.SetCursorPosition(0, 0);
                Console.WriteLine(editorBuffer.ToString().TrimEnd('\n'));
                try
                {
                    taskbar = $"\u001b[48;2;56;113;228m\u001b[35;1m | Save | New Sprite | Open File |{new string(' ', terminalWidth - (" | Save | New Sprite | Open File |".Length + 1 + taskbarMessage.Length))}{taskbarMessage} \u001b[0m";
                }
                catch (ArgumentOutOfRangeException)
                {
                    taskbar = $"\u001b[48;2;56;113;228m\u001b[35;1m | Save | New Sprite | Open File |{new string(' ', terminalWidth)}{taskbarMessage} \u001b[0m";
                }
                Console.Write(taskbar);
            }

            void EditorPrintLoop()
            {
                while (inEditor)
                {
                    terminalHeight = Console.WindowHeight;
                    terminalWidth = Console.WindowWidth;
                    capsLock = Console.CapsLock;
                    shift = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
                    ctrl = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                    EditorPrint(terminalHeight);
                }
            }

            void Editor()
            {
                int mouseThing;
                while (inEditor)
                {
                    for (int ee = 0; ee < 2; ee++)
                    {
                        ReadConsoleInput(GetStdHandle(STD_INPUT_HANDLE), out inputRecord, 100, out int uevents);
                        if (inputRecord.Event.MouseEvent.dwEventFlags == MouseEvents.MOUSE_WHEELED)
                        {
                            mouseThing = int.Parse(inputRecord.Event.MouseEvent.dwButtonState.ToString());
                            if (mouseThing == 8388608) { OnKeyPress("UpArrow"); }
                            else { OnKeyPress("DownArrow"); }
                            break;
                        }
                    }
                    if (inputRecord.EventType == InputEventTypeFlag.KEY_EVENT)
                    {
                        modifierKeys = inputRecord.Event.KeyEvent.dwControlKeyState;
                        OnKeyPress(AsciiTable[inputRecord.Event.KeyEvent.uChar.AsciiChar]);
                    }
                }
            }

            Thread mouseClicking;
            Thread cursorBlinkLoop;
            Thread editorprint;

            void StartThreads()
            {
                mouseClicking = new(new ThreadStart(mouseClicks));
                mouseClicking.SetApartmentState(ApartmentState.STA);
                mouseClicking.Start();
                cursorBlinkLoop = new(new ThreadStart(Increment));
                cursorBlinkLoop.Start();
                editorprint = new(new ThreadStart(EditorPrintLoop));
                editorprint.Start();
            }

            StartThreads();

            Console.CursorVisible = false;
            while (true)
            {
                if (state == "edit")
                {
                    inEditor = true;
                    Editor();
                }
                else if (state == "tree")
                {
                    string oldFolder = folder;
                    OpenFileDialog fd = new()
                    {
                        Title = "Choose a file.",
                        InitialDirectory = $"{Directory.GetParent(Directory.GetCurrentDirectory())}\\",
                        Filter = "ScratchScript 1 Files (*.ss1)|*.ss1|All Files (*.*)|*.*"
                    };
                    fd.ShowDialog();
                    folder = fd.FileName;
                    if (folder == "")
                    {
                        Error("Empty path.");
                        Environment.Exit(0);
                    }
                    string projectDir = $"{oldFolder}\\project.ss1";
                    File.Delete(projectDir);
                    foreach (string item in editorLines) { WriteToFile(projectDir, item.TrimEnd(' ')); }
                    editorLines.Clear();
                    string[] fff = File.ReadAllLines(folder);
                    folder = folder.Replace("\\project.ss1", "");
                    foreach (string line in fff) { editorLines.Add($"{line} "); }
                    editorLinesCount = editorLines.Count;
                    editorLinesWithSyntax.Clear();
                    Console.WriteLine("Loading...");
                    foreach (string line in editorLines) { editorLinesWithSyntax.Add(AddSyntax(line, false)); }
                    editorCurrentLine = 1;
                    editorChar = editorLines[0].Length;
                    realLine = 1;
                    quoteComplete = false;
                    singleQuoteComplete = false;
                    parenthesisComplete = 0;
                    capsLock = false;
                    state = "edit";
                    inEditor = true;
                    StartThreads();
                }
                else if (state == "new")
                {
                    Directory.SetCurrentDirectory("..");
                    int z = 0;
                    while (true)
                    {
                        z++;
                        if (!Directory.Exists($"Sprite{z}")) { break; }
                    }
                    Directory.CreateDirectory($"Sprite{z}\\assets");
                    Directory.SetCurrentDirectory($"Sprite{z}");
                    WriteToFile("project.ss1", "ss1");
                    editorLines.Clear();
                    editorLines.Add("ss1 ");
                    editorLinesCount = editorLines.Count;
                    editorLinesWithSyntax.Clear();
                    editorLinesWithSyntax.Add(AddSyntax("ss1 ", false));
                    editorCurrentLine = 1;
                    editorChar = editorLines[0].Length;
                    realLine = 1;
                    quoteComplete = false;
                    singleQuoteComplete = false;
                    parenthesisComplete = 0;
                    capsLock = false;
                    state = "edit";
                    taskbarMessage = "Created new sprite.";
                    inEditor = true;
                    StartThreads();
                }
            }
        }

        private const uint ENABLE_QUICK_EDIT = 0x0040;
        private const uint ENABLE_MOUSE_INPUT = 0x0010;
        private const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        private const uint ENABLE_WINDOW_INPUT = 0x0008;
        private const int STD_INPUT_HANDLE = -10;
        private const int STD_OUTPUT_HANDLE = -11;

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr GetStdHandle(int nStdHandle);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        internal static bool InitializeConsole()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
            if (!GetConsoleMode(consoleHandle, out uint consoleMode)) { return false; }
            consoleMode &= ~ENABLE_QUICK_EDIT;
            consoleMode |= ENABLE_MOUSE_INPUT;
            consoleMode |= ENABLE_EXTENDED_FLAGS;
            consoleMode |= ENABLE_WINDOW_INPUT;
            return SetConsoleMode(consoleHandle, consoleMode);
        }

        [LibraryImport("user32.dll")]
        private static partial IntPtr GetForegroundWindow();

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(IntPtr hWnd, int cmdShow);

        private static void Maximize()
        {
            ShowWindow(GetForegroundWindow(), 3);
        }
    }
}