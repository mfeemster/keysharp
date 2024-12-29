using Antlr4.Runtime;
using System.Collections.Generic;
using System.IO;
using static MainParser;
using System.Text.RegularExpressions;
using static Keysharp.Core.Scripting.Parser.Antlr.Helper;
using System.Runtime.ConstrainedExecution;

/// <summary>
/// All lexer methods that used in grammar
/// should start with Upper Case Char similar to Lexer rules.
/// </summary>
public abstract class MainLexerBase : Lexer
{

    private IToken _lastToken = null;
    private IToken _lastVisibleToken = null;

    /// <summary>
    /// Current nesting depth
    /// </summary>
    private int _currentDepth = 0;
    private bool _hotstringIsLiteral = true;

    private bool _whitespaceEnabled = true;

    private bool _ignoreNextEOL = false;

    /// <summary>
    /// Preserve nesting depth of template literals
    /// </summary>
    private Stack<int> templateDepthStack = new Stack<int>();

    public MainLexerBase(ICharStream input)
        : base(input)
    {
    }

    public MainLexerBase(ICharStream input, TextWriter output, TextWriter errorOutput) : this(input)
    {
        RemoveErrorListeners(); // Remove default error listeners
        AddErrorListener(new MainLexerErrorListener());
    }

    public bool IsStartOfFile(){
        return _lastToken == null;
    }

    public bool IsInTemplateString()
    {
        return templateDepthStack.Count > 0 && templateDepthStack.Peek() == _currentDepth;
    }

    /// <summary>
    /// Return the next token from the character stream and records this last
    /// token in case it resides on the default channel. This recorded token
    /// is used to determine when the lexer could possibly match a regex
    /// literal.
    /// 
    /// </summary>
    /// <returns>
    /// The next token from the character stream.
    /// </returns>
    /// 
    public override IToken NextToken()
    {
        // Get the next token.
        IToken next = base.NextToken();

        if (next.Channel == DefaultTokenChannel)
        {
            // Keep track of the last token on the default channel.
            _lastVisibleToken = next;
        }
        _lastToken = next;

        return next;
    }

    public override IToken Emit()
    {
        if (_lastToken != null) {
            if (this.Channel != Hidden)
            {
                if (_lastToken.Type == MainLexer.DerefEnd && Regex.IsMatch(this.Text, @"^[a-zA-Z0-9_]+$"))
                    this.Type = MainLexer.IdentifierContinuation;
                else if (this.Type == MainLexer.DerefStart && (Regex.IsMatch(_lastToken.Text, @"^[a-zA-Z0-9_]+$") || (_lastToken.Type == MainLexer.DerefEnd)))
                    this.Type = MainLexer.DerefContinuation;
                
                if (lineContinuationOperators.Contains(this.Type))
                    _ignoreNextEOL = true;
                else
                    _ignoreNextEOL = false;
            } else {
                if (this.Type == EOL || this.Type == MainLexer.IgnoreEOL)
                    _ignoreNextEOL = false;
            }
        } 

        return base.Emit();
    }

    protected int _processHotstringOptions(string input) {
        int res = -1;
        int strLength = input.Length;
        char[] charArray = input.ToCharArray();
        for (int i = 0; i < strLength; i++) { 
            var c = charArray[i];
            if (c == ':' || c == ';' || c == '/')
                break;
            if (c == 'x' || c == 'X') {
                res = (i == (strLength - 1)) ? 1 : (charArray[i + 1] == '0' ? 0 : 1);
            }
        }
        return res;
    }

    protected bool IsHotstringLiteral() {
        int intermediate = _processHotstringOptions(base.Text.TrimStart(':'));
        return intermediate == -1 ? _hotstringIsLiteral : intermediate == 0;
    }

    protected void ProcessHotstringOptions() {
        int intermediate = _processHotstringOptions(base.Text);
        if (intermediate != -1)
            _hotstringIsLiteral = intermediate == 1;
    }

    protected bool NoCommentAhead() {
        IToken next = base.NextToken();
        if (next.Type == MultiLineComment) {
            return false;
        }
        return true;
    }

    protected bool IsBeginningOfLine() {
        return Column == 0;
    }

    protected void ProcessOpenBrace()
    {
        _currentDepth++;
    }
    protected void ProcessCloseBrace()
    {
        _currentDepth--;
    }

