//#define CONCURRENT
using Antlr4.Runtime.Misc;
using Label = System.Reflection.Emit.Label;

namespace Keysharp.Core.Common.Invoke
{
    [PublicForTestOnly]
	public class MethodPropertyHolder
	{
		public Func<object, object[], object> _callFunc;
		public Func<object, object[], object> CallFunc
        {
            get
            {
                if (_callFunc != null)
                    return _callFunc;

				var del = DelegateFactory.CreateDelegate(this);

				if (isGuiType)
				{
					_callFunc = (inst, args) =>
					{
						var ctrl = (inst ?? args[0]).GetControl();
						object ret = null;
                        ValidateArgCount(inst, args);
						ctrl.CheckedInvoke(() =>
						{
							ret = del(inst, args);
						}, true);
						return ret;
					};
				}
				else
					_callFunc = (inst, args) => {
                        ValidateArgCount(inst, args);
                        return del(inst, args);
                    };

                return _callFunc;
			}
        }
		internal readonly MethodInfo mi;
		internal readonly ParameterInfo[] parameters;
		internal readonly PropertyInfo pi;
		internal readonly Action<object, object> SetProp;
		protected readonly ConcurrentStackArrayPool<object> paramsPool;
		internal readonly bool anyOptional;
		internal readonly bool isGuiType;
		internal readonly bool isSetter;
		internal readonly bool isItemSetter;
		internal readonly int variadicParamIndex = -1;

#if CONCURRENT
        internal static ConcurrentDictionary<MethodInfo, MethodPropertyHolder> methodCache = new();
		internal static ConcurrentDictionary<PropertyInfo, MethodPropertyHolder> propertyCache = new();
#else
		internal static Dictionary<MethodInfo, MethodPropertyHolder> methodCache = new();
		internal static Dictionary<PropertyInfo, MethodPropertyHolder> propertyCache = new();
#endif

		internal bool IsBind { get; private set; }
		internal bool IsStaticFunc { get; private set; }
		internal bool IsStaticProp { get; private set; }
		internal bool IsVariadic => variadicParamIndex != -1;
		internal int ParamLength { get; }
		internal int MinParams = 0;
		internal int MaxParams = 0;

        private const string setterPrefix = "set_";
        private const string classSetterPrefix = Keywords.ClassStaticPrefix + setterPrefix;


		public static MethodPropertyHolder GetOrAdd(MethodInfo mi)
        {
#if CONCURRENT
            return methodCache.GetOrAdd(mi, key => new MethodPropertyHolder(mi));
#else
			if (methodCache.TryGetValue(mi, out var mph))
                return mph;
            return methodCache[mi] = new MethodPropertyHolder(mi);
#endif
        }

		internal static MethodPropertyHolder GetOrAdd(PropertyInfo pi)
		{
#if CONCURRENT
            return propertyCache.GetOrAdd(pi, key => new MethodPropertyHolder(pi));
#else
			if (propertyCache.TryGetValue(pi, out var mph))
				return mph;
			return propertyCache[pi] = new MethodPropertyHolder(pi);
#endif
        }

        public MethodPropertyHolder() { }

		public MethodPropertyHolder(MethodInfo m)
        {
            mi = m;

			IsStaticFunc = mi.Attributes.HasFlag(MethodAttributes.Static);
			isGuiType = Gui.IsGuiType(mi.DeclaringType);

			parameters = mi.GetParameters();
			ParamLength = parameters.Length;

			// Determine if the method is a set_Item overload.
			isSetter = mi.Name.StartsWith(setterPrefix) || mi.Name.StartsWith(classSetterPrefix);
			isItemSetter = mi.Name == "set_Item";

			for (var i = 0; i < parameters.Length; i++)
			{
				var pmi = parameters[i];

                if (pmi.IsVariadic() || ((pmi.ParameterType == typeof(object[])) && (i == (isItemSetter ? parameters.Length - 2 : parameters.Length - 1))))
                    variadicParamIndex = i;
                else
                {
					if (!pmi.IsOptional)
						MinParams++;
				}
			}

            if (isSetter) // Allow value to be unset
                MinParams--;

			MaxParams = parameters.Length - (variadicParamIndex == -1 ? 0 : 1);

			anyOptional = variadicParamIndex != -1 || MinParams != MaxParams;

			var isFuncObj = typeof(IFuncObj).IsAssignableFrom(mi.DeclaringType);

			if (isFuncObj && mi.Name == "Bind")
				IsBind = true;

			if (isFuncObj && mi.Name == "Call")
			{
				_callFunc = (inst, obj) => ((IFuncObj)inst).Call(obj);
			}
		}

