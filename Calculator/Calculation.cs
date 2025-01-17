using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Calculator
{
    public class Calculation
    {
        private static readonly Dictionary<string, Func<double, double>> UnaryOperands = new()
        {
            { "sin", Math.Sin },
            { "arcsin", Math.Asin },
            { "cos", Math.Cos },
            { "arccos", Math.Acos },
            { "tan", Math.Tan },
            { "arctan", Math.Atan },
            { "ln", Math.Log }
        };

        private static readonly Dictionary<string, Func<double, double, double>> Logarithm = new()
        {
            { "log", (a, b) => Math.Log(b, a) }
        };

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

        public static string Calculate(string input, bool isDegrees)
        {
            try
            {
                //Validate input
                ValidateExpression(input);

                //Replce constants
                input = input.Replace("π", Math.PI.ToString(CultureInfo.InvariantCulture));
                input = input.Replace("e", Math.E.ToString(CultureInfo.InvariantCulture));

                //Handle roots
                input = HandleRoots(input);

                //Handle exponents
                input = HandleExponents(input);

                //Handle logarithms
                input = HandleLogarithms(input);

                var result = DetermineExpression(input);

                if (double.IsInfinity(result)) return "Infinity";

            } 
            catch (CalculationException) { throw; }

            catch (Exception) 
            {
                throw new CalculationException(
                    "Invalid expression",
                    "Check input for correct formatting and operators");
            }
        }

        private static void ValidateExpression(string input)
        {
            //Chech for empty input
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new CalculationException(
                    "Input is empty",
                    "Enter a mathematical expression"
                    );
            }

            //Check bracket balance
            int open = 0;
            foreach(char c in input)
            {
                if (c == '(') open++;
                if (c == ')') open--;
                if (open < 0)
                {
                    throw new CalculationException(
                        "Closing bracket is missing",
                        "Ensure all brackets are properly matched");
                }
                if (open > 0)
                {
                    throw new CalculationException(
                        "Opening bracket is missing",
                        "Ensure all brackets are properly matched");
                }
            }

            //Check for consecutive operators
            if (Regex.IsMatch(input, @"[\+\-\*\/]{2,}"))
            {
                throw new CalculationException(
                    "Consecutive operators",
                    "Ensure operators are separated by numbers or expressions");
            }

            //Check for division by 0
            if (input.Contains("/0"))
            {
                throw new CalculationException(
                    "Division by zero",
                    "Cannot divide by zero, modify your expression");
            }
        }
        
        private static string HandleRoots(string input)
        {
            var rootRegex = @"(\d*?√\((.*?)\)";
            return Regex.Replace(input, rootRegex, match =>
            {
                var degree = string.IsNullOrEmpty(match.Groups[1].Value) ? "2" : match.Groups[1].Value;
                var expression = match.Groups[2].Value;
                return $"pow({expression}, 1.0/{degree})";
            });
        }

        private static string HandleExponents(string input)
        {
            var expRegex = @"(\d+|\))^?\)).*?)\)";
            return Regex.Replace(input, expRegex, match =>
            {
                var baseNum = match.Groups[1].Value;
                var power = match.Groups[2].Value;
                return $"pow({base}, {power})";
            });
        }

        private static string HandleLogarithms(string input)
        {
            //Handle natural logarithm
            input = Regex.Replace(input, @"ln\((.*?\)", "log($1");

            //Handle base-10 logarithm
            input = Regex.Replace(input, @"lg\((.*?)\)", match =>
            {
                var value = match.Groups[1].Value;
                return $"log({value}, 10)";
            });

            //Handle base-n logarithm
            input = Regex.Replace(input, @"log_(d+)\((.*?)\)", match =>
            {
                var baseNum = match.Groups[1].Value;
                var value = match.Groups[2].Value;
                return $"log({value}, {baseNum})";
            });

            return input;
        }

        public static double DetermineExpression(string expression)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("expression", typeof(string), expression);
            var row = dataTable.NewRow();
            dataTable.Rows.Add(row);
            return double.Parse((string)row["expression"]);
        }

        private static double ConvertToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private static double ConvertToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }
    }
}
