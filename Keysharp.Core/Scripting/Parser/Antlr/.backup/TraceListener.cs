using Antlr4.Runtime.Tree;
using Antlr4.Runtime;

public class TraceListener : IParseTreeListener
{
    public void VisitTerminal(ITerminalNode node)
    {
        Debug.WriteLine($"Visit terminal: {node.Symbol.Text}");
    }

    public void VisitErrorNode(IErrorNode node)
    {
        Debug.WriteLine($"Visit error: {node.GetText()}");
    }

    public void EnterEveryRule(ParserRuleContext ctx)
    {
        Debug.WriteLine($"Enter rule: {ctx.GetType().Name}");
    }

    public void ExitEveryRule(ParserRuleContext ctx)
    {
        Debug.WriteLine($"Exit rule: {ctx.GetType().Name}");
    }
}