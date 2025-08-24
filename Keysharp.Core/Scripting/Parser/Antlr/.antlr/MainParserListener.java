// Generated from c:/Users/minip/source/repos/Keysharp_clone/Keysharp.Core/Scripting/Parser/Antlr/MainParser.g4 by ANTLR 4.13.1
import org.antlr.v4.runtime.tree.ParseTreeListener;

/**
 * This interface defines a complete listener for a parse tree produced by
 * {@link MainParser}.
 */
public interface MainParserListener extends ParseTreeListener {
	/**
	 * Enter a parse tree produced by {@link MainParser#program}.
	 * @param ctx the parse tree
	 */
	void enterProgram(MainParser.ProgramContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#program}.
	 * @param ctx the parse tree
	 */
	void exitProgram(MainParser.ProgramContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#sourceElements}.
	 * @param ctx the parse tree
	 */
	void enterSourceElements(MainParser.SourceElementsContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#sourceElements}.
	 * @param ctx the parse tree
	 */
	void exitSourceElements(MainParser.SourceElementsContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#sourceElement}.
	 * @param ctx the parse tree
	 */
	void enterSourceElement(MainParser.SourceElementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#sourceElement}.
	 * @param ctx the parse tree
	 */
	void exitSourceElement(MainParser.SourceElementContext ctx);
	/**
	 * Enter a parse tree produced by the {@code HotIfDirective}
	 * labeled alternative in {@link MainParser#positionalDirective}.
	 * @param ctx the parse tree
	 */
	void enterHotIfDirective(MainParser.HotIfDirectiveContext ctx);
	/**
	 * Exit a parse tree produced by the {@code HotIfDirective}
	 * labeled alternative in {@link MainParser#positionalDirective}.
	 * @param ctx the parse tree
	 */
	void exitHotIfDirective(MainParser.HotIfDirectiveContext ctx);
	/**
	 * Enter a parse tree produced by the {@code HotstringDirective}
	 * labeled alternative in {@link MainParser#positionalDirective}.
	 * @param ctx the parse tree
	 */
	void enterHotstringDirective(MainParser.HotstringDirectiveContext ctx);
	/**
	 * Exit a parse tree produced by the {@code HotstringDirective}
	 * labeled alternative in {@link MainParser#positionalDirective}.
	 * @param ctx the parse tree
	 */
	void exitHotstringDirective(MainParser.HotstringDirectiveContext ctx);
	/**
	 * Enter a parse tree produced by the {@code InputLevelDirective}
	 * labeled alternative in {@link MainParser#positionalDirective}.
	 * @param ctx the parse tree
	 */
	void enterInputLevelDirective(MainParser.InputLevelDirectiveContext ctx);
	/**
	 * Exit a parse tree produced by the {@code InputLevelDirective}
	 * labeled alternative in {@link MainParser#positionalDirective}.
	 * @param ctx the parse tree
	 */
	void exitInputLevelDirective(MainParser.InputLevelDirectiveContext ctx);
	/**
	 * Enter a parse tree produced by the {@code UseHookDirective}
	 * labeled alternative in {@link MainParser#positionalDirective}.
	 * @param ctx the parse tree
	 */
	void enterUseHookDirective(MainParser.UseHookDirectiveContext ctx);
	/**
	 * Exit a parse tree produced by the {@code UseHookDirective}
	 * labeled alternative in {@link MainParser#positionalDirective}.
	 * @param ctx the parse tree
	 */
	void exitUseHookDirective(MainParser.UseHookDirectiveContext ctx);
	/**
	 * Enter a parse tree produced by the {@code SuspendExemptDirective}
	 * labeled alternative in {@link MainParser#positionalDirective}.
	 * @param ctx the parse tree
	 */
	void enterSuspendExemptDirective(MainParser.SuspendExemptDirectiveContext ctx);
	/**
	 * Exit a parse tree produced by the {@code SuspendExemptDirective}
	 * labeled alternative in {@link MainParser#positionalDirective}.
	 * @param ctx the parse tree
	 */
	void exitSuspendExemptDirective(MainParser.SuspendExemptDirectiveContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#remap}.
	 * @param ctx the parse tree
	 */
	void enterRemap(MainParser.RemapContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#remap}.
	 * @param ctx the parse tree
	 */
	void exitRemap(MainParser.RemapContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#hotstring}.
	 * @param ctx the parse tree
	 */
	void enterHotstring(MainParser.HotstringContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#hotstring}.
	 * @param ctx the parse tree
	 */
	void exitHotstring(MainParser.HotstringContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#hotstringExpansion}.
	 * @param ctx the parse tree
	 */
	void enterHotstringExpansion(MainParser.HotstringExpansionContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#hotstringExpansion}.
	 * @param ctx the parse tree
	 */
	void exitHotstringExpansion(MainParser.HotstringExpansionContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#hotkey}.
	 * @param ctx the parse tree
	 */
	void enterHotkey(MainParser.HotkeyContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#hotkey}.
	 * @param ctx the parse tree
	 */
	void exitHotkey(MainParser.HotkeyContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#statement}.
	 * @param ctx the parse tree
	 */
	void enterStatement(MainParser.StatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#statement}.
	 * @param ctx the parse tree
	 */
	void exitStatement(MainParser.StatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#blockStatement}.
	 * @param ctx the parse tree
	 */
	void enterBlockStatement(MainParser.BlockStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#blockStatement}.
	 * @param ctx the parse tree
	 */
	void exitBlockStatement(MainParser.BlockStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#block}.
	 * @param ctx the parse tree
	 */
	void enterBlock(MainParser.BlockContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#block}.
	 * @param ctx the parse tree
	 */
	void exitBlock(MainParser.BlockContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#statementList}.
	 * @param ctx the parse tree
	 */
	void enterStatementList(MainParser.StatementListContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#statementList}.
	 * @param ctx the parse tree
	 */
	void exitStatementList(MainParser.StatementListContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#variableStatement}.
	 * @param ctx the parse tree
	 */
	void enterVariableStatement(MainParser.VariableStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#variableStatement}.
	 * @param ctx the parse tree
	 */
	void exitVariableStatement(MainParser.VariableStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#awaitStatement}.
	 * @param ctx the parse tree
	 */
	void enterAwaitStatement(MainParser.AwaitStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#awaitStatement}.
	 * @param ctx the parse tree
	 */
	void exitAwaitStatement(MainParser.AwaitStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#deleteStatement}.
	 * @param ctx the parse tree
	 */
	void enterDeleteStatement(MainParser.DeleteStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#deleteStatement}.
	 * @param ctx the parse tree
	 */
	void exitDeleteStatement(MainParser.DeleteStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#importStatement}.
	 * @param ctx the parse tree
	 */
	void enterImportStatement(MainParser.ImportStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#importStatement}.
	 * @param ctx the parse tree
	 */
	void exitImportStatement(MainParser.ImportStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#importFromBlock}.
	 * @param ctx the parse tree
	 */
	void enterImportFromBlock(MainParser.ImportFromBlockContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#importFromBlock}.
	 * @param ctx the parse tree
	 */
	void exitImportFromBlock(MainParser.ImportFromBlockContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#importModuleItems}.
	 * @param ctx the parse tree
	 */
	void enterImportModuleItems(MainParser.ImportModuleItemsContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#importModuleItems}.
	 * @param ctx the parse tree
	 */
	void exitImportModuleItems(MainParser.ImportModuleItemsContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#importAliasName}.
	 * @param ctx the parse tree
	 */
	void enterImportAliasName(MainParser.ImportAliasNameContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#importAliasName}.
	 * @param ctx the parse tree
	 */
	void exitImportAliasName(MainParser.ImportAliasNameContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#moduleExportName}.
	 * @param ctx the parse tree
	 */
	void enterModuleExportName(MainParser.ModuleExportNameContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#moduleExportName}.
	 * @param ctx the parse tree
	 */
	void exitModuleExportName(MainParser.ModuleExportNameContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#importedBinding}.
	 * @param ctx the parse tree
	 */
	void enterImportedBinding(MainParser.ImportedBindingContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#importedBinding}.
	 * @param ctx the parse tree
	 */
	void exitImportedBinding(MainParser.ImportedBindingContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#importDefault}.
	 * @param ctx the parse tree
	 */
	void enterImportDefault(MainParser.ImportDefaultContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#importDefault}.
	 * @param ctx the parse tree
	 */
	void exitImportDefault(MainParser.ImportDefaultContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#importNamespace}.
	 * @param ctx the parse tree
	 */
	void enterImportNamespace(MainParser.ImportNamespaceContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#importNamespace}.
	 * @param ctx the parse tree
	 */
	void exitImportNamespace(MainParser.ImportNamespaceContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#importFrom}.
	 * @param ctx the parse tree
	 */
	void enterImportFrom(MainParser.ImportFromContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#importFrom}.
	 * @param ctx the parse tree
	 */
	void exitImportFrom(MainParser.ImportFromContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#aliasName}.
	 * @param ctx the parse tree
	 */
	void enterAliasName(MainParser.AliasNameContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#aliasName}.
	 * @param ctx the parse tree
	 */
	void exitAliasName(MainParser.AliasNameContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ExportDeclaration}
	 * labeled alternative in {@link MainParser#exportStatement}.
	 * @param ctx the parse tree
	 */
	void enterExportDeclaration(MainParser.ExportDeclarationContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ExportDeclaration}
	 * labeled alternative in {@link MainParser#exportStatement}.
	 * @param ctx the parse tree
	 */
	void exitExportDeclaration(MainParser.ExportDeclarationContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ExportDefaultDeclaration}
	 * labeled alternative in {@link MainParser#exportStatement}.
	 * @param ctx the parse tree
	 */
	void enterExportDefaultDeclaration(MainParser.ExportDefaultDeclarationContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ExportDefaultDeclaration}
	 * labeled alternative in {@link MainParser#exportStatement}.
	 * @param ctx the parse tree
	 */
	void exitExportDefaultDeclaration(MainParser.ExportDefaultDeclarationContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#exportFromBlock}.
	 * @param ctx the parse tree
	 */
	void enterExportFromBlock(MainParser.ExportFromBlockContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#exportFromBlock}.
	 * @param ctx the parse tree
	 */
	void exitExportFromBlock(MainParser.ExportFromBlockContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#exportModuleItems}.
	 * @param ctx the parse tree
	 */
	void enterExportModuleItems(MainParser.ExportModuleItemsContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#exportModuleItems}.
	 * @param ctx the parse tree
	 */
	void exitExportModuleItems(MainParser.ExportModuleItemsContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#exportAliasName}.
	 * @param ctx the parse tree
	 */
	void enterExportAliasName(MainParser.ExportAliasNameContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#exportAliasName}.
	 * @param ctx the parse tree
	 */
	void exitExportAliasName(MainParser.ExportAliasNameContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#declaration}.
	 * @param ctx the parse tree
	 */
	void enterDeclaration(MainParser.DeclarationContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#declaration}.
	 * @param ctx the parse tree
	 */
	void exitDeclaration(MainParser.DeclarationContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#variableDeclarationList}.
	 * @param ctx the parse tree
	 */
	void enterVariableDeclarationList(MainParser.VariableDeclarationListContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#variableDeclarationList}.
	 * @param ctx the parse tree
	 */
	void exitVariableDeclarationList(MainParser.VariableDeclarationListContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#variableDeclaration}.
	 * @param ctx the parse tree
	 */
	void enterVariableDeclaration(MainParser.VariableDeclarationContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#variableDeclaration}.
	 * @param ctx the parse tree
	 */
	void exitVariableDeclaration(MainParser.VariableDeclarationContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#functionStatement}.
	 * @param ctx the parse tree
	 */
	void enterFunctionStatement(MainParser.FunctionStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#functionStatement}.
	 * @param ctx the parse tree
	 */
	void exitFunctionStatement(MainParser.FunctionStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#expressionStatement}.
	 * @param ctx the parse tree
	 */
	void enterExpressionStatement(MainParser.ExpressionStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#expressionStatement}.
	 * @param ctx the parse tree
	 */
	void exitExpressionStatement(MainParser.ExpressionStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#ifStatement}.
	 * @param ctx the parse tree
	 */
	void enterIfStatement(MainParser.IfStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#ifStatement}.
	 * @param ctx the parse tree
	 */
	void exitIfStatement(MainParser.IfStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#flowBlock}.
	 * @param ctx the parse tree
	 */
	void enterFlowBlock(MainParser.FlowBlockContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#flowBlock}.
	 * @param ctx the parse tree
	 */
	void exitFlowBlock(MainParser.FlowBlockContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#untilProduction}.
	 * @param ctx the parse tree
	 */
	void enterUntilProduction(MainParser.UntilProductionContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#untilProduction}.
	 * @param ctx the parse tree
	 */
	void exitUntilProduction(MainParser.UntilProductionContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#elseProduction}.
	 * @param ctx the parse tree
	 */
	void enterElseProduction(MainParser.ElseProductionContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#elseProduction}.
	 * @param ctx the parse tree
	 */
	void exitElseProduction(MainParser.ElseProductionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code SpecializedLoopStatement}
	 * labeled alternative in {@link MainParser#iterationStatement}.
	 * @param ctx the parse tree
	 */
	void enterSpecializedLoopStatement(MainParser.SpecializedLoopStatementContext ctx);
	/**
	 * Exit a parse tree produced by the {@code SpecializedLoopStatement}
	 * labeled alternative in {@link MainParser#iterationStatement}.
	 * @param ctx the parse tree
	 */
	void exitSpecializedLoopStatement(MainParser.SpecializedLoopStatementContext ctx);
	/**
	 * Enter a parse tree produced by the {@code LoopStatement}
	 * labeled alternative in {@link MainParser#iterationStatement}.
	 * @param ctx the parse tree
	 */
	void enterLoopStatement(MainParser.LoopStatementContext ctx);
	/**
	 * Exit a parse tree produced by the {@code LoopStatement}
	 * labeled alternative in {@link MainParser#iterationStatement}.
	 * @param ctx the parse tree
	 */
	void exitLoopStatement(MainParser.LoopStatementContext ctx);
	/**
	 * Enter a parse tree produced by the {@code WhileStatement}
	 * labeled alternative in {@link MainParser#iterationStatement}.
	 * @param ctx the parse tree
	 */
	void enterWhileStatement(MainParser.WhileStatementContext ctx);
	/**
	 * Exit a parse tree produced by the {@code WhileStatement}
	 * labeled alternative in {@link MainParser#iterationStatement}.
	 * @param ctx the parse tree
	 */
	void exitWhileStatement(MainParser.WhileStatementContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ForInStatement}
	 * labeled alternative in {@link MainParser#iterationStatement}.
	 * @param ctx the parse tree
	 */
	void enterForInStatement(MainParser.ForInStatementContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ForInStatement}
	 * labeled alternative in {@link MainParser#iterationStatement}.
	 * @param ctx the parse tree
	 */
	void exitForInStatement(MainParser.ForInStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#forInParameters}.
	 * @param ctx the parse tree
	 */
	void enterForInParameters(MainParser.ForInParametersContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#forInParameters}.
	 * @param ctx the parse tree
	 */
	void exitForInParameters(MainParser.ForInParametersContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#continueStatement}.
	 * @param ctx the parse tree
	 */
	void enterContinueStatement(MainParser.ContinueStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#continueStatement}.
	 * @param ctx the parse tree
	 */
	void exitContinueStatement(MainParser.ContinueStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#breakStatement}.
	 * @param ctx the parse tree
	 */
	void enterBreakStatement(MainParser.BreakStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#breakStatement}.
	 * @param ctx the parse tree
	 */
	void exitBreakStatement(MainParser.BreakStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#returnStatement}.
	 * @param ctx the parse tree
	 */
	void enterReturnStatement(MainParser.ReturnStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#returnStatement}.
	 * @param ctx the parse tree
	 */
	void exitReturnStatement(MainParser.ReturnStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#yieldStatement}.
	 * @param ctx the parse tree
	 */
	void enterYieldStatement(MainParser.YieldStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#yieldStatement}.
	 * @param ctx the parse tree
	 */
	void exitYieldStatement(MainParser.YieldStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#switchStatement}.
	 * @param ctx the parse tree
	 */
	void enterSwitchStatement(MainParser.SwitchStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#switchStatement}.
	 * @param ctx the parse tree
	 */
	void exitSwitchStatement(MainParser.SwitchStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#caseBlock}.
	 * @param ctx the parse tree
	 */
	void enterCaseBlock(MainParser.CaseBlockContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#caseBlock}.
	 * @param ctx the parse tree
	 */
	void exitCaseBlock(MainParser.CaseBlockContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#caseClause}.
	 * @param ctx the parse tree
	 */
	void enterCaseClause(MainParser.CaseClauseContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#caseClause}.
	 * @param ctx the parse tree
	 */
	void exitCaseClause(MainParser.CaseClauseContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#labelledStatement}.
	 * @param ctx the parse tree
	 */
	void enterLabelledStatement(MainParser.LabelledStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#labelledStatement}.
	 * @param ctx the parse tree
	 */
	void exitLabelledStatement(MainParser.LabelledStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#gotoStatement}.
	 * @param ctx the parse tree
	 */
	void enterGotoStatement(MainParser.GotoStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#gotoStatement}.
	 * @param ctx the parse tree
	 */
	void exitGotoStatement(MainParser.GotoStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#throwStatement}.
	 * @param ctx the parse tree
	 */
	void enterThrowStatement(MainParser.ThrowStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#throwStatement}.
	 * @param ctx the parse tree
	 */
	void exitThrowStatement(MainParser.ThrowStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#tryStatement}.
	 * @param ctx the parse tree
	 */
	void enterTryStatement(MainParser.TryStatementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#tryStatement}.
	 * @param ctx the parse tree
	 */
	void exitTryStatement(MainParser.TryStatementContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#catchProduction}.
	 * @param ctx the parse tree
	 */
	void enterCatchProduction(MainParser.CatchProductionContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#catchProduction}.
	 * @param ctx the parse tree
	 */
	void exitCatchProduction(MainParser.CatchProductionContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#catchAssignable}.
	 * @param ctx the parse tree
	 */
	void enterCatchAssignable(MainParser.CatchAssignableContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#catchAssignable}.
	 * @param ctx the parse tree
	 */
	void exitCatchAssignable(MainParser.CatchAssignableContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#catchClasses}.
	 * @param ctx the parse tree
	 */
	void enterCatchClasses(MainParser.CatchClassesContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#catchClasses}.
	 * @param ctx the parse tree
	 */
	void exitCatchClasses(MainParser.CatchClassesContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#finallyProduction}.
	 * @param ctx the parse tree
	 */
	void enterFinallyProduction(MainParser.FinallyProductionContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#finallyProduction}.
	 * @param ctx the parse tree
	 */
	void exitFinallyProduction(MainParser.FinallyProductionContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#functionDeclaration}.
	 * @param ctx the parse tree
	 */
	void enterFunctionDeclaration(MainParser.FunctionDeclarationContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#functionDeclaration}.
	 * @param ctx the parse tree
	 */
	void exitFunctionDeclaration(MainParser.FunctionDeclarationContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#classDeclaration}.
	 * @param ctx the parse tree
	 */
	void enterClassDeclaration(MainParser.ClassDeclarationContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#classDeclaration}.
	 * @param ctx the parse tree
	 */
	void exitClassDeclaration(MainParser.ClassDeclarationContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#classExtensionName}.
	 * @param ctx the parse tree
	 */
	void enterClassExtensionName(MainParser.ClassExtensionNameContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#classExtensionName}.
	 * @param ctx the parse tree
	 */
	void exitClassExtensionName(MainParser.ClassExtensionNameContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#classTail}.
	 * @param ctx the parse tree
	 */
	void enterClassTail(MainParser.ClassTailContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#classTail}.
	 * @param ctx the parse tree
	 */
	void exitClassTail(MainParser.ClassTailContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ClassMethodDeclaration}
	 * labeled alternative in {@link MainParser#classElement}.
	 * @param ctx the parse tree
	 */
	void enterClassMethodDeclaration(MainParser.ClassMethodDeclarationContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ClassMethodDeclaration}
	 * labeled alternative in {@link MainParser#classElement}.
	 * @param ctx the parse tree
	 */
	void exitClassMethodDeclaration(MainParser.ClassMethodDeclarationContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ClassPropertyDeclaration}
	 * labeled alternative in {@link MainParser#classElement}.
	 * @param ctx the parse tree
	 */
	void enterClassPropertyDeclaration(MainParser.ClassPropertyDeclarationContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ClassPropertyDeclaration}
	 * labeled alternative in {@link MainParser#classElement}.
	 * @param ctx the parse tree
	 */
	void exitClassPropertyDeclaration(MainParser.ClassPropertyDeclarationContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ClassFieldDeclaration}
	 * labeled alternative in {@link MainParser#classElement}.
	 * @param ctx the parse tree
	 */
	void enterClassFieldDeclaration(MainParser.ClassFieldDeclarationContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ClassFieldDeclaration}
	 * labeled alternative in {@link MainParser#classElement}.
	 * @param ctx the parse tree
	 */
	void exitClassFieldDeclaration(MainParser.ClassFieldDeclarationContext ctx);
	/**
	 * Enter a parse tree produced by the {@code NestedClassDeclaration}
	 * labeled alternative in {@link MainParser#classElement}.
	 * @param ctx the parse tree
	 */
	void enterNestedClassDeclaration(MainParser.NestedClassDeclarationContext ctx);
	/**
	 * Exit a parse tree produced by the {@code NestedClassDeclaration}
	 * labeled alternative in {@link MainParser#classElement}.
	 * @param ctx the parse tree
	 */
	void exitNestedClassDeclaration(MainParser.NestedClassDeclarationContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#methodDefinition}.
	 * @param ctx the parse tree
	 */
	void enterMethodDefinition(MainParser.MethodDefinitionContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#methodDefinition}.
	 * @param ctx the parse tree
	 */
	void exitMethodDefinition(MainParser.MethodDefinitionContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#propertyDefinition}.
	 * @param ctx the parse tree
	 */
	void enterPropertyDefinition(MainParser.PropertyDefinitionContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#propertyDefinition}.
	 * @param ctx the parse tree
	 */
	void exitPropertyDefinition(MainParser.PropertyDefinitionContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#classPropertyName}.
	 * @param ctx the parse tree
	 */
	void enterClassPropertyName(MainParser.ClassPropertyNameContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#classPropertyName}.
	 * @param ctx the parse tree
	 */
	void exitClassPropertyName(MainParser.ClassPropertyNameContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#propertyGetterDefinition}.
	 * @param ctx the parse tree
	 */
	void enterPropertyGetterDefinition(MainParser.PropertyGetterDefinitionContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#propertyGetterDefinition}.
	 * @param ctx the parse tree
	 */
	void exitPropertyGetterDefinition(MainParser.PropertyGetterDefinitionContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#propertySetterDefinition}.
	 * @param ctx the parse tree
	 */
	void enterPropertySetterDefinition(MainParser.PropertySetterDefinitionContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#propertySetterDefinition}.
	 * @param ctx the parse tree
	 */
	void exitPropertySetterDefinition(MainParser.PropertySetterDefinitionContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#fieldDefinition}.
	 * @param ctx the parse tree
	 */
	void enterFieldDefinition(MainParser.FieldDefinitionContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#fieldDefinition}.
	 * @param ctx the parse tree
	 */
	void exitFieldDefinition(MainParser.FieldDefinitionContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#formalParameterList}.
	 * @param ctx the parse tree
	 */
	void enterFormalParameterList(MainParser.FormalParameterListContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#formalParameterList}.
	 * @param ctx the parse tree
	 */
	void exitFormalParameterList(MainParser.FormalParameterListContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#formalParameterArg}.
	 * @param ctx the parse tree
	 */
	void enterFormalParameterArg(MainParser.FormalParameterArgContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#formalParameterArg}.
	 * @param ctx the parse tree
	 */
	void exitFormalParameterArg(MainParser.FormalParameterArgContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#lastFormalParameterArg}.
	 * @param ctx the parse tree
	 */
	void enterLastFormalParameterArg(MainParser.LastFormalParameterArgContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#lastFormalParameterArg}.
	 * @param ctx the parse tree
	 */
	void exitLastFormalParameterArg(MainParser.LastFormalParameterArgContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#arrayLiteral}.
	 * @param ctx the parse tree
	 */
	void enterArrayLiteral(MainParser.ArrayLiteralContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#arrayLiteral}.
	 * @param ctx the parse tree
	 */
	void exitArrayLiteral(MainParser.ArrayLiteralContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#mapLiteral}.
	 * @param ctx the parse tree
	 */
	void enterMapLiteral(MainParser.MapLiteralContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#mapLiteral}.
	 * @param ctx the parse tree
	 */
	void exitMapLiteral(MainParser.MapLiteralContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#mapElementList}.
	 * @param ctx the parse tree
	 */
	void enterMapElementList(MainParser.MapElementListContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#mapElementList}.
	 * @param ctx the parse tree
	 */
	void exitMapElementList(MainParser.MapElementListContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#mapElement}.
	 * @param ctx the parse tree
	 */
	void enterMapElement(MainParser.MapElementContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#mapElement}.
	 * @param ctx the parse tree
	 */
	void exitMapElement(MainParser.MapElementContext ctx);
	/**
	 * Enter a parse tree produced by the {@code PropertyExpressionAssignment}
	 * labeled alternative in {@link MainParser#propertyAssignment}.
	 * @param ctx the parse tree
	 */
	void enterPropertyExpressionAssignment(MainParser.PropertyExpressionAssignmentContext ctx);
	/**
	 * Exit a parse tree produced by the {@code PropertyExpressionAssignment}
	 * labeled alternative in {@link MainParser#propertyAssignment}.
	 * @param ctx the parse tree
	 */
	void exitPropertyExpressionAssignment(MainParser.PropertyExpressionAssignmentContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#propertyName}.
	 * @param ctx the parse tree
	 */
	void enterPropertyName(MainParser.PropertyNameContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#propertyName}.
	 * @param ctx the parse tree
	 */
	void exitPropertyName(MainParser.PropertyNameContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#dereference}.
	 * @param ctx the parse tree
	 */
	void enterDereference(MainParser.DereferenceContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#dereference}.
	 * @param ctx the parse tree
	 */
	void exitDereference(MainParser.DereferenceContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#arguments}.
	 * @param ctx the parse tree
	 */
	void enterArguments(MainParser.ArgumentsContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#arguments}.
	 * @param ctx the parse tree
	 */
	void exitArguments(MainParser.ArgumentsContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#argument}.
	 * @param ctx the parse tree
	 */
	void enterArgument(MainParser.ArgumentContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#argument}.
	 * @param ctx the parse tree
	 */
	void exitArgument(MainParser.ArgumentContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#expressionSequence}.
	 * @param ctx the parse tree
	 */
	void enterExpressionSequence(MainParser.ExpressionSequenceContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#expressionSequence}.
	 * @param ctx the parse tree
	 */
	void exitExpressionSequence(MainParser.ExpressionSequenceContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#memberIndexArguments}.
	 * @param ctx the parse tree
	 */
	void enterMemberIndexArguments(MainParser.MemberIndexArgumentsContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#memberIndexArguments}.
	 * @param ctx the parse tree
	 */
	void exitMemberIndexArguments(MainParser.MemberIndexArgumentsContext ctx);
	/**
	 * Enter a parse tree produced by the {@code PostIncrementDecrementExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterPostIncrementDecrementExpression(MainParser.PostIncrementDecrementExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code PostIncrementDecrementExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitPostIncrementDecrementExpression(MainParser.PostIncrementDecrementExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code AdditiveExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterAdditiveExpression(MainParser.AdditiveExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code AdditiveExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitAdditiveExpression(MainParser.AdditiveExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code RelationalExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterRelationalExpression(MainParser.RelationalExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code RelationalExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitRelationalExpression(MainParser.RelationalExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code TernaryExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterTernaryExpression(MainParser.TernaryExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code TernaryExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitTernaryExpression(MainParser.TernaryExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code PreIncrementDecrementExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterPreIncrementDecrementExpression(MainParser.PreIncrementDecrementExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code PreIncrementDecrementExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitPreIncrementDecrementExpression(MainParser.PreIncrementDecrementExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code LogicalAndExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterLogicalAndExpression(MainParser.LogicalAndExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code LogicalAndExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitLogicalAndExpression(MainParser.LogicalAndExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code PowerExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterPowerExpression(MainParser.PowerExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code PowerExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitPowerExpression(MainParser.PowerExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ContainExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterContainExpression(MainParser.ContainExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ContainExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitContainExpression(MainParser.ContainExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code FatArrowExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterFatArrowExpression(MainParser.FatArrowExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code FatArrowExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitFatArrowExpression(MainParser.FatArrowExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code LogicalOrExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterLogicalOrExpression(MainParser.LogicalOrExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code LogicalOrExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitLogicalOrExpression(MainParser.LogicalOrExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ExpressionDummy}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterExpressionDummy(MainParser.ExpressionDummyContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ExpressionDummy}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitExpressionDummy(MainParser.ExpressionDummyContext ctx);
	/**
	 * Enter a parse tree produced by the {@code UnaryExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterUnaryExpression(MainParser.UnaryExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code UnaryExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitUnaryExpression(MainParser.UnaryExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code RegExMatchExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterRegExMatchExpression(MainParser.RegExMatchExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code RegExMatchExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitRegExMatchExpression(MainParser.RegExMatchExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code FunctionExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterFunctionExpression(MainParser.FunctionExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code FunctionExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitFunctionExpression(MainParser.FunctionExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code AssignmentExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterAssignmentExpression(MainParser.AssignmentExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code AssignmentExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitAssignmentExpression(MainParser.AssignmentExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code BitAndExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterBitAndExpression(MainParser.BitAndExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code BitAndExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitBitAndExpression(MainParser.BitAndExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code BitOrExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterBitOrExpression(MainParser.BitOrExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code BitOrExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitBitOrExpression(MainParser.BitOrExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ConcatenateExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterConcatenateExpression(MainParser.ConcatenateExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ConcatenateExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitConcatenateExpression(MainParser.ConcatenateExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code BitXOrExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterBitXOrExpression(MainParser.BitXOrExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code BitXOrExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitBitXOrExpression(MainParser.BitXOrExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code EqualityExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterEqualityExpression(MainParser.EqualityExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code EqualityExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitEqualityExpression(MainParser.EqualityExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code VerbalNotExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterVerbalNotExpression(MainParser.VerbalNotExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code VerbalNotExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitVerbalNotExpression(MainParser.VerbalNotExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code MultiplicativeExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterMultiplicativeExpression(MainParser.MultiplicativeExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code MultiplicativeExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitMultiplicativeExpression(MainParser.MultiplicativeExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code CoalesceExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterCoalesceExpression(MainParser.CoalesceExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code CoalesceExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitCoalesceExpression(MainParser.CoalesceExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code BitShiftExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void enterBitShiftExpression(MainParser.BitShiftExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code BitShiftExpression}
	 * labeled alternative in {@link MainParser#expression}.
	 * @param ctx the parse tree
	 */
	void exitBitShiftExpression(MainParser.BitShiftExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code BitShiftExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterBitShiftExpressionDuplicate(MainParser.BitShiftExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code BitShiftExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitBitShiftExpressionDuplicate(MainParser.BitShiftExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code UnaryExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterUnaryExpressionDuplicate(MainParser.UnaryExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code UnaryExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitUnaryExpressionDuplicate(MainParser.UnaryExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code PostIncrementDecrementExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterPostIncrementDecrementExpressionDuplicate(MainParser.PostIncrementDecrementExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code PostIncrementDecrementExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitPostIncrementDecrementExpressionDuplicate(MainParser.PostIncrementDecrementExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code PreIncrementDecrementExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterPreIncrementDecrementExpressionDuplicate(MainParser.PreIncrementDecrementExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code PreIncrementDecrementExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitPreIncrementDecrementExpressionDuplicate(MainParser.PreIncrementDecrementExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code BitOrExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterBitOrExpressionDuplicate(MainParser.BitOrExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code BitOrExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitBitOrExpressionDuplicate(MainParser.BitOrExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code RegExMatchExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterRegExMatchExpressionDuplicate(MainParser.RegExMatchExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code RegExMatchExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitRegExMatchExpressionDuplicate(MainParser.RegExMatchExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code VerbalNotExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterVerbalNotExpressionDuplicate(MainParser.VerbalNotExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code VerbalNotExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitVerbalNotExpressionDuplicate(MainParser.VerbalNotExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code SingleExpressionDummy}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterSingleExpressionDummy(MainParser.SingleExpressionDummyContext ctx);
	/**
	 * Exit a parse tree produced by the {@code SingleExpressionDummy}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitSingleExpressionDummy(MainParser.SingleExpressionDummyContext ctx);
	/**
	 * Enter a parse tree produced by the {@code TernaryExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterTernaryExpressionDuplicate(MainParser.TernaryExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code TernaryExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitTernaryExpressionDuplicate(MainParser.TernaryExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code BitAndExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterBitAndExpressionDuplicate(MainParser.BitAndExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code BitAndExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitBitAndExpressionDuplicate(MainParser.BitAndExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ContainExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterContainExpressionDuplicate(MainParser.ContainExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ContainExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitContainExpressionDuplicate(MainParser.ContainExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code MultiplicativeExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterMultiplicativeExpressionDuplicate(MainParser.MultiplicativeExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code MultiplicativeExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitMultiplicativeExpressionDuplicate(MainParser.MultiplicativeExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code PowerExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterPowerExpressionDuplicate(MainParser.PowerExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code PowerExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitPowerExpressionDuplicate(MainParser.PowerExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code RelationalExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterRelationalExpressionDuplicate(MainParser.RelationalExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code RelationalExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitRelationalExpressionDuplicate(MainParser.RelationalExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code AdditiveExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterAdditiveExpressionDuplicate(MainParser.AdditiveExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code AdditiveExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitAdditiveExpressionDuplicate(MainParser.AdditiveExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code LogicalOrExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterLogicalOrExpressionDuplicate(MainParser.LogicalOrExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code LogicalOrExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitLogicalOrExpressionDuplicate(MainParser.LogicalOrExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code AssignmentExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterAssignmentExpressionDuplicate(MainParser.AssignmentExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code AssignmentExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitAssignmentExpressionDuplicate(MainParser.AssignmentExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code EqualityExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterEqualityExpressionDuplicate(MainParser.EqualityExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code EqualityExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitEqualityExpressionDuplicate(MainParser.EqualityExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ConcatenateExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterConcatenateExpressionDuplicate(MainParser.ConcatenateExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ConcatenateExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitConcatenateExpressionDuplicate(MainParser.ConcatenateExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code LogicalAndExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterLogicalAndExpressionDuplicate(MainParser.LogicalAndExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code LogicalAndExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitLogicalAndExpressionDuplicate(MainParser.LogicalAndExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code CoalesceExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterCoalesceExpressionDuplicate(MainParser.CoalesceExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code CoalesceExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitCoalesceExpressionDuplicate(MainParser.CoalesceExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code BitXOrExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void enterBitXOrExpressionDuplicate(MainParser.BitXOrExpressionDuplicateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code BitXOrExpressionDuplicate}
	 * labeled alternative in {@link MainParser#singleExpression}.
	 * @param ctx the parse tree
	 */
	void exitBitXOrExpressionDuplicate(MainParser.BitXOrExpressionDuplicateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ParenthesizedExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void enterParenthesizedExpression(MainParser.ParenthesizedExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ParenthesizedExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void exitParenthesizedExpression(MainParser.ParenthesizedExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code MapLiteralExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void enterMapLiteralExpression(MainParser.MapLiteralExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code MapLiteralExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void exitMapLiteralExpression(MainParser.MapLiteralExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ObjectLiteralExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void enterObjectLiteralExpression(MainParser.ObjectLiteralExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ObjectLiteralExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void exitObjectLiteralExpression(MainParser.ObjectLiteralExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code VarRefExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void enterVarRefExpression(MainParser.VarRefExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code VarRefExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void exitVarRefExpression(MainParser.VarRefExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code DynamicIdentifierExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void enterDynamicIdentifierExpression(MainParser.DynamicIdentifierExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code DynamicIdentifierExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void exitDynamicIdentifierExpression(MainParser.DynamicIdentifierExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code LiteralExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void enterLiteralExpression(MainParser.LiteralExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code LiteralExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void exitLiteralExpression(MainParser.LiteralExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ArrayLiteralExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void enterArrayLiteralExpression(MainParser.ArrayLiteralExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ArrayLiteralExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void exitArrayLiteralExpression(MainParser.ArrayLiteralExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code AccessExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void enterAccessExpression(MainParser.AccessExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code AccessExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void exitAccessExpression(MainParser.AccessExpressionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code IdentifierExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void enterIdentifierExpression(MainParser.IdentifierExpressionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code IdentifierExpression}
	 * labeled alternative in {@link MainParser#primaryExpression}.
	 * @param ctx the parse tree
	 */
	void exitIdentifierExpression(MainParser.IdentifierExpressionContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#accessSuffix}.
	 * @param ctx the parse tree
	 */
	void enterAccessSuffix(MainParser.AccessSuffixContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#accessSuffix}.
	 * @param ctx the parse tree
	 */
	void exitAccessSuffix(MainParser.AccessSuffixContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#memberDot}.
	 * @param ctx the parse tree
	 */
	void enterMemberDot(MainParser.MemberDotContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#memberDot}.
	 * @param ctx the parse tree
	 */
	void exitMemberDot(MainParser.MemberDotContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#memberIdentifier}.
	 * @param ctx the parse tree
	 */
	void enterMemberIdentifier(MainParser.MemberIdentifierContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#memberIdentifier}.
	 * @param ctx the parse tree
	 */
	void exitMemberIdentifier(MainParser.MemberIdentifierContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#dynamicIdentifier}.
	 * @param ctx the parse tree
	 */
	void enterDynamicIdentifier(MainParser.DynamicIdentifierContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#dynamicIdentifier}.
	 * @param ctx the parse tree
	 */
	void exitDynamicIdentifier(MainParser.DynamicIdentifierContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#initializer}.
	 * @param ctx the parse tree
	 */
	void enterInitializer(MainParser.InitializerContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#initializer}.
	 * @param ctx the parse tree
	 */
	void exitInitializer(MainParser.InitializerContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#assignable}.
	 * @param ctx the parse tree
	 */
	void enterAssignable(MainParser.AssignableContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#assignable}.
	 * @param ctx the parse tree
	 */
	void exitAssignable(MainParser.AssignableContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#objectLiteral}.
	 * @param ctx the parse tree
	 */
	void enterObjectLiteral(MainParser.ObjectLiteralContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#objectLiteral}.
	 * @param ctx the parse tree
	 */
	void exitObjectLiteral(MainParser.ObjectLiteralContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#functionHead}.
	 * @param ctx the parse tree
	 */
	void enterFunctionHead(MainParser.FunctionHeadContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#functionHead}.
	 * @param ctx the parse tree
	 */
	void exitFunctionHead(MainParser.FunctionHeadContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#functionHeadPrefix}.
	 * @param ctx the parse tree
	 */
	void enterFunctionHeadPrefix(MainParser.FunctionHeadPrefixContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#functionHeadPrefix}.
	 * @param ctx the parse tree
	 */
	void exitFunctionHeadPrefix(MainParser.FunctionHeadPrefixContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#functionExpressionHead}.
	 * @param ctx the parse tree
	 */
	void enterFunctionExpressionHead(MainParser.FunctionExpressionHeadContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#functionExpressionHead}.
	 * @param ctx the parse tree
	 */
	void exitFunctionExpressionHead(MainParser.FunctionExpressionHeadContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#fatArrowExpressionHead}.
	 * @param ctx the parse tree
	 */
	void enterFatArrowExpressionHead(MainParser.FatArrowExpressionHeadContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#fatArrowExpressionHead}.
	 * @param ctx the parse tree
	 */
	void exitFatArrowExpressionHead(MainParser.FatArrowExpressionHeadContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#functionBody}.
	 * @param ctx the parse tree
	 */
	void enterFunctionBody(MainParser.FunctionBodyContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#functionBody}.
	 * @param ctx the parse tree
	 */
	void exitFunctionBody(MainParser.FunctionBodyContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#assignmentOperator}.
	 * @param ctx the parse tree
	 */
	void enterAssignmentOperator(MainParser.AssignmentOperatorContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#assignmentOperator}.
	 * @param ctx the parse tree
	 */
	void exitAssignmentOperator(MainParser.AssignmentOperatorContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#literal}.
	 * @param ctx the parse tree
	 */
	void enterLiteral(MainParser.LiteralContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#literal}.
	 * @param ctx the parse tree
	 */
	void exitLiteral(MainParser.LiteralContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#boolean}.
	 * @param ctx the parse tree
	 */
	void enterBoolean(MainParser.BooleanContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#boolean}.
	 * @param ctx the parse tree
	 */
	void exitBoolean(MainParser.BooleanContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#numericLiteral}.
	 * @param ctx the parse tree
	 */
	void enterNumericLiteral(MainParser.NumericLiteralContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#numericLiteral}.
	 * @param ctx the parse tree
	 */
	void exitNumericLiteral(MainParser.NumericLiteralContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#bigintLiteral}.
	 * @param ctx the parse tree
	 */
	void enterBigintLiteral(MainParser.BigintLiteralContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#bigintLiteral}.
	 * @param ctx the parse tree
	 */
	void exitBigintLiteral(MainParser.BigintLiteralContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#getter}.
	 * @param ctx the parse tree
	 */
	void enterGetter(MainParser.GetterContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#getter}.
	 * @param ctx the parse tree
	 */
	void exitGetter(MainParser.GetterContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#setter}.
	 * @param ctx the parse tree
	 */
	void enterSetter(MainParser.SetterContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#setter}.
	 * @param ctx the parse tree
	 */
	void exitSetter(MainParser.SetterContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#identifierName}.
	 * @param ctx the parse tree
	 */
	void enterIdentifierName(MainParser.IdentifierNameContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#identifierName}.
	 * @param ctx the parse tree
	 */
	void exitIdentifierName(MainParser.IdentifierNameContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#identifier}.
	 * @param ctx the parse tree
	 */
	void enterIdentifier(MainParser.IdentifierContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#identifier}.
	 * @param ctx the parse tree
	 */
	void exitIdentifier(MainParser.IdentifierContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#reservedWord}.
	 * @param ctx the parse tree
	 */
	void enterReservedWord(MainParser.ReservedWordContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#reservedWord}.
	 * @param ctx the parse tree
	 */
	void exitReservedWord(MainParser.ReservedWordContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#keyword}.
	 * @param ctx the parse tree
	 */
	void enterKeyword(MainParser.KeywordContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#keyword}.
	 * @param ctx the parse tree
	 */
	void exitKeyword(MainParser.KeywordContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#s}.
	 * @param ctx the parse tree
	 */
	void enterS(MainParser.SContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#s}.
	 * @param ctx the parse tree
	 */
	void exitS(MainParser.SContext ctx);
	/**
	 * Enter a parse tree produced by {@link MainParser#eos}.
	 * @param ctx the parse tree
	 */
	void enterEos(MainParser.EosContext ctx);
	/**
	 * Exit a parse tree produced by {@link MainParser#eos}.
	 * @param ctx the parse tree
	 */
	void exitEos(MainParser.EosContext ctx);
}