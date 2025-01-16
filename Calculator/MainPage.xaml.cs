namespace Calculator
{
    public partial class MainPage : ContentPage
    {
        private string input;
        public MainPage()
        {
            InitializeComponent();

            input = string.Empty;

            HandleButtons();
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
            ClearPicker.SelectedIndexChanged += PickerIndexChanged;
        }

        private void OnButtonClicked(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                switch (btn.Text)
                {
                    case "sin":
                        SinBtn.IsVisible = false;
                        SinPicker.IsVisible = true;
                        break;

                    case "cos":
                        CosBtn.IsVisible = false;
                        CosPicker.IsVisible = true;
                        break;

                    case "tan":
                        TanBtn.IsVisible = false;
                        TanPicker.IsVisible = true;
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
                        //Further logic will be implemented in the futurue
                        BackspaceBtn.IsVisible = false;
                        ClearPicker.IsVisible = true;
                        break;

                    case "=":
                        //Logic for "equals" will be implemented in the future
                        break;

                    default:
                        input += btn.Text;
                        UpdateResult();
                        break;

                }
            }
        }

        private void PickerIndexChanged(object? sender, EventArgs e)
        {
            if (sender is Picker picker)
            {
                if (picker.SelectedIndex >= 0)
                {
                    string item = picker.Items[picker.SelectedIndex];

                    input += $" {item} ";

                    UpdateResult();
                }

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

                        case var _ when picker == ClearPicker:
                            ClearPicker.IsVisible = false;
                            BackspaceBtn.IsVisible = true;
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private void UpdateResult()
        {
            ResLabel.Text = input;
        }
    }
}