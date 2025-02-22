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

parser grammar MainParser;

options {
    tokenVocab = MainLexer;
    superClass = MainParserBase;
}

program
    : sourceElements EOF
    | EOF
    ;

sourceElements
    : sourceElement+
    ;

// Function declarations are handled as expressions
sourceElement
    : classDeclaration eos
    | positionalDirective eos
    | remap eos
    | hotstring eos
    | hotkey eos
    | statement eos
    | WS
    | EOL
    ;

positionalDirective
    : HotIf singleExpression?       # HotIfDirective
    | HotstringOptions              # HotstringDirective
    | InputLevel numericLiteral?     # InputLevelDirective
    | UseHook (numericLiteral | boolean)?          # UseHookDirective
    | SuspendExempt (numericLiteral | boolean)?    # SuspendExemptDirective
    ;

remap
    : RemapKey
    ;

hotstring
    : HotstringTrigger (EOL HotstringTrigger)* WS* (hotstringExpansion | EOL? functionDeclaration | EOL? statement)
    ;

hotstringExpansion
    : HotstringSingleLineExpansion
    | HotstringMultiLineExpansion
    ;

hotkey
    : HotkeyTrigger (EOL HotkeyTrigger)* s* (functionDeclaration | statement)
    ;

statement
    : variableStatement
    | ifStatement
    | (labelledStatement s*)? iterationStatement
    | expressionStatement
    | functionStatement
    | continueStatement
    | breakStatement
    | returnStatement
    | yieldStatement
    | labelledStatement
    | gotoStatement
    | switchStatement
    | throwStatement
    | tryStatement  // always followed by a statement which requires eos */
    | awaitStatement
    | deleteStatement
    | block
//    | importStatement
//    | exportStatement
//    | withStatement
    ;

block
    : '{' s* statementList? '}'
    ;

// Only to be used inside something, cannot meet EOF
statementList
    : (statement EOL)+
    ;

variableStatement
    : (Global | Local | Static) (WS* variableDeclarationList)?
    ;

awaitStatement
    : Await WS* singleExpression
    ;
    
deleteStatement
    : Delete WS* singleExpression
    ;

importStatement
    : Import WS* importFromBlock
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
    : assignable (assignmentOperator expression)? // Should only be used with Local, Global, Static keywords
    ;

functionStatement
    : primaryExpression (WS+ arguments)? // This is ambiguous with expressionStatements first expression. I don't know how to resolve it.
    ;

expressionStatement
    : {!this.isFunctionCallStatement()}? expressionSequence
    ;

ifStatement
    : If WS* singleExpression WS* flowBlock elseProduction
    ;

flowBlock
    : EOL+ statement
    | block // OTB block, other one is handled with statement
    ;

untilProduction
    : EOL Until s* singleExpression 
    ;

elseProduction
    : EOL Else s* statement 
    | {!this.second(Else)}? // This can be used to reduce the ambiguity in SLL mode, but has a negative effect on performance
    ;

iterationStatement
    : Loop WS* (singleExpression WS*)? flowBlock untilProduction? elseProduction      # LoopStatement
    | LoopFiles WS* singleExpression WS* (',' singleExpression WS*)? flowBlock untilProduction? elseProduction  # LoopFilesStatement
    | LoopRead WS* singleExpression WS* (',' singleExpression WS*)? flowBlock untilProduction? elseProduction  # LoopReadStatement
    | LoopReg WS* singleExpression WS* (',' singleExpression WS*)? flowBlock untilProduction? elseProduction    # LoopRegStatement
    | LoopParse WS* singleExpression WS* (',' (singleExpression WS*)?)* flowBlock untilProduction? elseProduction # LoopParseStatement
    | While WS* singleExpression WS* flowBlock untilProduction? elseProduction      # WhileStatement
    | For WS* forInParameters WS* flowBlock untilProduction? elseProduction          # ForInStatement
    ;

forInParameters
    : assignable? (',' assignable?)* WS* In WS* singleExpression
    | '(' assignable? (',' assignable?)* WS* In WS* singleExpression ')'
    ;

