using System.Text;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		//internal const string ScopeVar = ".";
		internal const string ScopeVar = "_";

		internal const string VarProperty = "Vars";
		private int internalID;
		private string InternalID => "e" + internalID++;

		private string Scope => GetScope(blocks.Count);

		private string GetScope(int depth)
		{
			var sb = new StringBuilder(64);
			//var catchct = 0;
			//var finallyct = 0;
			//var tryelsect = 0;
			var ct = 0;

			foreach (var block in blocks)
			{
				if (ct == depth)
					break;

				ct++;

				if (block.Kind == CodeBlock.BlockKind.Function
						|| block.Kind == CodeBlock.BlockKind.Prop
						|| block.Kind == CodeBlock.BlockKind.PropGet
						|| block.Kind == CodeBlock.BlockKind.PropSet
				   )
				{
					if (sb.Length > 0)
						_ = sb.Insert(0, ScopeVar);

					_ = sb.Insert(0, block.Method.ToLower() ?? mainScope);
				}

				//Unsure if these should be scoped. Probably not.
				//else if (block.Kind == CodeBlock.BlockKind.Catch)
				//{
				//  if (sb.Length > 0)
				//      _ = sb.Append(ScopeVar);
				//
				//  _ = sb.Append($"catch{catchct}");
				//  catchct++;
				//}
				//else if (block.Kind == CodeBlock.BlockKind.Finally)
				//{
				//  if (sb.Length > 0)
				//      _ = sb.Append(ScopeVar);
				//
				//  _ = sb.Append($"finally{finallyct}");
				//  finallyct++;
				//}
				//else if (block.Kind == CodeBlock.BlockKind.TryElse)
				//{
				//  if (sb.Length > 0)
				//      _ = sb.Append(ScopeVar);
				//
				//  _ = sb.Append($"tryelse{tryelsect}");
				//  tryelsect++;
				//}
			}

			return sb.Length > 0 ? sb.ToString() : mainScope;
		}
	}
}