		public MethodPropertyHolder(PropertyInfo p)
		{
            pi = p;
			isGuiType = Gui.IsGuiType(pi.DeclaringType);
			parameters = pi.GetIndexParameters();
			ParamLength = parameters.Length;
			MinParams = MaxParams = ParamLength;

			if (pi.GetAccessors().Any(x => x.IsStatic))
			{
				IsStaticProp = true;

				if (isGuiType)
				{
                    _callFunc = (inst, obj) =>//Gui calls aren't worth optimizing further.
                    {
                        object ret = null;
                        var ctrl = (inst ?? obj[0]).GetControl();//If it's a gui control, then invoke on the gui thread.
                        ctrl.CheckedInvoke(() =>
                        {
                            ret = pi.GetValue(null);
                        }, true);//This can be null if called before a Gui object is fully initialized.

                        if (ret is int i)
                            ret = (long)i;//Try to keep everything as long.

							return ret;
						};
						SetProp = (inst, arg) =>
						{
							var ctrl = inst.GetControl();//If it's a gui control, then invoke on the gui thread.
							ctrl.CheckedInvoke(() => pi.SetValue(null, arg), true);//This can be null if called before a Gui object is fully initialized.
						};
					}
					else
					{
						if (pi.PropertyType == typeof(int))
						{
							_callFunc = (inst, arg) =>
							{
								var ret = pi.GetValue(null);

							if (ret is int i)
								ret = (long)i;//Try to keep everything as long.

							return ret;
						};
					}
					else
						_callFunc = (inst, obj) => pi.GetValue(null);

					SetProp = (inst, obj) => pi.SetValue(null, obj);
				}
			}
			else
			{
				if (isGuiType)
				{
					_callFunc = (inst, args) =>
					{
						object ret = null;
						var ctrl = (inst ?? args[0]).GetControl();//If it's a gui control, then invoke on the gui thread.
						ctrl.CheckedInvoke(() =>
						{
							ret = pi.GetValue(inst ?? args[0]);
						}, true);//This can be null if called before a Gui object is fully initialized.

						if (ret is int i)
							ret = (long)i;//Try to keep everything as long.

							return ret;
						};
						SetProp = (inst, obj) =>
						{
							var ctrl = inst.GetControl();//If it's a gui control, then invoke on the gui thread.
							ctrl.CheckedInvoke(() => pi.SetValue(inst, obj), true);//This can be null if called before a Gui object is fully initialized.
						};
					}
					else
					{
						if (pi.PropertyType == typeof(int))
						{
							_callFunc = (inst, obj) =>
							{
								var ret = pi.GetValue(inst);

							if (ret is int i)
								ret = (long)i;//Try to keep everything as long.

							return ret;
						};
					}
					else
						_callFunc = (inst, obj) => pi.GetValue(inst);

					SetProp = pi.SetValue;
				}
			}
		}
		internal static void ClearCache()
		{
			methodCache.Clear();
            propertyCache.Clear();
		}

