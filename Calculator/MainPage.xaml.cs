namespace Calculator
{
    public partial class MainPage : ContentPage
    {
        private bool isNewCalculation = false;
        private string input;
        private int pointerPosition;
        private Stack<(int open, int close)> brackets = new();

        public MainPage()
        {
            InitializeComponent();

            input = string.Empty;
            pointerPosition = 0;

            Left.Clicked += OnLeftClicked;
            Right.Clicked += OnRightClicked;

            HandleButtons();
            UpdateResult();
        }

        private void HandleButtons()
        {
            foreach (var child in BtnGrid.Children)
            {
                if (child is Button btn)
                {
                    btn.Clicked += OnButtonClicked;
                }
            }

            //Assing picker change events
            ConstPicker.SelectedIndexChanged += PickerIndexChanged;
            LogPicker.SelectedIndexChanged += PickerIndexChanged;
            BracketPicker.SelectedIndexChanged += PickerIndexChanged;
        }

        private static bool HasTrigFunction(string expression)
        {
            //Check if calculation has any trigonometric functions
            return expression.Contains("sin(") || expression.Contains("cos(") || expression.Contains("tan(");
        }

        private async void OnButtonClicked(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                if (isNewCalculation && btn.Text != "=")
                {
                    //Reset input for a new calculation
                    input = string.Empty;
                    pointerPosition = 0;
                    isNewCalculation = false;
                }

                switch (btn.Text)
                {   
                    case "=":
                        try
                        {
                            bool? isDegrees = null;

                            if (HasTrigFunction(input))
                            {
                                //Ask user for the angle unit when calculation has trigonometric function
                                bool? result = await DisplayAlert("Angle unit",
                                    "Which angle units are you using?", "Degrees", "Radians");

                                if (result != null) isDegrees = result.Value;
                                else return;
                            }
                            
                            //Calculate the result and update the dispaly
                            var calcResult = Calculation.Calculate(input, isDegrees ?? false);
                            ResLabel.Text = calcResult;
                            input = calcResult;
                            pointerPosition = calcResult.Length;
                            isNewCalculation = true;
                        }
                        catch (Calculation.CalculationException ex)
                        {
                            await DisplayAlert("Calculation error",
                                $"{ex.Msg}\n\nSuggestion: {ex.Suggestion}",
                                "OK");
                        }
                        break;

                    case "π":
                        PiBtn.IsVisible = false;
                        ConstPicker.IsVisible = true;
                        break;

                    case "log()":
                        LogBtn.IsVisible = false;
                        LogPicker.IsVisible = true;
                        break;

                    case "( )":
                        BracketBtn.IsVisible = false;
                        BracketPicker.IsVisible = true;
                        break;

                    case "⌫":
                        if (pointerPosition > 0)
                        {
                            input = input.Remove(pointerPosition - 1, 1);
                            pointerPosition = Math.Max(0, pointerPosition - 1);
                        }
                        break;

                    default:
                        if (pointerPosition <= input.Length)
                        {
                            input = input.Insert(pointerPosition, btn.Text);
                            pointerPosition += btn.Text.Length;
                        }
                        break;
                }
                UpdateResult();
            }
        }

        private void OnLeftClicked(object? sender, EventArgs e)
        {
            if (pointerPosition > 0)
            {
                //Move cursor to left
                pointerPosition--;
                UpdateResult();
            }
        }

        private void OnRightClicked(object? sender, EventArgs e)
        {
            if (pointerPosition < input.Length)
            {
                //Move cursor to right
                pointerPosition++;
                UpdateResult();
            }
        }

        private void PickerIndexChanged(object? sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedIndex >= 0)
            {
                string item = picker.Items[picker.SelectedIndex];

                //Check if it's function with parentheses
                if (item.Contains("()"))
                {
                    if (pointerPosition <= input.Length)
                    {
                        input = input.Insert(pointerPosition, item);

                        //Store an opening bracket and a closing bracket positions' as a pair
                        int openingPosition = pointerPosition + item.IndexOf("(");
                        int closingPosition = pointerPosition + item.IndexOf(")");

                        brackets.Push((openingPosition, closingPosition));

                        //Move pointer's position between the brackets
                        pointerPosition = openingPosition;
                    }
                }
                else if (item == "(")
                {
                    if (pointerPosition <= input.Length)
                    {
                        int openPosition = pointerPosition;
                        input = input.Insert(pointerPosition, item);
                        pointerPosition += item.Length;

                        //Store closing bracket's position as -1
                        brackets.Push((openPosition, -1));
                    }
                }
                else if (item == ")")
                {
                    if (pointerPosition <= input.Length)
                    {
                        //Find the last unclosed opening bracket
                        var lastUnclosed = brackets.FirstOrDefault(br => br.close == -1);
                        if (lastUnclosed != default)
                        {
                            //Update bracket pair with correct closing position
                            brackets = new Stack<(int open, int close)>(
                                brackets.Select(br =>
                                    br.open == lastUnclosed.open ? (br.open, pointerPosition) : br)
                            );
                        }

                        input = input.Insert(pointerPosition, item);
                        pointerPosition += item.Length;
                    }
                }
                else
                {
                    if (pointerPosition <= input.Length)
                    {
                        input = input.Insert(pointerPosition, item);
                        pointerPosition += item.Length;
                    }
                }

                UpdateResult();

                //Hide picker and show button after selection
                if (picker.SelectedIndex != 1)
                {
                    switch (picker)
                    {
                        case var _ when picker == ConstPicker:
                            ConstPicker.IsVisible = false;
                            PiBtn.IsVisible = true;
                            break;

                        case var _ when picker == LogPicker:
                            LogPicker.IsVisible = false;
                            LogBtn.IsVisible = true;
                            break;

                        case var _ when picker == BracketPicker:
                            BracketPicker.IsVisible = false;
                            BracketBtn.IsVisible = true;
                            break;
                    }
                }
                picker.SelectedIndex = -1;
            }
        }

        private void UpdateResult()
        {
            //Ensuring that pointer's position (index) is in bounds
            pointerPosition = Math.Min(pointerPosition, input.Length);
            pointerPosition = Math.Max(0, pointerPosition);

            //Show navigation buttons if there's any inputs
            Left.IsVisible = input.Length > 0;
            Right.IsVisible = input.Length > 0;

            //Insert the pointer (cursor)
            string text = input.Insert(pointerPosition, "|");
            ResLabel.Text = text;
        }
    }
}