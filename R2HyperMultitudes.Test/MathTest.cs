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
        public void ParserTest1()
        {
            Assert.That(Parse("10 + 20"), Is.EqualTo(30));
            Assert.That(Parse("10 - 20"), Is.EqualTo(-10));
            Assert.That(Parse("700 + 20 + 7"), Is.EqualTo(727));
        }

        [Test]
        public void ParserTest2()
        {
            Assert.That(Parse("10 + -20"), Is.EqualTo(-10));
            Assert.That(Parse("-1 + -1"), Is.EqualTo(-2));
            Assert.That(Parse("-0.5 + 2.5"), Is.EqualTo(2));
            Assert.That(Parse("---10"), Is.EqualTo(-10));
        }

        [Test]
        public void ParserTest3()
        {
            Assert.That(Parse("2 * 3"), Is.EqualTo(6));
            Assert.That(Parse("10 * -100"), Is.EqualTo(-1000));
            Assert.That(Parse("-2 * -0.5"), Is.EqualTo(1));
            Assert.That(Parse("6 / 2"), Is.EqualTo(3));
            Assert.That(Parse("2 / 3"), Is.EqualTo(2.0 / 3.0));
            Assert.That(Parse("1 + 2 * 3 - 4 / 5"), Is.EqualTo(6.2));
        }

        [Test]
        public void ParserTest4()
        {
            Assert.That(Parse("(1 + 2) * 3"), Is.EqualTo(9));
            Assert.That(Parse("(1 + 2) * (3 + 4)"), Is.EqualTo(21));
            Assert.That(Parse("1 + (2 * 3)"), Is.EqualTo(7));
            Assert.That(Parse("((1 + 2) * 3 + 4) * 5"), Is.EqualTo(65));
        }

        [Test]
        public void ParserTest5()
        {
            Assert.That(Parse("2^3"), Is.EqualTo(8));
            Assert.That(Parse("4^0.5"), Is.EqualTo(2));
            Assert.That(Parse("2^(-1)"), Is.EqualTo(0.5));
            Assert.That(Parse("-2^4"), Is.EqualTo(-16));
        }
    }
}