		internal void ValidateArgCount(object inst, object[] args)
		{
            int lastProvided = args?.Length ?? 0;
			var provided = lastProvided + (inst == null ? 0 : 1);

            if (!mi.IsStatic)
                provided--;

            for (int i = lastProvided - 1; i >= 0; i--)
            {
                if (args[i] == null)
                    provided--;
                else
                    break;
            }

			if (provided < MinParams)
				throw new ValueError("Too few arguments provided");

			if (!IsVariadic && provided > MaxParams)
				throw new ValueError("Too many arguments provided");
		}
	}
    public class ArgumentError : Error
    {
        public ArgumentError()
            : base(new TargetParameterCountException().Message)
        {
        }
    }

	[PublicForTestOnly]
	public static class DelegateFactory
    {
        public static Func<object, object[], object> CreateDelegate(MethodInfo mi) => CreateDelegate(new MethodPropertyHolder(mi));
		/// <summary>
		/// Creates a delegate of type Func<object, object[], object></object>that will call the given MethodInfo.
		/// It will check for missing parameters and if the parameter is optional, it uses its DefaultValue.
		/// </summary>
		public static Func<object, object[], object> CreateDelegate(MethodPropertyHolder mph)
        {
            var method = mph.mi;

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            ParameterInfo[] parameters = method.GetParameters();

            string dynamicMethodName = "DynamicInvoke_" + (method.DeclaringType != null ? method.DeclaringType.Name + "." : "") + method.Name;

            DynamicMethod dm = new DynamicMethod(
                dynamicMethodName,
                typeof(object),
                new Type[] { typeof(object), typeof(object[]) },
                method.Module,
                true);

            ILGenerator il = dm.GetILGenerator();

            // --- Declare unified locals ---
            LocalBuilder paramOffset = il.DeclareLocal(typeof(int));   // will be 0 for static or for instance when ldarg0 is non-null; 1 for instance if ldarg0 is null.
            LocalBuilder argsLocal = il.DeclareLocal(typeof(object[]));  // the effective arguments array

            // Ensure that the caller-supplied argument array is not null.
            Label argsNonNull = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_1);        // load ldarg1
            il.Emit(OpCodes.Brtrue_S, argsNonNull); // if not null, branch
                                                    // Otherwise, create an empty object array.
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Starg_S, 1);       // store the empty array into ldarg1 (or into a local)
            il.MarkLabel(argsNonNull);

