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

    public class Program
    {
        public static object logerror1 = Keysharp.Core.Functions.Func("logerror1");
        public static object logerror2 = Keysharp.Core.Functions.Func("logerror2");
        public static object logerror3 = Keysharp.Core.Functions.Func("logerror3");
        public static object timerhandler = Keysharp.Core.Functions.Func("timerhandler");
        public static object x = null;
        public static object Logerror1(object exception, object mode)
        {
            x = x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, x, 1L);
            return "";
        }

        public static object Logerror2(object exception, object mode)
        {
            x = x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, x, 1L);
            return "";
        }

        public static object Logerror3(object exception, object mode)
        {
            x = x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, x, 1L);
            return Keysharp.Scripting.Script.OperateUnary(Keysharp.Scripting.Script.Operator.Minus, 1L);
        }

        public static object fileappend = Keysharp.Core.Functions.Func("fileappend");
        public static object fo1 = null;
        public static object Timerhandler()
        {
            x = x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, x, 1L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 1L)))
            {
                Keysharp.Core.Flow.SetTimer(fo1, 0L);
            }

            Keysharp.Core.Flow.Exit();
            x = 123L;
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
            Keysharp.Core.Errors.OnError("LogError1");
            Keysharp.Core.Errors.OnError("LogError2");
            Keysharp.Core.Errors.OnError("LogError3");
            x = 0L;
            Keysharp.Core.WindowX.WinActivate("C3D38B48-B165-4A69-9D8F-020DCD360712");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 3L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Core.Errors.OnError("LogError1", 0L);
            Keysharp.Core.Errors.OnError("LogError2", 0L);
            x = 0L;
            Keysharp.Core.WindowX.WinActivate("C3D38B48-B165-4A69-9D8F-020DCD360712");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 1L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = 0L;
            fo1 = Keysharp.Core.Functions.Func("TimerHandler");
            Keysharp.Core.Flow.SetTimer(fo1, 100L);
            Keysharp.Core.Flow.Sleep(1000L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 1L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Core.Errors.OnError("LogError3", 0L);
            Keysharp.Core.Flow.ExitApp();
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
            Keysharp.Core.Flow.ExitApp(0L);
            return "";
        }
    }
}