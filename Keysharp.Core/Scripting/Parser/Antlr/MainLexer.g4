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

MultiLineComment  : '/*' .*? '*/'             -> channel(HIDDEN);
SingleLineComment : ';' ~[\r\n\u2028\u2029]* {this.IsCommentPossible()}? -> channel(HIDDEN);
RegularExpressionLiteral:
    '/' RegularExpressionFirstChar RegularExpressionChar* {this.IsRegexPossible()}? '/' IdentifierPart*
;

/*
HotstringLiteral:
    HotstringTrigger {this.IsHotstringLiteral()}? (LineBreak HotstringTrigger)* RawString;  //| LineBreak '(' ~[\r\n]+ LineBreak (EscapeCharacter | .)*? LineBreak ')');
*/

// First try consuming a hotstring trigger
HotstringLiteralTrigger
    : HotstringTriggerPart {this.IsHotstringLiteral()}? -> pushMode(HOTSTRING_MODE), type(HotstringTrigger)
    ;
// These two are separated because I couldn't figure out how to conditionally pushMode(HOTSTRING_MODE)
HotstringTrigger
    : HotstringTriggerPart {!this.IsHotstringLiteral()}?
    ;

// If no hotstring is matchable, try matching a remap
RemapKey:
    HotkeyModifier* HotkeyCharacter {this.IsValidRemap()}?;

// Otherwise just match a hotkey trigger
HotkeyTrigger:
    HotkeyModifier* HotkeyCharacter (WS+ '&' WS+ HotkeyCharacter)? (WS+ 'up')? '::' {this.IsBOS()}?;

/*
HotkeyLiteral:
    NonColonStringCharacter+ DoubleColon;
*/


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
Read       : 'read';
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

Include : '#include' -> pushMode(DIRECTIVE_MODE);
IncludeAgain : '#includeagain' -> pushMode(DIRECTIVE_MODE);
HotIf : '#hotif';
HotIfTimeout : '#hotiftimeout';
ClipboardTimeout : '#clipboardtimeout';
DllLoad : '#dllload' -> pushMode(DIRECTIVE_MODE);
ErrorStdOut : '#errorstdout' -> pushMode(DIRECTIVE_MODE);
InputLevel : '#inputlevel';
MaxThreads : '#maxthreads';
MaxThreadsBuffer : '#maxthreadsbuffer';
MaxThreadsPerHotkey : '#maxthreadsperhotkey';
NoTrayIcon : '#notrayicon';
Requires : '#requires' -> pushMode(DIRECTIVE_MODE);
SingleInstance : '#singleinstance' -> pushMode(DIRECTIVE_MODE);
SuspendExempt : '#suspendexempt';
UseHook : '#usehook';
Warn : '#warn';
WinActivateForce : '#winactivateforce';
HotstringOptions : '#hotstring' WS+ RawString {this.ProcessHotstringOptions();};
AssemblyTitle : '#assemblytitle' -> pushMode(DIRECTIVE_MODE);
AssemblyDescription : '#assemblydescription' -> pushMode(DIRECTIVE_MODE);
AssemblyConfiguration : '#assemblyconfiguration' -> pushMode(DIRECTIVE_MODE);
AssemblyCompany : '#assemblycompany' -> pushMode(DIRECTIVE_MODE);
AssemblyProduct : '#assemblyproduct' -> pushMode(DIRECTIVE_MODE);
AssemblyCopyright : '#assemblycopyright' -> pushMode(DIRECTIVE_MODE);
AssemblyTrademark : '#assemblytrademark' -> pushMode(DIRECTIVE_MODE);
AssemblyVersion : '#assemblyversion' -> pushMode(DIRECTIVE_MODE);


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

fragment HotstringTriggerPart:
    Colon Options? Colon Trigger DoubleColon;

fragment HotstringOptionCharacter
    : [*?bcortsxz] ' '* '0'?
    | 'c1' 
    | 'si' | 'sp' | 'se' 
    | 'p' [0-9]+ 
    | 'k' ' '* '-' ' '* [0-9]+
    ;
fragment Options:
    HotstringOptionCharacter+;
fragment Trigger:
    NonColonStringCharacter+;

WhiteSpaces: WS+ -> channel(HIDDEN);

IgnoreEOL: LineBreak+ {this.IgnoreEOL()}? -> channel(HIDDEN);
EOL: LineBreak+ {!this.IgnoreEOL()}?;

/// Comments

UnexpectedCharacter : .                     -> channel(ERROR);

