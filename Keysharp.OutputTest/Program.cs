using static Keysharp.Core.Accessors;
using static Keysharp.Core.COM.Com;
using static Keysharp.Core.Collections;
using static Keysharp.Core.Common.Keyboard.HotkeyDefinition;
using static Keysharp.Core.Common.Keyboard.HotstringDefinition;
using static Keysharp.Core.Common.Keyboard.HotstringManager;
using static Keysharp.Core.ControlX;
using static Keysharp.Core.Dialogs;
using static Keysharp.Core.Dir;
using static Keysharp.Core.Dll;
using static Keysharp.Core.Drive;
using static Keysharp.Core.EditX;
using static Keysharp.Core.Env;
using static Keysharp.Core.Errors;
using static Keysharp.Core.External;
using static Keysharp.Core.Files;
using static Keysharp.Core.Flow;
using static Keysharp.Core.Functions;
using static Keysharp.Core.GuiHelper;
using static Keysharp.Core.ImageLists;
using static Keysharp.Core.Images;
using static Keysharp.Core.Ini;
using static Keysharp.Core.Input;
using static Keysharp.Core.Keyboard;
using static Keysharp.Core.Loops;
using static Keysharp.Core.Maths;
using static Keysharp.Core.Menu;
using static Keysharp.Core.Misc;
using static Keysharp.Core.Monitor;
using static Keysharp.Core.Mouse;
using static Keysharp.Core.Network;
using static Keysharp.Core.Processes;
using static Keysharp.Core.RealThreads;
using static Keysharp.Core.RegEx;
using static Keysharp.Core.Registrys;
using static Keysharp.Core.Screen;
using static Keysharp.Core.Security;
using static Keysharp.Core.SimpleJson;
using static Keysharp.Core.Sound;
using static Keysharp.Core.Strings;
using static Keysharp.Core.ToolTips;
using static Keysharp.Core.Types;
using static Keysharp.Core.WindowX;
using static Keysharp.Core.Windows.WindowsAPI;
using static Keysharp.Scripting.Script.Operator;
using static Keysharp.Scripting.Script;

