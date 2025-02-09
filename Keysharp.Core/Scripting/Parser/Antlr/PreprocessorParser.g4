// Eclipse Public License - v 1.0, http://www.eclipse.org/legal/epl-v10.html
// Copyright (c) 2013, Christian Wulf (chwchw@gmx.de)
// Copyright (c) 2016-2017, Ivan Kochurkin (kvanttt@gmail.com), Positive Technologies.

// $antlr-format alignTrailingComments true, columnLimit 150, minEmptyLines 1, maxEmptyLinesToKeep 1, reflowComments false, useTab false
// $antlr-format allowShortRulesOnASingleLine false, allowShortBlocksOnASingleLine true, alignSemicolons hanging, alignColons hanging

parser grammar PreprocessorParser;

options {
    tokenVocab = MainLexer;
    superClass = PreprocessorParserBase;
}

preprocessor_directive
    returns[Boolean value]
    : Define ConditionalSymbol directive_new_line_or_sharp { this.OnPreprocessorDirectiveDefine(); }                        # preprocessorDeclaration
    | Undef ConditionalSymbol directive_new_line_or_sharp { this.OnPreprocessorDirectiveUndef(); }                          # preprocessorDeclaration
    | If expr = preprocessor_expression directive_new_line_or_sharp { this.OnPreprocessorDirectiveIf(); }                    # preprocessorConditional
    | ElIf expr = preprocessor_expression directive_new_line_or_sharp { this.OnPreprocessorDirectiveElif(); }                # preprocessorConditional
    | Else directive_new_line_or_sharp { this.OnPreprocessorDirectiveElse(); }                                               # preprocessorConditional
    | EndIf directive_new_line_or_sharp { this.OnPreprocessorDirectiveEndif(); }                                             # preprocessorConditional
    | Line (Digits StringLiteral? | Default | DirectiveHidden) directive_new_line_or_sharp { this.OnPreprocessorDirectiveLine(); } # preprocessorLine
    | Error Text directive_new_line_or_sharp { this.OnPreprocessorDirectiveError(); }                                        # preprocessorDiagnostic
    | Warning Text directive_new_line_or_sharp { this.OnPreprocessorDirectiveWarning(); }                                    # preprocessorDiagnostic
    | Region Text? directive_new_line_or_sharp { this.OnPreprocessorDirectiveRegion(); }                                     # preprocessorRegion
    | EndRegion Text? directive_new_line_or_sharp { this.OnPreprocessorDirectiveEndregion(); }                               # preprocessorRegion
    | Pragma Text directive_new_line_or_sharp { this.OnPreprocessorDirectivePragma(); }                                      # preprocessorPragma
    | Nullable Text directive_new_line_or_sharp { this.OnPreprocessorDirectiveNullable(); }                                  # preprocessorNullable
    // The following directives are handled in PreReader.cs, because it's easier that way
    | ( Include
      | IncludeAgain
      | DllLoad
      | Requires
      | SingleInstance
      | Assembly ) Text directive_new_line_or_sharp                                                                          # preprocessorTextualDirective
    | Persistent (True | False | Digits)? directive_new_line_or_sharp                                                        # preprocessorPersistent
    | NoDynamicVars directive_new_line_or_sharp                                                                              # preprocessorNoDynamicVars
    | ErrorStdOut directive_new_line_or_sharp                                                                                # preprocessorErrorStdOut
    | ( HotIfTimeout
      | MaxThreads
      | MaxThreadsBuffer
      | MaxThreadsPerHotkey
      | ClipboardTimeout) Digits directive_new_line_or_sharp                                                                 # preprocessorNumericDirective
    ;

directive_new_line_or_sharp
    : DirectiveNewline
    | EOF
    ;

preprocessor_expression
    returns[String value]
    : True { this.OnPreprocessorExpressionTrue(); }
    | False { this.OnPreprocessorExpressionFalse(); }
    | ConditionalSymbol { this.OnPreprocessorExpressionConditionalSymbol(); }
    | OpenParen expr = preprocessor_expression CloseParen { this.OnPreprocessorExpressionConditionalOpenParens(); }
    | Hashtag expr = preprocessor_expression { this.OnPreprocessorExpressionConditionalBang(); }
    | expr1 = preprocessor_expression IdentityEquals expr2 = preprocessor_expression { this.OnPreprocessorExpressionConditionalEq(); }
    | expr1 = preprocessor_expression NotEquals expr2 = preprocessor_expression { this.OnPreprocessorExpressionConditionalNe(); }
    | expr1 = preprocessor_expression And expr2 = preprocessor_expression { this.OnPreprocessorExpressionConditionalAnd(); }
    | expr1 = preprocessor_expression Or expr2 = preprocessor_expression { this.OnPreprocessorExpressionConditionalOr(); }
    ;