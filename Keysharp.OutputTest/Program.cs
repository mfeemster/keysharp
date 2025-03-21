using static Keysharp.Core.Accessors;
using static Keysharp.Core.COM.Com;
using static Keysharp.Core.Collections;
using static Keysharp.Core.Common.Keyboard.HotkeyDefinition;
using static Keysharp.Core.Common.Keyboard.HotstringDefinition;
using static Keysharp.Core.Common.Keyboard.HotstringManager;
using static Keysharp.Core.ControlX;
using static Keysharp.Core.Debug;
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
using static Keysharp.Core.KeysharpEnhancements;
using static Keysharp.Core.Loops;
using static Keysharp.Core.Maths;
using static Keysharp.Core.Menu;
using static Keysharp.Core.Misc;
using static Keysharp.Core.Monitor;
using static Keysharp.Core.Mouse;
using static Keysharp.Core.Network;
using static Keysharp.Core.Processes;
using static Keysharp.Core.RegEx;
using static Keysharp.Core.Registrys;
using static Keysharp.Core.Screen;
using static Keysharp.Core.Sound;
using static Keysharp.Core.Strings;
using static Keysharp.Core.ToolTips;
using static Keysharp.Core.Types;
using static Keysharp.Core.WindowX;
using static Keysharp.Core.Windows.WindowsAPI;
using static Keysharp.Scripting.Script.Operator;
using static Keysharp.Scripting.Script;

[assembly: Keysharp.Scripting.AssemblyBuildVersionAttribute("0.0.0.9")]
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

    public class Program
    {
        public static object msgbox = Keysharp.Core.Functions.Func("msgbox");
        public static object exitapp = Keysharp.Core.Functions.Func("exitapp");
        public static object _ks_Hotkey_1(object thishotkey)
        {
            object idobject = null;
            object iid = null;
            object oacc = null;
            object x = null;
            object y = null;
            object w = null;
            object h = null;
            idobject = 4294967288L;
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Core.Dll.DllCall("oleacc\\AccessibleObjectFromWindow", "ptr", Keysharp.Core.WindowX.WinExist("A"), "uint", idobject = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.BitwiseAnd, idobject, 4294967295L), "ptr", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.Minus, 16L), Keysharp.Core.External.NumPut("int64", Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, idobject, 4294967280L)) ? 5044031582654955712L : 8186198323179293825L, Keysharp.Core.External.NumPut("int64", Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, idobject, 4294967280L)) ? 132096L : 1283310653150803680L, iid = Keysharp.Core.Collections.Buffer(16L)))), "ptr*", oacc = Keysharp.Core.COM.Com.ComValue(9L, 0L)), 0L)))
            {
                Keysharp.Scripting.Script.Invoke(oacc, "accLocation", new VarRef(() => x, (value) => x = value), new VarRef(() => y, (value) => y = value), new VarRef(() => w, (value) => w = value), new VarRef(() => h, (value) => h = value), 0L);
                Keysharp.Core.Dialogs.MsgBox(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, x, " "), y));
                Keysharp.Core.Flow.ExitApp();
            }

            Keysharp.Core.Dialogs.MsgBox("Failed!");
            return "";
        }

        [System.STAThreadAttribute()]
        public static int Main(string[] args)
        {
            try
            {
                string name = @"C:\Users\minip\Source\Repos\Keysharp_clone\bin\debug\net9.0-windows\*";
                Keysharp.Scripting.Script.Variables.AddPreLoadedDll("oleacc.dll", false);
                Keysharp.Scripting.Script.Variables.InitGlobalVars();
                Keysharp.Scripting.Script.SetName(name);
                if (Keysharp.Scripting.Script.HandleSingleInstance(name, eScriptInstance.Prompt))
                {
                    return 0;
                }

                Keysharp.Core.Env.HandleCommandLineParams(args);
                Keysharp.Scripting.Script.CreateTrayMenu();
                Keysharp.Scripting.Script.RunMainWindow(name, AutoExecSection, false);
                Keysharp.Scripting.Script.WaitThreads();
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
            }

            return Environment.ExitCode;
        }

        public static object AutoExecSection()
        {
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.AddHotkey(Keysharp.Core.Functions.Func(_ks_Hotkey_1), 0U, "F1");
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
            Keysharp.Scripting.Script.ExitIfNotPersistent();
            return "";
        }
    }
}