continueStatement
    : Continue WS* (propertyName | '(' propertyName ')')?
    ;

breakStatement
    : Break WS* ('(' propertyName ')' | propertyName)?
    ;

returnStatement
    : Return WS* expression?
    ;

yieldStatement
    : Yield WS* expression?
    ;

switchStatement
    : Switch WS* singleExpression? (',' literal)? s* caseBlock
    ;

caseBlock
    : '{' s* caseClauses? (defaultClause caseClauses?)? '}'
    ;

caseClauses
    : caseClause+
    ;

caseClause
    : Case WS* expressionSequence WS* ':' (s* statementList | EOL)
    ;

defaultClause
    : Default WS* ':' (s* statementList | EOL)
    ;

labelledStatement
    : Identifier ':' 
    ;

gotoStatement
    : Goto WS* propertyName
    | Goto WS* '(' propertyName ')'
    ;

throwStatement
    : Throw WS* singleExpression?
    ;

tryStatement
    : Try s* statement catchProduction* elseProduction finallyProduction?
    ;

catchProduction
    : EOL Catch WS* (catchAssignable WS*)? flowBlock
    ;

catchAssignable
    : catchClasses (WS* As)? (WS* identifier)?
    | '(' catchClasses (WS* As)? (WS* identifier)? ')'
    ;

catchClasses
    : identifier (',' identifier)*
    ;

finallyProduction
    : EOL Finally WS* flowBlock
    ;

functionDeclaration
    : functionHead functionBody
    ;

/*L
hotkeyDeclaration
    : (HotkeyLiteral+) (singleExpression eos | functionBody)
    ;

hotstringDeclaration
    : HotstringLiteral eos
    | HotstringTriggers (singleExpression eos | functionBody)
    ;
*/

classDeclaration
    : Class WS* identifier (WS+ Extends WS+ classExtensionName)? s* classTail
    ;

classExtensionName
    : identifier ('.' identifier)*
    ;

classTail
    : '{' (classElement EOL | EOL)* '}'
    ;

classElement
    : (Static WS*)? methodDefinition   # ClassMethodDeclaration
    | (Static WS*)? propertyDefinition # ClassPropertyDeclaration
    | (Static WS*)? fieldDefinition (',' fieldDefinition)*    # ClassFieldDeclaration
    | classDeclaration           # NestedClassDeclaration
    ;

methodDefinition
    : functionHead functionBody
    ;

propertyDefinition
    : classPropertyName '=>' expression
    | classPropertyName s* '{' (propertyGetterDefinition EOL | propertySetterDefinition EOL | EOL)+ '}'
    ;

classPropertyName
    : identifier
    | identifier '[' formalParameterList? s* ']'
    ;

propertyGetterDefinition
    : Get functionBody
    ;

propertySetterDefinition
    : Set functionBody
    ;

fieldDefinition
    : propertyName ':=' expression
    ;

formalParameterList
    : (formalParameterArg ',')* lastFormalParameterArg
    ;

formalParameterArg
    : BitAnd? identifier (':=' expression | QuestionMark)? // singleLineExpression not needed because is always enclosed in parenthesis
    ;

lastFormalParameterArg
    : formalParameterArg
    | identifier? Multiply
    ;

arrayLiteral
    : '[' (WS | EOL)* (arrayElementList (WS | EOL)*)? ']'
    ;

// Keysharp supports arrays like [,,1,2,,].
arrayElementList
    : (',' arrayElement?)+
    | arrayElement (',' arrayElement?)*
    ;

arrayElement
    : expression Multiply? // Spread
    ;

mapLiteral
    : '[' (WS | EOL)* mapElementList (WS | EOL)* ']'
    ;

// Keysharp supports arrays like [,,1,2,,].
mapElementList
    : ','* mapElement (',' mapElement?)*
    ;

mapElement
    : key = expression ':' value = expression // Map element
    ;

