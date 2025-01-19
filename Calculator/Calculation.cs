using System.Data;
using System.Globalization;

namespace Calculator
{
    public class Calculation
    {
        public class CalculationException : Exception
        {
            public string Msg { get; }
            public string Suggestion { get; }
            public CalculationException(string message, string suggestion) : base(message)
            {
                Msg = message;
                Suggestion = suggestion;
            }
        }

        public static string Calculate(string input, bool? isDegrees)
        {
            try
            {
                //Validate input
                ValidateExpression(input);

                //Replce constants
                input = input.Replace("π", Math.PI.ToString(CultureInfo.InvariantCulture));
                input = input.Replace("e", Math.E.ToString(CultureInfo.InvariantCulture));

                //Process mathematical constants
                input = ProcessFunctions(input, isDegrees ?? false);

                //Calculate the result
                var result = DetermineExpression(input);

                if (Math.Abs(result) > 1e10)
                {
                    return result.ToString("e", CultureInfo.InvariantCulture);
                }
                else
                {
                    return Math.Round(result, 5).ToString(CultureInfo.InvariantCulture);
                }
            }
            catch (CalculationException) { throw; }

            catch (Exception ex)
            {
                throw new CalculationException(
                    $"Calculation error: {ex.Message}",
                    "Check input for correct formatting and operators");
            }
        }

        private static double DetermineExpression(string expression)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(expression)) throw new CalculationException(
                    "Empty expression", "Enter a valid expression");

