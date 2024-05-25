using System.Globalization;
using System.IO;
using System.Text;

namespace R2HyperMultitudes.MathParser
{
    public class Tokenizer
    {
        private StringReader _reader;
        private char _currentChar;
        
        public double Number { get; private set; }
        public Token Token { get; private set; }
        
        public Tokenizer(StringReader reader)
        {
            _reader = reader;
            NextChar();
            NextToken();
        }

        private void NextChar()
        {
            int ch = _reader.Read();
            _currentChar = ch < 0 ? '\0' : (char)ch;
        }

        public void NextToken()
        {
            while (char.IsWhiteSpace(_currentChar))
            {
                NextChar();
            }

            switch (_currentChar)
            {
                case '\0':
                    Token = Token.None;
                    return;
                
                case '+':
                    NextChar();
                    Token = Token.Add;
                    return;
                
                case '-':
                    NextChar();
                    Token = Token.Subtract;
                    return;
                
                case '*':
                    NextChar();
                    Token = Token.Multiply;
                    return;
                
                case '/':
                    NextChar();
                    Token = Token.Divide;
                    return;
                
                case '(':
                    NextChar();
                    Token = Token.OpenParens;
                    return;
                
                case ')':
                    NextChar();
                    Token = Token.CloseParens;
                    return;
            }

            if (char.IsDigit(_currentChar) || _currentChar == '.')
            {
                var sb = new StringBuilder();
                bool haveDecimalPoint = false;
                while (char.IsDigit(_currentChar) || (!haveDecimalPoint && _currentChar == '.'))
                {
                    sb.Append(_currentChar);
                    haveDecimalPoint = haveDecimalPoint || _currentChar == '.';
                    NextChar();
                }

                Number = double.Parse(sb.ToString(), CultureInfo.InvariantCulture);
                Token = Token.Number;
                return;
            }

            throw new InvalidDataException($"Unexpected character: {_currentChar}");
        }
    }
}
