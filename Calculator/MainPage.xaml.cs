using System.Text.RegularExpressions;

namespace Calculator
{
    public partial class MainPage : ContentPage
    {
        private bool isDegrees = true;
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

            SinPicker.SelectedIndexChanged += PickerIndexChanged;
            CosPicker.SelectedIndexChanged += PickerIndexChanged;
            TanPicker.SelectedIndexChanged += PickerIndexChanged;
            ConstPicker.SelectedIndexChanged += PickerIndexChanged;
            LogPicker.SelectedIndexChanged += PickerIndexChanged;
            BracketPicker.SelectedIndexChanged += PickerIndexChanged;
        }

        private async void OnButtonClicked(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                if (isNewCalculation && btn.Text != "=")
                {
                    input = string.Empty;
                    pointerPosition = 0;
                    isNewCalculation = false;
                }

                string text = btn.Text;

                switch (text)
                {
                    case "sin":
                    case "cos":
                    case "tan":
                        Button? clickedTrigBtn = btn;
                        bool? result = await DisplayAlert("Angle units",
                            "Which units are you using?",
                            "Degrees", "Radians");

                        if (result != null)
                        {
                            isDegrees = result.Value;

                            switch (text)
                            {
                                case "sin":
                                    SinBtn.IsVisible = false;
                                    SinPicker.IsVisible = true;
                                    SinPicker.Focus();
                                    break;
                                case "cos":
                                    CosBtn.IsVisible = false;
                                    CosPicker.IsVisible = true;
                                    CosPicker.Focus();
                                    break;
                                case "tan":
                                    TanBtn.IsVisible = false;
                                    TanPicker.IsVisible = true;
                                    TanPicker.Focus();
                                    break;
                            }
                        }
                        break;
                    
                    case "=":
                        try
                        {
                            var calcResult = Calculation.Calculate(input, isDegrees);
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
                            brackets = new Stack<(int open, int close)>(
                                brackets.Where(br => br.open >= pointerPosition || br.close >= pointerPosition)
                                .Select(br => (
                                    open: br.open >= pointerPosition ? br.open - 1 : br.open,
                                    close: br.close >= pointerPosition ? br.close - 1 : br.close
                                ))
                            );
                        }
                        break;

                    default:
                        if (pointerPosition <= input.Length)
                        {
                            input = input.Insert(pointerPosition, text);
                            pointerPosition += text.Length;
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
                //Check if pointer is at closing bracket
                var _brackets = brackets.FirstOrDefault(br => br.close == pointerPosition - 1);
                if (_brackets != default)
                {
                    pointerPosition = _brackets.open + 1; //Move out of brackets before an opening bracket
                }
                else
                {
                    pointerPosition--; //Move left normally
                }
                UpdateResult();
            }
        }

        private void OnRightClicked(object? sender, EventArgs e)
        {
            if (pointerPosition < input.Length)
            {
                //Check if pointer is at opening bracket
                var _brackets = brackets.FirstOrDefault(br => br.open == pointerPosition);
                if (_brackets != default)
                {
                    pointerPosition = _brackets.close + 1; //Move out of brackets after a closing brcaket
                }
                else
                {
                    pointerPosition++; //Move right normally
                }
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
                        case var _ when picker == SinPicker:
                            SinPicker.IsVisible = false;
                            SinBtn.IsVisible = true;
                            break;

                        case var _ when picker == CosPicker:
                            CosPicker.IsVisible = false;
                            CosBtn.IsVisible = true;
                            break;

                        case var _ when picker == TanPicker:
                            TanPicker.IsVisible = false;
                            TanBtn.IsVisible = true;
                            break;

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

            //Format the expression for display
            string text = input;

            //Add proper spacing around operators
            text = Regex.Replace(text, @"([+\-*/^])", " $1 ");
            
            //Insert the pointer (cursor)
            text = input.Insert(pointerPosition, "|");

            ResLabel.Text = text;
        }
    }
}