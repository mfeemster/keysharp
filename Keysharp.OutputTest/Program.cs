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
        public static object a = null;
        public static object keysharpobject = Variables.Statics[typeof(KeysharpObject)];
        public static object _ks_anonlambda_1;
        public static object fileappend = Keysharp.Core.Functions.Func("fileappend");
        public static object b = null;
        public static object _ks_anonlambda_2;
        public static object test => Program.Test.__Static;

        public class Test : KeysharpObject
        {
            public new (Type, object) super => (typeof(KeysharpObject), this);

            public Test(params object[] args) : base(args)
            {
            }

            public static string __Class
            {
                get
                {
                    return "Test";
                }
            }

            new public static object __Static { get; set; }

            public object Hasownprop(params object[] _ks_args)
            {
                object args = new Array(_ks_args);
                return 1L;
            }

            public object get_protoGet()
            {
                return 2L;
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Test()
            {
                Script.InitStaticInstance(typeof(Test));
                __Static = Variables.Statics[typeof(Test)];
            }

            public Test staticCall(params object[] args)
            {
                return new Test(args);
            }
        }

        public static object testextend => Program.Testextend.__Static;

        public class Testextend : Test
        {
            public new (Type, object) super => (typeof(Test), this);

            public Testextend(params object[] args) : base(args)
            {
            }

            public static string __Class
            {
                get
                {
                    return "TestExtend";
                }
            }

            new public static object __Static { get; set; }

            public void __Init()
            {
                Invoke((object)super, "__Init");
                Keysharp.Scripting.Script.SetPropertyValue(this, "protoValue", 2L);
            }

            public void static__Init()
            {
            }

            static Testextend()
            {
                Script.InitStaticInstance(typeof(Testextend));
                __Static = Variables.Statics[typeof(Testextend)];
            }

            public Testextend staticCall(params object[] args)
            {
                return new Testextend(args);
            }
        }

        public static object o = null;
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
            Keysharp.Core.Common.Keyboard.HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
            static object _ks_AnonLambda_1(params object[] _ks_args)
            {
                object args = new Array(_ks_args);
                a = 1L;
                return "";
            }

            _ks_anonlambda_1 = Keysharp.Core.Functions.Func((Delegate)_ks_AnonLambda_1);
            static object _ks_AnonLambda_2(params object[] _ks_args)
            {
                object args = new Array(_ks_args);
                return 1L;
            }

            _ks_anonlambda_2 = Keysharp.Core.Functions.Func((Delegate)_ks_AnonLambda_2);
            a = 0L;
            Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(keysharpobject, "Prototype"), "DefineProp", "protoCall", Keysharp.Core.Objects.Object(new object[] { "call", _ks_anonlambda_1 }));
            Keysharp.Scripting.Script.Invoke(Keysharp.Core.Objects.Object(new object[] { }), "protoCall");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 1L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            b = 0L;
            Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(keysharpobject, "Prototype"), "DefineProp", "protoGet", Keysharp.Core.Objects.Object(new object[] { "get", _ks_anonlambda_2 }));
            b = Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Core.Objects.Object(new object[] { }), "protoGet");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, b, 1L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            b = 0L;
            Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(keysharpobject, "Prototype"), "DefineProp", "protoValue", Keysharp.Core.Objects.Object(new object[] { "value", 1L }));
            b = Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Core.Objects.Object(new object[] { }), "protoValue");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, b, 1L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            o = Invoke(testextend, "Call");
            b = 0L;
            b = Keysharp.Scripting.Script.Invoke(o, "HasOwnProp", "test");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, b, 1L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetPropertyValue(o, "base", Keysharp.Scripting.Script.GetPropertyValue(keysharpobject, "Prototype"));
            b = Keysharp.Scripting.Script.Invoke(o, "HasOwnProp", "test");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, b, 0L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, Keysharp.Core.Types.Type(o), "Object")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            a = 0L;
            Keysharp.Scripting.Script.Invoke(o, "protoCall");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 1L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            b = 0L;
            b = Keysharp.Scripting.Script.GetPropertyValue(o, "protoGet");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, b, 1L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            b = 0L;
            b = Keysharp.Scripting.Script.GetPropertyValue(o, "protoValue");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, b, 2L)))
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