/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2014 by Bart Kiers (original author) and Alexandre Vitorelli (contributor -> ported to CSharp)
 * Copyright (c) 2017-2020 by Ivan Kochurkin (Positive Technologies):
    added ECMAScript 6 support, cleared and transformed to the universal grammar.
 * Copyright (c) 2018 by Juan Alvarez (contributor -> ported to Go)
 * Copyright (c) 2019 by Student Main (contributor -> ES2020)
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

// $antlr-format alignTrailingComments true, columnLimit 150, maxEmptyLinesToKeep 1, reflowComments false, useTab false
// $antlr-format allowShortRulesOnASingleLine true, allowShortBlocksOnASingleLine true, minEmptyLines 0, alignSemicolons ownLine
// $antlr-format alignColons trailing, singleLineOverrulesHangingColon true, alignLexerCommands true, alignLabels true, alignTrailers true

lexer grammar MainLexer;

channels {
    ERROR
}

options {
    superClass = MainLexerBase;
    caseInsensitive = true;
}

tokens {
    DerefStart,
    DerefEnd,
    OpenParenNoWS,
    DerefContinuation,
    IdentifierContinuation,
    OpenBracketWithWS
}

// Insert here @header for C++ lexer.

MultiLineComment  : '/*' .*? '*/'             -> channel(HIDDEN);
SingleLineComment : ';' ~[\r\n\u2028\u2029]* -> channel(HIDDEN);
RegularExpressionLiteral:
    '/' RegularExpressionFirstChar RegularExpressionChar* {this.IsRegexPossible()}? '/' IdentifierPart*
;

OpenBracket                : '[' {this.ProcessOpenBracket();};
CloseBracket               : ']' {this.ProcessCloseBracket();};
OpenParen                  : '(' {this.ProcessOpenParen();};
CloseParen                 : ')' {this.ProcessCloseParen();};
OpenBrace                  : '{'; // {this.ProcessOpenBrace();};
CloseBrace                 : '}'; // {this.ProcessCloseBrace();};
SemiColon                  : ';';
Comma                      : ',';
Assign                     : ':=';
QuestionMark               : '?';
QuestionMarkDot            : '?.';
Colon                      : ':';
DoubleColon                : '::';
Ellipsis                   : '...';
DotConcat                  : (WhiteSpaces | LineBreak)+ '.' (WhiteSpaces | LineBreak)+;
Dot                        : '.';
PlusPlus                   : '++';
MinusMinus                 : '--';
Plus                       : '+';
Minus                      : '-';
BitNot                     : '~';
Not                        : '!';
Multiply                   : '*';
Divide                     : '/';
IntegerDivide              : '//';
Modulus                    : '%' {ProcessDeref();};
Power                      : '**';
NullCoalesce               : '??';
Hashtag                    : '#';
RightShiftArithmetic       : '>>';
LeftShiftArithmetic        : '<<';
RightShiftLogical          : '>>>';
LessThan                   : '<';
MoreThan                   : '>';
LessThanEquals             : '<=';
GreaterThanEquals          : '>=';
Equals_                    : '=';
NotEquals                  : '!=';
IdentityEquals             : '==';
IdentityNotEquals          : '!==';
RegExMatch                 : '~=';
BitAnd                     : '&';
BitXOr                     : '^';
BitOr                      : '|';
And                        : '&&';
Or                         : '||';
MultiplyAssign             : '*=';
DivideAssign               : '/=';
ModulusAssign              : '%=';
PlusAssign                 : '+=';
MinusAssign                : '-=';
LeftShiftArithmeticAssign  : '<<=';
RightShiftArithmeticAssign : '>>=';
RightShiftLogicalAssign    : '>>>=';
IntegerDivideAssign        : '//=';
ConcatenateAssign          : '.=';
BitAndAssign               : '&=';
BitXorAssign               : '^=';
BitOrAssign                : '|=';
PowerAssign                : '**=';
NullishCoalescingAssign    : '??=';
Arrow                      : '=>';

/// Null Literals

NullLiteral: 'null';
Unset: 'unset';

/// Boolean Literals

BooleanLiteral: 'true' | 'false';

/// Numeric Literals

