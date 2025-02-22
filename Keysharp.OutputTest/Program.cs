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
        public static object func2 = Keysharp.Core.Functions.Func("func2");
        public static object func3 = Keysharp.Core.Functions.Func("func3");
        public static object funcdef1 = Keysharp.Core.Functions.Func("funcdef1");
        public static object funcdef2 = Keysharp.Core.Functions.Func("funcdef2");
        public static object funcdef3 = Keysharp.Core.Functions.Func("funcdef3");
        public static object test = Keysharp.Core.Functions.Func("test");
        public static object x = null;
        public static object y = null;
        public static object z = null;
        public static object fileappend = Keysharp.Core.Functions.Func("fileappend");
        public static object Func2(object a, object b, [Optional, DefaultParameterValue(123L)] object c)
        {
            x = a;
            y = b;
            z = c;
            return "";
        }

        public static object Func3(object a, object b, [Optional, DefaultParameterValue(null)] object c)
        {
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Core.Types.IsSet(c)))
            {
                x = true;
            }

            return "";
        }

        public static object Funcdef1([Optional, DefaultParameterValue("")] object p)
        {
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, p, "")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            return "";
        }

        public static object Funcdef2([Optional, DefaultParameterValue("\"")] object p)
        {
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, p, "\"")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            return "";
        }

        public static object Funcdef3([Optional, DefaultParameterValue("asdf")] object p)
        {
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, p, "asdf")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            return "";
        }

        public static object Test([Optional, DefaultParameterValue("\r\n\t")] object linebreak)
        {
            return linebreak;
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
            x = 1L;
            y = 2L;
            z = 3L;
            x = 1L;
            y = 2L;
            z = 3L;
            Invoke(func2, "Call", 11L, 22L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 11L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, y, 22L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, z, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = 1L;
            y = 2L;
            z = 3L;
            Invoke(func2, "Call", null);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, null)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, y, null)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, z, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = 1L;
            y = 2L;
            z = 3L;
            Invoke(func2, "Call", 22L, 33L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, null)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, y, 22L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, z, 33L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = 1L;
            y = 2L;
            z = 3L;
            Invoke(func2, "Call", 11L, null, 33L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 11L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, y, null)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, z, 33L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = 1L;
            y = 2L;
            z = 3L;
            Invoke(func2, "Call", 11L, null);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 11L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, y, null)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, z, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = 1L;
            y = 2L;
            z = 3L;
            Invoke(func2, "Call", 11L, null, null);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 11L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, y, null)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, z, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = 1L;
            y = 2L;
            z = 3L;
            Invoke(func2, "Call", 22L, null);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, null)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, y, 22L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, z, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = 1L;
            y = 2L;
            z = 3L;
            Invoke(func2, "Call", null, null);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, null)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, y, null)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, z, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = false;
            Invoke(func3, "Call", null);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, false)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = false;
            Invoke(func3, "Call", 1L, null);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, false)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = false;
            Invoke(func3, "Call", 1L, 2L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, false)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = false;
            Invoke(func3, "Call", 1L, 2L, 3L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, true)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = false;
            Invoke(func3, "Call", null);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, false)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            x = false;
            Invoke(func3, "Call", null, null);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, false)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Invoke(funcdef1, "Call");
            Invoke(funcdef2, "Call");
            Invoke(funcdef3, "Call");
            x = Invoke(test, "Call");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, "\r\n\t")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Core.Common.Keyboard.HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
            Keysharp.Core.Flow.ExitApp(0L);
            return "";
        }
    }
}