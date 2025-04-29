namespace Keysharp.Core.Common.Containers
{
    public class KeysharpEnumerator
	{
		public IFuncObj fo;

		/// <summary>
		/// The number of items to return for each iteration. Allowed values are 1 and 2:
		/// 1: return just the value in the first position
		/// 2: return the index in the first position and the value in the second.
		/// </summary>
		public int Count { get; private set; }

		public KeysharpEnumerator(IFuncObj f, int count)
		{
			fo = f;
			Count = count;//Unsure what happens when this differs from the number of parameters fo expects. MethodPropertyHolder probably just fills them in.
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
		public virtual object Call(params object[] args) => fo.Call(args);

		public virtual object Call(object ovar1)
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

		public virtual object Call(object ovar1, object ovar2)
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