DecimalLiteral:
    DecimalIntegerLiteral '.' [0-9] [0-9_]* ExponentPart?
    | '.' [0-9] [0-9_]* ExponentPart?
    | DecimalIntegerLiteral ExponentPart?
;

/// Numeric Literals

HexIntegerLiteral    : '0x' [0-9a-f] HexDigit*;
OctalIntegerLiteral  : '0' [0-7]+;
OctalIntegerLiteral2 : '0o' [0-7] [_0-7]*;
BinaryIntegerLiteral : '0b' [01] [_01]*;

BigHexIntegerLiteral     : '0x' [0-9a-f] HexDigit* 'n';
BigOctalIntegerLiteral   : '0o' [0-7] [_0-7]* 'n';
BigBinaryIntegerLiteral  : '0b' [01] [_01]* 'n';
BigDecimalIntegerLiteral : DecimalIntegerLiteral 'n';

/// Keywords

Break      : 'break';
Do         : 'do';
Instanceof : 'instanceof';
Switch     : 'switch';
Case       : 'case';
Default    : 'default';
Else       : 'else';
Catch      : 'catch';
Finally    : 'finally';
Return     : 'return';
Continue   : 'continue';
For        : 'for';
While      : 'while';
Loop       : 'loop';
Until      : 'until';
Files      : 'files';
Reg        : 'reg';
Parse      : 'parse';
This       : 'this';
If         : 'if';
Throw      : 'throw';
Delete     : 'delete';
In         : 'in';
Try        : 'try';
Yield      : 'yield';
Is         : 'is';
Contains   : 'contains';
VerbalAnd  : 'and';
VerbalNot  : 'not';
VerbalOr   : 'or';

Goto       : 'goto';

/// Future Reserved Words

Class   : 'class';
Get     : 'get';
Set     : 'set';
Enum    : 'enum';
Extends : 'extends';
Super   : 'super';
Base    : 'base';
Export  : 'export';
Import  : 'import';
From    : 'from';
As      : 'as';

Async : 'async';
Await : 'await';

Static : 'static';
Global : 'global';
Local  : 'local';

Include : '#include';
IncludeAgain : 'includeagain';
HotIf : '#hotif';
HotIfTimeout : '#hotiftimeout';
ClipboardTimeout : '#clipboardtimeout';
DllLoad : '#dllload';
ErrorStdOut : '#errorstdout';
Hotstring : '#hotstring';
InputLevel : '#inputlevel';
MaxThreads : '#maxthreads';
MaxThreadsBuffer : '#maxthreadsbuffer';
MaxThreadsPerHotkey : '#maxthreadsperhotkey';
NoTrayIcon : '#notrayicon';
Requires : '#requires';
SingleInstance : '#singleinstance';
SuspendExempt : '#suspendexempt';
UseHook : '#usehook';
Warn : '#warn';
WinActivateForce : '#winactivateforce';

GeneralDirective
    : ClipboardTimeout WhiteSpaces IntegerLiteral
    | DllLoad WhiteSpaces RawString
    | ErrorStdOut WhiteSpaces RawString
    | HotIfTimeout WhiteSpaces IntegerLiteral
    | Hotstring WhiteSpaces ('nomouse' | 'endchars' WhiteSpaces RawString)
    | Include WhiteSpaces RawString
    | IncludeAgain WhiteSpaces RawString
    | MaxThreads WhiteSpaces IntegerLiteral
    | MaxThreadsBuffer WhiteSpaces (BooleanLiteral | IntegerLiteral)?
    | NoTrayIcon
    | Requires WhiteSpaces RawString
    | SingleInstance WhiteSpaces ('force' | 'ignore' | 'prompt' | 'off')
    | WinActivateForce
    ;

//HotstringOptions : Hotstring WhiteSpaces RawString {this.ProcessHotstringOptions();};

/// Identifier Names and Identifiers

Identifier: IdentifierStart IdentifierPart*;
/// String Literals
MultilineStringLiteral:
    ('"' ~[\r\n]* LineBreak '(' .*? LineBreak WhiteSpaces? ')"' 
    | '\'' ~[\r\n]* LineBreak '(' .*? LineBreak WhiteSpaces? ')\'') {this.ProcessStringLiteral();}
