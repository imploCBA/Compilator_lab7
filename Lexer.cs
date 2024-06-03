using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Text.RegularExpressions;

namespace wpfCopilator
{
    internal class Lexer
    {
        private string input;
        private int position;
        private List<string> poliz;

        public Lexer(string input)
        {
            this.input = input;
            this.position = 0;
            this.poliz = new List<string>();
        }

        private char Peek() => position < input.Length ? input[position] : '\0';

        private char Next() => position < input.Length ? input[position++] : '\0';

        private void Match(char expected)
        {
            if (Peek() == expected)
                Next();
            else
                throw new Exception($"Expected '{expected}' at position {position}");
        }

        public void Parse()
        {
            E();
            if (position < input.Length)
                throw new Exception("Unexpected characters at end of input");
        }

        private void E()
        {
            T();
            A();
        }

        private void A()
        {
            if (Peek() == '+')
            {
                Next();
                T();
                poliz.Add("+");
                A();
            }
            else if (Peek() == '-')
            {
                Next();
                T();
                poliz.Add("-");
                A();
            }
        }

        private void T()
        {
            O();
            B();
        }

        private void B()
        {
            if (Peek() == '*')
            {
                Next();
                O();
                poliz.Add("*");
                B();
            }
            else if (Peek() == '/')
            {
                Next();
                O();
                poliz.Add("/");
                B();
            }
        }

        private void O()
        {
            if (char.IsDigit(Peek()))
            {
                string number = "";
                while (char.IsDigit(Peek()))
                {
                    number += Next();
                }
                poliz.Add(number);
            }
            else if (Peek() == '(')
            {
                Next();
                E();
                Match(')');
            }
            else
            {
                throw new Exception($"Unexpected character '{Peek()}' at position {position}");
            }
        }

        public List<string> GetPoliz() => poliz;

        public double EvaluatePoliz()
        {
            Stack<double> stack = new Stack<double>();
            foreach (var token in poliz)
            {
                if (double.TryParse(token, out double number))
                {
                    stack.Push(number);
                }
                else
                {
                    double b = stack.Pop();
                    double a = stack.Pop();
                    switch (token)
                    {
                        case "+":
                            stack.Push(a + b);
                            break;
                        case "-":
                            stack.Push(a - b);
                            break;
                        case "*":
                            stack.Push(a * b);
                            break;
                        case "/":
                            stack.Push(a / b);
                            break;
                    }
                }
            }
            return stack.Pop();
        }
    }
}
