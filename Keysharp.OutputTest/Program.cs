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

    public class Program
    {
        public static object callbackzeroargs = Keysharp.Core.Functions.Func("callbackzeroargs");
        public static object callbacktwoargs = Keysharp.Core.Functions.Func("callbacktwoargs");
        public static object mcode = Keysharp.Core.Functions.Func("mcode");
        public static object Callbackzeroargs()
        {
            return 3L;
        }

        public static object Callbacktwoargs(object arg1, object arg2)
        {
            return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, arg1, arg2);
        }

        public static object ptr = null;
        public static object freq = null;
        public static object counterbefore = null;
        public static object result = null;
        public static object p = null;
        public static object counterafter = null;
        public static object msgbox = Keysharp.Core.Functions.Func("msgbox");
        public static object fileappend = Keysharp.Core.Functions.Func("fileappend");
        public static object MCODE_e = null;
        public static object MCODE_c = null;
        public static object Mcode(object mcode)
        {
            object p = null;
            object m = null;
            object s = null;
            object _ks_temp1 = null;
            object op = null;
            Keysharp.Scripting.Script.InitStaticVariable(ref MCODE_e, "Program_MCODE_e", () => Keysharp.Core.Collections.Map("1", 4L, "2", 1L));
            Keysharp.Scripting.Script.InitStaticVariable(ref MCODE_c, "Program_MCODE_c", () => Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, A_PtrSize, 8L)) ? "x64" : "x86");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Core.RegEx.RegExMatch(mcode, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "^([0-9]+),(", MCODE_c), ":|.*?,"), MCODE_c), ":)([^,]+)"), new VarRef(() => m, (value) => m = value)))))
            {
                return "";
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.LogicalNot, Keysharp.Core.Dll.DllCall("crypt32\\CryptStringToBinary", "str", Keysharp.Scripting.Script.GetPropertyValue(m, "3"), "uint", 0L, "uint", Keysharp.Scripting.Script.Index(MCODE_e, Keysharp.Scripting.Script.GetPropertyValue(m, "1")), "ptr", 0L, "uint*", Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = new VarRef(() => s, (value) => s = value), Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, "__Value", 0L), new VarRef(() => s, (value) => s = value)), "ptr", 0L, "ptr", 0L))))
            {
                return "";
            }

            p = Keysharp.Core.Dll.DllCall("GlobalAlloc", "uint", 0L, "ptr", s, "ptr");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, MCODE_c, "x64")))
            {
                Keysharp.Core.Dll.DllCall("VirtualProtect", "ptr", p, "ptr", s, "uint", 64L, "uint*", Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = new VarRef(() => op, (value) => op = value), Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, "__Value", 0L), new VarRef(() => op, (value) => op = value)));
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Core.Dll.DllCall("crypt32\\CryptStringToBinary", "str", Keysharp.Scripting.Script.GetPropertyValue(m, "3"), "uint", 0L, "uint", Keysharp.Scripting.Script.Index(MCODE_e, Keysharp.Scripting.Script.GetPropertyValue(m, "1")), "ptr", p, "uint*", new VarRef(() => s, (value) => s = value), "ptr", 0L, "ptr", 0L)))
            {
                return p;
            }

            Keysharp.Core.Dll.DllCall("GlobalFree", "ptr", p);
            return "";
        }

        [System.STAThreadAttribute()]
        public static int Main(string[] args)
        {
            try
            {
                string name = @"C:\Users\minip\Source\Repos\Keysharp_clone\bin\debug\net9.0-windows\*";
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
            object _ks_temp1 = null;
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
            ptr = Invoke(mcode, "Call", "2,x64:SInIidFEicJI/+A=");
            Keysharp.Core.Dll.DllCall("QueryPerformanceFrequency", "Int64*", Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = new VarRef(() => freq, (value) => freq = value), Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, "__Value", 0L), new VarRef(() => freq, (value) => freq = value)));
            Keysharp.Core.Dll.DllCall("QueryPerformanceCounter", "Int64*", Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = new VarRef(() => counterbefore, (value) => counterbefore = value), Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, "__Value", 0L), new VarRef(() => counterbefore, (value) => counterbefore = value)));
            {
                System.Collections.IEnumerator _ks_e1 = Keysharp.Core.Loops.Loop(100000L).GetEnumerator();
                Keysharp.Core.Loops.Push(Keysharp.Core.LoopType.Normal);
                try
                {
                    for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                    {
                        result = 0L;
                        result = Keysharp.Core.Dll.DllCall(ptr, "ptr", p = Keysharp.Core.Dll.CallbackCreate(callbacktwoargs), "int", Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.Minus, 1L), "int", 4L);
                        Keysharp.Core.Dll.CallbackFree(p);
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

            Keysharp.Core.Dll.DllCall("QueryPerformanceCounter", "Int64*", Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = new VarRef(() => counterafter, (value) => counterafter = value), Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, "__Value", 0L), new VarRef(() => counterafter, (value) => counterafter = value)));
            //Keysharp.Core.Dialogs.MsgBox(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, "Elapsed QPC time is ", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Divide, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, counterafter, counterbefore), freq), 1000L)), " ms"));
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, result, 3L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.ExitIfNotPersistent();
            return "";
        }
    }
}