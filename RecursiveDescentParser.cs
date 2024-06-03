using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfCopilator
{
    internal class RecursiveDescentParser
    {
        private string input;
        private int position;
        private List<string> parseSequence;

        public RecursiveDescentParser(string input)
        {
            this.input = input.Replace(" ", ""); // Удаляем пробелы для простоты разбора
            this.position = 0;
            this.parseSequence = new List<string>();
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
            try
            {
                E();
                if (position < input.Length)
                    throw new Exception("Unexpected characters at end of input");
            }
            catch (Exception ex)
            {
                parseSequence.Add($"Error: {ex.Message}");
            }
        }

        private void E()
        {
            parseSequence.Add("E");
            T();
            A();
        }

        private void A()
        {
            parseSequence.Add("A");
            if (Peek() == '+')
            {
                Next();
                T();
                A();
            }
            else if (Peek() == '-')
            {
                Next();
                T();
                A();
            }
            else
            {
                parseSequence.Add("ε");
            }
        }

        private void T()
        {
            parseSequence.Add("T");
            O();
            B();
        }

        private void B()
        {
            parseSequence.Add("B");
            if (Peek() == '*')
            {
                Next();
                O();
                B();
            }
            else if (Peek() == '/')
            {
                Next();
                O();
                B();
            }
            else
            {
                parseSequence.Add("ε");
            }
        }

        private void O()
        {
            parseSequence.Add("O");
            if (char.IsDigit(Peek()))
            {
                Num();
            }
            else if (char.IsLetter(Peek()))
            {
                Id();
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

        private void Num()
        {
            parseSequence.Add("num");
            while (char.IsDigit(Peek()))
            {
                Next();
            }
        }

        private void Id()
        {
            parseSequence.Add("id");
            if (char.IsLetter(Peek()))
            {
                Next();
                while (char.IsLetterOrDigit(Peek()))
                {
                    Next();
                }
            }
            else
            {
                throw new Exception($"Expected identifier at position {position}");
            }
        }

        public List<string> GetParseSequence() => parseSequence;
    }
}