propertyAssignment
    : memberIdentifier (WS | EOL)* ':' (WS | EOL)* expression                           # PropertyExpressionAssignment
    //| (Async WS*)? '*'? propertyName '(' formalParameterList? ')' functionBody # FunctionProperty
    //| getter '(' ')' functionBody                                        # PropertyGetter
    //| setter '(' formalParameterArg ')' functionBody                     # PropertySetter
    //| Ellipsis? singleExpression                                         # PropertyShorthand
    ;

propertyName
    : identifier
    | reservedWord
    | StringLiteral
    | numericLiteral
    ;

dereference
    : DerefStart expression DerefEnd
    ;

arguments
    : argument (',' argument?)*
    | (',' argument?)+
    ;

argument
    : expression
    | primaryExpression (Multiply | QuestionMark)
    ;

expressionSequence
    : expression (',' expression)*
    ;

memberIndexArguments
    : '[' s* arrayElementList? ']'
    ;

// ifStatement and loops require that they don't contain a bodied function expression.
// The only way I could solve this was to duplicate some of the expressions with and without function expressions.
expression
    : left = expression (op = '&&' | op = VerbalAnd) right = expression  # LogicalAndExpression
    | left = expression (op = '||' | op = VerbalOr) right = expression   # LogicalOrExpression
    | <assoc = right> left = expression op = '??' right = expression                               # CoalesceExpression
    | ternCond = expression (WS | EOL)* '?' (WS | EOL)* ternTrue = expression (WS | EOL)* ':' (WS | EOL)* ternFalse = expression # TernaryExpression 
    | fatArrowExpressionHead '=>' expression             # FatArrowExpression // Not sure why, but this needs to be lower than coalesce expression
    | functionExpressionHead (WS | EOL)* block           # FunctionExpression
    | operatorExpression                                 # ExpressionDummy  
    ;

singleExpression
    : left = singleExpression op = ('&&' | VerbalAnd) right = singleExpression     # LogicalAndExpressionDuplicate
    | left = singleExpression op = ('||' | VerbalOr) right = singleExpression      # LogicalOrExpressionDuplicate
    | left = singleExpression op = '??' right = singleExpression                   # CoalesceExpressionDuplicate
    | ternCond = singleExpression (WS | EOL)* '?' (WS | EOL)* ternTrue = singleExpression (WS | EOL)* ':' (WS | EOL)* ternFalse = singleExpression # TernaryExpressionDuplicate
    | operatorExpression                                            # SingleExpressionDummy
    ;

operatorExpression
    : primaryExpression                                                              # OperatorExpressionDummy
    | left = operatorExpression '++'                                                 # PostIncrementExpression
    | left = operatorExpression '--'                                                 # PostDecreaseExpression
    | '++' right = operatorExpression                                                # PreIncrementExpression
    | '--' right = operatorExpression                                                # PreDecreaseExpression
    | <assoc = right> left = operatorExpression '**' right = operatorExpression      # PowerExpression
    | '-' right = operatorExpression                                                 # UnaryMinusExpression
    | '!' WS* right = operatorExpression                                             # NotExpression
    | '+' right = operatorExpression                                                 # UnaryPlusExpression
    | '~' right = operatorExpression                                                 # BitNotExpression
    | left = operatorExpression ((WS | EOL)* op = ('*' | '/' | '//') (WS | EOL)*) right = operatorExpression  # MultiplicativeExpression
    | left = operatorExpression op = ('+' | '-') right = operatorExpression                        # AdditiveExpression
    | left = operatorExpression op = ('<<' | '>>' | '>>>') right = operatorExpression              # BitShiftExpression
    | left = operatorExpression ((WS | EOL)* op = '&' (WS | EOL)*) right = operatorExpression      # BitAndExpression
    | left = operatorExpression op = '^' right = operatorExpression                                # BitXOrExpression
    | left = operatorExpression op = '|' right = operatorExpression                                # BitOrExpression
    | left = operatorExpression (ConcatDot | WS+) right = operatorExpression                       # ConcatenateExpression
    | left = operatorExpression op = '~=' right = operatorExpression                               # RegExMatchExpression
    | left = operatorExpression op = ('<' | '>' | '<=' | '>=') right = operatorExpression          # RelationalExpression
    | left = operatorExpression op = ('=' | '!=' | '==' | '!==') right = operatorExpression        # EqualityExpression
    | left = operatorExpression (s* op = (Instanceof | Is | In | Contains) s*) right = operatorExpression  # ContainExpression
    | VerbalNot WS* right = operatorExpression                                                         # VerbalNotExpression
    | <assoc = right> left = primaryExpression op = assignmentOperator right = expression          # AssignmentExpression
    ;

