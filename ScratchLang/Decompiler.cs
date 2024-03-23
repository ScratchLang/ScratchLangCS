using System.IO.Compression;
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
            int j;
            int kv = -1;
            string character;
            string next = "";
            StringBuilder varname = new();
            StringBuilder varvalue = new();
            StringBuilder con = new();
            string word;
            bool novars;

            bool GetCharacter(string a1)
            {
                character = "";
                character = jsonfile[i].ToString();
                return $"-{character}" == a1;
            }

            string ExtractData()
            {
                StringBuilder word = new("");
                while (true)
                {
                    i++;
                    if (GetCharacter("-\"")) { break; }
                    word.Append(character);
                }
                return word.ToString();
            }

            void Nq(int num = 1, string special = "\"")
            {
                for (int k = 0; k < num; k++)
                {
                    while (true)
                    {
                        i++;
                        if (GetCharacter($"-{special}")) { break; }
                    }
                }
            }

            void Start(string a1 = "")
            {
                Nq(2);
                i += 2;
                if (!GetCharacter("-\"") && a1 == "") { next = "fin"; }
                else
                {
                    varname.Clear();
                    while (true)
                    {
                        i++;
                        if (GetCharacter("-\"")) { break; }
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
                if (!GetCharacter("-\"") && a1 == "")
                {
                    WriteToFile($"{dcd}\\project.ss1", "\\nscript");
                    Console.WriteLine("");
                }
                Dte($"{next}|");
            }

            void FindBlock(string wordFind) { i = jsonfile.IndexOf($"\"{wordFind}\":{{\"opcode\":"); }

            void WriteBlock(string block, int whichrepeat = -1)
            {
                WriteToFile($"{dcd}\\project.ss1", block);
                if (whichrepeat > 0)
                {
                    switch (whichrepeat)
                    {
                        case 0:
                            Console.WriteLine($"{RED}else | {NC} \"{block}\"");
                            break;

                        case 1:
                            Console.WriteLine($"{RED}Starting repeat | {NC} \"{block}\"");
                            break;

                        case 2:
                            Console.WriteLine($"{RED}Ended repeat | {NC} }}");
                            break;

                        case 3:
                            Console.WriteLine($"{RED}Starting forever | {NC} \"{block}\"");
                            break;

                        case 4:
                            Console.WriteLine($"{RED}Ended forever | {NC} }}");
                            break;

                        case 5:
                            Console.WriteLine($"{RED}Starting if | {NC} \"{block}\"");
                            break;

                        case 6:
                            Console.WriteLine($"{RED}Ended if | {NC} }}");
                            break;

                        case 7:
                            Console.WriteLine($"{RED}Starting if/else | {NC} \"{block}\"");
                            break;

                        case 8:
                            Console.WriteLine($"{RED}Ended if/else | {NC} }}");
                            break;

                        case 9:
                            Console.WriteLine($"{RED}Starting repeat until | {NC} \"{block}\"");
                            break;

                        case 10:
                            Console.WriteLine($"{RED}Ended repeat until | {NC} }}");
                            break;

                        case 11:
                            Console.WriteLine($"{RED}Starting while | {NC} \"{block}\"");
                            break;

                        case 12:
                            Console.WriteLine($"{RED}Ended while | {NC} }}");
                            break;

                        case 13:
                            Console.WriteLine($"{RED}Starting for | {NC} \"{block}\"");
                            break;

                        case 14:
                            Console.WriteLine($"{RED}Ended for | {NC} }}");
                            break;
                    }
                }
                else { Console.WriteLine($"{RED}Added block: {NC} \"{block}\""); }
            }

            void AddBlock(string a1, bool dcon = false)
            {
                string cnext = "";
                string amt = "";
                Start();
                // Global Blocks
                switch (a1)
                {
                    case "looks_switchbackdropto":
                        Nq(5);
                        FindBlock(ExtractData());
                        Nq(18);
                        WriteBlock($"switch backdrop to (\"{ExtractData()}\")");
                        break;

                    case "looks_switchbackdroptoandwait":
                        Nq(5);
                        FindBlock(ExtractData());
                        Nq(18);
                        WriteBlock($"switch backdrop to (\"{ExtractData()}\") and wait");
                        break;

                    case "looks_nextbackdrop":
                        WriteBlock($"next backdrop");
                        break;

                    case "sound_changeeffectby":
                    case "looks_changeeffectby":
                        Nq(5);
                        word = ExtractData();
                        if (word != "fields")
                        {
                            amt = word;
                            Nq(2);
                        }
                        else { amt = ""; }
                        Nq(3);
                        WriteBlock($"change [{ExtractData().ToLower()}] effect by (\"{amt}\")");
                        break;

                    case "sound_seteffectto":
                    case "looks_seteffectto":
                        Nq(5);
                        word = ExtractData();
                        amt = "";
                        if (word != "fields")
                        {
                            amt = word;
                            Nq(2);
                        }
                        Nq(3);
                        WriteBlock($"set [{ExtractData().ToLower()}] effect to (\"{amt}\")");
                        break;

                    case "looks_cleargraphiceffects":
                        WriteBlock("clear graphic effects");
                        break;

                    case "looks_backdropnumbername":
                        Nq(7);
                        con.Append($"(backdrop [{ExtractData()}])");
                        if (dcon) { WriteBlock(con.ToString()); }
                        break;

                    case "sound_playuntildone":
                        Nq(5);
                        FindBlock(ExtractData());
                        Nq(18);
                        WriteBlock($"play sound (\"{ExtractData()}\") until done");
                        break;

                    case "sound_play":
                        Nq(5);
                        FindBlock(ExtractData());
                        Nq(18);
                        WriteBlock($"start sound (\"{ExtractData()}\")");
                        break;

                    case "sound_stopallsounds":
                        WriteBlock("stop all sounds");
                        break;

                    case "sound_cleareeffects":
                        WriteBlock("clear sound effects");
                        break;

                    case "sound_changevolumeby":
                        Nq(5);
                        word = ExtractData();
                        if (word == "fields") { word = ""; }
                        WriteBlock($"change volume by (\"{word}\")");
                        break;

                    case "sound_setvolumeto":
                        Nq(5);
                        word = ExtractData();
                        if (word == "fields") { word = ""; }
                        WriteBlock($"set volume to (\"{word}\")");
                        break;

                    case "sound_volume":
                        con.Append("(volume)");
                        if (dcon) { WriteBlock(con.ToString()); }
                        break;

                    case "event_whenflagclicked":
                        WriteBlock("when flag clicked");
                        break;

                    case "event_whenkeypressed":
                        Nq(7);
                        WriteBlock($"when [{ExtractData()}] key pressed");
                        break;

                    case "event_whenstageclicked":
                        WriteBlock("when stage clicked");
                        break;

                    case "event_whenbackdropswitchesto":
                        Nq(7);
                        WriteBlock($"when backdrop switches to [{ExtractData()}]");
                        break;

                    case "event_whengreaterthan":
                        Nq(5);
                        word = ExtractData();
                        if (word == "fields") { word = ""; }
                        else { Nq(2); }
                        amt = word;
                        Nq(3);
                        WriteBlock($"when [{ExtractData().ToLower()}] > (\"{amt}\")");
                        break;

                    case "event_whenbroadcastreceived":
                        Nq(7);
                        WriteBlock($"when I receive [{ExtractData()}]");
                        break;

                    case "event_broadcast":
                        Nq(5);
                        WriteBlock($"broadcast [{word}]");
                        break;

                    case "event_broadcastandwait":
                        Nq(5);
                        WriteBlock($"broadcast [{word}] and wait");
                        break;

                    case "control_wait":
                        Nq(5);
                        word = ExtractData();
                        if (word == "fields") { word = ""; }
                        WriteBlock($"wait (\"{word}\") seconds");
                        break;

                    case "control_repeat":
                        cnext = next;
                        Nq(5);
                        word = ExtractData();
                        if (word == "SUBSTACK")
                        {
                            word = "";
                            WriteBlock($"repeat (\"{word}\") {{", 1);
                        }
                        else
                        {
                            Nq();
                            WriteBlock($"repeat (\"{word}\") {{", 1);
                            word = ExtractData();
                        }
                        if (word != "SUBSTACK")
                        {
                            Console.WriteLine();
                            WriteBlock("}", 2);
                        }
                        else
                        {
                            Nq();
                            word = ExtractData();
                            if (word != "fields")
                            {
                                FindBlock(word);
                                Nq(4);
                                AddBlock(ExtractData());
                                WriteBlock("}", 2);
                            }
                            else { WriteBlock("}", 2); }
                            next = cnext;
                        }
                        break;

                    case "control_forever":
                        cnext = next;
                        Nq(3);
                        word = ExtractData();
                        if (word == "SUBSTACK")
                        {
                            Nq();
                            word = ExtractData();
                            if (word == "fields") { word = ""; }
                        }
                        else { word = ""; }
                        WriteBlock("forever {", 3);
                        if (word != "")
                        {
                            FindBlock(word);
                            Nq(4);
                            AddBlock(ExtractData());
                            WriteBlock("}", 4);
                        }
                        else { WriteBlock("}", 4); }
                        next = cnext;
                        break;

                    case "control_if":
                        {
                            con.Clear();
                            cnext = next;
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "CONDITION" or "fields") { break; }
                            }
                            if (word == "CONDITION")
                            {
                                int k = i;
                                Nq(special: "]");
                                i--;
                                if (GetCharacter("-\""))
                                {
                                    i = k;
                                    Nq();
                                    FindBlock(ExtractData());
                                    Nq(4);
                                    AddBlock(ExtractData());
                                    WriteBlock($"if {con} {{", 5);
                                }
                                else { WriteBlock("if <> {", 5); }
                            }
                            else { WriteBlock("if <> {", 5); }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "SUBSTACK" or "fields") { break; }
                            }
                            if (word == "SUBSTACK")
                            {
                                int k = i;
                                Nq(special: "]");
                                if (GetCharacter("-\""))
                                {
                                    i = k;
                                    Nq();
                                    FindBlock(ExtractData());
                                    Nq(4);
                                    AddBlock(ExtractData());
                                }
                                WriteBlock("}", 6);
                            }
                            else { WriteBlock("}", 6); }
                            next = cnext;
                        }
                        break;

                    case "control_if_else":
                        {
                            con.Clear();
                            cnext = next;
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "CONDITION" or "fields") { break; }
                            }
                            if (word == "CONDITION")
                            {
                                int k = i;
                                Nq(special: "]");
                                i--;
                                if (GetCharacter("-\""))
                                {
                                    i = k;
                                    Nq();
                                    FindBlock(ExtractData());
                                    Nq(4);
                                    AddBlock(ExtractData());
                                    WriteBlock($"if {con} {{", 7);
                                }
                                else { WriteBlock("if <> {", 7); }
                            }
                            else { WriteBlock("if <> {", 7); }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "SUBSTACK" or "fields") { break; }
                            }
                            if (word == "SUBSTACK")
                            {
                                int k = i;
                                Nq(special: "]");
                                i--;
                                if (GetCharacter("-\""))
                                {
                                    i = k;
                                    Nq();
                                    FindBlock(ExtractData());
                                    Nq(4);
                                    AddBlock(ExtractData());
                                }
                                WriteBlock("} else {", 0);
                            }
                            else { WriteBlock("} else {", 0); }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "SUBSTACK2" or "fields") { break; }
                            }
                            if (word == "SUBSTACK2")
                            {
                                int k = i;
                                Nq(special: "]");
                                i--;
                                if (GetCharacter("-\""))
                                {
                                    i = k;
                                    Nq();
                                    FindBlock(ExtractData());
                                    Nq(4);
                                    AddBlock(ExtractData());
                                }
                                WriteBlock("}", 8);
                            }
                            else { WriteBlock("}", 8); }
                            next = cnext;
                        }
                        break;

                    case "control_wait_until":
                        {
                            con.Clear();
                            cnext = next;
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "CONDITION" or "fields") { break; }
                            }
                            if (word == "CONDITION")
                            {
                                int k = i;
                                Nq(special: "]");
                                i--;
                                if (GetCharacter("-\""))
                                {
                                    i = k;
                                    Nq();
                                    FindBlock(ExtractData());
                                    Nq(4);
                                    AddBlock(ExtractData());
                                    WriteBlock($"wait until {(con.ToString() == "" ? "<>" : "")}");
                                }
                                else { WriteBlock("wait until <>"); }
                            }
                            else { WriteBlock($"wait until {(con.ToString() == "" ? "<>" : "")}"); }
                            i = j;
                            next = cnext;
                        }
                        break;

                    case "control_repeat_until":
                        {
                            con.Clear();
                            cnext = next;
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "CONDITION" or "fields") { break; }
                            }
                            if (word == "CONDITION")
                            {
                                int k = i;
                                Nq(special: "]");
                                i--;
                                if (GetCharacter("-\""))
                                {
                                    i = k;
                                    Nq();
                                    FindBlock(ExtractData());
                                    Nq(4);
                                    AddBlock(ExtractData());
                                    WriteBlock($"repeat until {con}", 9);
                                }
                                else { WriteBlock("repeat until <>", 9); }
                            }
                            else { WriteBlock("repeat until <>", 9); }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "SUBSTACK" or "fields") { break; }
                            }
                            if (word == "SUBSTACK")
                            {
                                int k = i;
                                Nq(special: "]");
                                i--;
                                if (GetCharacter("-\""))
                                {
                                    i = k;
                                    Nq();
                                    FindBlock(ExtractData());
                                    Nq(4);
                                    AddBlock(ExtractData());
                                }
                                WriteBlock("}", 10);
                            }
                            else { WriteBlock("}", 10); }
                            next = cnext;
                        }
                        break;

                    case "control_while":
                        {
                            con.Clear();
                            cnext = next;
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "CONDITION" or "fields") { break; }
                            }
                            if (word == "CONDITION")
                            {
                                int k = i;
                                Nq(special: "]");
                                i--;
                                if (GetCharacter("-\""))
                                {
                                    i = k;
                                    Nq();
                                    FindBlock(ExtractData());
                                    Nq(4);
                                    AddBlock(ExtractData());
                                    WriteBlock($"while {con}", 11);
                                }
                                else { WriteBlock("while <>", 11); }
                            }
                            else { WriteBlock("while <>", 11); }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "SUBSTACK" or "fields") { break; }
                            }
                            if (word == "SUBSTACK")
                            {
                                int k = i;
                                Nq(special: "]");
                                i--;
                                if (GetCharacter("-\""))
                                {
                                    i = k;
                                    Nq();
                                    FindBlock(ExtractData());
                                    Nq(4);
                                    AddBlock(ExtractData());
                                }
                                WriteBlock("}", 12);
                            }
                            else { WriteBlock("}", 12); }
                            next = cnext;
                        }
                        break;

                    case "control_for_each":
                        {
                            cnext = next;
                            int j = i;
                            while (true) { if (ExtractData() == "VALUE") { break; } }
                            Nq();
                            string value = ExtractData();
                            i = j;
                            while (true) { if (ExtractData() == "VARIABLE") { break; } }
                            Nq();
                            string variable = ExtractData();
                            WriteBlock($"for [{variable}] in (\"{value}\") {{", 13);
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "SUBSTACK" or "shadow") { break; }
                            }
                            if (word == "SUBSTACK")
                            {
                                int k = i;
                                Nq(special: "]");
                                i--;
                                if (GetCharacter("-\""))
                                {
                                    i = k;
                                    Nq();
                                    FindBlock(ExtractData());
                                    Nq(4);
                                    AddBlock(ExtractData());
                                }
                                WriteBlock("}", 14);
                            }
                            else { WriteBlock("}", 14); }
                            next = cnext;
                        }
                        break;

                    case "control_create_clone_of":
                        Nq(5);
                        FindBlock(ExtractData());
                        Nq(18);
                        word = ExtractData();
                        if (word == "_myself_") { word = "myself"; }
                        WriteBlock($"create a clone of (\"{word}\")");
                        break;

                    case "control_stop":
                        Nq(7);
                        WriteBlock($"stop [{ExtractData()}]");
                        break;

                    case "sensing_askandwait":
                        Nq(5);
                        WriteBlock($"ask (\"{ExtractData()}\") and wait");
                        break;

                    case "sensing_answer":
                        con.Append("(answer)");
                        if (dcon) { WriteBlock(con.ToString()); }
                        break;

                    case "sensing_keypressed":
                        Nq(5);
                        FindBlock(ExtractData());
                        Nq(18);
                        con.Append($"<key (\"{ExtractData()}\") pressed?>");
                        if (dcon) { WriteBlock(con.ToString()); }
                        break;

                    case "sensing_mousedown":
                        con.Append("<mouse down?>");
                        if (dcon) { WriteBlock(con.ToString()); }
                        break;

                    case "sensing_mousex":
                        con.Append("(mouse x)");
                        if (dcon) { WriteBlock(con.ToString()); }
                        break;

                    case "sensing_mousey":
                        con.Append("(mouse y)");
                        if (dcon) { WriteBlock(con.ToString()); }
                        break;

                    case "sensing_loudness":
                        con.Append("(loudness)");
                        if (dcon) { WriteBlock(con.ToString()); }
                        break;

                    case "sensing_timer":
                        con.Append("(timer)");
                        if (dcon) { WriteBlock(con.ToString()); }
                        break;

                    case "sensing_resettimer":
                        WriteBlock("reset timer");
                        break;

                    case "sensing_of":
                        {
                            string sofprop;
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "PROPERTY" or "shadow") { break; }
                            }
                            if (word == "PROPERTY")
                            {
                                Nq();
                                sofprop = ExtractData();
                            }
                            else { sofprop = ""; }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "OBJECT" or "shadow") { break; }
                            }
                            if (word == "OBJECT")
                            {
                                Nq();
                                FindBlock(ExtractData());
                                Nq(18);
                                word = ExtractData();
                                if (word == "_stage_") { word = "Stage"; }
                            }
                            else { word = ""; }
                            con.Append($"([{sofprop}] of (\"{word}\"))");
                            if (dcon) { WriteBlock(con.ToString()); }
                        }
                        break;

                    case "sensing_current":
                        Nq(7);
                        word = ExtractData().ToLower();
                        con.Append($"(current [{word}])");
                        if (dcon) { WriteBlock(con.ToString()); }
                        break;

                    case "sensing_dayssince2000":
                        con.Append("(days since 2000)");
                        if (dcon) { WriteBlock(con.ToString()); }
                        break;

                    case "sensing_username":
                        con.Append("(username)");
                        if (dcon) { WriteBlock(con.ToString()); }
                        break;

                    case "operator_add":
                        {
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "NUM1" or "fields") { break; }
                            }
                            string op1 = "";
                            string op2 = "";
                            if (word == "NUM1")
                            {
                                Nq();
                                op1 = ExtractData();
                            }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "NUM2" or "fields") { break; }
                            }
                            if (word == "NUM2")
                            {
                                Nq();
                                op2 = ExtractData();
                            }
                            con.Append($"((\"{op1}\") + (\"{op2}\"))");
                            if (dcon) { WriteBlock(con.ToString()); }
                        }
                        break;

                    case "operator_subtract":
                        {
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "NUM1" or "fields") { break; }
                            }
                            string op1 = "";
                            string op2 = "";
                            if (word == "NUM1")
                            {
                                Nq();
                                op1 = ExtractData();
                            }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "NUM2" or "fields") { break; }
                            }
                            if (word == "NUM2")
                            {
                                Nq();
                                op2 = ExtractData();
                            }
                            con.Append($"((\"{op1}\") - (\"{op2}\"))");
                            if (dcon) { WriteBlock(con.ToString()); }
                        }
                        break;

                    case "operator_multiply":
                        {
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "NUM1" or "fields") { break; }
                            }
                            string op1 = "";
                            string op2 = "";
                            if (word == "NUM1")
                            {
                                Nq();
                                op1 = ExtractData();
                            }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "NUM2" or "fields") { break; }
                            }
                            if (word == "NUM2")
                            {
                                Nq();
                                op2 = ExtractData();
                            }
                            con.Append($"((\"{op1}\") * (\"{op2}\"))");
                            if (dcon) { WriteBlock(con.ToString()); }
                        }
                        break;

                    case "operator_divide":
                        {
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "NUM1" or "fields") { break; }
                            }
                            string op1 = "";
                            string op2 = "";
                            if (word == "NUM1")
                            {
                                Nq();
                                op1 = ExtractData();
                            }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "NUM2" or "fields") { break; }
                            }
                            if (word == "NUM2")
                            {
                                Nq();
                                op2 = ExtractData();
                            }
                            con.Append($"((\"{op1}\") / (\"{op2}\"))");
                            if (dcon) { WriteBlock(con.ToString()); }
                        }
                        break;

                    case "operator_random":
                        {
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "FROM" or "fields") { break; }
                            }
                            string op1 = "";
                            string op2 = "";
                            if (word == "FROM")
                            {
                                Nq();
                                op1 = ExtractData();
                            }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "TO" or "fields") { break; }
                            }
                            if (word == "TO")
                            {
                                Nq();
                                op2 = ExtractData();
                            }
                            con.Append($"(pick random (\"{op1}\") to (\"{op2}\"))");
                            if (dcon) { WriteBlock(con.ToString()); }
                        }
                        break;

                    case "operator_equals":
                        {
                            Nq(2);
                            int j = i;
                            string op1 = "";
                            string op2 = "";
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "OPERAND1" or "fields") { break; }
                            }
                            if (word == "OPERAND1")
                            {
                                int k = i;
                                Nq(special: ",");
                                i++;
                                if (GetCharacter("-\""))
                                {
                                    word = ExtractData();
                                    FindBlock(word);
                                    Nq(4);
                                    word = ExtractData();
                                    con.Append("<");
                                    AddBlock(word);
                                    con.Append(" = ");
                                }
                                else
                                {
                                    Nq(special: "]");
                                    i--;
                                    if (GetCharacter("-\""))
                                    {
                                        i = k;
                                        Nq();
                                        op1 = ExtractData();
                                    }
                                    con.Append($"<(\"{op1}\") = (\"");
                                }
                            }
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "OPERAND2" or "fields") { break; }
                            }
                            if (word == "OPERAND2")
                            {
                                int k = i;
                                Nq(special: ",");
                                i++;
                                if (GetCharacter("-\""))
                                {
                                    word = ExtractData();
                                    FindBlock(word);
                                    Nq(4);
                                    word = ExtractData();
                                    AddBlock(word);
                                    con.Append(">");
                                }
                                else
                                {
                                    Nq(special: "]");
                                    i--;
                                    if (GetCharacter("-\""))
                                    {
                                        i = k;
                                        Nq();
                                        op1 = ExtractData();
                                    }
                                    con.Append($"{op2}\")>");
                                }
                            }
                            else { con.Append($"{op2}\")>"); }
                            if (dcon) { WriteBlock(con.ToString()); }
                        }
                        break;

                    case "operator_gt":
                        {
                            Nq(2);
                            int j = i;
                            string op1 = "";
                            string op2 = "";
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "OPERAND1" or "fields") { break; }
                            }
                            if (word == "OPERAND1")
                            {
                                int k = i;
                                Nq(special: ",");
                                i++;
                                if (GetCharacter("-\""))
                                {
                                    word = ExtractData();
                                    FindBlock(word);
                                    Nq(4);
                                    word = ExtractData();
                                    con.Append("<");
                                    AddBlock(word);
                                    con.Append(" > ");
                                }
                                else
                                {
                                    Nq(special: "]");
                                    i--;
                                    if (GetCharacter("-\""))
                                    {
                                        i = k;
                                        Nq();
                                        op1 = ExtractData();
                                    }
                                    con.Append($"<(\"{op1}\") > (\"");
                                }
                            }
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "OPERAND2" or "fields") { break; }
                            }
                            if (word == "OPERAND2")
                            {
                                int k = i;
                                Nq(special: ",");
                                i++;
                                if (GetCharacter("-\""))
                                {
                                    word = ExtractData();
                                    FindBlock(word);
                                    Nq(4);
                                    word = ExtractData();
                                    AddBlock(word);
                                    con.Append(">");
                                }
                                else
                                {
                                    Nq(special: "]");
                                    i--;
                                    if (GetCharacter("-\""))
                                    {
                                        i = k;
                                        Nq();
                                        op1 = ExtractData();
                                    }
                                    con.Append($"{op2}\")>");
                                }
                            }
                            else { con.Append($"{op2}\")>"); }
                            if (dcon) { WriteBlock(con.ToString()); }
                        }
                        break;

                    case "operator_lt":
                        {
                            Nq(2);
                            int j = i;
                            string op1 = "";
                            string op2 = "";
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "OPERAND1" or "fields") { break; }
                            }
                            if (word == "OPERAND1")
                            {
                                int k = i;
                                Nq(special: ",");
                                i++;
                                if (GetCharacter("-\""))
                                {
                                    word = ExtractData();
                                    FindBlock(word);
                                    Nq(4);
                                    word = ExtractData();
                                    con.Append("<");
                                    AddBlock(word);
                                    con.Append(" < ");
                                }
                                else
                                {
                                    Nq(special: "]");
                                    i--;
                                    if (GetCharacter("-\""))
                                    {
                                        i = k;
                                        Nq();
                                        op1 = ExtractData();
                                    }
                                    con.Append($"<(\"{op1}\") < (\"");
                                }
                            }
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "OPERAND2" or "fields") { break; }
                            }
                            if (word == "OPERAND2")
                            {
                                int k = i;
                                Nq(special: ",");
                                i++;
                                if (GetCharacter("-\""))
                                {
                                    word = ExtractData();
                                    FindBlock(word);
                                    Nq(4);
                                    word = ExtractData();
                                    AddBlock(word);
                                    con.Append(">");
                                }
                                else
                                {
                                    Nq(special: "]");
                                    i--;
                                    if (GetCharacter("-\""))
                                    {
                                        i = k;
                                        Nq();
                                        op1 = ExtractData();
                                    }
                                    con.Append($"{op2}\")>");
                                }
                            }
                            else { con.Append($"{op2}\")>"); }
                            if (dcon) { WriteBlock(con.ToString()); }
                        }
                        break;

                    case "operator_and":
                        {
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "OPERAND1" or "fields") { break; }
                            }
                            if (word == "OPERAND1")
                            {
                                Nq();
                                word = ExtractData();
                                if (word is not ("fields" or "OPERAND2"))
                                {
                                    FindBlock(word);
                                    con.Append("<");
                                    Nq(4);
                                    word = ExtractData();
                                    AddBlock(word);
                                    con.Append(" and ");
                                }
                                else { con.Append("<<> and "); }
                            }
                            else { con.Append("<<> and "); }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "OPERAND2" or "fields") { break; }
                            }
                            if (word == "OPERAND2")
                            {
                                Nq();
                                word = ExtractData();
                                if (word is not ("fields" or "OPERAND1"))
                                {
                                    FindBlock(word);
                                    Nq(4);
                                    word = ExtractData();
                                    AddBlock(word);
                                    con.Append(">");
                                }
                                else { con.Append("<>>"); }
                            }
                            else { con.Append("<>>"); }
                            if (dcon) { WriteBlock(con.ToString()); }
                        }
                        break;

                    case "operator_or":
                        {
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "OPERAND1" or "fields") { break; }
                            }
                            if (word == "OPERAND1")
                            {
                                Nq();
                                word = ExtractData();
                                if (word is not ("fields" or "OPERAND2"))
                                {
                                    FindBlock(word);
                                    con.Append("<");
                                    Nq(4);
                                    word = ExtractData();
                                    AddBlock(word);
                                    con.Append(" or ");
                                }
                                else { con.Append("<<> or "); }
                            }
                            else { con.Append("<<> or "); }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "OPERAND2" or "fields") { break; }
                            }
                            if (word == "OPERAND2")
                            {
                                Nq();
                                word = ExtractData();
                                if (word is not ("fields" or "OPERAND1"))
                                {
                                    FindBlock(word);
                                    Nq(4);
                                    word = ExtractData();
                                    AddBlock(word);
                                    con.Append(">");
                                }
                                else { con.Append("<>>"); }
                            }
                            else { con.Append("<>>"); }
                            if (dcon) { WriteBlock(con.ToString()); }
                        }
                        break;

                    case "operator_not":
                        {
                            Nq(2);
                            int j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word is "OPERAND" or "fields") { break; }
                            }
                            if (word == "OPERAND")
                            {
                                Nq();
                                word = ExtractData();
                                if (word is not ("fields" or "OPERAND2"))
                                {
                                    FindBlock(word);
                                    con.Append("<not ");
                                    Nq(4);
                                    word = ExtractData();
                                    AddBlock(word);
                                    con.Append(">");
                                }
                                else { con.Append("<not <>>"); }
                            }
                            else { con.Append("<not <>>"); }
                            if (dcon) { WriteBlock(con.ToString()); }
                        }
                        break;

                    case "operator_join":
                        {
                            Nq(2);
                            j = i;
                            while (true)
                            {
                                word = ExtractData();
                                if (word == "STRING1") { break; }
                            }
                            int k = i;
                            Nq(special: ",");
                            i++;
                            if (GetCharacter("-\""))
                            {
                                word = ExtractData();
                                FindBlock(word);
                                con.Append("(join ");
                                Nq(4);
                                word = ExtractData();
                                AddBlock(word);
                            }
                            else
                            {
                                i = k;
                                Nq();
                                word = ExtractData();
                                con.Append($"(join (\"{word}\")");
                            }
                            i = j;
                            while (true)
                            {
                                word = ExtractData();
                                if (word == "STRING2") { break; }
                            }
                            k = i;
                            Nq(special: ",");
                            i++;
                            if (GetCharacter("-\""))
                            {
                                word = ExtractData();
                                FindBlock(word);
                                Nq(4);
                                word = ExtractData();
                                AddBlock(word);
                                con.Append(")");
                            }
                            else
                            {
                                i = k;
                                Nq();
                                word = ExtractData();
                                con.Append($"(\"{word}\"))");
                            }
                            if (dcon) { WriteBlock(con.ToString()); }
                        }
                        break;

                    default:
                        Console.WriteLine($"{RED}Unknown block: \"{a1}\" Skipping.{NC}");
                        WriteToFile($"{dcd}\\project.ss1", $"DECOMPERR: Unknown block: \"{a1}\"");
                        break;
                }
                if (next != "fin")
                {
                    FindBlock(next);
                    Nq(4);
                    AddBlock(ExtractData(), true);
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
                            if (GetCharacter("-[")) { break; }
                            if (GetCharacter("-}"))
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
                                if (GetCharacter("-\"")) { break; }
                                varname.Append(character);
                            }
                            i++;
                            varvalue.Clear();
                            while (true)
                            {
                                i++;
                                if (GetCharacter("-]")) { break; }
                                varvalue.Append(character);
                            }
                            if (!File.Exists($"{dcd}\\project.ss1")) { WriteToFile($"{dcd}\\project.ss1", "// There should be no empty lines.\nss1\n\\prep"); }
                            WriteToFile($"{dcd}\\project.ss1", $"var: {varname}={varvalue}");
                            Console.WriteLine($"{RED}Added variable: {NC}\"{varname}\".\n{RED}Value: {NC}{varvalue}\n");
                            varname.Clear();
                            i++;
                            if (GetCharacter("-}")) { break; }
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
                    if (GetCharacter("-}"))
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
                            if (GetCharacter("-[")) { break; }
                            if (GetCharacter("-}"))
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
                            if (GetCharacter("-\"")) { break; }
                            listname.Append(character);
                        }
                        i += 3;
                        if (!GetCharacter("-]"))
                        {
                            List<string> listContents = new();
                            while (true)
                            {
                                if (GetCharacter("-]")) { break; }
                                if (character == "\"")
                                {
                                    varname.Clear();
                                    varname.Append(ExtractData());
                                    i++;
                                    if (!GetCharacter("-]"))
                                    {
                                        listContents.Add($"\"{varname}\", ");
                                        i++;
                                    }
                                    else
                                    {
                                        listContents.Add($"\"{varname}\"");
                                        break;
                                    }
                                    if (GetCharacter("- ")) { i++; }
                                }
                                else
                                {
                                    i--;
                                    varname.Clear();
                                    while (true)
                                    {
                                        i++;
                                        if (GetCharacter("-,")) { break; }
                                        if (GetCharacter("-]")) { break; }
                                        varname.Append(character);
                                    }

                                    if (!GetCharacter("-]")) { listContents.Add($"\"{varname}\", "); }
                                    else
                                    {
                                        listContents.Add($"\"{varname}\"");
                                        break;
                                    }
                                    i++;
                                    if (GetCharacter("- ")) { i++; }
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
                    if (GetCharacter("-}")) { break; }
                }
                Console.WriteLine("Loading broadcasts...\n");
                while (true)
                {
                    StringBuilder testForBreak = new("");
                    while (true)
                    {
                        i++;
                        if (GetCharacter("-\"")) { break; }
                        testForBreak.Append(character);
                    }
                    if (testForBreak.ToString() == "broadcasts") { break; }
                }
                novars = false;
                i += 3;
                if (GetCharacter("-}"))
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
                        if (GetCharacter("-\"")) { break; }
                        if (GetCharacter("-}"))
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
                            if (GetCharacter("-\"")) { break; }
                            varname.Append(character);
                        }
                        WriteToFile($"{dcd}\\project.ss1", $"broadcast: {varname}");
                        Console.WriteLine($"{RED}Loaded broadcast: {NC}\"{varname}\"\n");
                        i++;
                        if (GetCharacter("-}")) { break; }
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
                            if (!GetCharacter("-\"")) { break; }
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
                            if (GetCharacter("-{")) { break; }
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
                try
                {
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
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine("Error formatting code. Leaving file unedited.");
                }
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