                var dataTable = new DataTable();
                //Replace x with * for calculations
                expression = expression.Replace("x", "*");
                var result = dataTable.Compute(expression, "");
                return Convert.ToDouble(result);
            }
            catch (DivideByZeroException) { throw new CalculationException("Division by zero", "Cannot divide by 0"); }
            catch (Exception) { throw new CalculationException("Invalid expression", "Check the format of expression"); }
        }

        private static string ProcessFunctions(string input, bool isDegrees)
        {
             //Process order: 1. Trigonometric/basic functions, 2. logarithms, 3. roots, 4. exponents

            if (string.IsNullOrWhiteSpace(input)) return input;

            string result = input;

            result = HandleTrigonometry(result, isDegrees);
            result = HandleLogarithms(result);
            result = HandleRoots(result);
            result = HandleExponents(result);

            return result;
        }

        private static string HandleTrigonometry(string input, bool isDegrees)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            var functions = new Dictionary<string, Func<double, double>> 
            { 
                {"sin", Math.Sin}, {"cos", Math.Cos}, {"tan", Math.Tan} 
            };

            foreach (var func in functions)
            {
                int startIndex;
                while ((startIndex = input.IndexOf(func.Key)) != -1)
                {
                    int openBracket = input.IndexOf('(', startIndex);
                    if (openBracket == -1) break;

                    int closeBracket = FindMatchingBracket(input, openBracket);
                    if (closeBracket == -1 || closeBracket <= openBracket) break;

                    string innerExp = input.Substring(openBracket + 1, closeBracket - openBracket - 1);
                    if (string.IsNullOrEmpty(innerExp)) break;

                    var value = DetermineExpression(innerExp);

                    //Convert to radians for trigonometric functions when needed
                    if (isDegrees) value = value * Math.PI / 180;

                    var calculated = func.Value(value);
                    input = input.Substring(0, startIndex) +
                        calculated.ToString(CultureInfo.InvariantCulture) + input.Substring(closeBracket + 1);
                }
            }
            return input;
        }

        private static string HandleLogarithms(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            //Handle natural logarithm
            int startIndex;
            while ((startIndex = input.IndexOf("ln(")) != -1)
            {
                int openBracket = startIndex + 2;
                if (openBracket >= input.Length) break;

                int closeBracket = FindMatchingBracket(input, openBracket);
                if (closeBracket == -1 || closeBracket <= openBracket) break;

                string innerExp = input.Substring(openBracket + 1, closeBracket - openBracket - 1);
                if (string.IsNullOrWhiteSpace(innerExp)) break;

                var value = DetermineExpression(innerExp);
                var result = Math.Log(value);

                input = input.Substring(0, startIndex) +
                    result.ToString(CultureInfo.InvariantCulture) + input.Substring(closeBracket + 1);
            }

            //Handle base-10 logarithm
            while ((startIndex = input.IndexOf("lg(")) != -1) 
            {
                int openBracket = startIndex + 2;
                if (openBracket >= input.Length) break;

                int closeBracket = FindMatchingBracket(input, openBracket);
                if (closeBracket == -1) break;

                string innerExp = input.Substring(openBracket + 1, closeBracket - openBracket - 1);
                if (string.IsNullOrWhiteSpace(innerExp)) break;

                var value = DetermineExpression(innerExp);
                var result = Math.Log10(value);

                input = input.Substring(0, startIndex) +
                    result.ToString(CultureInfo.InvariantCulture) + input.Substring(closeBracket + 1);
            }

            //Handle nth base logarithm
            while ((startIndex = input.IndexOf("log_")) != -1)
            {
                int baseStart = startIndex + 4;
                if (baseStart >= input.Length) break;

                int openBracket = input.IndexOf('(', baseStart);
                if (openBracket == -1 || openBracket <= baseStart) break;

                string baseStr = input.Substring(baseStart, openBracket - baseStart);
                if (!double.TryParse(baseStr, out double baseNum)) break;

                int closeBracket = FindMatchingBracket(input, openBracket);
                if (closeBracket == -1 || closeBracket <= openBracket) break;

                string innerExp = input.Substring(openBracket + 1, closeBracket - openBracket - 1);
                if (string.IsNullOrWhiteSpace(innerExp)) break;

                var value = DetermineExpression(innerExp);
                var result = Math.Log(value, baseNum);

                input = input.Substring(0, startIndex) +
                    result.ToString(CultureInfo.InvariantCulture) + input.Substring(closeBracket + 1);
            }
            return input;
        }

        private static string HandleRoots(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            int root;
            while ((root = input.IndexOf('√')) != -1)
            {
                if (root >= input.Length - 1) break;

                //Find the degree of the root
                int degree = 2; //Default to square root
                int start = root - 1;

                //Find numbers before root char
                string numberStr = "";
                while (start >= 0 && (char.IsDigit(input[start]) || input[start] == '.'))
                {
                    numberStr = input[start] + numberStr;
                    start--;
                }

                if (!string.IsNullOrEmpty(numberStr))
                {
                    if (int.TryParse(numberStr, out int parsed))
                    {
                        degree = parsed;
                        input = input.Remove(start + 1, numberStr.Length);
                        root -= numberStr.Length;
                    }
                }

                //Find and process the expression under the root
                int openBracket = input.IndexOf('(', root);
                if (openBracket == -1) break;

                int closeBracket = FindMatchingBracket(input, openBracket);
                if (closeBracket == -1 || closeBracket <= openBracket) break;

                string innerExp = input.Substring(openBracket + 1, closeBracket - openBracket - 1);
                if (string.IsNullOrEmpty(innerExp)) break;

                var value = DetermineExpression(innerExp);
                var result = Math.Pow(value, 1.0 / degree);

                input = input.Substring(0, root) +
                    result.ToString(CultureInfo.InvariantCulture) + input.Substring(closeBracket + 1);
            }
            return input;
        }

        private static string HandleExponents(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            int power;
            while ((power = input.IndexOf('^')) != -1)
            {
                if (power <= 0 || power >= input.Length - 1) break;

                //Find base
                int baseEnd = power;
                int baseStart = power - 1;

                while (baseStart >= 0 && (char.IsDigit(input[baseStart]) || input[baseStart] == '.')) 
                {
                    baseStart--;
                }
                baseStart++;

                if (baseStart >= baseEnd) throw new CalculationException(
                    "Invalid power operation", "Provide a number before ^");

                string baseStr = input.Substring(baseStart, baseEnd - baseStart);
                if (!double.TryParse(baseStr, out double baseNum))
                    throw new CalculationException("Invalid base number", "Provide a valid number before ^");

                //Find and process the exponent
                int openBracket = input.IndexOf('(', power);
                if (openBracket == -1) break;

                int closeBracket = FindMatchingBracket(input, openBracket);
                if (closeBracket == -1 || closeBracket <= openBracket) break;

                string innerExp = input.Substring(openBracket + 1, closeBracket - openBracket - 1);
                if (string.IsNullOrEmpty(innerExp)) break;

                var exponent = DetermineExpression(innerExp);
                var result = Math.Pow(baseNum, exponent);

                input = input.Substring(0, baseStart) +
                    result.ToString(CultureInfo.InvariantCulture) + input.Substring(closeBracket + 1);
            }
            return input;
        }

        private static int FindMatchingBracket(string input, int openingBracket)
        {
            if (openingBracket < 0 || openingBracket >= input.Length) return -1;

            int count = 1;
            for (int i = openingBracket + 1; i < input.Length; i++)
            {
                if (input[i] == '(') count++;
                else if (input[i] == ')') count--;

                if (count == 0) return i;
            }
            return -1;
        }

        private static void ValidateExpression(string input)
        {
            //Chech for empty input
            if (string.IsNullOrWhiteSpace(input)) throw new CalculationException(
                "Input is empty", "Enter a mathematical expression");

            //Check for invalid characters
            string valid = "0123456789+-.*/()π√e^_xsinarcostanlg ";
            foreach(char c in input)
            {
                if (!valid.Contains(c)) throw new CalculationException(
                    "Invalid characters in expression", "Use only valid operators and functions");
            }

            //Check for consective operators
            string operators = "+-*/";
            for (int i = 0; i < input.Length; i++)
            {
                char current = input[i];
                if (operators.Contains(current))
                {
                    if (i == input.Length - 1) throw new CalculationException(
                        "Expression ends with an operator", "Complete the expression with a number");

                    char next = input[i + 1];
                    if (operators.Contains(next)) throw new CalculationException(
                        "Consecutive operators found", "Ensure there are numbers between operatos");
                }
            }

            //Check bracket balance
            int open = 0;
            foreach (char c in input)
            {
                if (c == '(') open++;
                if (c == ')') open--;
                if (open < 0) throw new CalculationException(
                    "Mismatched brackets", "Ensure all brackets are properly matched");
            }
            if (open != 0) throw new CalculationException(
                    "Mismatched brackets", "Ensure all brackets are properly matched");

            //Check for empty parentheses
            for (int i =0; i < input.Length - 1; i++)
            {
                if (input[i] == '(' && input[i + 1] == ')') throw new CalculationException(
                    "Empty brackets detected", "Ensure all brackets contain values");
            }

            //Check for division by 0
            for (int i = 0; i < input.Length - 1; i++)
            {
                if (input[i] == '/' && input[i + 1] == '0')
                {
                    if (i + 2 >= input.Length || (!char.IsDigit(input[i + 2]) && input[i + 2] != '.'))
                        throw new CalculationException("Division by zero", "Cannot divide by 0");
                }
            }
        }
    }
}