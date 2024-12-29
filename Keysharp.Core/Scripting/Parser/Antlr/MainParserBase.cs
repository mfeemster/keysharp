using Antlr4.Runtime;
using Keysharp.Scripting;
using System.Collections.Generic;
using System.IO;
using static MainParser;
using static Keysharp.Core.Scripting.Parser.Antlr.Helper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Antlr4.Runtime.Atn;

public class MainParserErrorListener : IAntlrErrorListener<IToken>
{
    public void SyntaxError(
        TextWriter errorOutput,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        var codeLine = codeLines[line-1];

        // Handle syntax errors here
#if DEBUG
        Console.Error.WriteLine($"Syntax error at line {codeLine.LineNumber}, column {charPositionInLine}: {offendingSymbol.Text} msg: {msg}");
#endif
        // Optionally, throw an exception to stop parsing
        throw new ParseException($"Syntax error at line {codeLine.LineNumber}:{charPositionInLine} - {msg}", codeLine);
    }
}

/// <summary>
/// All parser methods that used in grammar (p, prev, notEOL, etc.)
/// should start with lower case char similar to parser rules.
/// </summary>
public abstract class MainParserBase : Antlr4.Runtime.Parser
{
    private readonly Stack<string> _tagNames = new Stack<string>();
    private uint _derefDepth = 0;
    public MainParserBase(ITokenStream input)
        : base(input)
    {
    }

        public void EnableProfiling()
    {
        this.Interpreter = new ProfilingATNSimulator(this);
    }

    public ProfilingATNSimulator GetProfiler()
    {
        return this.Interpreter as ProfilingATNSimulator;
    }

    public MainParserBase(ITokenStream input, TextWriter output, TextWriter errorOutput) : this(input)
    {
        RemoveErrorListeners();
        AddErrorListener(new MainParserErrorListener());
    }

    /// <summary>
    /// Short form for prev(String str)
    /// </summary>
    protected bool p(string str)
    {
        return prev(str);
    }

    /// <summary>
    /// Whether the previous token value equals to str
    /// Ignores hidden channel.
    /// </summary>
    protected bool prev(string str)
    {
        return ((ITokenStream)this.InputStream).LT(-1).Text.Equals(str);
    }

    // Short form for next(String str)
    protected bool n(string str)
    {
        return next(str);
    }

    // Whether the next token value equals to @param str
    // Ignores hidden channels
    protected bool next(string str)
    {
        return ((ITokenStream)this.InputStream).LT(1).Text.Equals(str, StringComparison.InvariantCultureIgnoreCase);
    }

    protected bool notEOL()
    {
        return !EOLAhead();
    }

    protected bool isEOS() {
        for (int i = CurrentToken.TokenIndex + 1; i < InputStream.Size; i++)
        {
            var nextToken = TokenStream.Get(i);
            if (nextToken.Channel != Lexer.Hidden) {
                if (nextToken.Type == CloseBrace)
                    return true;
                return false;
            }

            if (nextToken.Type == MainLexer.EOL || nextToken.Type == SingleLineComment)
                return true;

            if (nextToken.Type == MultiLineComment) {
                if (nextToken.Text.Contains("\r") || nextToken.Text.Contains("\n"))
                    return false;
            }
        }
        return true;
    }

    protected bool isBOS() {
        for (int i = CurrentToken.TokenIndex - 1; i >= 0; i--)
        {
            var prevToken = TokenStream.Get(i);
            if (prevToken.Channel != Lexer.Hidden) {
                if (prevToken.Type == OpenBrace)
                    return true;
                return false;
            }

            if (prevToken.Type == MainLexer.EOL)
                return true;

            if (prevToken.Type == MultiLineComment) {
                if (prevToken.Text.Contains("\r") || prevToken.Text.Contains("\n"))
                    return true;
            }
        }
        return true;
    }

    protected bool isPrevWS() {
        if (CurrentToken.TokenIndex < 1) return true;
        IToken prev = ((ITokenStream)InputStream).Get(CurrentToken.TokenIndex-1);

        return prev.Channel == Lexer.Hidden;
    }

    protected bool isPrevWSDebug() {
        if (CurrentToken.TokenIndex < 1) return true;
        IToken prev = ((ITokenStream)InputStream).Get(CurrentToken.TokenIndex-1);
        Debug.WriteLine($"isPrevWSDebug: current token: {CurrentToken.Text}, prev: {prev?.Text}, result: {prev.Channel == Lexer.Hidden}");
        return prev.Channel == Lexer.Hidden;
    }

    protected bool isNextWS() {
        if (CurrentToken.TokenIndex+1 >= InputStream.Size) return true;
        IToken next = ((ITokenStream)InputStream).Get(CurrentToken.TokenIndex+1);

        return next.Channel == Lexer.Hidden;
    }

    protected bool isNextAndPrevWS() {
        return isNextWS() && isPrevWS();
    }

