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
        public static object freq = null;
        public static object _ks_temp1 = null;
        public static object counterbefore = null;
        public static object arr = null;
        public static object counterafter = null;
        public static object msgbox = Keysharp.Core.Functions.Func("msgbox");
        [System.STAThreadAttribute()]
        public static int Main(string[] args)
        {
            try
            {
                string name = @"C:\Users\minip\source\repos\Keysharp_clone\bin\debug\net9.0-windows\*";
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

        public static object _ks_UserMainCode()
        {
            Keysharp.Core.Dll.DllCall("QueryPerformanceFrequency", "Int64*", Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = new VarRef(() => freq, (value) => freq = value), Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, "__Value", 0L), new VarRef(() => freq, (value) => freq = value)));
            Keysharp.Core.Dll.DllCall("QueryPerformanceCounter", "Int64*", Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = new VarRef(() => counterbefore, (value) => counterbefore = value), Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, "__Value", 0L), new VarRef(() => counterbefore, (value) => counterbefore = value)));
            arr = new Keysharp.Core.Array([]);
            {
                System.Collections.IEnumerator _ks_e1 = Keysharp.Core.Loops.Loop(1000000L).GetEnumerator();
                Keysharp.Core.Loops.Push(Keysharp.Core.LoopType.Normal);
                try
                {
                    for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                    {
                        Keysharp.Scripting.Script.Invoke(arr, "Push", 1L);
                        Keysharp.Scripting.Script.Invoke(arr, "Pop");
                    _ks_e1_next:
                        ;
                    }
                }
                finally
                {
                    Keysharp.Core.Loops.Pop();
                }

            _ks_e1_end:
                ;
            }

            Keysharp.Core.Dll.DllCall("QueryPerformanceCounter", "Int64*", Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = new VarRef(() => counterafter, (value) => counterafter = value), Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, "__Value", 0L), _ks_temp1));
            Keysharp.Core.Dialogs.MsgBox(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "Elapsed QPC time is ", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Divide, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, counterafter, counterbefore), freq), 1000L)), " ms"));
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
            Keysharp.Core.Flow.ExitApp(0L);
            return "";
        }
    }
}