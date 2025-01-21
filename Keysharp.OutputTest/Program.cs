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
        public static object myfunc = FuncObj("myfunc");
        public static object sum = FuncObj("sum");
        public static object sum2 = FuncObj("sum2");
        public static object doublefunc = FuncObj("doublefunc");
        public static object doublefunc2 = FuncObj("doublefunc2");
        public static object multfunc = FuncObj("multfunc");
        public static object assignfunc = FuncObj("assignfunc");
        public static object func2__ = FuncObj("func2__");
        public static object func = FuncObj("func");
        public static object myfunc10 = FuncObj("myfunc10");
        public static object myclass = Myclass.__Static;
        public static object x = null;
        public static object myclassobj = null;
        public static object double1 = null;
        public static object double2 = null;
        public static object myfunc2 = null;
        public static object myfunc3 = null;
        public static object myfunc4 = null;
        public static object myfunc5 = null;
        public static object myfunc6 = null;
        public static object myfunc7 = null;
        public static object myfunc8 = null;
        public static object y = null;
        public static object myfunc9 = null;
        public static object m = null;
        public static object arr = null;
        public static object b = null;
        public static object val = null;
        public static object gval = null;
        public static object lam = null;
        public static object tot = null;
        public static object f = null;
        public static object myclass2 = Myclass2.__Static;
        public static object class2obj = null;
        public static object z = null;
        public static object lamanondef = null;
        public static object lamnameddef = null;
        public static object myclass3 = Myclass3.__Static;
        public static object _ks_temp1 = null;
        public class Myclass : KeysharpObject
        {
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
            public object x { get; set; }
            public object t { get; set; }
            public object member1 { get; set; }
            public object member2 { get; set; }
            public object member3 { get; set; }
            public object member4 { get; set; }
            public object member5 { get; set; }
            public object member6 { get; set; }
            public object member7 { get; set; }
            public object member8 { get; set; }

            public object myprop
            {
                get
                {
                    return Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.GetPropertyValue(this, "t")) ? 1L : 0L;
                }

                set
                {
                    x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, 10L, value);
                    return "";
                }
            }

            public object Myclassfunc(object a, object b)
            {
                return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, 10L, x), a), b);
            }

            public object Callmember1(object a, object b)
            {
                return Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(this, "member1"), "call", a, b);
            }

            public void __Init()
            {
                Invoke((object)Super, "__Init");
                x = 123L;
                t = true;
                member1 = FuncObj((object a, object b) =>
                {
                    {
                        return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, b), 2L);
                    }
                });
                member2 = FuncObj((object a, object b) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, b), 2L);
                });
                member3 = FuncObj((object a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, 2L);
                });
                member4 = FuncObj((params object[] a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.Index(a, 1L), Keysharp.Scripting.Script.Index(a, 2L)), 2L);
                });
                member5 = FuncObj(() =>
                {
                    return 123L;
                });
                member6 = FuncObj((params object[] args) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.Index(args, 1L), Keysharp.Scripting.Script.Index(args, 2L)), 2L);
                });
                member7 = FuncObj((object a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, Keysharp.Scripting.Script.GetPropertyValue(this, "x"));
                });
                member8 = FuncObj((object a) =>
                {
                    return Keysharp.Scripting.Script.MultiStatement(Keysharp.Scripting.Script.SetPropertyValue(this, "x", 100L), Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, Keysharp.Scripting.Script.GetPropertyValue(this, "x")));
                });
            }

            public void static__Init()
            {
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

        public static object Myfunc()
        {
            {
                return 123L;
            }
        }

        public static object Sum(object a, object b)
        {
            {
                return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, a, b);
            }
        }

        public static object Sum2(object a, object b, params object[] c)
        {
            {
                return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, a, b), Keysharp.Scripting.Script.Index(c, 1L)), Keysharp.Scripting.Script.Index(c, 2L));
            }
        }

        public static object Multfunc(object a, object b)
        {
            {
                return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, b);
            }
        }

        public static object Assignfunc(object xx)
        {
            {
                b = xx;
                return "";
            }
        }

        public static object Func2__(object x)
        {
            {
                tot = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, tot, x);
                return "";
            }
        }

        public static object Func(object a, object b, object c)
        {
            {
                tot = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, tot, Invoke(a, "Call", 1L));
                tot = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, tot, Invoke(b, "Call", 2L));
                tot = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, tot, Invoke(c, "Call", 3L));
                Invoke(f, "Call", 10L);
                return "";
            }
        }

        public class Myclass2 : KeysharpObject
        {
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

            public object Func(object a, object b, object c)
            {
                tot = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, tot, Invoke(a, "Call", 1L));
                tot = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, tot, Invoke(b, "Call", 2L));
                tot = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, tot, Invoke(c, "Call", 3L));
                Invoke(f, "Call", 20L);
                return "";
            }

            public void __Init()
            {
                Invoke((object)Super, "__Init");
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

        public static object Myfunc10(object a, object b, object c)
        {
            {
                return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Invoke(a, "Call"), Invoke(b, "Call")), Invoke(c, "Call"));
            }
        }

        public class Myclass3 : KeysharpObject
        {
            public Myclass3(params object[] args) : base(args)
            {
            }

            public static string __Class
            {
                get
                {
                    return "myclass3";
                }
            }

            new public static object __Static { get; set; }
            public object member1 { get; set; }
            public object member2 { get; set; }
            public object member3 { get; set; }
            public object member4 { get; set; }
            public object member5 { get; set; }
            public object member6 { get; set; }

            public void __Init()
            {
                Invoke((object)Super, "__Init");
                member1 = FuncObj((object a, [Optional, DefaultParameterValue(2L)] object b) =>
                {
                    {
                        return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, b), 2L);
                    }
                });
                member2 = FuncObj(([Optional, DefaultParameterValue(2L)] object a, [Optional, DefaultParameterValue(3L)] object b) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, b), 2L);
                });
                member3 = FuncObj(([Optional, DefaultParameterValue(123L)] object a) =>
                {
                    return a;
                });
                member4 = FuncObj((object a, VarRef b, [Optional, DefaultParameterValue(5L)] object c, params object[] p) =>
                {
                    {
                        return Keysharp.Scripting.Script.SetPropertyValue(((VarRef)b), "__Value", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, ((VarRef)b).__Value), c), Keysharp.Scripting.Script.Index(p, 1L)));
                    }
                });
                member5 = FuncObj((object a, VarRef b, [Optional, DefaultParameterValue(5L)] object c, params object[] p) =>
                {
                    return Keysharp.Scripting.Script.SetPropertyValue(((VarRef)b), "__Value", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, ((VarRef)b).__Value), c), Keysharp.Scripting.Script.Index(p, 1L)));
                });
                member6 = FuncObj((object a, [Optional, DefaultParameterValue(5L)] object b, [Optional, DefaultParameterValue(null)] VarRef c) =>
                {
                    c ??= new VarRef(10L);
                    return Keysharp.Scripting.Script.SetPropertyValue(((VarRef)c), "__Value", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, a, b), ((VarRef)c).__Value));
                });
            }

            public void static__Init()
            {
            }

            static Myclass3()
            {
                Script.InitStaticInstance(typeof(Myclass3));
                __Static = Variables.Statics[typeof(Myclass3)];
            }

            public Myclass3 staticCall(params object[] args)
            {
                return new Myclass3(args);
            }
        }

        [System.STAThreadAttribute()]
        public static int Main(string[] args)
        {
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
        }

        public static object _ks_UserMainCode()
        {
            {
                object Doublefunc(object a, object b)
                {
                    {
                        return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, b), 2L);
                    }
                }

                doublefunc = FuncObj((Delegate)Doublefunc);
                object Doublefunc2(object a, object b, params object[] c)
                {
                    {
                        return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, b), Keysharp.Scripting.Script.Index(c, 1L)), 2L);
                    }
                }

                doublefunc2 = FuncObj((Delegate)Doublefunc2);
                myclassobj = Invoke(myclass, "Call");
                x = Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "myprop");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 1L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                Keysharp.Scripting.Script.SetPropertyValue(myclassobj, "myprop", 10L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "x"), 100L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                Keysharp.Scripting.Script.SetPropertyValue(myclassobj, "x", 10L);
                x = Keysharp.Scripting.Script.Invoke(myclassobj, "myclassfunc", 10L, 20L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 20000L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Keysharp.Scripting.Script.Invoke(myclassobj, "callmember1", 4L, 5L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 40L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member2"), "call", 1L, 2L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 4L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member3"), "call", 3L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 6L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member4"), "call", 3L, 4L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 14L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member5"), "call");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 123L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member6"), "call", 3L, 4L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 14L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                Keysharp.Scripting.Script.SetPropertyValue(myclassobj, "x", 123L);
                x = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member7"), "call", 2L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 246L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                Keysharp.Scripting.Script.SetPropertyValue(myclassobj, "x", 123L);
                x = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member8"), "call", 2L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 200L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Invoke(myfunc, "Call");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 123L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Invoke(sum, "Call", 1L, 2L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 3L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Invoke(sum2, "Call", 1L, 2L, 3L, 4L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 10L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                double1 = doublefunc;
                x = Invoke(double1, "Call", 1L, 2L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 4L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                double2 = doublefunc2;
                x = Invoke(double2, "Call", 1L, 2L, 3L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 12L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                myfunc2 = FuncObj(() =>
                {
                    return 123L;
                });
                x = Invoke(myfunc2, "Call");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 123L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                myfunc3 = FuncObj((params object[] a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Index(a, 1L), Keysharp.Scripting.Script.Index(a, 2L)), 2L);
                });
                x = Invoke(myfunc3, "Call", 1L, 2L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 4L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                myfunc4 = FuncObj((params object[] a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Index(a, 1L), Keysharp.Scripting.Script.Index(a, 2L)), 2L);
                });
                x = Invoke(myfunc4, "Call", 3L, 4L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 24L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                myfunc5 = FuncObj((object a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, 2L);
                });
                x = Invoke(myfunc5, "Call", 3L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 6L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                myfunc6 = FuncObj((object a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, 2L);
                });
                x = Invoke(myfunc6, "Call", 4L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 8L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                myfunc7 = FuncObj((params object[] args) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Index(args, 1L), Keysharp.Scripting.Script.Index(args, 2L)), 2L);
                });
                x = Invoke(myfunc7, "Call", 1L, 2L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 4L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = 0L;
                myfunc8 = FuncObj(() =>
                {
                    return x = 123L;
                });
                y = Invoke(myfunc8, "Call");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 123L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, y, 123L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                myfunc9 = FuncObj(() =>
                {
                    object a = null;
                    object b = null;
                    object c = null;
                    return Keysharp.Scripting.Script.MultiStatement(a = 123L, b = 456L, c = 789L);
                });
                x = Invoke(myfunc9, "Call");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 789L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                m = Keysharp.Core.Objects.Object(new object[] { "two", FuncObj((object @this) =>
                {
                    return 2L;
                }) });
                x = Keysharp.Scripting.Script.Invoke(m, "two");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 2L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                m = Keysharp.Core.Objects.Object(new object[] { "two", FuncObj((object @this, object a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, 2L);
                }) });
                x = Keysharp.Scripting.Script.Invoke(m, "two", 5L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 10L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                m = Keysharp.Core.Objects.Object(new object[] { "two", FuncObj((object @this) =>
                {
                    return Invoke(multfunc, "Call", 3L, 4L);
                }) });
                x = Keysharp.Scripting.Script.Invoke(m, "two");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 12L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                m = Keysharp.Core.Objects.Object(new object[] { "two", FuncObj((object @this, object a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, Invoke(multfunc, "Call", 3L, 4L));
                }) });
                x = Keysharp.Scripting.Script.Invoke(m, "two", 2L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 24L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = m = Keysharp.Core.Objects.Object(new object[] { "one", 1L, "two", FuncObj((object @this, object a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, Invoke(multfunc, "Call", 3L, 4L));
                }) });
                x = Keysharp.Scripting.Script.Invoke(m, "two", 2L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 24L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                m = Keysharp.Core.Objects.Object(new object[] { "one", 1L, "two", FuncObj((object @this, object a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, Invoke(multfunc, "Call", 3L, 4L));
                }), "three", FuncObj((object @this, object a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, 2L);
                }) });
                x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Invoke(m, "two", 2L), Keysharp.Scripting.Script.Invoke(m, "three", 3L));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 144L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                arr = new Keysharp.Core.Array([FuncObj(() =>
                {
                    return 1L;
                }), FuncObj(() =>
                {
                    return 2L;
                }), FuncObj(() =>
                {
                    return 3L;
                })]);
                x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Invoke(Keysharp.Scripting.Script.Index(arr, 1L), "Call"), Invoke(Keysharp.Scripting.Script.Index(arr, 2L), "Call")), Invoke(Keysharp.Scripting.Script.Index(arr, 3L), "Call"));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 6L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                b = "";
                Invoke(assignfunc, "Call", FuncObj(() =>
                {
                    return 1L;
                }));
                x = Invoke(b, "Call");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 1L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                m = Keysharp.Core.Objects.Object(new object[] { "one", Keysharp.Core.Objects.Object(new object[] { "oneone", 11L, "onef", FuncObj((object @this, object a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, 2L);
                }) }), "two", Keysharp.Core.Objects.Object(new object[] { "twotwo", 22L }), "three", Keysharp.Core.Objects.Object(new object[] { "threethree", 33L, "threethreearr", new Keysharp.Core.Array([10L, 20L, 30L]) }) });
                x = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(m, "one"), "onef", 5L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 10L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(m, "one"), "onef", 5L), Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(m, "two"), "twotwo"));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 220L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(m, "one"), "onef", 5L), Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(m, "two"), "twotwo")), Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(m, "three"), "threethree"));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 7260L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(m, "one"), "onef", 5L), Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(m, "two"), "twotwo")), Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(m, "three"), "threethree")), Keysharp.Scripting.Script.Index(Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(m, "three"), "threethreearr"), 3L));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 217800L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                val = 5L;
                m = Keysharp.Core.Objects.Object(new object[] { "one", FuncObj((object @this, VarRef a) =>
                {
                    return Keysharp.Scripting.Script.SetPropertyValue(((VarRef)a), "__Value", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, ((VarRef)a).__Value, 2L));
                }) });
                x = Keysharp.Scripting.Script.Invoke(m, "one", new VarRef(() => val, (value) => val = value));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 10L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 10L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                gval = 0L;
                lam = FuncObj(() =>
                {
                    return gval = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, gval, 123L);
                });
                x = Invoke(lam, "Call");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 123L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, gval, 123L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                tot = 0L;
                f = Keysharp.Core.Functions.FuncObj("func2__");
                Invoke(func, "Call", FuncObj((object o) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, o, 1L);
                }), FuncObj((object o) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, o, 2L);
                }), FuncObj((object o) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, o, 3L);
                }));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, tot, 24L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                tot = 0L;
                class2obj = Invoke(myclass2, "Call");
                Keysharp.Scripting.Script.Invoke(class2obj, "func", FuncObj((object o) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, o, 1L);
                }), FuncObj((object o) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, o, 2L);
                }), FuncObj((object o) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, o, 3L);
                }));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, tot, 34L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                y = false;
                y = Keysharp.Scripting.Script.IfTest(true) ? FuncObj((object a) =>
                {
                    return 1L;
                }) : FuncObj((object b) =>
                {
                    return 2L;
                });
                z = Invoke(y, "Call");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, z, 1L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                lamanondef = FuncObj(([Optional, DefaultParameterValue(123L)] object a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, 2L);
                });
                val = Invoke(lamanondef, "Call");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 246L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                lamnameddef = FuncObj(([Optional, DefaultParameterValue(3L)] object a) =>
                {
                    return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, a, 2L);
                });
                val = Invoke(lamnameddef, "Call");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 6L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                val = Invoke(myfunc10, "Call", FuncObj(([Optional, DefaultParameterValue(1L)] object x) =>
                {
                    return x;
                }), FuncObj(([Optional, DefaultParameterValue(2L)] object y) =>
                {
                    return y;
                }), FuncObj(([Optional, DefaultParameterValue(3L)] object z) =>
                {
                    return z;
                }));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 6L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                myclassobj = Invoke(myclass3, "Call");
                val = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member1"), "call", 5L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 20L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                val = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member2"), "call", 5L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 30L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                val = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member2"), "call");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 12L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                val = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member3"), "call");
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 123L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                val = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member3"), "call", 55L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 55L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                val = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member4"), "call", 1L, Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = new VarRef(() => b, (value) => b = value), Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, "__Value", 2L), new VarRef(() => b, (value) => b = value)), 3L, 4L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.ForceBool(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 24L)) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, b, 24L) : Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 24L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                val = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member5"), "call", 1L, Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = new VarRef(() => b, (value) => b = value), Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, "__Value", 2L), new VarRef(() => b, (value) => b = value)), 3L, 4L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.ForceBool(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 24L)) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, b, 24L) : Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 24L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                val = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member5"), "call", 1L, Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = new VarRef(() => b, (value) => b = value), Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, "__Value", 2L), new VarRef(() => b, (value) => b = value)), null, 4L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.ForceBool(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 40L)) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, b, 40L) : Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 40L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member6"), "call", 20L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 35L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member6"), "call", 20L, 25L);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 55L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member6"), "call", 20L, null);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 35L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                x = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(myclassobj, "member6"), "call", 1L, null, Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = new VarRef(() => z, (value) => z = value), Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, "__Value", 11L), new VarRef(() => z, (value) => z = value)));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.ForceBool(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 17L)) ? Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, z, 17L) : Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, x, 17L)))
                    Keysharp.Core.Files.FileAppend("pass", "*");
                else
                    Keysharp.Core.Files.FileAppend("fail", "*");
                Keysharp.Core.Common.Keyboard.HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
                Keysharp.Core.Flow.ExitApp(0L);
                return "";
            }
        }
    }
}