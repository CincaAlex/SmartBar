using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace SmartBar
{
    public class Calculator
    {

        private const string numbers = "0123456789";
        private const string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private bool isEquation = false;
        private bool isInequality = false;

        private List<string> Tokenize(string input)
        {
            string trimmedInput = input.Replace(" ", "");
            List<string> tokens = new List<string>();
            int size = trimmedInput.Length;

            const string numbers = "0123456789";
            bool isEquation = trimmedInput.Any(c => char.IsLetter(c));

            if (isEquation)
            {
                return new List<string>();
            }

            int i = 0;
            while (i < size)
            {
                char current = trimmedInput[i];

                if (current == '(' || current == ')')
                {
                    tokens.Add(current.ToString());
                    i++;
                }
                else if (current == '-' && i + 1 < size && trimmedInput[i + 1] == '(')
                {
                    tokens.Add("-");
                    i++;
                }
                else if (current == '-' && (i == 0 ||
                    tokens.Count > 0 && (tokens.Last() == "(" || IsOperator(tokens.Last()))))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(current);
                    i++;

                    while (i < size && (numbers.Contains(trimmedInput[i]) || trimmedInput[i] == '.'))
                    {
                        sb.Append(trimmedInput[i]);
                        i++;
                    }

                    tokens.Add(sb.ToString());
                }
                else if (numbers.Contains(current) || current == '.')
                {
                    StringBuilder sb = new StringBuilder();
                    while (i < size && (numbers.Contains(trimmedInput[i]) || trimmedInput[i] == '.'))
                    {
                        sb.Append(trimmedInput[i]);
                        i++;
                    }
                    tokens.Add(sb.ToString());
                }
                else
                {
                    tokens.Add(current.ToString());
                    i++;
                }
            }

            return tokens;
        }

        private bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/" || token == "^";
        }

        private int Precedence(string op)
        {
            if(op == "+" || op == "-")
            {
                return 0;
            }
            if(op == "*" || op == "/")
            {
                return 1;
            }
            if(op == "^")
            {
                return 2;
            }
            return -1;
        }

        private Queue<string> ConvertToPostfix(List<string> tokens)
        {
            Stack<string> operators = new Stack<string>();
            Queue<string> elements = new Queue<string>();

            int size = tokens.Count;

            for (int i = 0; i < size; i++)
            {
                if (tokens[i] == "(")
                {
                    operators.Push(tokens[i]);
                }
                else if (tokens[i] == ")")
                {
                    while (operators.Count > 0)
                    {
                        elements.Enqueue(operators.Pop());
                        if (operators.Count > 0 && operators.Peek() == "(")
                        {
                            operators.Pop();
                            break;
                        }
                    }
                }
                else if (IsOperator(tokens[i]))
                {
                    string topOp = null;
                    if (operators.Count > 0)
                    {
                        topOp = operators.Peek();
                    }
                    while (operators.Count > 0 && Precedence(topOp) >= Precedence(tokens[i]))
                    {
                        elements.Enqueue(operators.Pop());
                        if (operators.Count > 0)
                        {
                            topOp = operators.Peek();
                        }
                    }
                    operators.Push(tokens[i]);
                }
                else
                {
                    elements.Enqueue(tokens[i]);
                }
            }
            while (operators.Count > 0)
            {
                elements.Enqueue(operators.Pop());
            }

            return elements;
        }

        private double StringToDouble(string input)
        {
            if (double.TryParse(input, out double result))
            {
                return result;
            }
            else
            {
                throw new FormatException("The input string is not a valid number.");
            }
        }

        private double Calculate(double a, double b, string op)
        {
            if (IsOperator(op))
            {
                if(op == "+")
                {
                    return a + b;
                }else if(op == "-")
                {
                    return a - b;
                }else if(op == "*")
                {
                    return a * b;
                }else if(op == "/")
                {
                    return a / b;
                }else if(op == "^")
                {
                    return Math.Pow(a,b);
                }
            }
            return 0;
        }

        private double BuildTreeFromPostfix(Queue<string> postfix)
        {
            Stack<double> stack = new Stack<double>();

            while (postfix.Count > 0)
            {
                string token = postfix.Dequeue();

                if (IsOperator(token))
                {
                    double b = stack.Pop();
                    double a = stack.Pop();
                    double result = Calculate(a, b, token);
                    stack.Push(result);
                }
                else
                {
                    stack.Push(StringToDouble(token));
                }
            }

            return stack.Pop();
        }


        private double BuildExpressionTree(List<string> tokens)
        {
            Queue<string> postfix = ConvertToPostfix(tokens);
            return BuildTreeFromPostfix(postfix);
        }

        public string Compute(string input)
        {
            if(string.IsNullOrEmpty(input))
            {
                return "NULL";
            }


            List<string> tokens = Tokenize(input);

            return BuildExpressionTree(tokens).ToString();
        }
    }
}
