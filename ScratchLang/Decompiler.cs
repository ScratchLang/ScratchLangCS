using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static ScratchLang.Functions;
using static ScratchLang.GlobalVariables;

namespace Decompiler
{
    public static partial class Decompiler
    {
        public static void Decompile(string[] args, bool args2, string realCWD)
        {
            const bool removeJson = false;
            string dv2dt = "0";

            void Dte(string arg1)
            {
                if (dv2dt == "1") { Console.WriteLine(arg1); }
            }

            if (File.Exists("var\\devmode")) { dv2dt = "1"; }
            Console.WriteLine($"\n{RED}Decompiler C# Version 1.0{NC}");
            string dcd = "Stage";
            if (dv2dt == "1")
            {
                Console.WriteLine($"\n{RED}Todo list:{NC}\nHigher priorities go first.\n-------------------------------------------------------------------\n{RED}* {NC}Port Python Decompiler to C# with optimizations\n\nOrder of items may change.\n-------------------------------------------------------------------\n");
            }
            Console.WriteLine("\nSelect the .sb3 you want to decompile.\n");
            string sb3file;
            if (!args2)
            {
                Thread.Sleep(2000);
                OpenFileDialog dialog = new()
                {
                    Filter = "Scratch SB3 (*.sb3)|*.sb3|All Files (*.*)|*.*",
                    InitialDirectory = $"{GetDownloadsPath()}\\",
                    Title = "Choose a Scratch project."
                };
                dialog.ShowDialog();
                sb3file = dialog.FileName;
            }
            else
            {
                sb3file = args[1];
                Directory.SetCurrentDirectory(realCWD);
                if (!File.Exists(sb3file))
                {
                    Error($"File ({sb3file}) does not exist.");
                    Environment.Exit(0);
                }
            }
            if (sb3file == "")
            {
                Error("Empty path.");
                Environment.Exit(0);
            }
            Console.WriteLine("Enter name of project.");
            string name = Console.ReadLine();
            Console.WriteLine("");
            if (name == "")
            {
                Error("Project name cannot be empty.");
                Environment.Exit(0);
            }
            Directory.SetCurrentDirectory("..");
            if (!Directory.Exists("projects")) { Directory.CreateDirectory("projects"); }
            Directory.SetCurrentDirectory("projects");
            if (Directory.Exists(name))
            {
                Console.WriteLine($"Project {name} already exists. Replace? [Y/N]");
                string anss = GetInput();
                if (anss.ToLower() == "y") { Directory.Delete(name, true); }
                else { Environment.Exit(0); }
                Console.WriteLine("");
            }
            Console.WriteLine("Decompiling project...\n");
            Thread.Sleep(1000);
            Console.WriteLine("Extracting sb3...\n");
            Console.WriteLine(sb3file);
            ZipFile.ExtractToDirectory(sb3file, name);
            Directory.SetCurrentDirectory(name);
            string baseDir = Directory.GetCurrentDirectory();
            WriteToFile(".maindir", "Please don't remove this file.");
            Directory.CreateDirectory("Stage\\assets");
            string jsonfile = File.ReadAllText("project.json");
            int i = 55;
            int kv = -1;
            bool b;
            string character;
            string next = "";
            StringBuilder varname = new();
            StringBuilder varvalue = new();
            StringBuilder con = new();
            string word;
            bool novars;

            void GetCharacter(string a1)
            {
                character = "";
                b = false;
                character = jsonfile[i].ToString();
                if ($"-{character}" == a1) { b = true; }
            }

            string ExtractData()
            {
                StringBuilder word = new("");
                while (true)
                {
                    i++;
                    GetCharacter("-\"");
                    if (b) { break; }
                    word.Append(character);
                }
                return word.ToString();
            }

            void Nq(int num = 1)
            {
                for (int k = 0; k < num; k++)
                {
                    while (true)
                    {
                        i++;
                        GetCharacter("-\"");
                        if (b) { break; }
                    }
                }
            }

            void Start(string a1 = "")
            {
                Nq(2);
                i += 2;
                GetCharacter("-\"");
                if (!b && a1 == "") { next = "fin"; }
                else
                {
                    varname.Clear();
                    while (true)
                    {
                        i++;
                        GetCharacter("-\"");
                        if (b) { break; }
                        varname.Append(character);
                    }
                    if (a1 == "")
                    {
                        next = varname.ToString();
                        Dte($"next: {next}, {varname}");
                    }
                }
                Nq(2);
                i += 2;
                GetCharacter("-\"");
                if (!b && a1 == "")
                {
                    WriteToFile($"{dcd}\\project.ss1", "\\nscript");
                    Console.WriteLine("");
                }
                Dte($"{next}|");
            }

            void FindBlock(string wordFind) { i = jsonfile.IndexOf($"\"{wordFind}\":{{\"opcode\":"); }

            void WriteBlock(string block)
            {
                WriteToFile($"{dcd}\\project.ss1", block);
                Console.WriteLine($"{RED}Added block: {NC} \"{block}\"");
            }

            void AddBlock(string a1, bool dcon = false)
            {
                string cnext = "";
                Start();
                // Global Blocks
                switch (a1)
                {
                    case "looks_switchbackdropto":
                        Nq(5);
                        FindBlock(ExtractData());
                        Nq(18);
                        word = ExtractData();
                        WriteBlock($"switch backdrop to (\"{word}\")");
                        break;

                    default:
                        Console.WriteLine($"{RED}Unknown block: \"{a1}\" Skipping.{NC}");
                        WriteToFile($"{dcd}\\project.ss1", $"DECOMPERR: Unknown block: \"{a1}\"");
                        break;
                }
            }

            while (true)
            {
                Console.WriteLine("Defining variables...\n");
                int di = i;
                Nq();
                word = ExtractData();
                i = di;
                if (word != "lists")
                {
                    while (true)
                    {
                        Nq(2);
                        novars = false;
                        while (true)
                        {
                            i++;
                            GetCharacter("-[");
                            if (b) { break; }
                            GetCharacter("-}");
                            if (b)
                            {
                                novars = true;
                                break;
                            }
                        }
                        if (!novars)
                        {
                            i++;
                            varname.Clear();
                            while (true)
                            {
                                i++;
                                GetCharacter("-\"");
                                if (b) { break; }
                                varname.Append(character);
                            }
                            i++;
                            varvalue.Clear();
                            while (true)
                            {
                                i++;
                                GetCharacter("-]");
                                if (b) { break; }
                                varvalue.Append(character);
                            }
                            if (!File.Exists($"{dcd}\\project.ss1")) { WriteToFile($"{dcd}\\project.ss1", "// There should be no empty lines.\nss1\n\\prep"); }
                            WriteToFile($"{dcd}\\project.ss1", $"var: {varname}={varvalue}");
                            Console.WriteLine($"{RED}Added variable: {NC}\"{varname}\".\n{RED}Value: {NC}{varvalue}\n");
                            varname.Clear();
                            i++;
                            GetCharacter("-}");
                            if (b) { break; }
                            i -= 2;
                        }
                        else
                        {
                            WriteToFile($"{dcd}\\project.ss1", "// There should be no empty lines.\nss1\n\\prep");
                            break;
                        }
                    }
                }
                Console.WriteLine("Building lists...\n");
                i = di;
                while (true)
                {
                    word = ExtractData();
                    if (word == "lists") { break; }
                }
                i++;
                while (true)
                {
                    i += 2;
                    GetCharacter("-}");
                    if (b)
                    {
                        novars = true;
                        i -= 2;
                    }
                    else
                    {
                        i -= 2;
                        Nq(2);
                        novars = false;
                        while (true)
                        {
                            i++;
                            GetCharacter("-[");
                            if (b) { break; }
                            GetCharacter("-}");
                            if (b)
                            {
                                novars = true;
                                break;
                            }
                        }
                    }
                    if (!novars)
                    {
                        i++;
                        StringBuilder listname = new("");
                        while (true)
                        {
                            i++;
                            GetCharacter("-\"");
                            if (b) { break; }
                            listname.Append(character);
                        }
                        i += 3;
                        GetCharacter("-]");
                        if (!b)
                        {
                            List<string> listContents = new();
                            while (true)
                            {
                                GetCharacter("-]");
                                if (b) { break; }
                                if (character == "\"")
                                {
                                    varname.Clear();
                                    varname.Append(ExtractData());
                                    i++;
                                    GetCharacter("-]");
                                    if (!b)
                                    {
                                        listContents.Add($"\"{varname}\", ");
                                        i++;
                                    }
                                    else
                                    {
                                        listContents.Add($"\"{varname}\"");
                                        break;
                                    }
                                    GetCharacter("- ");
                                    if (b) { i++; }
                                }
                                else
                                {
                                    i--;
                                    varname.Clear();
                                    while (true)
                                    {
                                        i++;
                                        GetCharacter("-,");
                                        if (b) { break; }
                                        GetCharacter("-]");
                                        if (b) { break; }
                                        varname.Append(character);
                                    }
                                    GetCharacter("-]");
                                    if (!b) { listContents.Add($"\"{varname}\", "); }
                                    else
                                    {
                                        listContents.Add($"\"{varname}\"");
                                        break;
                                    }
                                    i++;
                                    GetCharacter("- ");
                                    if (b) { i++; }
                                }
                            }
                            StringBuilder list = new("");
                            foreach (string listing in listContents) { list.Append(listing.Replace("\n", "")); }
                            WriteToFile($"{dcd}\\project.ss1", $"list: {listname}={list}");
                            Console.WriteLine($"{RED}Added list: {NC}\"{listname}\".\n{RED}Contents: {NC}{list}");
                        }
                        else
                        {
                            WriteToFile($"{dcd}\\project.ss1", $"list: {listname}=,");
                            Console.WriteLine($"{RED}Added list: {NC}\"{listname}\".\n{RED}Contents: {NC}Nothing.");
                            if (novars) { break; }
                        }
                    }
                    if (novars) { break; }
                    i += 2;
                    GetCharacter("-}");
                    if (b) { break; }
                }
                Console.WriteLine("Loading broadcasts...\n");
                while (true)
                {
                    StringBuilder testForBreak = new("");
                    while (true)
                    {
                        i++;
                        GetCharacter("-\"");
                        if (b) { break; }
                        testForBreak.Append(character);
                    }
                    if (testForBreak.ToString() == "broadcasts") { break; }
                }
                novars = false;
                i += 3;
                GetCharacter("-}");
                if (b)
                {
                    novars = true;
                    i -= 2;
                }
                else
                {
                    i -= 2;
                    while (true)
                    {
                        i++;
                        GetCharacter("-\"");
                        if (b) { break; }
                        GetCharacter("-}");
                        if (b)
                        {
                            novars = true;
                            break;
                        }
                    }
                    Nq();
                }
                if (!novars)
                {
                    while (true)
                    {
                        i++;
                        Nq();
                        varname.Clear();
                        while (true)
                        {
                            i++;
                            GetCharacter("-\"");
                            if (b) { break; }
                            varname.Append(character);
                        }
                        WriteToFile($"{dcd}\\project.ss1", $"broadcast: {varname}");
                        Console.WriteLine($"{RED}Loaded broadcast: {NC}\"{varname}\"\n");
                        i++;
                        GetCharacter("-}");
                        if (b) { break; }
                        i += 2;
                        Nq();
                    }
                }
                Console.WriteLine("Making blocks...\n");
                int k = kv;
                bool done = false;
                while (true)
                {
                    i = k;
                    while (true)
                    {
                        word = ExtractData();
                        if (word == "parent")
                        {
                            k = i;
                            i += 2;
                            GetCharacter("-\"");
                            if (!b) { break; }
                        }
                        if (word == "comments")
                        {
                            done = true;
                            break;
                        }
                    }
                    if (!done)
                    {
                        while (true)
                        {
                            i--;
                            GetCharacter("-{");
                            if (b) { break; }
                        }
                        i++;
                        Nq(2);
                        word = ExtractData();
                        con.Clear();
                        AddBlock(word, true);
                    }
                    else { break; }
                }
                di = i;
                Console.WriteLine("\nAdding assets...");
                string assetsDir = $"{baseDir}\\assets_key.yaml";
                WriteToFile(assetsDir, $"{dcd}:");
                while (true)
                {
                    word = ExtractData();
                    if (word == "costumes") { break; }
                }
                while (true)
                {
                    while (true)
                    {
                        word = ExtractData();
                        if (word is "md5ext" or "sounds") { break; }
                    }
                    if (word == "sounds") { break; }
                    Nq();
                    string asset_file = ExtractData();
                    try
                    {
                        WriteToFile(assetsDir, $"  - \"{asset_file}\"");
                        File.Move($"./{asset_file}", $"{dcd}\\assets\\{asset_file}");
                        Console.WriteLine($"{asset_file} >> {dcd}\\assets\\{asset_file}");
                        Nq(2);
                        string a1d = ExtractData();
                        Nq();
                        string a2d = ExtractData();
                        WriteToFile(assetsDir, $"  - \"{a1d.TrimStart(':').TrimEnd(',')}\"");
                        WriteToFile(assetsDir, $"  - \"{a2d.TrimStart(':').TrimEnd("},{".ToCharArray()).TrimEnd("}]".ToCharArray())}\"");
                    }
                    catch (FileNotFoundException)
                    {
                        // pass
                    }
                }
                if (word != "sounds")
                {
                    while (true)
                    {
                        word = ExtractData();
                        if (word == "sounds") { break; }
                    }
                }
                while (true)
                {
                    while (true)
                    {
                        word = ExtractData();
                        if (word is "md5ext" or "volume") { break; }
                    }
                    if (word == "volume") { break; }
                    Nq();
                    string asset_file = ExtractData();
                    try
                    {
                        WriteToFile(assetsDir, $"  - \"{asset_file}\"");
                        File.Move($"./{asset_file}", $"{dcd}\\assets\\{asset_file}");
                        Console.WriteLine($"{asset_file} >> {dcd}\\assets\\{asset_file}");
                    }
                    catch (FileNotFoundException)
                    {
                        // pass
                    }
                }
                Console.WriteLine("\nFormatting code...\n");
                if (!File.Exists($"{dcd}\\project.ss1")) { WriteToFile($"{dcd}/project.ss1", "// There should be no empty lines.\nss1"); }
                string[] f = File.ReadAllLines($"{dcd}\\project.ss1");
                string spaces = "";
                i = 0;
                int q = 0;
                int flen = f.Length;
                int proglen = 55;
                int tabSize = 2;
                float per;
                foreach (string line in f)
                {
                    q++;
                    per = (float)q / flen;
                    Console.WriteLine($"\u001b[A[{new string('#', (int)Math.Round(proglen * per))}{new string(' ', (int)(proglen - Math.Round(proglen * per)))}] {Math.Round(per * 100)}%");
                    if (line.Contains('}') && line.Contains('{'))
                    {
                        i--;
                        spaces = string.Concat(Enumerable.Repeat(new string(' ', tabSize), i));
                        i++;
                    }
                    else if (line.Contains('{'))
                    {
                        spaces = string.Concat(Enumerable.Repeat(new string(' ', tabSize), i));
                        i++;
                    }
                    else if (line.Contains('}'))
                    {
                        i--;
                        spaces = string.Concat(Enumerable.Repeat(new string(' ', tabSize), i));
                    }
                    else { spaces = string.Concat(Enumerable.Repeat(new string(' ', tabSize), i)); }
                    if (line != "") { WriteToFile($"{dcd}\\a.txt", $"{spaces}{line}"); }
                }
                File.Delete($"{dcd}\\project.ss1");
                File.Move($"{dcd}\\a.txt", $"{dcd}\\project.ss1");
                i = di;
                while (true)
                {
                    word = ExtractData();
                    if (word is "isStage" or "agent") { break; }
                }
                if (word == "agent") { break; }
                kv = i;
                Nq(3);
                dcd = ExtractData().Replace(' ', '-');
                string[] replacings = { "/", "\\", "<", ">", ":", "\"", "|", "?", "*" };
                foreach (string replacing in replacings) { dcd = dcd.Replace(replacing, ""); } // makes sure the folder name is valid
                Directory.CreateDirectory($"{dcd}\\assets");
                Nq(2);
                i += 2;
                Console.WriteLine($"Now entering: {dcd}");
            }
            Directory.SetCurrentDirectory("..");
            string dir = Directory.GetCurrentDirectory();
            if (removeJson) { File.Delete($"{dir}\\{name}\\proejct.json"); }
            Console.WriteLine($"{RED}Your project can be found in {dir}\\{name}.{NC}\nOpen in Scratchlang editor? [Y/N]");
            Console.WriteLine("I'm sorry, but the C# version of the editor is not available yet. You can still use the Python version though.");
            Environment.Exit(0);
        }

        private static Guid FolderDownloads = new("374DE290-123F-4565-9164-39C4925E467B");

        [LibraryImport("shell32.dll")]
        private static partial int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);

        public static string GetDownloadsPath()
        {
            if (Environment.OSVersion.Version.Major < 6) { throw new NotSupportedException(); }
            IntPtr pathPtr = IntPtr.Zero;
            try
            {
                SHGetKnownFolderPath(ref FolderDownloads, 0, IntPtr.Zero, out pathPtr);
                return Marshal.PtrToStringUni(pathPtr);
            }
            finally { Marshal.FreeCoTaskMem(pathPtr); }
        }
    }
}