    private HashSet<int> lineContinuationOperators = new HashSet<int> {
        MainLexer.OpenBracket,
        MainLexer.OpenBrace,
        MainLexer.OpenBracketWithWS,
        MainLexer.OpenParenNoWS,
        MainLexer.OpenParen,
        MainLexer.DerefStart,
        MainLexer.Comma,
        MainLexer.Assign,
        MainLexer.QuestionMark,
        MainLexer.QuestionMarkDot,
        MainLexer.Colon,
        MainLexer.DotConcat,
        MainLexer.Plus,
        MainLexer.Minus,
        MainLexer.Divide,
        MainLexer.IntegerDivide,
        MainLexer.NullCoalesce,
        MainLexer.RightShiftArithmetic,
        MainLexer.LeftShiftArithmetic,
        MainLexer.RightShiftLogical,
        MainLexer.LessThan,
        MainLexer.MoreThan,
        MainLexer.LessThanEquals,
        MainLexer.GreaterThanEquals,
        MainLexer.Equals_,
        MainLexer.NotEquals,
        MainLexer.IdentityEquals,
        MainLexer.IdentityNotEquals,
        MainLexer.RegExMatch,
        MainLexer.BitAnd,
        MainLexer.BitXOr,
        MainLexer.BitOr,
        MainLexer.And,
        MainLexer.Or,
        MainLexer.MultiplyAssign,
        MainLexer.DivideAssign,
        MainLexer.MultiplyAssign,
        MainLexer.DivideAssign,
        MainLexer.ModulusAssign,
        MainLexer.PlusAssign,
        MainLexer.MinusAssign,
        MainLexer.LeftShiftArithmeticAssign,
        MainLexer.RightShiftArithmeticAssign,
        MainLexer.RightShiftLogicalAssign,
        MainLexer.IntegerDivideAssign,
        MainLexer.ConcatenateAssign,
        MainLexer.BitAndAssign,
        MainLexer.BitXorAssign,
        MainLexer.BitOrAssign,
        MainLexer.PowerAssign,
        MainLexer.NullishCoalescingAssign,
        MainLexer.Arrow,
    };
    protected void ProcessOpenBracket()
    {
        _currentDepth++;
        if (_lastToken == null 
            || _lastToken.Channel == Hidden 
            || lineContinuationOperators.Contains(_lastToken.Type))
            this.Type = MainLexer.OpenBracketWithWS;
    }
    protected void ProcessCloseBracket()
    {
        _currentDepth--;
    }
    protected void ProcessOpenParen()
    {
        _currentDepth++;
        if (_lastToken != null 
            && _lastToken.Channel != Hidden 
            && (_lastToken.Type == Identifier || _lastToken.Type == DerefEnd)
            && Regex.IsMatch(_lastToken.Text, @"^[a-zA-Z0-9_%]+$"))
            this.Type = MainLexer.OpenParenNoWS;
    }
    protected void ProcessCloseParen()
    {
        _currentDepth--;
    }

    protected void ProcessStringLiteral()
    {
        if (_lastVisibleToken == null || _lastVisibleToken.Type == OpenBrace)
        {
        }
    }

    protected void ProcessTemplateOpenBrace() {
        _currentDepth++;
        templateDepthStack.Push(_currentDepth);
    }

    protected void ProcessTemplateCloseBrace() {
        templateDepthStack.Pop();
        _currentDepth--;
    }
    private uint _derefDepth = 0;
    protected void ProcessDeref() {
        if (_derefDepth == 0) {
            _derefDepth++;
            _currentDepth++;

            if (_lastToken != null && _lastToken.Channel != Hidden && ((Regex.IsMatch(_lastToken.Text, @"^[a-zA-Z0-9_]+$") || (_lastToken.Type == MainLexer.DerefEnd))))
                this.Type = MainLexer.DerefContinuation;
            else
                this.Type = MainLexer.DerefStart;
        } else {
            _derefDepth--;
            _currentDepth--;
            this.Type = MainLexer.DerefEnd;
        }
    }

    protected bool IgnoreEOL() {
        if (_ignoreNextEOL || (_lastVisibleToken != null && lineContinuationOperators.Contains(_lastVisibleToken.Type))) 
            return true;
        return _currentDepth != 0;
    }

    /// <summary>
    /// Returns true if the lexer can match a regex literal.
    /// </summary>
    protected bool IsRegexPossible()
    {
        if (_lastToken == null)
        {
            // No token has been produced yet: at the start of the input,
            // no division is possible, so a regex literal _is_ possible.
            return true;
        }

        switch (_lastToken.Type)
        {
            case Identifier:
            case NullLiteral:
            case BooleanLiteral:
            case This:
            case CloseBracket:
            case CloseParen:
            case OctalIntegerLiteral:
            case DecimalLiteral:
            case HexIntegerLiteral:
            case StringLiteral:
            case PlusPlus:
            case MinusMinus:
                // After any of the tokens above, no regex literal can follow.
                return false;
            default:
                // In all other cases, a regex literal _is_ possible.
                return true;
        }
    }

    public override void Reset()
    {
        _lastToken = null;
        _lastVisibleToken = null;
        _currentDepth = 0;
        templateDepthStack.Clear();
        base.Reset();
    }
}

public class MainLexerErrorListener : IAntlrErrorListener<int>
{
    public void SyntaxError(
        TextWriter output, 
        IRecognizer recognizer,
        int offendingSymbol, 
        int line,
        int charPositionInLine, 
        string msg,
        RecognitionException e)
    {
        var codeLine = codeLines[line-1];
        string sourceName = recognizer.InputStream.SourceName;
#if DEBUG
        Console.Error.WriteLine($"Error at line {codeLine.LineNumber}, column {charPositionInLine}, source {sourceName}: {msg}");
#endif
        throw new ParseException($"Syntax error at line {codeLine.LineNumber}:{charPositionInLine}, source {sourceName} - {msg}", codeLine);
    }
}