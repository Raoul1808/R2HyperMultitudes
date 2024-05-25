using System;

namespace R2HyperMultitudes.MathParser
{
    public abstract class Node
    {
        public abstract double Eval(IContext ctx);
    }

    public class NodeNumber : Node
    {
        private double _number;

        public NodeNumber(double number)
        {
            _number = number;
        }

        public override double Eval(IContext ctx)
        {
            return _number;
        }
    }

    public class NodeBinary : Node
    {
        private Node _lhs;
        private Node _rhs;
        private Func<double, double, double> _op;

        public NodeBinary(Node lhs, Node rhs, Func<double, double, double> op)
        {
            _lhs = lhs;
            _rhs = rhs;
            _op = op;
        }

        public override double Eval(IContext ctx)
        {
            return _op(_lhs.Eval(ctx), _rhs.Eval(ctx));
        }
    }

    public class NodeUnary : Node
    {
        private Node _rhs;
        private Func<double, double> _op;

        public NodeUnary(Node rhs, Func<double, double> op)
        {
            _rhs = rhs;
            _op = op;
        }

        public override double Eval(IContext ctx)
        {
            return _op(_rhs.Eval(ctx));
        }
    }
    
    public class NodeVariable : Node
    {
        private string _variableName;
        
        public NodeVariable(string variable)
        {
            _variableName = variable;
        }
        
        public override double Eval(IContext ctx)
        {
            return ctx.ResolveVariable(_variableName);
        }
    }
}
