using Microsoft.VisualBasic;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace kp
{
     class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var line = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(line))
                    return;

                var parser = new Parser(line);
                var expression = parser.Prase();


                var color  = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGray;

                    PrettyPrint(expression);

                Console.ForegroundColor = color;
            }
        }

        static void PrettyPrint(SyntaxNode node, string indent = "")
        {
            Console.Write(node.Kind);

                if(node is SyntaxToken t && t.Value != null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }

                Console.WriteLine();

                Indent += "    ";

            foreach (var child in node.GetChildern())
                PrettyPrint(child, indent);
        }
    }
    enum SyntaxKind
    {
        NumberToken,
        WhitespaceToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        OpenParenthesisToken,
        ClosedParenthesisToken,
        BadToken,
        EndOfFileToken
    }

    class SyntaxToken
    {
        public SyntaxToken(SyntaxKind kind, int position, string text, object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }

        public override SyntaxKind Kind { get; }
        public int Position { get; }
        public string Text { get; }
        public object Value { get; } 

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }
    }

    class Lexer
    {
        private readonly string _text;
        private int _position;

        public Lexer(string text)
        {
            _text = text;
        }


        private char Current
        {
            get
            {
                if (_position >= _text.Length)
                    return '\0';
             return _text[_position];    
            }
        }

        private void Next()
        {
            _position++;
        }

        public SyntaxToken NextToken()
        {
            // <Numbers>
            // + - * / ()
            //whitespace
            if (_position >= _text.Length)
            {
                return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0", null);
            }

            if (char.IsDigit(Current))
            {
                var start = _position;

                while (char.IsDigit(Current))
                    Next();

                var length = _position - start;
               var text = _text.Substring(start, length);
                int.TryParse(text, out var value);
                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
            }

            if(char.IsWhiteSpace(Current))
            {
                 var start = _position;

                while (char.IsWhiteSpace(Current))
                    Next();

                var length = _position - start;
               var text = _text.Substring(start, length);
                return new SyntaxToken(SyntaxKind.WhitespaceToken, start, text, null);
            }

            if(Current == '+')
               return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);
           else if(Current == '-')
               return new SyntaxToken(SyntaxKind.MinusToken, _position++, "_", null);
           else if(Current == '*')
               return new SyntaxToken(SyntaxKind.StarToken, _position++, "*", null);
           else if(Current == '/')
               return new SyntaxToken(SyntaxKind.SlashToken, _position++, "/", null);
           else if(Current == '(')
               return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(", null);
           else if(Current == ')')
               return new SyntaxToken(SyntaxKind.ClosedParenthesisToken, _position++, ")", null);

            return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);
            
        }
    }

  abstract class SyntaxNode
    {
        public abstract SyntaxKind kind { get; }

        public IEnumerable<SyntaxNode> GetChildern();
    }

   abstract class ExpressionSyntax : SyntaxNode
    {

    }
     
    sealed class NumberExpressionSyntax : ExpressionSyntax
    {
        public NumberExpressionSyntax(SyntaxToken numberToken)
        {
            NumberToken = numberToken;
        }

        public override SyntaxKind kind => SyntaxKind.NumberExpression;
        public SyntaxToken NumberToken { get; }

        public override IEnumerable<SyntaxNode> GetChildern()
        {
            yield return NumberToken;
        }
    }

    sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(ExpressionSyntax left,SyntaxToken operatorToken, ExpressionSyntax rigth) 
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = rigth; 
        }
        public override SyntaxKind Kind => SyntaxKind.BinaryExpressions;
        public ExpressionSyntax Left { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Right { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }

        
    }

    class Parser
    {

        private readonly SyntaxToken[] _tokens;
        private int _position;

        public Parser(string text)
        {
            var tokens = new List<SyntaxToken>();
            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.NextToken();
                if(token.Kind != SyntaxKind.WhitespaceToken && 
                   token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }

            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _tokens = tokens.ToArray();
        }

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
                return _tokens(_tokens.Length = 1);

            return _tokens[index]; 
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }

            private SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            return new SyntaxToken(kind, Current.Position, null, null);
        }

        public ExpressionSyntax Prase()
        {
            var left = ParsePrimaryExpression();

            while (Current.Kind == SyntaxKind.PlusToken ||
                Current.Kind == SyntaxKind.MinusToken)
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                primary = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            var numberToken = Match(SyntaxKind.NumberToken);
            return new NumberExpressionSyntax(numberToken);
        }
    }
}