mode HOTSTRING_MODE;
HotstringEOL: LineBreak -> type(EOL), popMode;
HotstringOpenBrace: '{' {this.ProcessHotstringOpenBrace();} -> type(OpenBrace), popMode;
HotstringWhitespaces: WS+ -> channel(HIDDEN);
HotstringExpansion: (~[;`\r\n {] | '`' EscapeSequence) RawString?;
HotstringUnexpectedCharacter: . -> channel(ERROR);

mode DIRECTIVE_MODE;
DirectiveEOL: LineBreak -> type(EOL), popMode;
DirectiveWhitespaces: WS+ -> channel(HIDDEN);
DirectiveContent: (~[;`\r\n ] | '`' EscapeSequence) RawString?;
DirectiveUnexpectedCharacter: . -> channel(ERROR);


// Fragment rules

fragment WS: [\t\u000B\u000C\u0020\u00A0];

fragment DoubleStringCharacter: ~["`] | '`' EscapeSequence;

fragment SingleStringCharacter: ~['`] | '`' EscapeSequence;

fragment NonColonStringCharacter: ~[;:`\r\n] | '`' EscapeSequence;

fragment RawStringCharacter
    : ~[`\t\n\r\u2028\u2029\f ] ';'         // Match semicolon only if not preceded by whitespace
    | ~[;`\r\n\u2028\u2029]                 // Match any character except semicolon, newline, or carriage return
    | '`' EscapeSequence       // Match escape sequences starting with backtick
    ;

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

fragment HotkeyModifierKey: [#!^+<>];

fragment HotkeyModifier: HotkeyModifierKey | [*~$];

fragment HotkeyCharacter 
    : 'NumpadEnter'
    | 'Delete'
    | 'Del'
    | 'Insert'
    | 'Ins'
    | 'Clear'
    | 'Up'
    | 'Down'
    | 'Left'
    | 'Right'
    | 'Home'
    | 'End'
    | 'PgUp'
    | 'PgDn'
    | 'Numpad0'
    | 'Numpad1'
    | 'Numpad2'
    | 'Numpad3'
    | 'Numpad4'
    | 'Numpad5'
    | 'Numpad6'
    | 'Numpad7'
    | 'Numpad8'
    | 'Numpad9'
    | 'NumpadMult'
    | 'NumpadDiv'
    | 'NumpadAdd'
    | 'NumpadSub'
    | 'NumpadDot'
    | 'Numlock'
    | 'ScrollLock'
    | 'CapsLock'
    | 'Escape'
    | 'Esc'
    | 'Tab'
    | 'Space'
    | 'Backspace'
    | 'BS'
    | 'Enter'
    | 'NumpadDel'
    | 'NumpadIns'
    | 'NumpadClear'
    | 'NumpadUp'
    | 'NumpadDown'
    | 'NumpadLeft'
    | 'NumpadRight'
    | 'NumpadHome'
    | 'NumpadEnd'
    | 'NumpadPgUp'
    | 'NumpadPgDn'
    | 'PrintScreen'
    | 'CtrlBreak'
    | 'Pause'
    | 'Help'
    | 'Sleep'
    | 'AppsKey'
    | 'LControl'
    | 'RControl'
    | 'LCtrl'
    | 'RCtrl'
    | 'LShift'
    | 'RShift'
    | 'LAlt'
    | 'RAlt'
    | 'LWin'
    | 'RWin'
    | 'Control'
    | 'Ctrl'
    | 'Alt'
    | 'Shift'
    | 'F1'
    | 'F2'
    | 'F3'
    | 'F4'
    | 'F5'
    | 'F6'
    | 'F7'
    | 'F8'
    | 'F9'
    | 'F10'
    | 'F11'
    | 'F12'
    | 'F13'
    | 'F14'
    | 'F15'
    | 'F16'
    | 'F17'
    | 'F18'
    | 'F19'
    | 'F20'
    | 'F21'
    | 'F22'
    | 'F23'
    | 'F24'
    | 'LButton'
    | 'RButton'
    | 'MButton'
    | 'XButton1'
    | 'XButton2'
    | 'WheelDown'
    | 'WheelUp'
    | 'WheelLeft'
    | 'WheelRight'
    | 'Browser_Back'
    | 'Browser_Forward'
    | 'Browser_Refresh'
    | 'Browser_Stop'
    | 'Browser_Search'
    | 'Browser_Favorites'
    | 'Browser_Home'
    | 'Volume_Mute'
    | 'Volume_Down'
    | 'Volume_Up'
    | 'Media_Next'
    | 'Media_Prev'
    | 'Media_Stop'
    | 'Media_Play_Pause'
    | 'Launch_Mail'
    | 'Launch_Media'
    | 'Launch_App1'
    | 'Launch_App2'
    | 'AltTab'
    | 'ShiftAltTab'
    | ~[`\r\n\u2028\u2029 ]   // Match any character except semicolon, newline, carriage return, or whitespace
    | '`' EscapeSequence       // Match escape sequences starting with backtick
    ;

fragment HotkeyCombinatorCharacter: '&';