;
StringLiteral:
    ('"' DoubleStringCharacter* '"' | '\'' SingleStringCharacter* '\'') {this.ProcessStringLiteral();}
;

/*
HotstringLiteral:
    HotstringTrigger {this.IsHotstringLiteral()}? (LineBreak HotstringTrigger)* (RawString | LineBreak '(' ~[\r\n]+ LineBreak (EscapeCharacter | .)*? LineBreak ')');

HotstringTriggers:
   HotstringTrigger (LineBreak HotstringTrigger)*;

HotkeyLiteral:
    NonColonStringCharacter+ DoubleColon;

fragment HotstringTrigger:
    (Colon Options? Colon Trigger DoubleColon (WhiteSpaces SingleLineComment)?);
    */

fragment Options:
    NonColonStringCharacter+;
fragment Trigger:
    NonColonStringCharacter+;

WhiteSpaces: [\t\u000B\u000C\u0020\u00A0]+ -> channel(HIDDEN);

IgnoreEOL: [\r\n\u2028\u2029]+ {this.IgnoreEOL()}? -> channel(HIDDEN);
EOL: [\r\n\u2028\u2029]+ {!this.IgnoreEOL()}?;

/// Comments

HtmlComment         : '<!--' .*? '-->'      -> channel(HIDDEN);
CDataComment        : '<![CDATA[' .*? ']]>' -> channel(HIDDEN);
UnexpectedCharacter : .                     -> channel(ERROR);

mode NOWHITESPACE;
NowhitespaceWhiteSpaces: [\t\u000B\u000C\u0020\u00A0]+ -> popMode; // End mode if whitespace is detected.

// Fragment rules

fragment DoubleStringCharacter: ~["`] | '`' EscapeSequence;

fragment SingleStringCharacter: ~['`] | '`' EscapeSequence;

fragment NonColonStringCharacter: ~[;:`\r\n] | '`' EscapeSequence;

fragment RawStringCharacter: ~[;`\r\n] | '`' EscapeSequence;

fragment AnyCharacter: .;

fragment RawString: RawStringCharacter+;

fragment EscapeSequence:
    CharacterEscapeSequence
    | '0' // no digit ahead! TODO
    | HexEscapeSequence
    | UnicodeEscapeSequence
    | ExtendedUnicodeEscapeSequence
;

fragment CharacterEscapeSequence: SingleEscapeCharacter | NonEscapeCharacter;

fragment HexEscapeSequence: 'x' HexDigit HexDigit;

fragment UnicodeEscapeSequence:
    'u' HexDigit HexDigit HexDigit HexDigit
    | 'u' '{' HexDigit HexDigit+ '}'
;

fragment ExtendedUnicodeEscapeSequence: 'u' '{' HexDigit+ '}';

fragment SingleEscapeCharacter: [`;:'"bfnrtvsa];

fragment NonEscapeCharacter: ~[`;:'"bfnrtvsa0-9xu\r\n];

fragment EscapeCharacter: SingleEscapeCharacter | [0-9] | [xu];

fragment LineBreak: [\r\n\u2028\u2029];
fragment LineBreaks: [\r\n\u2028\u2029]+;

fragment HexDigit: [_0-9a-f];

fragment DecimalIntegerLiteral: '0' | [1-9] [0-9_]*;

fragment IntegerLiteral: Minus? DecimalIntegerLiteral;

fragment ExponentPart: 'e' [+-]? [0-9_]+;

fragment IdentifierPart: IdentifierStart | [\p{Mn}] | [\p{Nd}] | [\p{Pc}] | '\u200C' | '\u200D';

fragment IdentifierStart: [\p{L}] | [$_] | '\\' UnicodeEscapeSequence;

fragment RegularExpressionFirstChar:
    ~[*\r\n\u2028\u2029\\/[]
    | RegularExpressionBackslashSequence
    | '[' RegularExpressionClassChar* ']'
;

fragment RegularExpressionChar:
    ~[\r\n\u2028\u2029\\/[]
    | RegularExpressionBackslashSequence
    | '[' RegularExpressionClassChar* ']'
;

fragment RegularExpressionClassChar: ~[\r\n\u2028\u2029\]\\] | RegularExpressionBackslashSequence;

fragment RegularExpressionBackslashSequence: '\\' ~[\r\n\u2028\u2029];