    protected bool isBeginningOfLine() {
        if (CurrentToken.TokenIndex == 0)
            return true;

        for (int i = CurrentToken.TokenIndex - 1; i >= 0; i--)
        {
            var previousToken = TokenStream.Get(i);

            if (previousToken.Channel == Lexer.Hidden) {
                if (previousToken.Type == MainLexer.EOL)
                    return true;
            } else
                return false;
        }
        return false;
    }
    protected bool notOpenBraceAndNotFunction()
    {
        int nextTokenType = ((ITokenStream)this.InputStream).LT(1).Type;
        if (CurrentToken.Column == 0 && CurrentToken.Type == Identifier && nextTokenType == OpenParen)
            return false;
        return nextTokenType != OpenBrace;
    }

    protected bool closeBrace()
    {
        return ((ITokenStream)this.InputStream).LT(1).Type == CloseBrace;
    }

    protected bool noWhitespaceAhead()
    {
        if (CurrentToken.TokenIndex < 1) return true;
        if (CurrentToken.TokenIndex == (this.InputStream.Size - 1))
            return false;
        IToken ahead = ((ITokenStream)this.InputStream).Get(CurrentToken.TokenIndex+1);

        if (ahead.Channel == Lexer.Hidden && (ahead.Type == EOL || ahead.Type == WhiteSpaces))
            return false;

        return true;
    }

    protected bool whitespaceNoEOLBehind()
    {
        if (CurrentToken.TokenIndex < 1) return false;

        IToken behind = ((ITokenStream)this.InputStream).Get(CurrentToken.TokenIndex-1);

        if (behind.Channel == Lexer.Hidden) {
            if (behind.Type == EOL)
                return false;
            if (behind.Type == WhiteSpaces && CurrentToken.TokenIndex > 1) {
                var possibleIndexEosToken = CurrentToken.TokenIndex - 2;
                if (possibleIndexEosToken < 1) return false;
                var beforeWS = ((ITokenStream)this.InputStream).Get(possibleIndexEosToken);
                if (beforeWS.Type == EOL) return false;
                return true;
            }
            return false;
        }

        return false;
    }

    /// <summary>
    /// Returns true if on the current index of the parser's
    /// token stream a token exists on the Hidden channel which
    /// either is a line terminator, or is a multi line comment that
    /// contains a line terminator.
    /// </summary>
    protected bool EOLAhead()
    {
        // Get the token ahead of the current index.
        int possibleIndexEosToken = CurrentToken.TokenIndex - 1;
        if (possibleIndexEosToken < 0) return false;
        IToken ahead = ((ITokenStream)this.InputStream).Get(possibleIndexEosToken);

        if (ahead.Channel != Lexer.Hidden)
        {
            // We're only interested in tokens on the Hidden channel.
            return false;
        }

        if (ahead.Type == EOL)
        {
            // There is definitely a line terminator ahead.
            return true;
        }

        if (ahead.Type == WhiteSpaces)
        {
            // Get the token ahead of the current whitespaces.
            possibleIndexEosToken = CurrentToken.TokenIndex - 2;
            if (possibleIndexEosToken < 0) return false;
            ahead = ((ITokenStream)this.InputStream).Get(possibleIndexEosToken);
        }

        // Get the token's text and type.
        string text = ahead.Text;
        int type = ahead.Type;

        // Check if the token is, or contains a line terminator.
        return (type == MultiLineComment && (text.Contains("\r") || text.Contains("\n"))) ||
                (type == EOL);
    }
    protected bool validateDeref()
    {
        if (_derefDepth != 0)
        {
            if (((ITokenStream)InputStream).LT(-1).Type != OpenParen)
                return false;
        }
        return true;
    }

    protected bool isDerefAllowed() {
        return _derefDepth == 0;
    }
    protected void startDeref() {
        _derefDepth++;
    }

    protected void endDeref() {
        _derefDepth--;
    }

    protected bool hasNoLinebreaks(ParserRuleContext context)
    {
        if (context != null)
            return false;
        return true;
    }

    protected bool isPart()
    {
        if (_derefDepth != 0)
            return false;
        if (CurrentToken.TokenIndex > 0) {
            var prev = ((ITokenStream)InputStream).Get(CurrentToken.TokenIndex - 1);
            if (prev.Type == Modulus)
                return true;
        }
        if (CurrentToken.TokenIndex < (InputStream.Size - 1)) {
            var next = ((ITokenStream)InputStream).Get(CurrentToken.TokenIndex + 1);
            if (next.Type == Modulus)
                return true;
        }
        return false;
}

/*
    	public void enableWS() {
            if (InputStream is MultiChannelTokenStream) {
                ((MultiChannelTokenStream) InputStream).Enable(TokenConstants.HiddenChannel);
            }
        }
        
        public void disableWS() {
            if (InputStream is MultiChannelTokenStream) {
                ((MultiChannelTokenStream) InputStream).Disable(TokenConstants.HiddenChannel);
            }
        }
        */
}