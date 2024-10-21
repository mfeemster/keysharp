namespace Keysharp.Scripting
{
	internal class CodeSwitchStatement : CodeStatement
	{
		internal CodeStatementCollection AllStatements { get; } = [];
		internal Dictionary<string, CodeStatementCollection> CaseBodyStatements { get; } = [];
		internal Dictionary<string, object> CaseExpressions { get; } = [];
		internal StringComparison? CaseSense { get; } = null;
		internal CodeConditionStatement Condition { get; } = new CodeConditionStatement();
		internal CodeLabeledStatement DefaultLabelStatement { get; } = new CodeLabeledStatement();
		internal CodeStatementCollection DefaultStatements { get; } = [];
		internal CodeLabeledStatement FinalLabelStatement { get; } = new CodeLabeledStatement();
		internal string SwitchVar { get; }

		internal string SwitchVarTempName { get; }

		internal CodeSwitchStatement(string switchVar, object caseSense, uint switchCount)
		{
			SwitchVar = switchVar;

			if (caseSense != null)
			{
				CaseSense = Conversions.ParseComparisonOption(caseSense);

				if (CaseSense == StringComparison.CurrentCulture)
					CaseSense = StringComparison.CurrentCultureIgnoreCase;
			}

			SwitchVarTempName = $"ks_switchvar{switchCount}";
			FinalLabelStatement = new CodeLabeledStatement($"ks_finallabel{switchCount}");
			DefaultLabelStatement = new CodeLabeledStatement($"ks_defaultlabel{switchCount}");
		}
	}
}