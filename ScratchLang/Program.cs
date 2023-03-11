using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using static ScratchLang.Functions;
using static ScratchLang.GlobalVariables;
[assembly: DisableRuntimeMarshalling]

namespace ScratchLang
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            bool arg = false;
            bool arg2 = false;
            string realCWD = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory($"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\\data\\mainscripts");

            async void StartCS(string a1 = "")
            {
                Directory.SetCurrentDirectory("..");
                string ver = File.ReadAllText("version");
                Directory.SetCurrentDirectory("mainscripts");
                if (!File.Exists("var\\asked") && !File.Exists("var\\vc"))
                {
                    Console.WriteLine("Would you like ScratchLang to check its version every time you start it? [Y/N]");
                    string ff = GetInput();
                    if (ff.ToLower() == "y")
                    {
                        WriteToFile("var\\vc", "Don't remove this file please.");
                    }
                    WriteToFile("var\\asked", "Don't remove this file please.");
                }
                try
                {
                    if (File.Exists("var\\vc") && a1 != "nope")
                    {
                        Console.WriteLine("Checking version...");
                        if (File.Exists("version"))
                        {
                            File.Delete("version");
                        }
                        using (HttpClient client = new())
                        {
                            using Task<Stream> s = client.GetStreamAsync("https://raw.githubusercontent.com/ScratchLang/ScratchLang/main/version");
                            using FileStream fs = new("version", FileMode.OpenOrCreate);
                            s.Result.CopyTo(fs);
                        }
                        string utd = "1";
                        string gver = File.ReadAllText("version");
                        if (ver != gver) { utd = "0"; }
                        if (utd == "0")
                        {
                            Console.WriteLine($"Your version of ScratchLang ({ver}) is outdated. The current version is {gver}. Would you like to update? [Y/N]");
                            string hh = GetInput();
                            if (hh.ToLower() == "y")
                            {
                                ProcessStartInfo info = new("git", "pull origin main")
                                {
                                    UseShellExecute = false
                                };
                                Process? proc = Process.Start(info);
                                proc.WaitForExit();
                            }
                            File.Delete("version");
                        }
                    }
                }
                catch (AggregateException) { Console.WriteLine("Failed to check version."); }
                try
                {
                    if (args[0] == "--help")
                    {
                        Console.WriteLine(@"scratchlang.exe

  -1                Create a project.
  -2                Remove a project.
  -3                Compile a project.
  -4                Decompile a project.
  -5                Export a project.
  -6                Import a project.
  --debug [FILE]    Debug a ScratchScript file. Currently not available.
  --help            Display this help message.
  --edit            Edit a ScratchLang project.");
                        Environment.Exit(0);
                    }
                    arg = true;
                    try
                    {
                        if (args[1] == "")
                        {
                            // Check for 2nd argument
                        }
                        arg2 = true;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        // Do nothing
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    // Do nothing
                }
                Console.Clear();
                Console.WriteLine($"{P}\n      /$$$$$$                                 /$$               /$$       /$$                                    \r\n     /$$__  $$                               | $$              | $$      | $$                                    \r\n    | $$  \\__/  /$$$$$$$  /$$$$$$  /$$$$$$  /$$$$$$    /$$$$$$$| $$$$$$$ | $$        /$$$$$$  /$$$$$$$   /$$$$$$ \r\n    |  $$$$$$  /$$_____/ /$$__  $$|____  $$|_  $$_/   /$$_____/| $$__  $$| $$       |____  $$| $$__  $$ /$$__  $$\r\n     \\____  $$| $$      | $$  \\__/ /$$$$$$$  | $$    | $$      | $$  \\ $$| $$        /$$$$$$$| $$  \\ $$| $$  \\ $$\r\n     /$$  \\ $$| $$      | $$      /$$__  $$  | $$ /$$| $$      | $$  | $$| $$       /$$__  $$| $$  | $$| $$  | $$\r\n    |  $$$$$$/|  $$$$$$$| $$     |  $$$$$$$  |  $$$$/|  $$$$$$$| $$  | $$| $$$$$$$$|  $$$$$$$| $$  | $$|  $$$$$$$\r\n     \\______/  \\_______/|__/      \\_______/   \\___/   \\_______/|__/  |__/|________/ \\_______/|__/  |__/ \\____  $$\r\n                                                                                                        /$$  \\ $$\r\n                                                                                                       |  $$$$$$/\r\n                                                                                                        \\______/ {NC}\n"); // Print the logo
                if (!arg || a1 == "nope")
                {
                    Console.WriteLine($"Welcome to ScratchLang {ver}. (Name suggested by @MagicCrayon9342 on Scratch.)");
                    InputLoop();
                }
                else
                {
                    if (args[0] == "--edit") { SL.Edit(realCWD); }
                    else { InputLoop(args[0]); }
                }
                Directory.SetCurrentDirectory("..");
                if (Directory.Exists("projects") && !Directory.EnumerateFileSystemEntries("projects").Any())
                { Directory.Delete("projects"); }
            }

            void InputLoop(string ia1 = "")
            {
                string inp = "";
                string a1 = ia1 != "" ? ia1.Replace("-", "") : "";
                Console.WriteLine("");
                if (a1 == "")
                {
                    Console.WriteLine(@"1) Create a project.
2) Remove a project.
3) Compile a project.
4) Decompile a .sb3 file.
5) Export project.
6) Import project.
7) Are options 3 and 4 not working? Input 7 to install dependencies.");
                    if (!File.Exists("var\\devmode"))
                    {
                        Console.WriteLine(@"8. Enable Developer Mode.
9) Exit.");
                    }
                    else
                    {
                        Console.WriteLine(@"8. Disable Developer Mode.
9) Delete all variables.
0) Prepare for commit and push.
-). Exit.");
                    }
                    inp = GetInput();
                }
                else
                {
                    try
                    {
                        if (int.Parse(a1) is > 0 and < 7) { inp = a1; }
                        else
                        {
                            Error($"{ia1} is not an argument.");
                            Thread.Sleep(2000);
                            StartCS("nope");
                        }
                    }
                    catch (FormatException)
                    {
                        Error($"{ia1} is not an argument.");
                        Thread.Sleep(2000);
                        StartCS("nope");
                    }
                }
                if (inp == "1")
                {
                    Console.WriteLine("\nName your project. Keep in mind that it cannot be empty or it will not be created properly.\n");
                    string name = Console.ReadLine();
                    Directory.SetCurrentDirectory("..");
                    if (name == "")
                    {
                        Error("Project name empty.");
                        Environment.Exit(0);
                    }
                    else if (Directory.Exists($"projects\\{name}"))
                    {
                        Console.WriteLine($"Project {name} already exists. Replace? [Y/N]");
                        string yessor = GetInput();
                        if (yessor.ToLower() == "y") { Directory.Delete($"projects\\{name}", true); }
                        else if (yessor.ToLower() == "n") { Environment.Exit(0); }
                        else
                        {
                            Error($"{yessor} is not an input.");
                            Environment.Exit(0);
                        }
                    }
                    Console.WriteLine($"You named your project {name}. If you want to rename it, use the File Explorer.");
                    Directory.CreateDirectory($"projects\\{name}\\Stage\\assets");
                    File.Copy("resources\\cd21514d0531fdffb22204e0ec5ed84a.svg", $"projects\\{name}\\Stage\\assets\\cd21514d0531fdffb22204e0ec5ed84a.svg");
                    WriteToFile($"projects\\{name}\\.maindir", "Please don't remove this file.");
                    WriteToFile($"projects\\{name}\\Stage\\project.ss1", "// There should be no empty lines.\nss1");
                    Directory.CreateDirectory($"projects\\{name}\\Sprite1\\assets");
                    WriteToFile($"projects\\{name}\\Sprite1\\project.ss1", "// There should be no empty lines.\nss1");
                    File.Copy("resources\\341ff8639e74404142c11ad52929b021.svg", $"projects\\{name}\\Sprite1\\assets\\341ff8639e74404142c11ad52929b021.svg");
                    Directory.SetCurrentDirectory("mainscripts");
                    SL.Edit(realCWD, $"..\\projects\\{name}");
                }
                else if (inp == "2")
                {
                    Directory.SetCurrentDirectory("..");
                    if (!Directory.Exists("projects"))
                    {
                        Error("There are no projects to delete.");
                        Environment.Exit(0);
                    }
                    Directory.SetCurrentDirectory("projects");
                    DirectoryInfo d = new(Directory.GetCurrentDirectory());
                    DirectoryInfo[] dirs = d.GetDirectories();
                    Console.WriteLine("");
                    foreach (DirectoryInfo fi in dirs) { Console.WriteLine(fi.Name); }
                    Console.WriteLine("\nChoose a project to get rid of, or input nothing to cancel.\n");
                    string pgrd = Console.ReadLine();
                    if (pgrd != "")
                    {
                        if (Directory.Exists(pgrd)) { Directory.Delete(pgrd, true); }
                        else { Error($"Directory {pgrd} does not exist."); }
                    }
                    Environment.Exit(0);
                }
                else if (inp == "3")
                {
                    // Compilation scripts go here
                    SL.Compile();
                    Environment.Exit(0);
                }
                else if (inp == "4")
                {
                    SL.Decompile(args, arg2, realCWD);
                    Environment.Exit(0);
                }
                else if (inp == "5")
                {
                    Directory.SetCurrentDirectory("..");
                    if (!Directory.Exists("projects"))
                    {
                        Error("There are no projects to export.");
                        Environment.Exit(0);
                    }
                    Directory.SetCurrentDirectory("projects");
                    DirectoryInfo d = new(Directory.GetCurrentDirectory());
                    DirectoryInfo[] dirs = d.GetDirectories();
                    Console.WriteLine("");
                    foreach (DirectoryInfo fi in dirs) { Console.WriteLine(fi.Name); }
                    Console.WriteLine("\nChoose a project to export, or input nothing to cancel.\n");
                    string pgrd = Console.ReadLine();
                    if (pgrd != "")
                    {
                        if (Directory.Exists(pgrd))
                        {
                            ZipFile.CreateFromDirectory(pgrd, $"{pgrd}.ssa");
                            Directory.SetCurrentDirectory("..");
                            File.Move($"projects\\{pgrd}.ssa", $"exports\\{pgrd}.ssa");
                            Console.WriteLine($"Your project {pgrd}.ssa can be found in the exports folder.");
                        }
                        else { Error($"Directory {pgrd} does not exist."); }
                    }
                    Environment.Exit(0);
                }
                else if (inp == "6")
                {
                    OpenFileDialog dialog = new()
                    {
                        Filter = "ScratchScript Archive (*.ssa)|*.ssa|All Files (*.*)|*.*",
                        InitialDirectory = $"{Directory.GetCurrentDirectory()}\\..\\exports\\",
                        Title = "Choose a ScratchScript Archive."
                    };
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Directory.SetCurrentDirectory("..");
                        if (!Directory.Exists("projects")) { Directory.CreateDirectory("projects"); }
                        Directory.SetCurrentDirectory("projects");
                        try { ZipFile.ExtractToDirectory(dialog.FileName, Path.GetFileNameWithoutExtension(dialog.FileName)); }
                        catch (IOException)
                        {
                            Console.WriteLine($"Project {Path.GetFileNameWithoutExtension(dialog.FileName)} already exists. Replace? [Y/N]");
                            string replace = GetInput();
                            if (replace.ToLower() == "y")
                            {
                                Directory.Delete(Path.GetFileNameWithoutExtension(dialog.FileName), true);
                                ZipFile.ExtractToDirectory(dialog.FileName, Path.GetFileNameWithoutExtension(dialog.FileName));
                            }
                        }
                        Console.WriteLine("Remove .ssa file? [Y/N]");
                        string f = GetInput();
                        if (f.ToLower() == "y") { File.Delete(dialog.FileName); }
                    }
                    else { Error("No file selected."); }
                    Environment.Exit(0);
                }
                else if (inp == "7") 
                { 
                    Console.WriteLine("\nScratchLang can't automatically install dependencies for you yet, but all you should need it Git for Windows. Search it up and install it yourself."); 
                    Environment.Exit(0);
                }
                else if (inp == "8")
                {
                    if (!File.Exists("var\\devmode")) { WriteToFile("var\\devmode", "This is the file that tells ScratchLang you're in dev mode. You can manually remove it to disable dev mode."); }
                    else { File.Delete("var\\devmode"); }
                    StartCS("nope");
                    Environment.Exit(0);
                }
                else if (inp == "9")
                {
                    if (!File.Exists("var\\devmode")) { Environment.Exit(0); }
                    else
                    {
                        string[] varlists = { "var\\devmode", "var\\zenity", "var\\ds", "var\\asked", "var\\vc", "var\\pe" };
                        foreach (string? remove in from string remove in varlists
                                                   where File.Exists(remove)
                                                   select remove)
                        {
                            File.Delete(remove);
                        }
                    }
                    Environment.Exit(0);
                }
                if (File.Exists("var\\devmode"))
                {
                    if (inp.ToLower() == "0")
                    {
                        string[] varlists = { "var\\devmode", "var\\zenity", "var\\ds", "var\\asked", "var\\vc", "var\\pe" };
                        foreach (string? remove in from string remove in varlists
                                                   where File.Exists(remove)
                                                   select remove)
                        {
                            File.Delete(remove);
                        }
                        Directory.SetCurrentDirectory("..");
                        if (Directory.Exists("projects")) { Directory.Delete("projects", true); }
                        if (Directory.Exists("exports")) { Directory.Delete("exports", true); }
                        Directory.CreateDirectory("exports");
                        WriteToFile("exports\\.temp", "");
                    }
                    else if (inp.ToLower() == "-") { Environment.Exit(0); }
                    else
                    {
                        Error($"{inp} is not an input.");
                        InputLoop();
                    }
                }
                else
                {
                    Error($"{inp} is not an input.");
                    InputLoop();
                }
            }
            StartCS();
        }
    }
}