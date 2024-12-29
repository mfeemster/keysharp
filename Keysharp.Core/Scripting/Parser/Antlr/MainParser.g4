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

// $antlr-format alignTrailingComments true, columnLimit 150, minEmptyLines 1, maxEmptyLinesToKeep 1, reflowComments false, useTab false
// $antlr-format allowShortRulesOnASingleLine false, allowShortBlocksOnASingleLine true, alignSemicolons hanging, alignColons hanging

parser grammar MainParser;

// Insert here @header for C++ parser.

options {
    tokenVocab = MainLexer;
    superClass = MainParserBase;
}

program
    : sourceElements EOF
    ;

sourceElements
    : sourceElement+
    ;

sourceElement
//    : directive
//    | hotkeyDeclaration
//    | hotstringDeclaration
    : classDeclaration
    | functionDeclaration // required for auto-execute section function declarations
    | statement
    | EOL+
    ;

statement
    : block eos?
//    | importStatement
//    | exportStatement
    | functionDeclaration eos   // required for nested functions
    | classDeclaration eos
    | functionStatement eos     // requires OpenBrace or LineTerminator before it
    | expressionStatement eos   // requires OpenBrace or LineTerminator before it
    | ifStatement // always followed by a statement which requires eos
    | iterationStatement
    | continueStatement eos
    | breakStatement eos
    | returnStatement eos
//    | yieldStatement
    | labelledStatement eos
    | gotoStatement eos
    | switchStatement eos
    | throwStatement eos
    | tryStatement  // always followed by a statement which requires eos
    | variableStatement eos
    ;

/*
statement
    : block
    | variableStatement
    | importStatement
    | exportStatement
    | classDeclaration
    | functionDeclaration
    | expressionStatement
    | ifStatement
    | iterationStatement
    | continueStatement
    | breakStatement
    | returnStatement
    | yieldStatement
    | withStatement
    | labelledStatement
    | switchStatement
    | throwStatement
    | tryStatement
    | debuggerStatement
    ;
*/

block
    : EOL? '{' statementList? EOL? '}'
    ;

statementList
    : statement+
    ;

variableStatement
    : (Global | Local | Static) variableDeclarationList?
    ;

importStatement
    : Import importFromBlock
    ;

importFromBlock
    : importDefault? (importNamespace | importModuleItems) importFrom
    | StringLiteral
    ;

importModuleItems
    : '{' (importAliasName ',')* (importAliasName ','?)? '}'
    ;

importAliasName
    : moduleExportName (As importedBinding)?
    ;

moduleExportName
    : identifierName
    | StringLiteral
    ;

// yield and await are permitted as BindingIdentifier in the grammar
importedBinding
    : Identifier
    | Yield
    | Await
    ;

importDefault
    : aliasName ','
    ;

importNamespace
    : ('*' | identifierName) (As identifierName)?
    ;

importFrom
    : From StringLiteral
    ;

aliasName
    : identifierName (As identifierName)?
    ;

exportStatement
    : Export Default? (exportFromBlock | declaration) # ExportDeclaration
    | Export Default singleExpression                 # ExportDefaultDeclaration
    ;

exportFromBlock
    : importNamespace importFrom
    | exportModuleItems importFrom?
    ;

exportModuleItems
    : '{' (exportAliasName ',')* (exportAliasName ','?)? '}'
    ;

exportAliasName
    : moduleExportName (As moduleExportName)?
    ;

declaration
    : classDeclaration
    | functionDeclaration
    ;

variableDeclarationList
    : variableDeclaration (',' variableDeclaration)*
    ;

variableDeclaration
    : assignable (':=' singleExpression)? // Should only be used with Local, Global, Static keywords
    ;

functionStatement
    : identifier arguments?
    ;

expressionStatement
    : /*{this.notOpenBraceAndNotFunction()}?*/ expressionSequence
    ;

ifStatement
    : If singleExpression EOL? statement (EOL? Else EOL? statement)?
    ;

iterationStatement
    : Loop singleExpression? EOL? statement (Until singleExpression EOL)? (Else statement)?      # LoopStatement
    | Loop Files singleExpression (',' singleExpression)? statement (Until singleExpression EOL)? (Else statement)?  # LoopFilesStatement
    | Loop Reg singleExpression (',' singleExpression)? statement (Until singleExpression EOL)? (Else statement)?    # LoopRegStatement
    | Loop Parse singleExpression (',' singleExpression?)* statement (Until singleExpression EOL)? (Else statement)? # LoopParseStatement
    | While singleExpression EOL? statement (Until singleExpression EOL)? (Else statement)?      # WhileStatement
    | For forInParameters EOL? statement (Else statement)?                                       # ForInStatement
    ;