[assembly: Keysharp.Scripting.AssemblyBuildVersionAttribute("0.0.0.8")]
namespace Keysharp.CompiledMain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Keysharp.Core;
    using Keysharp.Core.Common;
    using Keysharp.Core.Common.File;
    using Keysharp.Core.Common.Invoke;
    using Keysharp.Core.Common.ObjectBase;
    using Keysharp.Core.Common.Strings;
    using Keysharp.Core.Common.Threading;
    using Keysharp.Scripting;
    using Array = Keysharp.Core.Array;
    using Buffer = Keysharp.Core.Buffer;

    public class program
    {
        public static object namedhotkeyfunction = FuncObj("namedhotkeyfunction");
        public static object testfunc = FuncObj("testfunc");
        public static object _ks_Hotkey_1()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind<^>^}b{b DownR}");
            return "";
        }

        public static object _ks_Hotkey_2()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{b Up}");
            return "";
        }

        public static object _ks_Hotkey_3(object thishotkey)
        {
            Keysharp.Core.ToolTips.ToolTip(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "You pressed ", thishotkey));
            return "";
        }

        public static object _ks_Hotkey_4()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}b{b DownR}");
            return "";
        }

        public static object _ks_Hotkey_5()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{b Up}");
            return "";
        }

        public static object _ks_Hotkey_6()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}b{b DownR}");
            return "";
        }

        public static object _ks_Hotkey_7()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{b Up}");
            return "";
        }

        public static object _ks_Hotkey_8()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind<^>^}c{^c DownR}");
            return "";
        }

        public static object _ks_Hotkey_9()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{^c Up}");
            return "";
        }

        public static object _ks_Hotkey_10()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}Shift{Shift DownR}");
            return "";
        }

        public static object _ks_Hotkey_11()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{Shift Up}");
            return "";
        }

        public static object _ks_Hotkey_12()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}LButton{LButton DownR}");
            return "";
        }

        public static object _ks_Hotkey_13()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{LButton Up}");
            return "";
        }

        public static object _ks_Hotkey_14()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}RButton{RButton DownR}");
            return "";
        }

        public static object _ks_Hotkey_15()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{RButton Up}");
            return "";
        }

        public static object _ks_Hotkey_16()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}Ctrl{Ctrl DownR}");
            return "";
        }

        public static object _ks_Hotkey_17()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{Ctrl Up}");
            return "";
        }

        public static object _ks_Hotkey_18()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}LButton{^LButton DownR}");
            return "";
        }

        public static object _ks_Hotkey_19()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{^LButton Up}");
            return "";
        }

        public static object _ks_Hotkey_20()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}AppsKey{AppsKey DownR}");
            return "";
        }

        public static object _ks_Hotkey_21()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{AppsKey Up}");
            return "";
        }

        public static object _ks_Hotkey_22()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}RWin{RWin DownR}");
            return "";
        }

        public static object _ks_Hotkey_23()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{RWin Up}");
            return "";
        }

        public static object _ks_Hotkey_24()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}Alt{Alt DownR}");
            return "";
        }

        public static object _ks_Hotkey_25()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{Alt Up}");
            return "";
        }

        public static object _ks_Hotkey_26(object thishotkey)
        {
            return "";
        }

        public static object _ks_HotIf_1()
        {
            return Keysharp.Core.WindowX.WinActive("ahk_class Notepad");
        }

        public static object _ks_Hotkey_27()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}d{d DownR}");
            return "";
        }

        public static object _ks_Hotkey_28()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{d Up}");
            return "";
        }

        public static object _ks_Hotkey_29()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}b{b DownR}");
            return "";
        }

        public static object _ks_Hotkey_30()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{b Up}");
            return "";
        }

        public static object _ks_Hotkey_31(object thishotkey)
        {
            Keysharp.Core.ToolTips.ToolTip(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "You pressed ", thishotkey));
            return "";
        }

        public static object _ks_Hotkey_32(object thishotkey)
        {
            Keysharp.Core.Keyboard.SendEvent("{Blind}{Click Down}");
            return "";
        }

        public static object _ks_Hotkey_33(object thishotkey)
        {
            Keysharp.Core.Keyboard.SendEvent("{Blind}{Click Up}");
            return "";
        }

        public static object x = null;
        public static object _ks_Hotkey_34(object thishotkey)
        {
            object x = null;
            x = 999L;
            return "";
        }

        public static object _ks_Hotkey_35(object thishotkey)
        {
            Keysharp.Core.ToolTips.ToolTip(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, A_ThisHotkey, " is being pressed"));
            Keysharp.Core.Keyboard.KeyWait(A_ThisHotkey);
            Keysharp.Core.ToolTips.ToolTip();
            return "";
        }

        public static object _ks_Hotkey_36(object thishotkey)
        {
            Keysharp.Core.ToolTips.ToolTip(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, A_ThisHotkey, " is being pressed"));
            Keysharp.Core.Keyboard.KeyWait(A_ThisHotkey);
            Keysharp.Core.ToolTips.ToolTip();
            return "";
        }

        public static object _ks_Hotkey_37(object thishotkey)
        {
            object Namedhotkeyfunction(object hk)
            {
                object x = null;
                x = 123L;
                return "";
            }

            object namedhotkeyfunction = FuncObj((Delegate)Namedhotkeyfunction);
            Keysharp.Scripting.Script.MultiStatement(namedhotkeyfunction);
            return "";
        }

        public static object _ks_Hotkey_38()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind>!}AltTab{AltTab DownR}");
            return "";
        }

        public static object _ks_Hotkey_39()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{AltTab Up}");
            return "";
        }

        public static object _ks_Hotkey_40()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind>!}ShiftAltTab{ShiftAltTab DownR}");
            return "";
        }

        public static object _ks_Hotkey_41()
        {
            Keysharp.Core.Keyboard.SetKeyDelay(-1);
            Keysharp.Core.Keyboard.Send("{Blind}{ShiftAltTab Up}");
            return "";
        }

        public static object _ks_Hotkey_42(object thishotkey)
        {
            Keysharp.Core.Keyboard.Send("{Alt down}{tab}");
            return "";
        }

        public static object _ks_Hotkey_43(object thishotkey)
        {
            Keysharp.Core.Keyboard.Send("{Alt up}");
            return "";
        }

        public static object _ks_HotIf_2()
        {
            return Keysharp.Core.WindowX.WinExist("ahk_group AltTabWindow");
        }

        public static object _ks_Hotkey_44(object thishotkey)
        {
            Keysharp.Core.Keyboard.Send("{Alt up}");
            return "";
        }

        public static object _ks_Hotkey_45(object thishotkey)
        {
            return "";
        }

        public static object _ks_Hotkey_46(object thishotkey)
        {
            Keysharp.Core.Dialogs.MsgBox("hello");
            return "";
        }

        public static object _ks_Hotkey_47(object thishotkey)
        {
            Keysharp.Core.Dialogs.MsgBox("hello");
            return "";
        }

        public static object _ks_Hotkey_48(object thishotkey)
        {
            Keysharp.Core.Dialogs.MsgBox("hello");
            return "";
        }

        public static object _ks_Hotkey_49(object thishotkey)
        {
            Keysharp.Core.Dialogs.MsgBox("hello");
            return "";
        }

        public static object _ks_Hotkey_50(object thishotkey)
        {
            Keysharp.Core.Dialogs.MsgBox("hello");
            return "";
        }

        public static object _ks_Hotkey_51(object thishotkey)
        {
            Keysharp.Core.Dialogs.MsgBox("hello");
            return "";
        }

        public static object _ks_Hotkey_52(object thishotkey)
        {
            Keysharp.Core.Dialogs.MsgBox("hello");
            return "";
        }

        public static object a = null;
        public static object Testfunc(object p1)
        {
            a = p1;
            return "";
        }

        [System.STAThreadAttribute()]
        public static int Main(string[] args)
        {
            try
            {
                string name = @"C:\Users\minip\Source\Repos\Keysharp_clone\bin\debug\net9.0-windows\Keysharp.ks";
                Keysharp.Scripting.Script.Variables.InitGlobalVars();
                Keysharp.Scripting.Script.SetName(name);
                if (Keysharp.Scripting.Script.HandleSingleInstance(name, eScriptInstance.Prompt))
                {
                    return 0;
                }

                Keysharp.Core.Env.HandleCommandLineParams(args);
                Keysharp.Scripting.Script.CreateTrayMenu();
                Keysharp.Scripting.Script.RunMainWindow(name, _ks_UserMainCode, false);
                Keysharp.Scripting.Script.WaitThreads();
                return Environment.ExitCode;
            }
            catch (Keysharp.Core.Error kserr)
            {
                if (ErrorOccurred(kserr))
                {
                    var (_ks_pushed, _ks_btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
                    MsgBox("Uncaught Keysharp exception:\r\n" + kserr, $"{Accessors.A_ScriptName}: Unhandled exception", "iconx");
                    Keysharp.Core.Common.Threading.Threads.EndThread(_ks_pushed);
                }

                Keysharp.Core.Flow.ExitApp(1);
                return Environment.ExitCode;
            }
            catch (System.Exception mainex)
            {
                var ex = mainex.InnerException ?? mainex;
                if (ex is Keysharp.Core.Error kserr)
                {
                    if (ErrorOccurred(kserr))
                    {
                        var (_ks_pushed, _ks_btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
                        MsgBox("Uncaught Keysharp exception:\r\n" + kserr, $"{Accessors.A_ScriptName}: Unhandled exception", "iconx");
                        Keysharp.Core.Common.Threading.Threads.EndThread(_ks_pushed);
                    }
                }
                else
                {
                    var (_ks_pushed, _ks_btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
                    MsgBox("Uncaught exception:\r\n" + "Message: " + ex.Message + "\r\nStack: " + ex.StackTrace, $"{Accessors.A_ScriptName}: Unhandled exception", "iconx");
                    Keysharp.Core.Common.Threading.Threads.EndThread(_ks_pushed);
                };
                Keysharp.Core.Flow.ExitApp(1);
                return Environment.ExitCode;
            }
        }

        public static object _ks_UserMainCode()
        {
            Keysharp.Core.Common.Keyboard.HotstringManager.AddHotstring("::btw", null, ":btw", "btw", "by the way", false);
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_1"), 0U, "**^a");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_2"), 0U, "*^a up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_3), 0U, "^!a");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_3), 0U, "^!b");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_4"), 0U, "**a");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_5"), 0U, "*a up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_6"), 0U, "**+A");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_7"), 0U, "*+A up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_8"), 0U, "**^x");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_9"), 0U, "*^x up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_10"), 0U, "**MButton");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_11"), 0U, "*MButton up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_12"), 0U, "**XButton1");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_13"), 0U, "*XButton1 up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_14"), 0U, "**RAlt");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_15"), 0U, "*RAlt up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_16"), 0U, "**CapsLock");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_17"), 0U, "*CapsLock up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_18"), 0U, "**XButton2");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_19"), 0U, "*XButton2 up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_20"), 0U, "**LAlt");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_21"), 0U, "*LAlt up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_22"), 0U, "**RCtrl");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_23"), 0U, "*RCtrl up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_24"), 0U, "**Ctrl");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_25"), 0U, "*Ctrl up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_26), 0U, "RWin");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.HotIf(Keysharp.Core.Functions.FuncObj("_ks_HotIf_1"));
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_27"), 0U, "**c");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_28"), 0U, "*c up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.HotIf("");
            Keysharp.Core.Accessors.A_UseHook = 1;
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_29"), 0U, "**e");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_30"), 0U, "*e up");
            Keysharp.Core.Accessors.A_UseHook = 0;
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_31), 0U, "^!f");
            Keysharp.Core.Accessors.A_UseHook = 1;
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_32), 0U, "*Numpad0");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_33), 0U, "*Numpad0 up");
            Keysharp.Core.Accessors.A_UseHook = 0;
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_34), 0U, "~LButton");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_35), 0U, "c");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_35), 0U, "d");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_36), 0U, "g");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_36), 0U, "h");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_37), 0U, "^+o");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_37), 0U, "^+e");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_38"), 0U, "*RAlt & j");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_39"), 0U, "RAlt & j up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_40"), 0U, "*RAlt & k");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj("_ks_Hotkey_41"), 0U, "RAlt & k up");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_42), 0U, "*F1");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_43), 0U, "!F2");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.HotIf(Keysharp.Core.Functions.FuncObj("_ks_HotIf_2"));
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_44), 0U, "~*Esc");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.HotIf("");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_45), 0U, "#h");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_46), 0U, "[");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_47), 0U, "]");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_48), 0U, "(");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_49), 0U, ")");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_50), 0U, "{");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_51), 0U, "}");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.FuncObj(_ks_Hotkey_52), 0U, "`;");
            Keysharp.Core.Common.Keyboard.HotstringManager.AddHotstring("::btw", null, ":btw", "btw", "by the way", false);
            Keysharp.Core.Common.Keyboard.HotstringManager.AddHotstring("::`/`*", null, ":`/`*", "`/`*", "abcd`*`/", false);
            x = 0L;
            Invoke(namedhotkeyfunction, "Call", "");
            Keysharp.Core.WindowX.GroupAdd("AltTabWindow", "ahk_class MultitaskingViewFrame");
            a = 123L;
            ((ICallable)testfunc).Call("::");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, "::")))
                Keysharp.Core.Files.FileAppend("pass", "*");
            else
                Keysharp.Core.Files.FileAppend("fail", "*");
            Keysharp.Core.Files.FileAppend("pass", "*");
            Keysharp.Core.Flow.ExitApp();
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
            return "";
        }
    }
}