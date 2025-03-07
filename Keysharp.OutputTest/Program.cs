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
        public static object myclass => Program.Myclass.__Static;

        public static object c = null;
        public class Myclass : KeysharpObject
        {
            public new (Type, object) super => (typeof(KeysharpObject), this);

            public Myclass(params object[] args) : base(args)
            {
            }

            public static string __Class
            {
                get
                {
                    return "myclass";
                }
            }

            new public static object __Static { get; set; }

            public object Classfunc()
            {
                return 123L;
            }

            public object staticClassfuncstatic()
            {
                return Keysharp.Scripting.Script.GetPropertyValue(this, "s1");
            }

            public object staticClassfuncstatic2()
            {
                return 456L;
            }

            public object Classfuncusesstatic()
            {
                return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.GetPropertyValue(myclass, "s1"), Keysharp.Scripting.Script.GetPropertyValue(this, "x"));
            }

            public object Classfuncwithlocalvars()
            {
                object lv1 = null;
                object lv2 = null;
                lv1 = 10L;
                lv2 = 10L;
                return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, lv1, lv2);
            }

            public object Classfuncwithreadmembervars()
            {
                return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.GetPropertyValue(this, "x"), Keysharp.Scripting.Script.GetPropertyValue(this, "y"));
            }

            public object Classfuncwithwritelocalmembervars()
            {
                object x = null;
                object y = null;
                x = 88L;
                y = 99L;
                return "";
            }

            public object Classfuncwithwritemembervars()
            {
                Keysharp.Scripting.Script.SetPropertyValue(this, "x", 88L);
                Keysharp.Scripting.Script.SetPropertyValue(this, "y", 99L);
                return "";
            }

            public static object CLASSFUNCWITHLOCALSTATICVARS_aa = null;
            public object Classfuncwithlocalstaticvars()
            {
                Keysharp.Scripting.Script.InitStaticVariable(ref CLASSFUNCWITHLOCALSTATICVARS_aa, "Myclass_CLASSFUNCWITHLOCALSTATICVARS_aa", () => 100L);
                return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, aa, 10L);
            }

            public object Classfuncwriteglobalvars()
            {
                Keysharp.Scripting.Script.SetPropertyValue(this, "a", 0L);
                a = 1L;
                return "";
            }

            public object staticClassfuncstaticwithparams(object val1, object val2)
            {
                return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, val1, val2);
            }

            public object Classfuncwithparams(object val1, object val2)
            {
                return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, val1, val2);
            }

            public object Classvarfunc(object p1, params object[] _ks_theparams)
            {
                object temp = null;
                object n = null;
                object theparams = new Array(_ks_theparams);
                temp = p1;
                {
                    var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(theparams, Misc.MakeVarRef(() => n, (Val) => n = Val)).GetEnumerator();
                    object[] _ks_e1_backup = new object[1]
                    {
                        n
                    };
                    Keysharp.Core.Loops.Push();
                    try
                    {
                        for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                        {
                            Keysharp.Core.Loops.Inc();
                            temp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, temp, Keysharp.Scripting.Script.Index(theparams, A_Index));
                        _ks_e1_next:
                            ;
                        }
                    }
                    finally
                    {
                        n = _ks_e1_backup[0];
                        Keysharp.Core.Loops.Pop();
                    }

                _ks_e1_end:
                    ;
                }

                temp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, temp, p1);
                {
                    var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(theparams, Misc.MakeVarRef(() => n, (Val) => n = Val)).GetEnumerator();
                    object[] _ks_e1_backup = new object[1]
                    {
                        n
                    };
                    Keysharp.Core.Loops.Push();
                    try
                    {
                        for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                        {
                            Keysharp.Core.Loops.Inc();
                            temp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, temp, n);
                        _ks_e1_next:
                            ;
                        }
                    }
                    finally
                    {
                        n = _ks_e1_backup[0];
                        Keysharp.Core.Loops.Pop();
                    }

                _ks_e1_end:
                    ;
                }

                return temp;
            }

            public object staticClassvarfuncstatic(object p1, params object[] _ks_theparams)
            {
                object temp = null;
                object n = null;
                object theparams = new Array(_ks_theparams);
                temp = p1;
                {
                    var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(theparams, Misc.MakeVarRef(() => n, (Val) => n = Val)).GetEnumerator();
                    object[] _ks_e1_backup = new object[1]
                    {
                        n
                    };
                    Keysharp.Core.Loops.Push();
                    try
                    {
                        for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                        {
                            Keysharp.Core.Loops.Inc();
                            temp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, temp, Keysharp.Scripting.Script.Index(theparams, A_Index));
                        _ks_e1_next:
                            ;
                        }
                    }
                    finally
                    {
                        n = _ks_e1_backup[0];
                        Keysharp.Core.Loops.Pop();
                    }

                _ks_e1_end:
                    ;
                }

                temp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, temp, p1);
                {
                    var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(theparams, Misc.MakeVarRef(() => n, (Val) => n = Val)).GetEnumerator();
                    object[] _ks_e1_backup = new object[1]
                    {
                        n
                    };
                    Keysharp.Core.Loops.Push();
                    try
                    {
                        for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                        {
                            Keysharp.Core.Loops.Inc();
                            temp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, temp, n);
                        _ks_e1_next:
                            ;
                        }
                    }
                    finally
                    {
                        n = _ks_e1_backup[0];
                        Keysharp.Core.Loops.Pop();
                    }

                _ks_e1_end:
                    ;
                }

                return temp;
            }

            public object Classfuncwiththis()
            {
                object val = null;
                Keysharp.Scripting.Script.SetPropertyValue(this, "a", 999L);
                val = Keysharp.Scripting.Script.GetPropertyValue(this, "a");
                return val;
            }

            public object Classfunccasesensitive()
            {
                Keysharp.Scripting.Script.SetPropertyValue(this, "a", 1000L);
                Keysharp.Scripting.Script.Invoke(this, "ClassFuncCaseSensitive2");
                Keysharp.Scripting.Script.Invoke(this, "classfunccasesensitive2");
                return "";
            }

            public object Classfunccasesensitive2()
            {
                Keysharp.Scripting.Script.SetPropertyValue(this, "b", 2000L);
                return "";
            }

            public object staticClassfunccasesensitivestatic()
            {
                Keysharp.Scripting.Script.Invoke(this, "ClassFuncCaseSensitiveStatic2");
                Keysharp.Scripting.Script.Invoke(this, "classfunccasesensitivestatic2");
                return "";
            }

            public object staticClassfunccasesensitivestatic2()
            {
                Keysharp.Scripting.Script.SetPropertyValue(this, "s1", 999L);
                return "";
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
                Keysharp.Scripting.Script.SetPropertyValue(this, "a", "");
                Keysharp.Scripting.Script.SetPropertyValue(this, "b", c = "asdf");
                Keysharp.Scripting.Script.SetPropertyValue(this, "x", 123L);
                Keysharp.Scripting.Script.SetPropertyValue(this, "y", Keysharp.Scripting.Script.GetPropertyValue(this, "x"));
            }

            public void static__Init()
            {
                Keysharp.Scripting.Script.SetPropertyValue(this, "s1", 10L);
            }

            static Myclass()
            {
                Script.InitStaticInstance(typeof(Myclass));
                __Static = Variables.Statics[typeof(Myclass)];
            }

            public Myclass staticCall(params object[] args)
            {
                return new Myclass(args);
            }
        }

        public static object classobj = null;
        public static object val = null;
        public static object fileappend = Keysharp.Core.Functions.Func("fileappend");
        public static object fo = null;
        public static object funcadd = null;
        public static object myclass2 => Program.Myclass2.__Static;

        public class Myclass2 : KeysharpObject
        {
            public new (Type, object) super => (typeof(KeysharpObject), this);

            public Myclass2(params object[] args) : base(args)
            {
            }

            public static string __Class
            {
                get
                {
                    return "myclass2";
                }
            }

            new public static object __Static { get; set; }

            public object Classfunc0()
            {
                a = 0L;
                return 0L;
            }

            public object Classfunc1(object p1)
            {
                a = p1;
                return p1;
            }

            public object Classfunc2(object p1, [Optional, DefaultParameterValue(5L)] object p2)
            {
                object temp = null;
                temp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, p1, p2);
                a = temp;
                return temp;
            }

            public object Classfunc3(object p1, [Optional, DefaultParameterValue(5L)] object p2, params object[] _ks_p3)
            {
                object temp = null;
                object n = null;
                object p3 = new Array(_ks_p3);
                temp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, p1, p2);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.GetPropertyValue(p3, "Length")))
                {
                    {
                        var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(p3, Misc.MakeVarRef(() => n, (Val) => n = Val)).GetEnumerator();
                        object[] _ks_e1_backup = new object[1]
                        {
                            n
                        };
                        Keysharp.Core.Loops.Push();
                        try
                        {
                            for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                            {
                                Keysharp.Core.Loops.Inc();
                                temp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, temp, Keysharp.Scripting.Script.Index(p3, A_Index));
                            _ks_e1_next:
                                ;
                            }
                        }
                        finally
                        {
                            n = _ks_e1_backup[0];
                            Keysharp.Core.Loops.Pop();
                        }

                    _ks_e1_end:
                        ;
                    }
                }

                a = temp;
                return temp;
            }

            public object Classfunc4(object p1, [Optional, DefaultParameterValue(5L)] object p2, [Optional, DefaultParameterValue(null)] VarRef p3)
            {
                p3 ??= new VarRef(10L);
                return Keysharp.Scripting.Script.SetPropertyValue(((VarRef)p3), "__Value", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, p1, p2), ((VarRef)p3).__Value));
            }

            public object Classfuncimplicit(params object[] _ks_args)
            {
                object temp = null;
                object n = null;
                object args = new Array(_ks_args);
                temp = 0L;
                {
                    var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(args, Misc.MakeVarRef(() => n, (Val) => n = Val)).GetEnumerator();
                    object[] _ks_e1_backup = new object[1]
                    {
                        n
                    };
                    Keysharp.Core.Loops.Push();
                    try
                    {
                        for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                        {
                            Keysharp.Core.Loops.Inc();
                            temp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, temp, Keysharp.Scripting.Script.Index(args, A_Index));
                        _ks_e1_next:
                            ;
                        }
                    }
                    finally
                    {
                        n = _ks_e1_backup[0];
                        Keysharp.Core.Loops.Pop();
                    }

                _ks_e1_end:
                    ;
                }

                a = temp;
                return temp;
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Myclass2()
            {
                Script.InitStaticInstance(typeof(Myclass2));
                __Static = Variables.Statics[typeof(Myclass2)];
            }

            public Myclass2 staticCall(params object[] args)
            {
                return new Myclass2(args);
            }
        }

        public static object arr = null;
        public static object class2obj = null;
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
            object _ks_temp1 = null;
            a = "";
            classobj = Invoke(myclass, "Call");
            val = Keysharp.Scripting.Script.Invoke(classobj, "classfunc");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(myclass, "classfuncstatic");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 10L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = 0L;
            fo = Keysharp.Scripting.Script.GetPropertyValue(myclass, "classfuncstatic");
            val = Invoke(fo, "Call", myclass);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 10L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            fo = Keysharp.Scripting.Script.IfTest(true) ? Keysharp.Scripting.Script.GetPropertyValue(myclass, "classfuncstatic") : Keysharp.Scripting.Script.GetPropertyValue(myclass, "classfuncstatic2");
            val = Invoke(fo, "Call", myclass);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 10L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(classobj, "classfuncusesstatic");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 1230L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetPropertyValue(myclass, "s1", 1L);
            val = Keysharp.Scripting.Script.Invoke(classobj, "classfuncusesstatic");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(classobj, "classfuncwithlocalvars");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 100L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(classobj, "classfuncwithreadmembervars");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 15129L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.Invoke(classobj, "classfuncwithwritelocalmembervars");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(classobj, "x"), 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(classobj, "y"), 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.Invoke(classobj, "classfuncwithwritemembervars");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(classobj, "x"), 88L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(classobj, "y"), 99L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(classobj, "classfuncwithlocalstaticvars");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 1000L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.Invoke(classobj, "classfuncwriteglobalvars");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(classobj, "a"), 0L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 1L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(myclass, "classfuncstaticwithparams", 150L, 2L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 300L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(classobj, "classfuncwithparams", 500L, 2L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 1000L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(myclass, "classvarfuncstatic", 1L, 2L, 3L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 12L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(classobj, "classvarfunc", 1L, 2L, 3L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 12L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(classobj, "classfuncwiththis");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 999L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.Invoke(classobj, "ClassFuncCaseSensitive");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(classobj, "a"), 1000L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(classobj, "b"), 2000L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetPropertyValue(classobj, "a", "");
            Keysharp.Scripting.Script.SetPropertyValue(classobj, "b", "");
            Keysharp.Scripting.Script.Invoke(classobj, "classfunccasesensitive");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(classobj, "a"), 1000L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(classobj, "b"), 2000L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetPropertyValue(myclass, "s1", "");
            Keysharp.Scripting.Script.Invoke(myclass, "ClassFuncCaseSensitiveStatic");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(myclass, "s1"), 999L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            funcadd = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(classobj, "classfuncwithparams"), "Bind", classobj);
            val = Invoke(funcadd, "Call", 10L, 20L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 200L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            funcadd = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclass, "classfuncstaticwithparams"), "Bind", myclass);
            val = Invoke(funcadd, "Call", 10L, 10L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 100L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            funcadd = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(classobj, "classvarfunc"), "Bind", classobj);
            val = Invoke(funcadd, "Call", 1L, 2L, 3L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 12L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            funcadd = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclass, "classvarfuncstatic"), "Bind", myclass);
            val = Invoke(funcadd, "Call", 1L, 2L, 3L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 12L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            a = "";
            arr = new Keysharp.Core.Array(1L, 2L, 3L);
            class2obj = Invoke(myclass2, "Call");
            Keysharp.Scripting.Script.Invoke(class2obj, "classfunc0");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 0L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            a = "";
            val = Keysharp.Scripting.Script.Invoke(class2obj, "classfunc0");
            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 0L)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 0L))).ParseObject()))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            a = "";
            Keysharp.Scripting.Script.Invoke(class2obj, "classfunc1", 1L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 1L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            a = "";
            Keysharp.Scripting.Script.Invoke(class2obj, "classfunc2", 1L, 2L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 3L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            a = "";
            Keysharp.Scripting.Script.Invoke(class2obj, "classfunc3", 1L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 6L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            a = "";
            Keysharp.Scripting.Script.Invoke(class2obj, "classfunc3", 1L, 2L, 4L, 5L, 6L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 18L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            a = "";
            Keysharp.Scripting.Script.Invoke(class2obj, "classfunc3", [1L, 2L, .. Keysharp.Scripting.Script.FlattenParam(arr)]);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 9L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(class2obj, "classfunc4", 1L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 16L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(class2obj, "classfunc4", 1L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 16L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(class2obj, "classfunc4", 1L, 10L, Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = new VarRef(() => a, (value) => a = value), Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, "__Value", 15L), new VarRef(() => a, (value) => a = value)));
            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 26L)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 26L))).ParseObject()))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Invoke(class2obj, "classfuncimplicit");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 0L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            fo = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(class2obj, "classfunc0"), "Bind", class2obj);
            a = "";
            val = Invoke(fo, "Call");
            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 0L)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 0L))).ParseObject()))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            fo = Keysharp.Scripting.Script.Invoke(class2obj, "GetMethod", "classfunc1");
            a = "";
            val = Invoke(fo, "Call", class2obj, 123L);
            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 123L)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 123L))).ParseObject()))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            fo = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(class2obj, "classfunc2"), "Bind", class2obj);
            a = "";
            val = Invoke(fo, "Call", 123L);
            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 128L)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 128L))).ParseObject()))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            fo = Keysharp.Scripting.Script.GetPropertyValue(class2obj, "classfunc3");
            a = "";
            val = Invoke(fo, "Call", class2obj, 1L);
            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 6L)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 6L))).ParseObject()))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            a = "";
            val = Invoke(fo, "Call", class2obj, 1L, 2L, 4L, 5L, 6L);
            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 18L)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 18L))).ParseObject()))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            fo = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(class2obj, "classfuncimplicit"), "Bind", class2obj);
            a = "";
            val = Invoke(fo, "Call");
            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 0L)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 0L))).ParseObject()))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            a = "";
            val = Invoke(fo, "Call", 1L, 2L, 4L, 5L, 6L);
            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 18L)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 18L))).ParseObject()))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }
            double i = 0;

            a = "";
            val = Invoke(fo, "Call", [.. Keysharp.Scripting.Script.FlattenParam(arr)]);
            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 6L)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, a, 6L))).ParseObject()))
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