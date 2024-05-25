using System.IO;
using NUnit.Framework;
using R2HyperMultitudes.MathParser;

namespace R2HyperMultitudes.Test
{
    public class Tests
    {
        private double Parse(string expression)
        {
            return new ExpressionParser(expression).Parse().Eval();
        }
        
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TokenizerTest()
        {
            var reader = new StringReader("10 + 20");
            var t = new Tokenizer(reader);
            
            Assert.That(t.Token, Is.EqualTo(Token.Number));
            Assert.That(t.Number, Is.EqualTo(10));
            t.NextToken();
            
            Assert.That(t.Token, Is.EqualTo(Token.Add));
            t.NextToken();
            
            Assert.That(t.Token, Is.EqualTo(Token.Number));
            Assert.That(t.Number, Is.EqualTo(20));
            t.NextToken();
            
            Assert.That(t.Token, Is.EqualTo(Token.None));
        }

        [Test]
        public void ParserTest()
        {
            Assert.That(Parse("10 + 20"), Is.EqualTo(30));
            Assert.That(Parse("10 - 20"), Is.EqualTo(-10));
            Assert.That(Parse("700 + 20 + 7"), Is.EqualTo(727));
        }
    }
}
