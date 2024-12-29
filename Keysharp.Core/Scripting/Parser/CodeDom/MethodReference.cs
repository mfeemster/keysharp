using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Keysharp.Scripting
{
	internal class MethodReference
	{
		internal string MethodName { get; set; }

		internal Type TargetObject { get; set; }

		internal Type[] TypeArguments { get; set; }

		internal MethodReference(Type targetObject, string methodName)
			: this(targetObject, methodName, null) { }

		internal MethodReference(Type targetObject, string methodName, Type[] typeArguments)
		{
			TargetObject = targetObject;
			MethodName = methodName;
			TypeArguments = typeArguments;
		}

		public static explicit operator InvocationExpressionSyntax(MethodReference source)
		{
			return SyntaxFactory.InvocationExpression(Keysharp.Core.Scripting.Parser.Antlr.Helper.CreateQualifiedName(source.TargetObject.FullName + "." + source.MethodName));
        }


        public static explicit operator CodeMethodInvokeExpression(MethodReference source)
		{
			var method = new CodeMethodInvokeExpression();
			method.Method = (CodeMethodReferenceExpression)source;
			return method;
		}

		public static explicit operator CodeMethodReferenceExpression(MethodReference source)
		{
			var method = new CodeMethodReferenceExpression();
			method.TargetObject = new CodeTypeReferenceExpression(source.TargetObject);
			method.MethodName = source.MethodName;

			if (source.TypeArguments != null)
			{
				foreach (var argument in source.TypeArguments)
				{
					_ = method.TypeArguments.Add(new CodeTypeReference(argument));
				}
			}

			var mis = (MethodInfo)source;
			//Always include qualification for methods because there can easily be name collisions, such as the Pop() used for loops, and Array.Pop().
			method.UserData.Add("MethodInfo", mis);
			return method;
		}

		public static explicit operator MethodInfo(MethodReference source)
		{
			return source.TypeArguments == null ? source.TargetObject.GetMethod(source.MethodName) : source.TargetObject.GetMethod(source.MethodName, source.TypeArguments);
		}
	}
}