forInParameters
    : assignable? (',' assignable?)* In singleExpression
    | '(' assignable? (',' assignable?)* In singleExpression ')'
    ;

continueStatement
    : Continue propertyName?
    | Continue ('(' propertyName ')')?
    ;

breakStatement
    : Break propertyName?
    | Break ('(' propertyName ')')?
    ;

returnStatement
    : Return singleExpression?
    ;

yieldStatement
    : Yield ({this.notEOL()}? singleExpression)?
    ;

switchStatement
    : Switch singleExpression? (',' literal)? EOL? caseBlock
    ;

caseBlock
    : '{' caseClauses? (EOL? defaultClause EOL? caseClauses?)? EOL? '}'
    ;

caseClauses
    : caseClause+
    ;

caseClause
    : Case expressionSequence ':' statementList? EOL?
    ;

defaultClause
    : Default ':' statementList?
    ;

labelledStatement
    : propertyName ':'
    ;

gotoStatement
    : Goto propertyName
    | Goto '(' propertyName ')'
    ;

throwStatement
    : Throw {this.notEOL()}? singleExpression
    ;

tryStatement
    : Try EOL? statement EOL? catchProduction* elseProduction? finallyProduction?
    ;

catchProduction
    : Catch catchAssignable? EOL? statement EOL?
    ;

catchAssignable
    : assignable (As identifier)?
    | '(' assignable (As identifier)? ')'
    ;

elseProduction
    : Else statement EOL?
    ;

finallyProduction
    : Finally statement EOL?
    ;

functionDeclaration
    : Async? identifier OpenParenNoWS formalParameterList? ')' functionBody
    | Async? identifier arrowFunctionParameters '=>' arrowFunctionBody
    ;

/*
hotkeyDeclaration
    : (HotkeyLiteral+) (singleExpression eos | functionBody)
    ;

hotstringDeclaration
    : HotstringLiteral eos
    | HotstringTriggers (singleExpression eos | functionBody)
    ;
*/

classDeclaration
    : Class identifier classTail
    ;

classTail
    : (Extends identifier ('.' identifier)*)? EOL? '{' classElement* EOL? '}' EOL?
    ;

classElement
    : Static? methodDefinition EOL   # ClassMethodDeclaration
    | Static? propertyDefinition EOL # ClassPropertyDeclaration
    | Static? fieldDefinition EOL    # ClassFieldDeclaration
    | classDeclaration EOL           # NestedClassDeclaration
    ;

methodDefinition
    : Async? propertyName OpenParenNoWS formalParameterList? ')' functionBody
    | Async? propertyName OpenParenNoWS formalParameterList? ')' '=>' arrowFunctionBody
    ;

propertyDefinition
    : classPropertyName '=>' singleExpression
    | classPropertyName EOL? '{' propertyGetterDefinition? propertySetterDefinition? '}' EOL?
    ;

classPropertyName
    : identifier
    | identifier '[' formalParameterList ']'
    ;

propertyGetterDefinition
    : Get functionBody
    | Get '=>' arrowFunctionBody EOL?
    ;

propertySetterDefinition
    : Set functionBody
    | Set '=>' arrowFunctionBody EOL?
    ;

fieldDefinition
    : propertyName ((':=' propertyName)* EOL? initializer)?
    ;

formalParameterList
    : (formalParameterArg ',')* lastFormalParameterArg
    ;

formalParameterArg
    : BitAnd? assignable (':=' singleExpression | QuestionMark)? // singleLineExpression not needed because is always enclosed in parenthesis
    ;

lastFormalParameterArg
    : formalParameterArg Multiply? // Spread
    | Multiply
    ;

functionBody
    : EOL? '{' statementList? '}' EOL?
    ;

arrayLiteral
    : (OpenBracketWithWS arrayElementList ']')
    ;

// Keysharp supports arrays like [,,1,2,,].
arrayElementList
    : (arrayElement? ',')* arrayElement?
    ;

arrayElement
    : singleExpression Multiply? // Spread
    ;

mapLiteral
    : ('[' mapElementList ']')
    ;

