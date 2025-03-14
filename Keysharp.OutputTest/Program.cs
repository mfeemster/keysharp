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
        public static object return123func = Keysharp.Core.Functions.Func("return123func");
        public static object testclass => Program.Testclass.__Static;

        public class Testclass : KeysharpObject
        {
            public new (Type, object) super => (typeof(KeysharpObject), this);

            public Testclass(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object Basecasesensitivefunc()
            {
                Keysharp.Scripting.Script.SetPropertyValue(testclass, "a", 999L);
                Keysharp.Scripting.Script.SetPropertyValue(this, "a", 1212L);
                return "";
            }

            public object staticBasecasesensitivefuncstatic()
            {
                Keysharp.Scripting.Script.SetPropertyValue(testclass, "c", 3131L);
                return "";
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
                Keysharp.Scripting.Script.SetPropertyValue(this, "a", 123L);
                Keysharp.Scripting.Script.SetPropertyValue(this, "b", 456L);
            }

            public void static__Init()
            {
                Keysharp.Scripting.Script.SetPropertyValue(this, "c", 888L);
                Keysharp.Scripting.Script.SetPropertyValue(this, "d", 1000L);
            }

            static Testclass()
            {
                Script.InitStaticInstance(typeof(Testclass));
                __Static = Variables.Statics[typeof(Testclass)];
                Keysharp.Scripting.Script.SetPropertyValue(Variables.Prototypes[typeof(Testclass)], "__Class", "testclass");
            }

            public Testclass staticCall(params object[] args)
            {
                return new Testclass(args);
            }
        }

        public static object testsubclass => Program.Testsubclass.__Static;

        public class Testsubclass : Testclass
        {
            public new (Type, object) super => (typeof(Testclass), this);

            public Testsubclass(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_a()
            {
                return Keysharp.Scripting.Script.GetPropertyValue(this, "_a");
            }

            public void set_a(object value)
            {
                Keysharp.Scripting.Script.SetPropertyValue(this, "_a", value);
            }

            public object get_c()
            {
                return Keysharp.Scripting.Script.GetPropertyValue(this, "_c");
            }

            public void set_c(object value)
            {
                Keysharp.Scripting.Script.SetPropertyValue(this, "_c", value);
            }

            public object Setbasea()
            {
                Keysharp.Scripting.Script.SetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(this, "base"), "a", 500L);
                return "";
            }

            public object Getbasea()
            {
                return Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(this, "base"), "a");
            }

            public object Subcasesensitivefunc()
            {
                Keysharp.Scripting.Script.SetPropertyValue(this, "a", 1212L);
                Keysharp.Scripting.Script.Invoke(this, "basecasesensitivefunc");
                return "";
            }

            public object staticSubcasesensitivefuncstatic()
            {
                Keysharp.Scripting.Script.Invoke(testclass, "basecasesensitivefuncstatic");
                return "";
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
                Keysharp.Scripting.Script.SetPropertyValue(this, "_a", 321L);
                Keysharp.Scripting.Script.SetPropertyValue(this, "_c", 999L);
            }

            public void static__Init()
            {
                Keysharp.Scripting.Script.SetPropertyValue(this, "b", 654L);
                Keysharp.Scripting.Script.SetPropertyValue(this, "d", 2000L);
            }

            static Testsubclass()
            {
                Script.InitStaticInstance(typeof(Testsubclass));
                __Static = Variables.Statics[typeof(Testsubclass)];
            }

            public Testsubclass staticCall(params object[] args)
            {
                return new Testsubclass(args);
            }
        }

        public static object testclassobj = null;
        public static object testsubclassobj = null;
        public static object fileappend = Keysharp.Core.Functions.Func("fileappend");
        public static object val = null;
        public static object classname = null;
        public static object myarray => Program.Myarray.__Static;

        public class Myarray : Array
        {
            public new (Type, object) super => (typeof(Array), this);

            public Myarray(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_Item(object index)
            {
                return 123L;
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Myarray()
            {
                Script.InitStaticInstance(typeof(Myarray));
                __Static = Variables.Statics[typeof(Myarray)];
            }

            public Myarray staticCall(params object[] args)
            {
                return new Myarray(args);
            }
        }

        public static object mymap => Program.Mymap.__Static;

        public class Mymap : Map
        {
            public new (Type, object) super => (typeof(Map), this);

            public Mymap(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_Item(object index)
            {
                return 321L;
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Mymap()
            {
                Script.InitStaticInstance(typeof(Mymap));
                __Static = Variables.Statics[typeof(Mymap)];
            }

            public Mymap staticCall(params object[] args)
            {
                return new Mymap(args);
            }
        }

        public static object base1 => Program.Base1.__Static;

        public class Base1 : KeysharpObject
        {
            public new (Type, object) super => (typeof(KeysharpObject), this);

            public Base1(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_Item(object index)
            {
                return 1L;
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Base1()
            {
                Script.InitStaticInstance(typeof(Base1));
                __Static = Variables.Statics[typeof(Base1)];
            }

            public Base1 staticCall(params object[] args)
            {
                return new Base1(args);
            }
        }

        public static object sub1 => Program.Sub1.__Static;

        public class Sub1 : Base1
        {
            public new (Type, object) super => (typeof(Base1), this);

            public Sub1(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_Item(object index)
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

            static Sub1()
            {
                Script.InitStaticInstance(typeof(Sub1));
                __Static = Variables.Statics[typeof(Sub1)];
            }

            public Sub1 staticCall(params object[] args)
            {
                return new Sub1(args);
            }
        }

        public static object subarr1 => Program.Subarr1.__Static;

        public class Subarr1 : Array
        {
            public new (Type, object) super => (typeof(Array), this);

            public Subarr1(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Subarr1()
            {
                Script.InitStaticInstance(typeof(Subarr1));
                __Static = Variables.Statics[typeof(Subarr1)];
            }

            public Subarr1 staticCall(params object[] args)
            {
                return new Subarr1(args);
            }
        }

        public static object subarr2 => Program.Subarr2.__Static;

        public class Subarr2 : Subarr1
        {
            public new (Type, object) super => (typeof(Subarr1), this);

            public Subarr2(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_Item(object index)
            {
                return 3L;
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Subarr2()
            {
                Script.InitStaticInstance(typeof(Subarr2));
                __Static = Variables.Statics[typeof(Subarr2)];
            }

            public Subarr2 staticCall(params object[] args)
            {
                return new Subarr2(args);
            }
        }

        public static object obj = null;
        public static object submap1 => Program.Submap1.__Static;

        public class Submap1 : Map
        {
            public new (Type, object) super => (typeof(Map), this);

            public Submap1(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Submap1()
            {
                Script.InitStaticInstance(typeof(Submap1));
                __Static = Variables.Statics[typeof(Submap1)];
            }

            public Submap1 staticCall(params object[] args)
            {
                return new Submap1(args);
            }
        }

        public static object submap2 => Program.Submap2.__Static;

        public class Submap2 : Submap1
        {
            public new (Type, object) super => (typeof(Submap1), this);

            public Submap2(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_Item(object index)
            {
                return 4L;
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Submap2()
            {
                Script.InitStaticInstance(typeof(Submap2));
                __Static = Variables.Statics[typeof(Submap2)];
            }

            public Submap2 staticCall(params object[] args)
            {
                return new Submap2(args);
            }
        }

        public static object myfunc = null;
        public static object myarrayclass1 => Program.Myarrayclass1.__Static;

        public class Myarrayclass1 : Array
        {
            public new (Type, object) super => (typeof(Array), this);

            public Myarrayclass1(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_Item(object k1)
            {
                return 1L;
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Myarrayclass1()
            {
                Script.InitStaticInstance(typeof(Myarrayclass1));
                __Static = Variables.Statics[typeof(Myarrayclass1)];
            }

            public Myarrayclass1 staticCall(params object[] args)
            {
                return new Myarrayclass1(args);
            }
        }

        public static object mac = null;
        public static object myarrayclass2 => Program.Myarrayclass2.__Static;

        public class Myarrayclass2 : Array
        {
            public new (Type, object) super => (typeof(Array), this);

            public Myarrayclass2(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public void set_Item(object p1, object value)
            {
                Keysharp.Scripting.Script.SetObject(value, (object)super, p1);
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Myarrayclass2()
            {
                Script.InitStaticInstance(typeof(Myarrayclass2));
                __Static = Variables.Statics[typeof(Myarrayclass2)];
            }

            public Myarrayclass2 staticCall(params object[] args)
            {
                return new Myarrayclass2(args);
            }
        }

        public static object mymapclass1 => Program.Mymapclass1.__Static;

        public class Mymapclass1 : Map
        {
            public new (Type, object) super => (typeof(Map), this);

            public Mymapclass1(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_Item(params object[] _ks_a)
            {
                object sum = null;
                object k = null;
                object a = new Array(_ks_a);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(a, "Length"), 1L)))
                {
                    return Keysharp.Scripting.Script.Index((object)super, Keysharp.Scripting.Script.Index(a, 1L));
                }

                sum = 0L;
                {
                    var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(a, Misc.MakeVarRef(() => k, (Val) => k = Val)).GetEnumerator();
                    object[] _ks_e1_backup = new object[1]
                    {
                        k
                    };
                    Keysharp.Core.Loops.Push();
                    try
                    {
                        for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                        {
                            Keysharp.Core.Loops.Inc();
                            sum = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, sum, k);
                        _ks_e1_next:
                            ;
                        }
                    }
                    finally
                    {
                        k = _ks_e1_backup[0];
                        Keysharp.Core.Loops.Pop();
                    }

                _ks_e1_end:
                    ;
                }

                return Keysharp.Scripting.Script.Index((object)super, sum);
            }

            public void set_Item(object[] _ks_a, object value)
            {
                object sum = null;
                object k = null;
                object a = new Array(_ks_a);
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(a, "Length"), 1L)))
                {
                    Keysharp.Scripting.Script.SetObject(value, (object)super, Keysharp.Scripting.Script.Index(a, 1L));
                    return;
                }

                sum = 0L;
                {
                    var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(a, Misc.MakeVarRef(() => k, (Val) => k = Val)).GetEnumerator();
                    object[] _ks_e1_backup = new object[1]
                    {
                        k
                    };
                    Keysharp.Core.Loops.Push();
                    try
                    {
                        for (; IsTrueAndRunning(_ks_e1.MoveNext());)
                        {
                            Keysharp.Core.Loops.Inc();
                            sum = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, sum, k);
                        _ks_e1_next:
                            ;
                        }
                    }
                    finally
                    {
                        k = _ks_e1_backup[0];
                        Keysharp.Core.Loops.Pop();
                    }

                _ks_e1_end:
                    ;
                }

                Keysharp.Scripting.Script.SetObject(sum, (object)super, sum);
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Mymapclass1()
            {
                Script.InitStaticInstance(typeof(Mymapclass1));
                __Static = Variables.Statics[typeof(Mymapclass1)];
            }

            public Mymapclass1 staticCall(params object[] args)
            {
                return new Mymapclass1(args);
            }
        }

        public static object mmp = null;
        public static object myarrayclass3 => Program.Myarrayclass3.__Static;

        public class Myarrayclass3 : Array
        {
            public new (Type, object) super => (typeof(Array), this);

            public Myarrayclass3(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public void set_Item(object[] _ks_p1, object value)
            {
                object temp = null;
                object n = null;
                object p1 = new Array(_ks_p1);
                temp = 0L;
                {
                    var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(p1, Misc.MakeVarRef(() => n, (Val) => n = Val)).GetEnumerator();
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

                Keysharp.Scripting.Script.SetObject(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, temp, value), (object)super, Keysharp.Scripting.Script.Index(p1, 1L));
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Myarrayclass3()
            {
                Script.InitStaticInstance(typeof(Myarrayclass3));
                __Static = Variables.Statics[typeof(Myarrayclass3)];
            }

            public Myarrayclass3 staticCall(params object[] args)
            {
                return new Myarrayclass3(args);
            }
        }

        public static object myarrayclass4 => Program.Myarrayclass4.__Static;

        public class Myarrayclass4 : Array
        {
            public new (Type, object) super => (typeof(Array), this);

            public Myarrayclass4(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_Item(params object[] _ks_p1)
            {
                object temp = null;
                object n = null;
                object p1 = new Array(_ks_p1);
                temp = 0L;
                {
                    var _ks_e1 = Keysharp.Core.Loops.MakeEnumerable(p1, Misc.MakeVarRef(() => n, (Val) => n = Val)).GetEnumerator();
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

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Myarrayclass4()
            {
                Script.InitStaticInstance(typeof(Myarrayclass4));
                __Static = Variables.Statics[typeof(Myarrayclass4)];
            }

            public Myarrayclass4 staticCall(params object[] args)
            {
                return new Myarrayclass4(args);
            }
        }

        public static object myarrayclass5 => Program.Myarrayclass5.__Static;

        public class Myarrayclass5 : Array
        {
            public new (Type, object) super => (typeof(Array), this);

            public Myarrayclass5(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_Item(object p1, object p2, params object[] _ks_p3)
            {
                object temp = null;
                object n = null;
                object p3 = new Array(_ks_p3);
                temp = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, p1, p2);
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

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Myarrayclass5()
            {
                Script.InitStaticInstance(typeof(Myarrayclass5));
                __Static = Variables.Statics[typeof(Myarrayclass5)];
            }

            public Myarrayclass5 staticCall(params object[] args)
            {
                return new Myarrayclass5(args);
            }
        }

        public static object myarrayclass6 => Program.Myarrayclass6.__Static;

        public class Myarrayclass6 : Array
        {
            public new (Type, object) super => (typeof(Array), this);

            public Myarrayclass6(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_doublecount()
            {
                return Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.GetPropertyValue(this, "length"), 2L);
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Myarrayclass6()
            {
                Script.InitStaticInstance(typeof(Myarrayclass6));
                __Static = Variables.Statics[typeof(Myarrayclass6)];
            }

            public Myarrayclass6 staticCall(params object[] args)
            {
                return new Myarrayclass6(args);
            }
        }

        public static object myinitclass => Program.Myinitclass.__Static;

        public class Myinitclass : KeysharpObject
        {
            public new (Type, object) super => (typeof(KeysharpObject), this);

            public Myinitclass(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public void __Init()
            {
                Invoke((object)super, "__Init");
                Keysharp.Scripting.Script.SetPropertyValue(this, "p1", 123L);
            }

            public void static__Init()
            {
            }

            static Myinitclass()
            {
                Script.InitStaticInstance(typeof(Myinitclass));
                __Static = Variables.Statics[typeof(Myinitclass)];
            }

            public Myinitclass staticCall(params object[] args)
            {
                return new Myinitclass(args);
            }
        }

        public static object mic = null;
        public static object Return123func()
        {
            return 123L;
        }

        public static object myfuncinitclass => Program.Myfuncinitclass.__Static;

        public class Myfuncinitclass : KeysharpObject
        {
            public new (Type, object) super => (typeof(KeysharpObject), this);

            public Myfuncinitclass(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public void __Init()
            {
                Invoke((object)super, "__Init");
                Keysharp.Scripting.Script.SetPropertyValue(this, "p1", Invoke(return123func, "Call"));
            }

            public void static__Init()
            {
            }

            static Myfuncinitclass()
            {
                Script.InitStaticInstance(typeof(Myfuncinitclass));
                __Static = Variables.Statics[typeof(Myfuncinitclass)];
            }

            public Myfuncinitclass staticCall(params object[] args)
            {
                return new Myfuncinitclass(args);
            }
        }

        public static object mybaseclass => Program.Mybaseclass.__Static;

        public class Mybaseclass : KeysharpObject
        {
            public new (Type, object) super => (typeof(KeysharpObject), this);

            public Mybaseclass(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object Basefunc()
            {
                Keysharp.Scripting.Script.SetPropertyValue(this, "x", 123L);
                return "";
            }

            public object Retfunc(object xx)
            {
                return xx;
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
                Keysharp.Scripting.Script.SetPropertyValue(this, "x", 100L);
            }

            public void static__Init()
            {
            }

            static Mybaseclass()
            {
                Script.InitStaticInstance(typeof(Mybaseclass));
                __Static = Variables.Statics[typeof(Mybaseclass)];
            }

            public Mybaseclass staticCall(params object[] args)
            {
                return new Mybaseclass(args);
            }
        }

        public static object mysubclass => Program.Mysubclass.__Static;

        public class Mysubclass : Mybaseclass
        {
            public new (Type, object) super => (typeof(Mybaseclass), this);

            public Mysubclass(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object Basefunc()
            {
                object temp = null;
                object val = null;
                object _ks_temp1 = null;
                object _ks_temp2 = null;
                Keysharp.Scripting.Script.Invoke((object)super, "basefunc");
                Keysharp.Scripting.Script.MultiStatement(_ks_temp1 = this, _ks_temp2 = "x", Keysharp.Scripting.Script.SetPropertyValue(_ks_temp1, _ks_temp2, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, Keysharp.Scripting.Script.GetPropertyValue(_ks_temp1, _ks_temp2), 1L)));
                temp = Keysharp.Scripting.Script.GetPropertyValue(this, "x");
                val = Keysharp.Scripting.Script.Invoke(this, "retfunc", Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Divide, Keysharp.Scripting.Script.SetPropertyValue(this, "x", 99L), 3L));
                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(this, "x"), 99L)))
                {
                    Keysharp.Core.Files.FileAppend("pass", "*");
                }
                else
                {
                    Keysharp.Core.Files.FileAppend("fail", "*");
                }

                if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 33L)))
                {
                    Keysharp.Core.Files.FileAppend("pass", "*");
                }
                else
                {
                    Keysharp.Core.Files.FileAppend("fail", "*");
                }

                Keysharp.Scripting.Script.SetPropertyValue(this, "x", temp);
                return "";
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Mysubclass()
            {
                Script.InitStaticInstance(typeof(Mysubclass));
                __Static = Variables.Statics[typeof(Mysubclass)];
            }

            public Mysubclass staticCall(params object[] args)
            {
                return new Mysubclass(args);
            }
        }

        public static object msc = null;
        public static object bigarr => Program.Bigarr.__Static;

        public class Bigarr : Array
        {
            public new (Type, object) super => (typeof(Array), this);

            public Bigarr(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public void __Init()
            {
                Invoke((object)super, "__Init");
                Keysharp.Scripting.Script.SetPropertyValue(this, "Capacity", 10000L);
            }

            public void static__Init()
            {
            }

            static Bigarr()
            {
                Script.InitStaticInstance(typeof(Bigarr));
                __Static = Variables.Statics[typeof(Bigarr)];
            }

            public Bigarr staticCall(params object[] args)
            {
                return new Bigarr(args);
            }
        }

        public static object mybigarr = null;
        public static object mapi => Program.Mapi.__Static;

        public class Mapi : Map
        {
            public new (Type, object) super => (typeof(Map), this);

            public Mapi(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object __New(params object[] _ks_args)
            {
                object args = new Array(_ks_args);
                Keysharp.Scripting.Script.SetPropertyValue(this, "DerivedDefault", null);
                Keysharp.Scripting.Script.Invoke((object)super, "__New", [.. Keysharp.Scripting.Script.FlattenParam(args)]);
                return "";
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
                Keysharp.Scripting.Script.SetPropertyValue(this, "CaseSense", false);
                Keysharp.Scripting.Script.SetPropertyValue(this, "DerivedDefault", "");
            }

            public void static__Init()
            {
            }

            static Mapi()
            {
                Script.InitStaticInstance(typeof(Mapi));
                __Static = Variables.Statics[typeof(Mapi)];
            }

            public Mapi staticCall(params object[] args)
            {
                return new Mapi(args);
            }
        }

        public static object cim = null;
        public static object dupepropsbase => Program.Dupepropsbase.__Static;

        public class Dupepropsbase : KeysharpObject
        {
            public new (Type, object) super => (typeof(KeysharpObject), this);

            public Dupepropsbase(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_a()
            {
                return 123L;
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
            }

            public void static__Init()
            {
            }

            static Dupepropsbase()
            {
                Script.InitStaticInstance(typeof(Dupepropsbase));
                __Static = Variables.Statics[typeof(Dupepropsbase)];
            }

            public Dupepropsbase staticCall(params object[] args)
            {
                return new Dupepropsbase(args);
            }
        }

        public static object dupepropssub => Program.Dupepropssub.__Static;

        public class Dupepropssub : Dupepropsbase
        {
            public new (Type, object) super => (typeof(Dupepropsbase), this);

            public Dupepropssub(params object[] args) : base(args)
            {
            }

            new public static object __Static { get; set; }

            public object get_a()
            {
                return Keysharp.Scripting.Script.GetPropertyValue(this, "_a");
            }

            public void set_a(object value)
            {
                Keysharp.Scripting.Script.SetPropertyValue(this, "_a", value);
                return;
            }

            public object Getlocala()
            {
                object a = null;
                a = 1L;
                return a;
            }

            public object Getthisa()
            {
                return Keysharp.Scripting.Script.GetPropertyValue(this, "a");
            }

            public object Getsupera()
            {
                return Keysharp.Scripting.Script.GetPropertyValue((object)super, "a");
            }

            public void __Init()
            {
                Invoke((object)super, "__Init");
                Keysharp.Scripting.Script.SetPropertyValue(this, "_a", 999L);
            }

            public void static__Init()
            {
            }

            static Dupepropssub()
            {
                Script.InitStaticInstance(typeof(Dupepropssub));
                __Static = Variables.Statics[typeof(Dupepropssub)];
            }

            public Dupepropssub staticCall(params object[] args)
            {
                return new Dupepropssub(args);
            }
        }

        public static object classobj = null;
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
            testclassobj = Invoke(testclass, "Call");
            testsubclassobj = Invoke(testsubclass, "Call");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, testclassobj, "testclass")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, testsubclassobj, "testclass")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, testsubclassobj, "testsubclass")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.GetPropertyValue(testclassobj, "a");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.GetPropertyValue(testsubclassobj, "a");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 321L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.GetPropertyValue(testclassobj, "b");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 456L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.GetPropertyValue(testsubclass, "b");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 654L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.GetPropertyValue(testclass, "c");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 888L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.GetPropertyValue(testsubclassobj, "c");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 999L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.GetPropertyValue(testclass, "d");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 1000L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.GetPropertyValue(testsubclass, "d");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 2000L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.Invoke(testsubclassobj, "setbasea");
            val = Keysharp.Scripting.Script.Invoke(testsubclassobj, "getbasea");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 500L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.GetPropertyValue(testsubclassobj, "a");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 321L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(testsubclassobj, "base"), "a", 777L);
            val = Keysharp.Scripting.Script.Invoke(testsubclassobj, "getbasea");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 777L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.GetPropertyValue(testsubclassobj, "a");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 321L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            classname = Keysharp.Scripting.Script.GetPropertyValue(testclassobj, "__Class");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, classname, "testclass")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            classname = Keysharp.Scripting.Script.GetPropertyValue(testsubclassobj, "__Class");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, classname, "testsubclass")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetPropertyValue(testsubclassobj, "a", "");
            Keysharp.Scripting.Script.SetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(testsubclassobj, "base"), "a", "");
            Keysharp.Scripting.Script.Invoke(testsubclassobj, "subcasesensitivefunc");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(testsubclass, "a"), 999L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(testsubclassobj, "a"), 1212L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetPropertyValue(testclass, "c", "");
            Keysharp.Scripting.Script.Invoke(testsubclass, "subcasesensitivefuncstatic");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(testclass, "c"), 3131L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            classname = Invoke(myarray, "Call");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, classname, "Array")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, classname, "MyArray")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Index(classname, 100L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            classname = Invoke(mymap, "Call");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, classname, "Map")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, classname, "MyMap")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Index(classname, 100L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 321L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            obj = Invoke(sub1, "Call");
            val = Keysharp.Scripting.Script.Index(obj, 999L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 2L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            obj = Invoke(subarr2, "Call");
            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, obj, "subarr2")) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, obj, "subarr1"))).ParseObject()) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, obj, "Array"))).ParseObject()))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Index(obj, 999L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 3L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            obj = Invoke(submap2, "Call");
            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, obj, "submap2")) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, obj, "submap1"))).ParseObject()) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, obj, "Map"))).ParseObject()))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            val = Keysharp.Scripting.Script.Index(obj, 999L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 4L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetPropertyValue(testclass, "c", 101L);
            myfunc = Keysharp.Scripting.Script.GetPropertyValue(testsubclass, "basecasesensitivefuncstatic");
            Invoke(myfunc, "Call", testsubclass);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(testclass, "c"), 3131L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetPropertyValue(testclass, "c", 101L);
            myfunc = Keysharp.Scripting.Script.GetPropertyValue(testsubclass, "subcasesensitivefuncstatic");
            Invoke(myfunc, "Call", testsubclass);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(testclass, "c"), 3131L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetPropertyValue(testsubclassobj, "a", 0L);
            myfunc = Keysharp.Scripting.Script.GetPropertyValue(testsubclassobj, "basecasesensitivefunc");
            Invoke(myfunc, "Call", testsubclassobj);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(testsubclassobj, "a"), 1212L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetPropertyValue(testsubclassobj, "a", 0L);
            myfunc = Keysharp.Scripting.Script.GetPropertyValue(testsubclassobj, "SubCaseSensitiveFunc");
            Invoke(myfunc, "Call", testsubclassobj);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(testsubclassobj, "a"), 1212L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            mac = Invoke(myarrayclass1, "Call");
            Keysharp.Scripting.Script.Invoke(mac, "Push", 123L);
            val = Keysharp.Scripting.Script.Index(mac, 1L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 1L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetObject(999L, mac, 1L);
            val = Keysharp.Scripting.Script.Index(mac, 1L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 1L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            mac = Invoke(myarrayclass2, "Call");
            Keysharp.Scripting.Script.Invoke(mac, "Push", 123L);
            val = Keysharp.Scripting.Script.Index(mac, 1L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetObject(999L, mac, 1L);
            val = Keysharp.Scripting.Script.Index(mac, 1L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 999L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            mmp = Invoke(mymapclass1, "Call");
            Keysharp.Scripting.Script.SetObject(123L, mmp, "asdf");
            val = Keysharp.Scripting.Script.Index(mmp, "asdf");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetObject(123L, mmp, 1L, 2L);
            val = Keysharp.Scripting.Script.Index(mmp, 1L, 2L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 3L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            Keysharp.Scripting.Script.SetObject(123L, mmp, 1L, 2L, 3L);
            val = Keysharp.Scripting.Script.Index(mmp, 1L, 2L, 3L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 6L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            mac = Invoke(myarrayclass3, "Call");
            Keysharp.Scripting.Script.Invoke(mac, "Push", 1L);
            Keysharp.Scripting.Script.SetObject(100L, mac, 1L, 2L, 3L, 4L);
            val = Keysharp.Scripting.Script.Index(mac, 1L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 110L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            mac = Invoke(myarrayclass4, "Call");
            val = Keysharp.Scripting.Script.Index(mac, 1L, 2L, 3L, 4L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 10L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            mac = Invoke(myarrayclass5, "Call");
            val = Keysharp.Scripting.Script.Index(mac, 1L, 2L, 3L, 4L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 10L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            mac = Invoke(myarrayclass6, "Call", 1L, 2L, 3L, 4L);
            val = Keysharp.Scripting.Script.GetPropertyValue(mac, "doublecount");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 8L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            mic = Invoke(myinitclass, "Call");
            val = Keysharp.Scripting.Script.GetPropertyValue(mic, "p1");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            mic = Invoke(myfuncinitclass, "Call");
            val = Keysharp.Scripting.Script.GetPropertyValue(mic, "p1");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            msc = Invoke(mysubclass, "Call");
            Keysharp.Scripting.Script.Invoke(msc, "basefunc");
            val = Keysharp.Scripting.Script.GetPropertyValue(msc, "x");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 124L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            msc = Invoke(mysubclass, "Call");
            Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(msc, "base"), "base"), "basefunc");
            val = Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(msc, "base"), "x");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, val, 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            mybigarr = Invoke(bigarr, "Call", 1L, 2L, 3L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, mybigarr, "Array")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, mybigarr, "bigarr")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(mybigarr, "Capacity"), 10000L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.Index(mybigarr, 1L), 1L)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.Index(mybigarr, 2L), 2L))).ParseObject()) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.Index(mybigarr, 3L), 3L))).ParseObject()))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            cim = Invoke(mapi, "Call", "a", 1L, "B", 2L);
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, cim, "Map")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Is, cim, "Mapi")))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest((Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.Index(cim, "A"), 1L)) && Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.Index(cim, "b"), 2L))).ParseObject()))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            classobj = Invoke(dupepropssub, "Call");
            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(classobj, "a"), 999L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.GetPropertyValue(Keysharp.Scripting.Script.GetPropertyValue(classobj, "super"), "a"), 123L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.Invoke(classobj, "getlocala"), 1L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.Invoke(classobj, "getthisa"), 999L)))
            {
                Keysharp.Core.Files.FileAppend("pass", "*");
            }
            else
            {
                Keysharp.Core.Files.FileAppend("fail", "*");
            }

            if (Keysharp.Scripting.Script.IfTest(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.IdentityEquality, Keysharp.Scripting.Script.Invoke(classobj, "getsupera"), 123L)))
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