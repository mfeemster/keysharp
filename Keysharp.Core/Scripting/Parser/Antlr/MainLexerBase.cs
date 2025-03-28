using Antlr4.Runtime;
using static MainParser;
using System.Text.RegularExpressions;

/// <summary>
/// All lexer methods that used in grammar
/// should start with Upper Case Char similar to Lexer rules.
/// </summary>
public abstract class MainLexerBase : Lexer
{
    protected readonly ICharStream _input;
    private IToken _lastToken = null;
    private IToken _lastVisibleToken = null;

    private bool _isBOS = true;

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
        _input = input;
    }

    public MainLexerBase(ICharStream input, TextWriter output, TextWriter errorOutput) : this(input)
    {
        RemoveErrorListeners(); // Remove default error listeners
        AddErrorListener(new MainLexerErrorListener());
    }

    private readonly Queue<IToken> _insertedTokens = new Queue<IToken>();

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
        /*
        // Return any inserted tokens first
        if (_insertedTokens.Count > 0)
        {
            _lastToken = _insertedTokens.Dequeue();
            return _lastToken;
        }
        */

        // Get the next token.
        IToken next = base.NextToken();
        /*
        // Check if the last token was a CloseBrace and insert an EOL after it if there isn't one present already
        if (_lastToken?.Type == CloseBrace && next.Type != EOL) {
            var eolToken = new CommonToken(EOL)
            {
                Line = _lastToken.Line,
                Column = _lastToken.Column + _lastToken.Text.Length
            };
            _insertedTokens.Enqueue(next);
            _lastToken = eolToken;
            return eolToken;
        }
        // Check if the next token is CloseBrace and insert an EOL before it if needed
        if (next?.Type == CloseBrace && _lastToken?.Type != EOL)
        {
            // Insert an EOL token after CloseBrace
            var eolToken = new CommonToken(EOL)
            {
                Line = _lastToken.Line,
                Column = _lastToken.Column + _lastToken.Text.Length
            };
            _insertedTokens.Enqueue(next);
            _lastToken = eolToken;
            return eolToken;
        } else
        */

        if (next.Channel == DefaultTokenChannel)
            _lastVisibleToken = next;

        if (next.Type == EOL || next.Type == DirectiveNewline)
            _isBOS = true;
        else if (next.Type != WS)
            _isBOS = false;

        _lastToken = next;

        return next;
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
        int intermediate = _processHotstringOptions(base.Text.Substring(1));
        return intermediate == -1 ? _hotstringIsLiteral : intermediate == 0;
    }

    protected void ProcessHotstringOptions() {
        int intermediate = _processHotstringOptions(base.Text);
        if (intermediate != -1)
            _hotstringIsLiteral = intermediate == 0;
    }

    protected bool IsBeginningOfLine() {
        return Column == 0;
    }

    protected void ProcessOpenBrace()
    {
        //_currentDepth++;
    }
    protected void ProcessCloseBrace()
    {
        //_currentDepth--;
    }

    protected bool IgnoreEOL() {
        return _currentDepth != 0;
    }

    protected void ProcessEOL() {
        //if (_currentDepth != 0)
        //    this.Type = MainLexer.WS;
        //else
            //this.Text = "\n\r";
        if (_lastVisibleToken == null) return;
        if (_lastVisibleToken.Type != MainLexer.OpenBrace && lineContinuationOperators.Contains(_lastVisibleToken.Type))
            this.Channel = Hidden;
    }

    protected void ProcessWS() {
        if (_lastVisibleToken == null) return;
        if (lineContinuationOperators.Contains(_lastVisibleToken.Type))
            this.Channel = Hidden;
        //if (_lastToken?.Type == EOL || _lastToken?.Type == CloseBrace)
        //    Skip();
    }

    private HashSet<int> unaryOperators = new HashSet<int> {
        MainLexer.BitNot,
        MainLexer.Not,
        MainLexer.PlusPlus,
        MainLexer.MinusMinus
    };

    public static HashSet<int> flowKeywords = new HashSet<int> {
        MainLexer.Try
    };

    public static HashSet<int> lineContinuationOperators = new HashSet<int> {
        MainLexer.OpenBracket,
        MainLexer.OpenBrace,
        MainLexer.OpenParen,
        MainLexer.DerefStart,
        MainLexer.Comma,
        MainLexer.Assign,
        MainLexer.QuestionMark,
        MainLexer.QuestionMarkDot,
        //MainLexer.Colon,
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
    }
    protected void ProcessCloseBracket()
    {
        _currentDepth--;
    }
    protected void ProcessOpenParen()
    {
        _currentDepth++;
    }
    protected void ProcessCloseParen()
    {
        _currentDepth--;
    }

    protected void ProcessHotstringOpenBrace()
    {
        this.Type = MainLexer.OpenBrace;
        ProcessOpenBrace();
    }

    protected void ProcessStringLiteral()
    {
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
            this.Type = DerefStart;
        } else {
            _derefDepth--;
            _currentDepth--;
            this.Type = DerefEnd;
        }
    }

    protected bool IsBOS()
    {
        return _isBOS;
    }
    
    protected bool IsValidRemap() {
        int i = 0;
        if (_input.LA(-1) == '{' && _input.LA(-2) != '`')
            return false;
        while (true) {
            i++;
            var nextToken = _input.LA(i);
            switch (nextToken) {
                case Eof:
                case '\n':
                case '\r':
                    return true;
                case ' ':
                case '\u2028':
                case '\u2029':
                    continue;
                case ';':
                    if (i == 1)
                        return false;
                    return true;
                case '/':
                    if (_input.LA(i+1) == '*') {
                        return true;
                    }
                    return false;
            }
            return false;
        }
    }

    protected bool IsValidDotDecimal() {
        if (_lastToken == null || _lastVisibleToken == null || _lastToken.Channel != DefaultTokenChannel)
            return true;
        if (lineContinuationOperators.Contains(_lastVisibleToken.Type))
            return true;
        return false;
    }

    protected bool IsCommentPossible() {
        int start = this.TokenStartCharIndex;
        if (start == 0)
            return true;
        Antlr4.Runtime.Misc.Interval interval = new(start - 1, start - 1);
        string prevCharText = _input.GetText(interval);
        if (string.IsNullOrEmpty(prevCharText)) {
            return false;
        }
        char prevChar = prevCharText[0];
        return char.IsWhiteSpace(prevChar) || prevChar == '\n' || prevChar == '\r' || 
            prevChar == '\u2028' || prevChar == '\u2029';
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
            case MainParser.True:
            case MainParser.False:
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
        string sourceName = recognizer.InputStream.SourceName;
        Console.Error.WriteLine($"Error at line {line}, column {charPositionInLine}, source {sourceName}: {msg}");
    }
}