            // --- Compute paramOffset and argsLocal ---
            if (!method.IsStatic)
            {
                // Instance method:
                // Use ldarg0 if non-null; otherwise use args[0] and set offset=1.
                LocalBuilder target = il.DeclareLocal(typeof(object));
                Label useArg0 = il.DefineLabel();
                Label afterTarget = il.DefineLabel();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Brtrue_S, useArg0);
                // ldarg0 is null: set paramOffset = 1 and target = args[0].
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Stloc, paramOffset);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Stloc, target);
                il.Emit(OpCodes.Br_S, afterTarget);
                il.MarkLabel(useArg0);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc, paramOffset);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Stloc, target);
                il.MarkLabel(afterTarget);
                // Push the target (cast as needed) for the eventual call.
                il.Emit(OpCodes.Ldloc, target);
                if (method.DeclaringType.IsValueType)
                    il.Emit(OpCodes.Unbox_Any, method.DeclaringType);
                else
                    il.Emit(OpCodes.Castclass, method.DeclaringType);

                // For instance methods, we use the caller's argument array unchanged.
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stloc, argsLocal);
            }
            else
            {
                // Static method: always set paramOffset = 0.
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc, paramOffset);
                // Set argsLocal = ldarg_1.
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stloc, argsLocal);

                // If the delegate's instance (ldarg0) is non-null, inject it into the argument array.
                Label useOriginal = il.DefineLabel();
                Label afterCombine = il.DefineLabel();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Brfalse_S, useOriginal);

                LocalBuilder origLen = il.DeclareLocal(typeof(int));
                il.Emit(OpCodes.Ldloc, argsLocal);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Conv_I4);
                il.Emit(OpCodes.Stloc, origLen);

                // length = origLen + 1.
                LocalBuilder newLen = il.DeclareLocal(typeof(int));
                il.Emit(OpCodes.Ldloc, origLen);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stloc, newLen);

                // Allocate new array of length newLen.
                il.Emit(OpCodes.Ldloc, newLen);
                il.Emit(OpCodes.Newarr, typeof(object));
                LocalBuilder combined = il.DeclareLocal(typeof(object[]));
                il.Emit(OpCodes.Stloc, combined);

                // combined[0] = ldarg0.
                il.Emit(OpCodes.Ldloc, combined);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Stelem_Ref);

                // If newLen > 1, copy original arguments from argsLocal into combined starting at index 1.
                Label skipCopy = il.DefineLabel();
                il.Emit(OpCodes.Ldloc, newLen);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ble_S, skipCopy);
                LocalBuilder idx = il.DeclareLocal(typeof(int));
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc, idx);
                Label loopStart = il.DefineLabel();
                Label loopEnd = il.DefineLabel();
                il.MarkLabel(loopStart);
                il.Emit(OpCodes.Ldloc, idx);
                il.Emit(OpCodes.Ldloc, origLen);
                il.Emit(OpCodes.Bge_S, loopEnd);
                // combined[idx + 1] = argsLocal[idx]
                il.Emit(OpCodes.Ldloc, combined);
                il.Emit(OpCodes.Ldloc, idx);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Ldloc, argsLocal);
                il.Emit(OpCodes.Ldloc, idx);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Stelem_Ref);
                il.Emit(OpCodes.Ldloc, idx);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stloc, idx);
                il.Emit(OpCodes.Br_S, loopStart);
                il.MarkLabel(loopEnd);
                il.MarkLabel(skipCopy);
                // Update argsLocal with the combined array.
                il.Emit(OpCodes.Ldloc, combined);
                il.Emit(OpCodes.Stloc, argsLocal);
                il.MarkLabel(useOriginal);
            }


            // --------------------------------------------------
            // Pre-check: compute the combined argument count.
            // argsLocal was created earlier and holds the combined arguments.
            LocalBuilder providedCountLocal = il.DeclareLocal(typeof(int));
            il.Emit(OpCodes.Ldloc, argsLocal);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Stloc, providedCountLocal);

            // --- Parameter Processing ---
            // For each formal parameter at index i, load the effective argument from:
            //    argsLocal[paramOffset + i]
            // If no argument is present, load the default (or for params, an empty array) or throw.
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo pi = parameters[i];

                // Determine special flags.
                // For set_Item, the final parameter ("value") should come from the last argument.
                bool isSetItemValue = mph.isItemSetter && (i == parameters.Length - 1);
                // For non-set_Item methods, a normal params parameter is the last parameter marked with [ParamArray].
                bool isParams = i == mph.variadicParamIndex;

                // --- Branch for Params (or special set_Item params) ---
                if (isParams)
                {
                    // Compute count: the number of arguments to pack.
                    // For special params, reserve one argument for the final "value":
                    //    count = argsLocal.Length - (paramOffset + i) - 1
                    // For normal params:
                    //    count = argsLocal.Length - (paramOffset + i)
                    LocalBuilder countLocal = il.DeclareLocal(typeof(int));
                    if (mph.isItemSetter)
                    {
                        il.Emit(OpCodes.Ldloc, argsLocal);   // push argsLocal
                        il.Emit(OpCodes.Ldlen);                // push argsLocal.Length
                        il.Emit(OpCodes.Conv_I4);
                        il.Emit(OpCodes.Ldloc, paramOffset);
                        il.Emit(OpCodes.Ldc_I4, i);
                        il.Emit(OpCodes.Add);                  // compute (paramOffset + i)
                        il.Emit(OpCodes.Sub);                  // argsLocal.Length - (paramOffset + i)
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Sub);                  // subtract 1 for the final "value"
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc, argsLocal);
                        il.Emit(OpCodes.Ldlen);
                        il.Emit(OpCodes.Conv_I4);
                        il.Emit(OpCodes.Ldloc, paramOffset);
                        il.Emit(OpCodes.Ldc_I4, i);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Sub);                  // argsLocal.Length - (paramOffset + i)
                    }
                    il.Emit(OpCodes.Stloc, countLocal);

                    // Repack the remaining arguments into a new array.
                    Label doRepack = il.DefineLabel();
                    Label repackEnd = il.DefineLabel();
                    il.Emit(OpCodes.Ldloc, countLocal);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Bgt_S, doRepack);
                    {
                        // If count is 0, push an empty array.
                        Type elemType = pi.ParameterType.GetElementType();
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Newarr, elemType);
                        il.Emit(OpCodes.Br_S, repackEnd);
                    }
                    il.MarkLabel(doRepack);
                    LocalBuilder newArray = il.DeclareLocal(pi.ParameterType);
                    il.Emit(OpCodes.Ldloc, countLocal);
                    il.Emit(OpCodes.Newarr, pi.ParameterType.GetElementType());
                    il.Emit(OpCodes.Stloc, newArray);
                    LocalBuilder loopIndex = il.DeclareLocal(typeof(int));
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Stloc, loopIndex);
                    Label loopStart = il.DefineLabel();
                    Label loopCheck = il.DefineLabel();
                    il.Emit(OpCodes.Br_S, loopCheck);
                    il.MarkLabel(loopStart);
                    il.Emit(OpCodes.Ldloc, newArray);
                    il.Emit(OpCodes.Ldloc, loopIndex);
                    il.Emit(OpCodes.Ldloc, argsLocal);
                    il.Emit(OpCodes.Ldloc, paramOffset);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Add);                  // starting index: paramOffset + i
                    il.Emit(OpCodes.Ldloc, loopIndex);
                    il.Emit(OpCodes.Add);                  // add loop index
                    il.Emit(OpCodes.Ldelem_Ref);
                    il.Emit(OpCodes.Stelem_Ref);
                    il.Emit(OpCodes.Ldloc, loopIndex);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Add);
                    il.Emit(OpCodes.Stloc, loopIndex);
                    il.MarkLabel(loopCheck);
                    il.Emit(OpCodes.Ldloc, loopIndex);
                    il.Emit(OpCodes.Ldloc, countLocal);
                    il.Emit(OpCodes.Blt_S, loopStart);
                    il.Emit(OpCodes.Ldloc, newArray);
                    il.MarkLabel(repackEnd);
                }
                else
                {
                    // --- For non-params parameters, load a single element.
                    // Compute effective index: normally paramOffset + i,
                    // except for set_Item value parameter, where it's providedCountLocal - 1.
                    LocalBuilder effectiveIndex = il.DeclareLocal(typeof(int));
                    if (isSetItemValue)
                    {
                        il.Emit(OpCodes.Ldloc, providedCountLocal);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Sub);
                        il.Emit(OpCodes.Stloc, effectiveIndex);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc, paramOffset);
                        il.Emit(OpCodes.Ldc_I4, i);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Stloc, effectiveIndex);
                    }

                    // Check if an argument was provided.
                    Label argProvided = il.DefineLabel();
                    Label afterLoad = il.DefineLabel();
                    il.Emit(OpCodes.Ldloc, effectiveIndex);  // load effective index
                    il.Emit(OpCodes.Ldloc, providedCountLocal);  // load providedCountLocal
                    il.Emit(OpCodes.Blt_S, argProvided);

                    // No argument provided for this parameter.
                    if (pi.IsOptional || (mph.isSetter && i == (parameters.Length - 1)))
                    {
                        // Load the default value.
                        object defVal = pi.DefaultValue;
                        if (defVal == null || defVal == System.Reflection.Missing.Value)
                        {
                            il.Emit(OpCodes.Ldnull);
                        }
                        else
                        {
                            EmitLoadConstant(il, defVal);
                            if (!pi.ParameterType.IsValueType && defVal.GetType().IsValueType)
                                il.Emit(OpCodes.Box, defVal.GetType());
                        }
                        il.Emit(OpCodes.Br_S, afterLoad);
                    }
                    else
                    {
                        // Not optional: throw exception.
                        ConstructorInfo exCtor = typeof(ArgumentError)
                                                    .GetConstructor(Type.EmptyTypes);
                        il.Emit(OpCodes.Newobj, exCtor);
                        il.Emit(OpCodes.Throw);
                    }
                    il.MarkLabel(argProvided);

                    // Load the argument from the effective index.
                    il.Emit(OpCodes.Ldloc, argsLocal);
                    il.Emit(OpCodes.Ldloc, effectiveIndex);
                    il.Emit(OpCodes.Ldelem_Ref);

                    // Now, check if the loaded value is null.
                    Label notNull = il.DefineLabel();
                    il.Emit(OpCodes.Dup);         // duplicate the loaded argument for the test
                    il.Emit(OpCodes.Brtrue_S, notNull); // if not null, jump to notNull

                    // It is null: remove the null value.
                    il.Emit(OpCodes.Pop);
                    if (pi.IsOptional || (mph.isSetter && i == (parameters.Length - 1)))
                    {
                        // Load the default value.
                        object defVal = pi.DefaultValue;
                        if (defVal == null || defVal == System.Reflection.Missing.Value)
                            il.Emit(OpCodes.Ldnull);
                        else
                        {
                            EmitLoadConstant(il, defVal);
                            if (!pi.ParameterType.IsValueType && defVal.GetType().IsValueType)
                                il.Emit(OpCodes.Box, defVal.GetType());
                        }
                        il.Emit(OpCodes.Br_S, afterLoad);
                    }
                    else
                    {
                        // If not optional, throw an exception.
                        ConstructorInfo exCtor2 = typeof(ArgumentError)
                                                    .GetConstructor(Type.EmptyTypes);
                        il.Emit(OpCodes.Newobj, exCtor2);
                        il.Emit(OpCodes.Throw);
                    }
                    il.MarkLabel(notNull);

                    il.MarkLabel(afterLoad);
                }

                // Finally, if the formal parameter is a value type, unbox/cast as needed.
                if (pi.ParameterType.IsValueType)
                    il.Emit(OpCodes.Unbox_Any, pi.ParameterType);
                else
                    il.Emit(OpCodes.Castclass, pi.ParameterType);
            }

            // --- Call the underlying method ---
            if (method.IsStatic)
                il.Emit(OpCodes.Call, method);
            else
                il.Emit(OpCodes.Callvirt, method);
            if (method.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else if (method.ReturnType.IsValueType)
                il.Emit(OpCodes.Box, method.ReturnType);
            il.Emit(OpCodes.Ret);

            return (Func<object, object[], object>)dm.CreateDelegate(typeof(Func<object, object[], object>));
        }

        /// <summary>
        /// Creates a delegate from a PropertyInfo by using its getter method.
        /// </summary>
        public static Func<object, object[], object> CreateDelegate(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            MethodInfo getter = property.GetGetMethod(true);
            if (getter == null)
                throw new ArgumentException("The provided property does not have a getter.", nameof(property));

            return CreateDelegate(new MethodPropertyHolder(getter));
        }

        public static void EmitThrowError(ILGenerator il, string errorMessage, string methodName, LocalBuilder code)
        {
            // Create a new object[3]
            il.Emit(OpCodes.Ldc_I4_3);                  // push constant 3 (array size)
            il.Emit(OpCodes.Newarr, typeof(object));    // create new object[3]

            // Store errorMessage at index 0
            il.Emit(OpCodes.Dup);                       // duplicate array reference
            il.Emit(OpCodes.Ldc_I4_0);                  // index 0
            il.Emit(OpCodes.Ldstr, errorMessage);       // push errorMessage string
            il.Emit(OpCodes.Stelem_Ref);                // array[0] = errorMessage

            
            // Store methodName at index 1
            il.Emit(OpCodes.Dup);                       // duplicate array reference
            il.Emit(OpCodes.Ldc_I4_1);                  // index 1
            il.Emit(OpCodes.Ldstr, methodName);         // push methodName string
            il.Emit(OpCodes.Stelem_Ref);                // array[1] = methodName


            // Store code at index 2
            il.Emit(OpCodes.Dup);                       // duplicate array reference
            il.Emit(OpCodes.Ldc_I4_2);                  // index 2
            il.Emit(OpCodes.Ldloc, code);               // push code (int)
            il.Emit(OpCodes.Box, typeof(int));          // box the int value
            il.Emit(OpCodes.Stelem_Ref);                // array[2] = code

            // Get the constructor: Error(object[] args)
            ConstructorInfo errorCtor = typeof(Error).GetConstructor(new[] { typeof(object[]) });
            // Create a new Error instance using the array
            il.Emit(OpCodes.Newobj, errorCtor);
            // Throw the error.
            il.Emit(OpCodes.Throw);
        }

        /// <summary>
        /// Emits IL to load the specified constant onto the evaluation stack.
        /// Supports common primitives, strings, booleans, and enums.
        /// </summary>
        private static void EmitLoadConstant(ILGenerator il, object value)
        {
            if (value == null)
            {
                il.Emit(OpCodes.Ldnull);
                return;
            }

            Type type = value.GetType();
            if (type == typeof(int))
            {
                il.Emit(OpCodes.Ldc_I4, (int)value);
            }
            else if (type == typeof(long))
            {
                il.Emit(OpCodes.Ldc_I8, (long)value);
            }
            else if (type == typeof(float))
            {
                il.Emit(OpCodes.Ldc_R4, (float)value);
            }
            else if (type == typeof(double))
            {
                il.Emit(OpCodes.Ldc_R8, (double)value);
            }
            else if (type == typeof(string))
            {
                il.Emit(OpCodes.Ldstr, (string)value);
            }
            else if (type == typeof(bool))
            {
                il.Emit((bool)value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            }
            else if (type == typeof(DBNull))
            {
                //FieldInfo dbNullField = typeof(DBNull).GetField("Value", BindingFlags.Public | BindingFlags.Static);
                //il.Emit(OpCodes.Ldsfld, dbNullField);
                il.Emit(OpCodes.Ldnull);
            }
            else if (type.IsEnum)
            {
                // For enums, load the underlying value and box the enum type.
                Type underlying = Enum.GetUnderlyingType(type);
                if (underlying == typeof(int))
                {
                    il.Emit(OpCodes.Ldc_I4, (int)value);
                }
                else if (underlying == typeof(long))
                {
                    il.Emit(OpCodes.Ldc_I8, (long)value);
                }
                else
                {
                    throw new NotSupportedException("Enum underlying type not supported: " + underlying);
                }
                il.Emit(OpCodes.Box, type);
            }
            else if (type == typeof(decimal))
            {
                // Decompose the decimal into its 4 int bits.
                decimal d = (decimal)value;
                int[] bits = decimal.GetBits(d);
                int lo = bits[0];
                int mid = bits[1];
                int hi = bits[2];
                int flags = bits[3];
                bool isNegative = (flags & 0x80000000) != 0;
                byte scale = (byte)((flags >> 16) & 0x7F);

                // Load the constants and call the decimal constructor (int lo, int mid, int hi, bool isNegative, byte scale)
                il.Emit(OpCodes.Ldc_I4, lo);
                il.Emit(OpCodes.Ldc_I4, mid);
                il.Emit(OpCodes.Ldc_I4, hi);
                il.Emit(isNegative ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Conv_U1); // Convert the top int to a byte for scale

                ConstructorInfo ctor = typeof(decimal).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte) });
                if (ctor == null)
                    throw new InvalidOperationException("The required Decimal constructor was not found.");
                il.Emit(OpCodes.Newobj, ctor);
            }
            else
            {
                throw new NotSupportedException("Constant type not supported: " + type.FullName);
            }
        }
    }

}