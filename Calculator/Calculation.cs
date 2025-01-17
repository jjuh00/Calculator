using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

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

        public static string Calculate(string input, bool isDegrees)
        {
            try
            {
                //Validate input
                ValidateExpression(input);

                //Replce constants
                input = input.Replace("π", Math.PI.ToString(CultureInfo.InvariantCulture));
                input = input.Replace("e", Math.E.ToString(CultureInfo.InvariantCulture));

                //Handle trigonometric functions
                if (isDegrees)
                {
                    input = ProcessTrigonometricInput(input);
                }

                //Calculate all mathematical functions
                input = ProcessFunctions(input);

                //Calculate the result
                var result = DetermineExpression(input);

                if (double.IsInfinity(result)) return "Infinity";

                //Convert result back to degrees if needed
                if (isDegrees && (input.Contains("arcsin") || input.Contains("arccos") || input.Contains("arctan")))
                {
                    result = result * 180 / Math.PI;
                }

                return Math.Abs(result) > 1e10 ?
                    result.ToString("E", CultureInfo.InvariantCulture) :
                    Math.Round(result, 5).ToString("E", CultureInfo.InvariantCulture);
            } 
            catch (CalculationException) { throw; }

            catch (Exception ex) 
            {
                throw new CalculationException(
                    $"Calculation error: {ex.Message}",
                    "Check input for correct formatting and operators");
            }
        }


        private static string ProcessFunctions(string input)
        {
            /*
             Process order:
            1) Trigonometric and basic functions
            2) Logarithms
            3) Roots
            4) Exponents
            */

            string result = input;

            //Handle trigonometric functions
            result = HandleTrigonometry(result);

            //Handle logarithms
            result = HandleLogarithms(result);

            //Handle roots
            result = HandleRoots(result);

            //Handle exponents
            result = HandleExponents(result);

            return result;
        }

        private static string HandleTrigonometry(string input)
        {
            var functions = new Dictionary<string, Func<double, double>>
            {
                {"sin", Math.Sin},
                {"cos", Math.Cos},
                {"tan", Math.Tan},
                {"arcsin", Math.Asin},
                {"arccos", Math.Acos},
                {"arctan", Math.Atan}
            };

            string result = input;

            foreach (var func in functions)
            {
                var trigRegex = $@"\b{func.Key}\(([^()]+)\)";
                result = Regex.Replace(result, trigRegex, match =>
                {
                    var innerExp = match.Groups[1].Value;
                    var value = DetermineExpression(innerExp);
                    var calculated = func.Value(value);
                    return calculated.ToString(CultureInfo.InvariantCulture);
                });
            }
            return input;
        }

        private static string HandleLogarithms(string input)
        {
            //Process natural logarithm
            input = Regex.Replace(input, @"ln\(([^()]+)\)", match =>
            {
                var value = DetermineExpression(match.Groups[1].Value);
                return Math.Log(value).ToString(CultureInfo.InvariantCulture);
            });

            //Process base-10 logarithm
            input = Regex.Replace(input, @"lg\(([^()]+)\)", match =>
            {
                var value = DetermineExpression(match.Groups[1].Value);
                return Math.Log10(value).ToString(CultureInfo.InvariantCulture);
            });

            //Process nth base logarithm
            input = Regex.Replace(input, @"log_(\d+)\(([^()]+)\)", match =>
            {
                var baseNum = double.Parse(match.Groups[1].Value);
                var value = DetermineExpression(match.Groups[2].Value);
                return Math.Log(value, baseNum).ToString(CultureInfo.InvariantCulture);
            });

            return input;
        }

        private static string HandleRoots(string input)
        {
            //Handle nth root
            var rootRegex = @"(\d+)?√\(([^()]+)\)";
            return Regex.Replace(input, rootRegex, match =>
            {
                var degree = string.IsNullOrEmpty(match.Groups[1].Value) ? 2 : int.Parse(match.Groups[1].Value);
                var value = DetermineExpression(match.Groups[2].Value);
                return Math.Pow(value, 1.0 / degree).ToString(CultureInfo.InvariantCulture);
            });
        }

        private static string HandleExponents(string input)
        {
            var expRegex = @"(\d*\.?\d+)\^\(([^()]+)\)";
            return Regex.Replace(input, expRegex, match =>
            {
                var baseNum = double.Parse(match.Groups[1].Value);
                var power = DetermineExpression(match.Groups[2].Value);
                return Math.Pow(baseNum, power).ToString(CultureInfo.InvariantCulture);
            });
        }

        private static string ProcessTrigonometricInput(string input)
        {
            //Convert degrees to radians for regualar trigonometric functions
            var functions = new[] { "sin", "cos", "tan" };
            foreach (var function in functions)
            {
                input = Regex.Replace(input, $@"{function}\(([^()+]\)", match =>
                {
                    var degrees = DetermineExpression(match.Groups[1].Value);
                    var radians = degrees * Math.PI / 180;
                    return $"{function}({radians.ToString(CultureInfo.InvariantCulture)})";
                });
            }
            return input;
        }

        private static double DetermineExpression(string expression)
        {
            try
            {
                var dataTable = new DataTable();
                //Replace x with * for calculations
                expression = expression.Replace("x", "*");
                var result = dataTable.Compute(expression, "");
                return Convert.ToDouble(result);
            }
            catch (Exception)
            {
                throw new CalculationException(
                    "Invalid expression",
                    "Check the format of expression");
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

            //Check for invalid characters
            if (Regex.IsMatch(input, @"[^0-9+\-*/().πe√^_xsin\sarcostanlg]"))
            {
                throw new CalculationException(
                    "Invalid characters in expression",
                    "Use only valid operators and functions");
            }

            //Check for missing operands
            if (Regex.IsMatch(input, @"[+\-*/]\s*[+\-*/]")) //Don't allow consecutive operators
            {
                throw new CalculationException(
                    "Missing operand",
                    "Ensure there are numbers between operators");
            }

            //Check bracket balance
            var open = 0;
            foreach (char c in input)
            {
                if (c == '(') open++;
                if (c == ')') open--;
                if (open < 0)
                {
                    throw new CalculationException(
                        "Mismatched brackets",
                        "Ensure all brackets are properly matched");
                }
            }
            if (open != 0)
            {
                throw new CalculationException(
                    "Mismatched brackets",
                    "Ensure all brackets are properly matched");
            }

            //Check for empty parentheses
            if (Regex.IsMatch(input, @"\(\s*\)"))
            {
                throw new CalculationException(
                    "Empty brackets detected",
                    "Ensure all brackets contain values");
            }

            //Check for power operation without base
            if (Regex.IsMatch(input, @"\^\s*$"))
            {
                throw new CalculationException(
                    "Missing base for power operation",
                    "Provide a number before (^)");
            }

            //Check for division by 0
            if (Regex.IsMatch(input, @"/\s*0(?!\.\d)"))
            {
                throw new CalculationException(
                    "Division by zero",
                    "Cannot divide by zero, modify your expression");
            }
        }
    }
}
