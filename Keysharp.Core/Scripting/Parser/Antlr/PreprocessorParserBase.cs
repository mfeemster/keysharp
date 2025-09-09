using System;
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

public abstract class PreprocessorParserBase : Antlr4.Runtime.Parser
{
    protected PreprocessorParserBase(ITokenStream input)
        : base(input)
    {
        conditions.Push(true);
		RemoveErrorListeners();
		AddErrorListener(new MainParserErrorListener());
	}

    protected PreprocessorParserBase(ITokenStream input, TextWriter output, TextWriter errorOutput)
        : base(input, output, errorOutput)
    {
        conditions.Push(true);
		RemoveErrorListeners();
		AddErrorListener(new MainParserErrorListener());
	}

    Stack<bool> conditions = new Stack<bool>();
    public HashSet<string> ConditionalSymbols  =
			[
				"KEYSHARP"
#if WINDOWS
				, "WINDOWS"
#elif LINUX
				, "LINUX"
#endif
#if DEBUG
                , "DEBUG"
#endif
			];

    protected bool AllConditions()
    {
	    foreach (bool condition in conditions)
	    {
		    if (!condition)
			    return false;
	    }
	    return true;
    }
    
    protected void OnPreprocessorDirectiveDefine()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.PreprocessorDeclarationContext;
        ConditionalSymbols.Add(d.ConditionalSymbol().GetText());
	    d.value = AllConditions();
    }

    protected void OnPreprocessorDirectiveUndef()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.PreprocessorDeclarationContext;
        ConditionalSymbols.Remove(d.ConditionalSymbol().GetText());
        d.value = AllConditions();
    }

    protected void OnPreprocessorDirectiveIf()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.PreprocessorConditionalContext;
        d.value = d.expr.value.Equals("true") && AllConditions();
	    conditions.Push(d.expr.value.Equals("true"));
    }

    protected void OnPreprocessorDirectiveElif()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.PreprocessorConditionalContext;
        if (!conditions.Peek())
        {
            conditions.Pop();
            d.value = d.expr.value.Equals("true") && AllConditions();
            conditions.Push(d.expr.value.Equals("true"));
        }
        else
        {
            d.value = false;
        }
    }

    protected void OnPreprocessorDirectiveElse()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.PreprocessorConditionalContext;
        if (!conditions.Peek())
        {
            conditions.Pop();
            d.value = true && AllConditions();
            conditions.Push(true);
        }
        else
        {
            d.value = false;
        }
    }

    protected void OnPreprocessorDirectiveEndif()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.PreprocessorConditionalContext;
        conditions.Pop();
        d.value = conditions.Peek();
    }

    protected void OnPreprocessorDirectiveLine()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.PreprocessorLineContext;
        d.value = AllConditions();
    }

    protected void OnPreprocessorDirectiveError()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.PreprocessorDiagnosticContext;
        d.value = AllConditions();
    }

    protected void OnPreprocessorDirectiveWarning()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.PreprocessorDiagnosticContext;
        d.value = AllConditions();
    }

    protected void OnPreprocessorDirectiveRegion()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.PreprocessorRegionContext;
        d.value = AllConditions();
    }

    protected void OnPreprocessorDirectiveEndregion()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.PreprocessorRegionContext;
        d.value = AllConditions();
    }

    protected void OnPreprocessorDirectivePragma()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.PreprocessorPragmaContext;
        d.value = AllConditions();
    }

    protected void OnPreprocessorDirectiveNullable()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.PreprocessorNullableContext;
        d.value = AllConditions();
    }

    protected void OnPreprocessorExpressionTrue()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.Preprocessor_expressionContext;
        d.value = "true";
    }

    protected void OnPreprocessorExpressionFalse()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.Preprocessor_expressionContext;
        d.value = "false";
    }

        protected void OnPreprocessorExpressionDigits()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.Preprocessor_expressionContext;
        if (!int.TryParse(d.Digits().GetText(), out int result))
            result = 1;
        d.value = result == 0 ? "false" : "true";
    }

    protected void OnPreprocessorExpressionConditionalSymbol()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.Preprocessor_expressionContext;
        d.value = ConditionalSymbols.Contains(d.ConditionalSymbol().GetText()) ? "true" : "false";
    }

    protected void OnPreprocessorExpressionConditionalOpenParens()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.Preprocessor_expressionContext;
        d.value = d.expr.value;
    }

    protected void OnPreprocessorExpressionConditionalNot()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.Preprocessor_expressionContext;
	    d.value = d.expr.value.Equals("true") ? "false" : "true";
    }

    protected void OnPreprocessorExpressionConditionalEq()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.Preprocessor_expressionContext;
	    d.value = (d.expr1.value == d.expr2.value ? "true" : "false");
    }

    protected void OnPreprocessorExpressionConditionalNe()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.Preprocessor_expressionContext;
	    d.value = (d.expr1.value != d.expr2.value ? "true" : "false");
    }

    protected void OnPreprocessorExpressionConditionalAnd()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.Preprocessor_expressionContext;
	    d.value = (d.expr1.value.Equals("true") && d.expr2.value.Equals("true") ? "true" : "false");
    }

    protected void OnPreprocessorExpressionConditionalOr()
    {
        ParserRuleContext c = this.Context;
        var d = c as PreprocessorParser.Preprocessor_expressionContext;
	    d.value = (d.expr1.value.Equals("true") || d.expr2.value.Equals("true") ? "true" : "false");
    }
}