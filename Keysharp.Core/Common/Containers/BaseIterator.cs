namespace Keysharp.Core.Common.Containers
{
	internal class BaseIteratorData<T>
	{
		/// <summary>
		/// Cache for iterators with either 1 or 2 parameters.
		/// This prevents reflection from having to always be done to find the Call method.
		/// </summary>
		internal FuncObj p1, p2;

		/// <summary>
		/// Static constructor to initialize function objects.
		/// </summary>
		internal BaseIteratorData()
		{
			Error err;
			var mi1 = Reflections.FindAndCacheMethod(typeof(T), "Call", 1);
			p1 = new FuncObj(mi1, null);

			if (!p1.IsValid)
				_ = Errors.ErrorOccurred(err = new MethodError($"Existing function object p1 for type {typeof(T)} was invalid.")) ? throw err : "";

			var mi2 = Reflections.FindAndCacheMethod(typeof(T), "Call", 2);
			p2 = new FuncObj(mi2, null);

			if (!p2.IsValid)
				_ = Errors.ErrorOccurred(err = new MethodError($"Existing function object p2 for type {typeof(T)} was invalid.")) ? throw err : "";
		}
	}

	public class KeysharpEnumerator
	{
		public IFuncObj fo;
		private object[] args;

		/// <summary>
		/// The number of items to return for each iteration. Allowed values are 1 and 2:
		/// 1: return just the value in the first position
		/// 2: return the index in the first position and the value in the second.
		/// </summary>
		public int Count { get; private set; }

		public IFuncObj CallFunc { get; protected set; }

		public KeysharpEnumerator(IFuncObj f, int count)
		{
			fo = f;
			Count = count;//Unsure what happens when this differs from the number of parameters fo expects. MethodPropertyHolder probably just fills them in.
			args = new object[count];
		}

		/*
		    Call() method overloads were generated with this code:
		    var sb = new StringBuilder(10000);

		    for (var i = 1; i <= 19; ++i)
		    {
		        var paramsSb = new StringBuilder(256);
		        paramsSb.Append($"ref object ovar1");

		        for (var j = 2; j <= i; ++j)
		        {
		            paramsSb.Append($",ref object ovar{j}");
		        }

		        paramsSb.Append(")");
		        var assignToArgsSb = new StringBuilder(256);

		        for (var j = 0; j < i; ++j)
		        {
		            assignToArgsSb.AppendLine($"\targs[{j}] = ovar{j + 1};");
		        }

		        var assignToParamsSb = new StringBuilder(256);

		        for (var j = 0; j < i; ++j)
		        {
		            assignToParamsSb.AppendLine($"\tovar{j + 1} = args[{j}];");
		        }

		        sb.Append("public virtual object Call(");
		        sb.AppendLine(paramsSb.ToString());
		        sb.AppendLine("{");
		        sb.AppendLine("\ttry");
		        sb.AppendLine("\t{");
		        sb.Append(assignToArgsSb.ToString());
		        sb.AppendLine("\tvar ret = fo.CallWithRefs(args);");
		        sb.Append(assignToParamsSb.ToString());
		        sb.AppendLine("\treturn ret;");
		        sb.AppendLine("\t}");
		        sb.AppendLine("\tcatch (Exception e)");
		        sb.AppendLine("\t{");
		        sb.AppendLine("\t\tthrow new Error(e.Message);");
		        sb.AppendLine("\t}");
		        sb.AppendLine("}\n");
		    }

		    File.WriteAllText("./enumcalls.txt", sb.ToString());
		*/
		public virtual object Call(params object[] args)
		{
			try
			{
				return fo.Call(args);
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call([ByRef] object ovar1)
		{
			try
			{
				return fo.Call(ovar1);
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}

		public virtual object Call([ByRef] object ovar1, [ByRef] object ovar2 = null)
		{
			try
			{
				return fo.Call(ovar1, ovar2);
			}
			catch (Exception e)
			{
				throw new Error(e.Message);
			}
		}
	}
}
