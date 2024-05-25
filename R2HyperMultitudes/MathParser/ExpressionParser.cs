using System;
using System.IO;

namespace R2HyperMultitudes.MathParser
{
    public class ExpressionParser
    {
        private Tokenizer _tokenizer;
        
        public ExpressionParser(string expression)
        {
            _tokenizer = new Tokenizer(new StringReader(expression));
        }

        public Node Parse()
        {
            var expr = ParseAddSubtract();

            if (_tokenizer.Token != Token.None)
                throw new InvalidDataException("Unexpected characters at end of expression");

            return expr;
        }

        private Node ParseAddSubtract()
        {
            var lhs = ParseLeaf();

            while (true)
            {
                Func<double, double, double> op = null;
                switch (_tokenizer.Token)
                {
                    case Token.Add:
                        op = (a, b) => a + b;
                        break;
                    case Token.Subtract:
                        op = (a, b) => a - b;
                        break;
                }

                if (op == null)
                    return lhs;
                
                _tokenizer.NextToken();
                var rhs = ParseLeaf();
                lhs = new NodeBinary(lhs, rhs, op);
            }
        }

        private Node ParseLeaf()
        {
            if (_tokenizer.Token == Token.Number)
            {
                var node = new NodeNumber(_tokenizer.Number);
                _tokenizer.NextToken();
                return node;
            }

            throw new InvalidDataException($"Unexpected token: {_tokenizer.Token}");
        }
    }
}
