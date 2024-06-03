using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace wpfCopilator
{
    internal class Analyzer
    {
        public static string correctText = "";
        public static DataTable CheckStrings(string text, bool rewrite)
        {
            correctText = "";
            text += 'e';
            DataTable table = new DataTable();
            table.Columns.Add("Ошибка", typeof(string));
            table.Columns.Add("Место", typeof(string));

            string[] patternsFull =
            {
            @"\b\w+\b",                //0
            @"\s*=\s*",                //1
            @"\s*\{\s*",               //2
            @"\s*\b\d+\b\s*",          //3
            @"\s*'\s*",                //4
            @"\s*\w+\s*",              //5
            @"\s*'\s*",                //6
            @"\s*:\s*",                //7
            @"\s*\b\d+\b\s*",          //8
            @"\s*'\s*",                //9
            @"\s*(.*?)'\s*",           //10
            @"\s*'\s*",                //11
            @"\s*,\s*",                //12
            @"\s*\}\s*",               //13
            @"\s*;\s*"                 //14
            };

            string[] patterns =
            {
            @"\b\w+\b",                 //0
            @"^\s*=\s*",                //1
            @"^\s*\{\s*",               //2
            @"^\s*\b\d+\b\s*",          //3
            @"^\s*'\s*",                //4
            @"^\s*\w+\s*",              //5
            @"^\s*'\s*",                //6
            @"^\s*:\s*",                //7
            @"^\s*\b\d+\b\s*",          //8
            @"^\s*'\s*",                //9
            @"^(.*?)'",                 //10
            @"^\s*'\s*",                //11
            @"^\s*,\s*",                //12
            @"^\s*\}\s*",               //13
            @"^\s*;\s*"                 //14
            };

            Match match;
            Match match2;

            int state = 0;
            int currentIndex = 0;
            int errorString = -1;

            while (currentIndex < text.Length)
            {
                char charOfText = text[currentIndex];

                switch (state)
                {
                    case 0: // Состояние 0: Имя переменной
                        match = Regex.Match(text, patterns[0]);
                        if (match.Success)
                        {
                            currentIndex += match.Value.Length-1;
                            correctText += match.Value;
                            state = 1;
                        }
                        else
                        {
                            AddRowToTable(table, "Ожидалось Имя переменной"+ charOfText, currentIndex);
                        }
                        break;
                    
                    case 1: // Состояние 1: Знак "="
                        match = Regex.Match(text.Substring(currentIndex), patterns[1]);
                        if (match.Success)
                        {
                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидался Знак \"=\", встретился " +
                                    text.Substring(errorString, currentIndex-errorString), currentIndex);
                                errorString = -1;
                            }
                            currentIndex += match.Value.Length - 1;
                            correctText += match.Value;
                            state = 2;
                        }
                        else
                        {
                            match = Regex.Match(text.Substring(currentIndex), patternsFull[1]);
                            if (match.Success)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            else
                            {
                                if (errorString == -1)
                                {
                                    correctText += "=";
                                }
                                goto case 2;
                            }
                        }
                        break;

                    case 2: // Состояние 2: Открывающая фигурная скобка
                        match = Regex.Match(text.Substring(currentIndex), patterns[2]);
                        if (match.Success)
                        {
                            if (state != 2)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидался Знак \"=\", встретился \"{\"", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table,"Ожидался Знак \"=\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидался \"{\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match.Value.Length - 1;
                            correctText += match.Value;
                            state = 3;
                        }
                        else
                        {
                            if (state != 2)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }


                            match = Regex.Match(text.Substring(currentIndex), patternsFull[2]);
                            if (match.Success)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            else
                            {
                                if (errorString == -1)
                                {
                                    correctText += "{";
                                }
                                goto case 3;
                            }
                        }
                        break;

                    case 3: // Состояние 3: Числовой ключ или Строковый ключ
                        match = Regex.Match(text.Substring(currentIndex), patterns[3]);
                        match2 = Regex.Match(text.Substring(currentIndex), patterns[4]);
                        if (match.Success)
                        {
                            if (state != 3)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидался \"{\", встретился Числовой ключ", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидался \"{\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидался Числовой ключ или Строковый ключ, встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match.Value.Length - 1;
                            correctText += match.Value;
                            state = 6;
                        }
                        else if (match2.Success)
                        {
                            if (state != 3)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидался \"{\", встретился Строковый ключ", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидался \"{\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидался Числовой ключ или Строковый ключ, встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match2.Value.Length - 1;
                            correctText += match2.Value;
                            state = 4;
                        }
                        else
                        {
                            if (state != 3)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }


                            match = Regex.Match(text.Substring(currentIndex), patternsFull[3]);
                            match2 = Regex.Match(text.Substring(currentIndex), patternsFull[4]);
                            if (match.Success || match2.Success)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            else
                            {
                                goto case 6;
                            }
                        }
                        break;

                    case 4: // Состояние 4: строковый ключ
                        match = Regex.Match(text.Substring(currentIndex), patterns[5]);
                        if (match.Success)
                        {
                            if (state != 4)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидался числовой или знаковый ключ", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидался числовой или знаковый ключ, встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидался \"string\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match.Value.Length - 1;
                            correctText += match.Value;
                            state = 5;
                        }
                        else
                        {
                            if (state != 4)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            match = Regex.Match(text.Substring(currentIndex), patternsFull[5]);
                            if (match.Success)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            else
                            {
                                if (errorString == -1)
                                {
                                    correctText += "ex1";
                                }
                                goto case 5;
                            }
                        }
                        break;
                    
                    case 5: // Состояние 5: закрывающая ковычка
                        match = Regex.Match(text.Substring(currentIndex), patterns[6]);
                        if (match.Success)
                        {
                            if (state != 5)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидался \"string\", встретился \" ' \"", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидалось \" ' \", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидалось \" ' \", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match.Value.Length - 1;
                            correctText += match.Value;
                            state = 6;
                        }
                        else
                        {
                            if (state != 5)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }

                            match = Regex.Match(text.Substring(currentIndex), patternsFull[6]);
                            if (match.Success)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            else
                            {
                                if (errorString == -1)
                                {
                                    correctText += "\'";
                                }
                                goto case 6;
                            }
                        }
                        break;
                    
                    case 6: // Состояние 6: Двоеточие
                        match = Regex.Match(text.Substring(currentIndex), patterns[7]);
                        if (match.Success)
                        {
                            if (state != 6)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидалось \" ' \", встретился \":\" ", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидался \":\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидался \":\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match.Value.Length - 1;
                            correctText += match.Value;
                            state = 7;
                        }
                        else
                        {
                            if (state != 6)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }

                            match = Regex.Match(text.Substring(currentIndex), patternsFull[7]);
                            if (match.Success)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            else
                            {
                                if (errorString == -1)
                                {
                                    correctText += ":";
                                }
                                goto case 7;
                            }
                        }
                        break;
                    
                    case 7: // Состояние 7: int или '
                        match = Regex.Match(text.Substring(currentIndex), patterns[8]);
                        match2 = Regex.Match(text.Substring(currentIndex), patterns[9]);
                        if (match.Success)
                        {
                            if (state != 7)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидалось \" : \", встретился Числовое значение ", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидался Числовое или Строковое значение, встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидался Числовое или Строковое значение, встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match.Value.Length - 1;
                            correctText += match.Value;
                            state = 11;
                        }
                        else if (match2.Success)
                        {
                            if (state != 7)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидалось \" : \", встретился Строковое значение ", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидался Числовое или Строковое значение, встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидался Числовое или Строковое значение, встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match2.Value.Length - 1;
                            correctText += match2.Value;
                            state = 8;
                        }
                        else
                        {
                            if (state != 7)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }


                            match = Regex.Match(text.Substring(currentIndex), patternsFull[8]);
                            match2 = Regex.Match(text.Substring(currentIndex), patternsFull[9]);
                            if (match.Success || match2.Success)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            else
                            {
                                goto case 11;
                            }

                        }
                        break;
                    
                    case 8: // Состояние 8: строка значение
                        match = Regex.Match(text.Substring(currentIndex), patterns[10]);
                        if (match.Success)
                        {
                            if (state != 8)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидался Числовое или Строковое значение, встретился Строковое значение \"string\" ", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидалось \"string\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидалось \"string\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match.Value.Length - 2;
                            correctText += match.Value.Substring(0, match.Value.Length - 1);
                            state = 9;
                        }
                        else
                        {
                            if (state != 8)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }

                            match = Regex.Match(text.Substring(currentIndex), patternsFull[10]);
                            if (match.Success)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            else
                            {
                                if (errorString == -1)
                                {
                                    correctText += "ex2";
                                }
                                goto case 9;
                            }
                        }
                        break;

                    case 9: // Состояние 9: ковычка
                        match = Regex.Match(text.Substring(currentIndex), patterns[11]);
                        if (match.Success)
                        {
                            if (state != 9)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидалось \"string\", встретился \" ' \" ", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидалось \" ' \", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидалось \" ' \", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match.Value.Length - 1;
                            correctText += match.Value;
                            state = 10;
                        }
                        else
                        {

                            if (state != 9)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            match = Regex.Match(text.Substring(currentIndex), patternsFull[11]);
                            if (match.Success)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            else
                            {
                                if (errorString == -1)
                                {
                                    //MessageBox.Show("er errorString = "+ errorString.ToString()+"\n match.Value = \"" + match.Value+"\"");
                                    correctText += "\'";
                                }
                                goto case 10;
                            }
                        }
                        break;

                    case 10: // Состояние 10: Закрывающая фигурная скобка или запятая

                        match = Regex.Match(text.Substring(currentIndex), patterns[12]);
                        match2 = Regex.Match(text.Substring(currentIndex), patterns[13]);
                        if (match.Success)
                        {
                            if (state != 10)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидалось \" ' \", встретился \",\" ", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидался \"}\" или \",\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидался \"}\" или \",\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match.Value.Length - 1;
                            correctText += match.Value;
                            state = 3;
                        }
                        else if (match2.Success)
                        {
                            if (state != 10)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидалось \" ' \", встретился \"}\" ", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидался \"}\" или \",\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидался \"}\" или \",\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match2.Value.Length - 1;
                            correctText += match2.Value;
                            state = 12;
                        }
                        else
                        {
                            if (state != 10)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }


                            match = Regex.Match(text.Substring(currentIndex), patternsFull[12]);
                            match2 = Regex.Match(text.Substring(currentIndex), patternsFull[13]);
                            if (match.Success || match2.Success)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            else
                            {
                                if (errorString == -1)
                                {
                                    correctText += "}";
                                }
                                goto case 12;
                            }
                        }
                        break;

                    case 11: // Состояние 11: Закрывающая фигурная скобка или запятая
                        match = Regex.Match(text.Substring(currentIndex), patterns[12]);
                        match2 = Regex.Match(text.Substring(currentIndex), patterns[13]);
                        if (match.Success)
                        {
                            if (state != 11)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидалось \" ' \", встретился \",\" ", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидался \"}\" или \",\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидался \"}\" или \",\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match.Value.Length - 1;
                            correctText += match.Value;
                            state = 3;
                        }
                        else if (match2.Success)
                        {
                            if (state != 11)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидалось \" ' \", встретился \"}\" ", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидался \"}\" или \",\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидался \"}\" или \",\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match2.Value.Length - 1;
                            correctText += match2.Value;
                            state = 12;
                        }
                        else
                        {
                            if (state != 11)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }


                            match = Regex.Match(text.Substring(currentIndex), patternsFull[12]);
                            match2 = Regex.Match(text.Substring(currentIndex), patternsFull[13]);
                            if (match.Success || match2.Success)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            else
                            {
                                if (errorString == -1)
                                {
                                    correctText += "}";
                                }
                                goto case 12;
                            }
                        }
                        break;

                    case 12: // Состояние 12: Точка с запятой
                        match = Regex.Match(text.Substring(currentIndex), patterns[14]);
                        if (match.Success)
                        {
                            if (state != 12)
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Ожидался \"}\", встретился \";\" ", currentIndex);
                                }
                                else
                                {
                                    AddRowToTable(table, "Ожидалось \";\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                    errorString = -1;
                                }
                            }

                            if (errorString != -1)
                            {
                                AddRowToTable(table, "Ожидалось \";\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                            }

                            currentIndex += match.Value.Length - 1;
                            correctText += match.Value;
                            state = 13;
                        }
                        else
                        {
                            match = Regex.Match(text.Substring(currentIndex), patternsFull[14]);
                            if (match.Success)
                            {
                                if (errorString == -1)
                                {
                                    errorString = currentIndex;
                                }
                            }
                            else
                            {
                                if (errorString == -1)
                                {
                                    AddRowToTable(table, "Отсутствует \";\"", currentIndex);
                                    correctText += ";";
                                    errorString = currentIndex;
                                }
                            }
                            /*
                            if (errorString != -1 && currentIndex == text.Length-1)
                            {
                                AddRowToTable(table, "Ожидалось \";\", встретился " +
                                    text.Substring(errorString, currentIndex - errorString), currentIndex);
                                errorString = -1;
                                correctText += ";";
                            }*/
                        }
                        break;

                    case 13: // Состояние 13: конечное
                        //MessageBox.Show(table.Rows.Count.ToString());
                        if (table.Rows.Count <= 0)
                        {
                            return getSuccessTable();
                        }
                        break;
                    //AddRowToTable(table, "Завершился без ошибок.", 0, currentIndex);
                    //return table;
                }

                currentIndex++;
            }

            return table;
        }
        private static DataTable getSuccessTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Успешность", typeof(string));
            DataRow row = table.NewRow();
            row["Успешность"] = "Код завершился без ошибок";
            table.Rows.Add(row);
            return table;
        }
        private static void AddRowToTable(DataTable table, string content, int indexEnd)
        {
            DataRow row = table.NewRow();
            row["Ошибка"] = content;
            row["Место"] = $"{indexEnd} символ";
            table.Rows.Add(row);
        }
    }
}