// Keysharp supports arrays like [,,1,2,,].
mapElementList
    : (mapElement? ',')*
    ;

mapElement
    : singleExpression Multiply? // Spread
    | singleExpression ':' singleExpression // Map element
    ;

propertyAssignment
    : propertyName ':' singleExpression                                  # PropertyExpressionAssignment
    | Async? '*'? propertyName OpenParenNoWS formalParameterList? ')' functionBody # FunctionProperty
    | getter OpenParenNoWS ')' functionBody                                        # PropertyGetter
    | setter OpenParenNoWS formalParameterArg ')' functionBody                     # PropertySetter
    //| Ellipsis? singleExpression                                         # PropertyShorthand
    ;

dynamicPropertyName
    : (propertyName | dereference) derefContinuation*
    ;

propertyName
    : Identifier
    | reservedWord
    | StringLiteral
    | numericLiteral
    ;

derefContinuation
    : DerefContinuation singleExpression DerefEnd
    | IdentifierContinuation
    ;

dereference
    : DerefStart singleExpression DerefEnd
    ;

arguments
    : argument (',' argument?)*
    ;

argument
    : singleExpression Multiply?
    ;

// NOTE: this allows implicit concatenation because it must only be used inside parentheses/brackets
expressionSequence
    : singleExpression (',' singleExpression)*
    ;

/*
    Explanation: 
    In AHK there is a problem with expression concatenation ambiguity when omitting the dot operator.
    For example, `FileAppend "hello" "world"` is valid, but "hello" on the next line is not. 
    This is different if surrounded by parenthesis, because then there is no ambiguity about when the
    expression ends, thus `FileAppend ("hello" "world")` is allowed even on the next line. 
    With other expressions there should be no ambiguity because, for example with
    `FileAppend a := \n"hello"` "hello" is allowed because the assignment operation requires an expression.

    So, singleExpressionConcatenation is a single expression that does *not* start on a new line.
    singleLineExpressionSequence might start on a new line but continues on the same (or is continued
    explicitly with the comma operator).
*/

singleExpressionConcatenation
    : {!this.isBOS()}? singleExpression
    ;

functionCallArguments
    : OpenParenNoWS arguments? ')'
    ;

memberIndexArguments
    : {!this.isPrevWS()}? '[' expressionSequence? ']'
    ;

singleExpression
    : Class Identifier? classTail                                          # ClassExpression // nested class
    | anonymousFunction                                                    # FunctionExpression
    | singleExpression '?.' singleExpression                               # OptionalChainExpression
    | singleExpression '?.'? memberIndexArguments                          # MemberIndexExpression
    | singleExpression functionCallArguments                               # FunctionCallExpression
    | singleExpression '++'                                                # PostIncrementExpression
    | singleExpression '--'                                                # PostDecreaseExpression
    | Delete singleExpression                                              # DeleteExpression
    | '++' singleExpression                                                # PreIncrementExpression
    | '--' singleExpression                                                # PreDecreaseExpression
    | <assoc = right> singleExpression '**' singleExpression               # PowerExpression
    | '-' singleExpression                                                 # UnaryMinusExpression
    | '+' singleExpression                                                 # UnaryPlusExpression
    | '~' singleExpression                                                 # BitNotExpression
    | Await singleExpression                                               # AwaitExpression
    | singleExpression ('*' | '/' | '//') singleExpression                 # MultiplicativeExpression
    | singleExpression ('+' | '-') singleExpression                        # AdditiveExpression
    | singleExpression ('<<' | '>>' | '>>>') singleExpression              # BitShiftExpression
    | singleExpression '&' singleExpression                                # BitAndExpression
    | singleExpression '^' singleExpression                                # BitXOrExpression
    | singleExpression '|' singleExpression                                # BitOrExpression
    | singleExpression DotConcat singleExpression                          # DotConcatenateExpression
    | singleExpression singleExpressionConcatenation+                      # ImplicitConcatenateExpression
    | singleExpression '?'? '.' dynamicPropertyName                      # MemberDotExpression
    | singleExpression '~=' singleExpression                               # RegExMatchExpression
    | singleExpression ('<' | '>' | '<=' | '>=') singleExpression          # RelationalExpression
    | singleExpression ('=' | '!=' | '==' | '!==') singleExpression        # EqualityExpression
    | singleExpression Instanceof singleExpression                         # InstanceofExpression
    | singleExpression Is singleExpression                                 # IsExpression
    | singleExpression In singleExpression                                 # InExpression
    | singleExpression Contains singleExpression                           # ContainsExpression
    | ('!' | VerbalNot) EOL? singleExpression                                   # NotExpression
    | singleExpression ('&&' | VerbalAnd) singleExpression                 # LogicalAndExpression
    | singleExpression ('||' | VerbalOr) singleExpression                  # LogicalOrExpression
    | singleExpression '??' singleExpression                               # CoalesceExpression
    | singleExpression '?' singleExpression ':' singleExpression           # TernaryExpression
    | <assoc = right> singleExpression assignmentOperator singleExpression # AssignmentOperatorExpression
    | '(' expressionSequence ')'                                           # ParenthesizedExpression
    | This                                                                 # ThisExpression
    | Super                                                                # SuperExpression
    | Base                                                                 # BaseExpression
    | BitAnd singleExpression                                              # VarRefExpression
    | dynamicIdentifier                                                    # DynamicIdentifierExpression
    | identifierName                                                       # IdentifierExpression
    | literal                                                              # LiteralExpression
    | arrayLiteral                                                         # ArrayLiteralExpression
    | objectLiteral                                                        # ObjectLiteralExpression
    ;