primaryExpression
    : primaryExpression ('.' | '?.') memberIdentifier                      # MemberDotExpression
    | primaryExpression '(' arguments? ')'                                 # FunctionCallExpression
    | primaryExpression '?.'? memberIndexArguments                         # MemberIndexExpression
    | BitAnd primaryExpression                                             # VarRefExpression
    | identifier                                                           # IdentifierExpression
    | dynamicIdentifier                                                    # DynamicIdentifierExpression
    | literal                                                              # LiteralExpression
    | arrayLiteral                                                         # ArrayLiteralExpression
    | mapLiteral                                                           # MapLiteralExpression
    | objectLiteral                                                        # ObjectLiteralExpression
    | '(' expressionSequence ')'                                           # ParenthesizedExpression
    ;

memberDot
    : (WS | EOL)+ '.'
    | '.' (WS | EOL)*
    | (WS | EOL)* '?.' (WS | EOL)*
    ;

memberIdentifier
    : identifier
    | dynamicIdentifier
    | reservedWord
    | literal
    ;

dynamicIdentifier
    : propertyName dereference (propertyName | dereference)*
    | dereference (propertyName | dereference)*
    ;

initializer
    : ':=' expression
    ;

assignable
    : identifier
    | arrayLiteral
    | objectLiteral
    ;

objectLiteral
    : '{' s* (propertyAssignment (',' propertyAssignment)* s*)? '}'
    ;

functionHead
    : (Async WS*)? identifier '(' formalParameterList? ')'
    ;

functionExpressionHead
    : functionHead
    | (Async WS*)? '(' formalParameterList? ')'
    ;

fatArrowExpressionHead
    : ((Async WS*)? identifier)? Multiply 
    | (Async WS*)? BitAnd? identifier QuestionMark?
    | functionExpressionHead
    ;

functionBody
    : '=>' expression
    | (WS | EOL)* '{' s* statementList? '}'
    ;

assignmentOperator
    : (':='
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
    | '??=')
    ;

literal
    : boolean
    | numericLiteral
    | bigintLiteral
    | (NullLiteral
    | Unset
    | StringLiteral)
    ;

boolean : 
    (True
    | False);

numericLiteral
    : (DecimalLiteral
    | HexIntegerLiteral
    | OctalIntegerLiteral
    | OctalIntegerLiteral2
    | BinaryIntegerLiteral)
    ;

bigintLiteral
    : (BigDecimalIntegerLiteral
    | BigHexIntegerLiteral
    | BigOctalIntegerLiteral
    | BigBinaryIntegerLiteral)
    ;

getter
    : Get propertyName
    ;

setter
    : Set propertyName
    ;

identifierName
    : identifier
    | reservedWord
    ;

identifier
    : (Identifier
    | Default
    | This
//    | Class
    | Enum
    | Extends
    | Super
    | Base
    | From
    | Get
    | Set)
    ;

reservedWord
    : keyword
    | NullLiteral
    | Unset
    | boolean
    ;

keyword
    : (Local
    | Global
    | Static
    | Class
    | If
    | Else
    | Loop
    | For
    | Do
    | While
    | Until
    | Break
    | Continue
    | Goto
    | Return
    | Switch
    | Case
    | Try
    | Catch
    | Finally
    | Throw
    | As
    | VerbalAnd
    | Contains
    | In
    | Is
    | VerbalNot
    | VerbalOr
    | Super
    | Unset
    | Instanceof
    | Import
    | Export
    | Delete
    | Yield  
    | Async
    | Await)
    ;

s
    : WS
    | EOL
    ;

eos
    : EOF
    | EOL
    ;