dynamicIdentifier
    : identifier derefContinuation+ 
    | dereference derefContinuation*
    ;

initializer
    : ':=' singleExpression
    ;

assignable
    : identifier
    | keyword
    | arrayLiteral
    | objectLiteral
    ;

objectLiteral
    : '{' (propertyAssignment (',' propertyAssignment)* EOL?)? '}'
    ;

anonymousFunction
    : Async? arrowFunctionParameters '=>' arrowFunctionBody           # ArrowFunction
    // | functionDeclaration                                             # NamedFunction
    // | Async? '(' formalParameterList? ')' functionBody                # AnonymousFunctionDecl
    ;

arrowFunctionParameters
    : OpenParenNoWS formalParameterList? ')'
    ;

arrowFunctionBody
    : singleExpression
    | functionBody
    ;

assignmentOperator
    : ':='
    | '%='
    | '+='
    | '-='
    | '*='
    | '/='
    | '//='
    | '.='
    | '|='
    | '&='
    | '^='
    | '>>='
    | '<<='
    | '>>>='
    | '**='
    | '??='
    ;

literal
    : NullLiteral
    | Unset
    | BooleanLiteral
    | MultilineStringLiteral
    | StringLiteral
    | RegularExpressionLiteral
    | numericLiteral
    | bigintLiteral
    ;

numericLiteral
    : DecimalLiteral
    | HexIntegerLiteral
    | OctalIntegerLiteral
    | OctalIntegerLiteral2
    | BinaryIntegerLiteral
    ;

bigintLiteral
    : BigDecimalIntegerLiteral
    | BigHexIntegerLiteral
    | BigOctalIntegerLiteral
    | BigBinaryIntegerLiteral
    ;

getter
    : {this.n("get")}? identifier propertyName
    ;

setter
    : {this.n("set")}? identifier propertyName
    ;
/*
directive
    : generalDirective
    | positionalDirective
    ;

positionalDirective
    : HotIf singleExpression
    | HotstringOptions
    | InputLevel numericLiteral
    | MaxThreadsPerHotkey numericLiteral
    | SuspendExempt (BooleanLiteral | numericLiteral)?
    | UseHook (BooleanLiteral | numericLiteral)?
    | Warn ((identifier) (Comma (identifier))?)?
    ;

generalDirective : GeneralDirective;
*/

identifierName
    : identifier
    | reservedWord
    ;

identifier
    : Identifier
    | Async
    | Yield
    ;

reservedWord
    : keyword
    | NullLiteral
    | Unset
    | BooleanLiteral
    ;

keyword
    : Break
    | Do
    | Instanceof
    | Case
    | Else
    | Catch
    | Finally
    | Return
    | Continue
    | For
    | Switch
    | While
    | Loop
    | Until
    | This
    | If
    | Throw
    | Delete
    | In
    | Is
    | Try
    | Class
    | Enum
    | Extends
    | Super
    | Export
    | Import
    | Static
    | Global
    | Local
    | Yield  
    | Async
    | Await
    | Default
    | VerbalAnd
    | VerbalOr
    | VerbalNot
    ;

eos
    : EOL
    | EOF
    ;