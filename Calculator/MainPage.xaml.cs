using System.Text;

namespace Calculator
{
    public partial class MainPage : ContentPage
    {
        private readonly StringBuilder inputBuilder;
        public MainPage()
        {
            InitializeComponent();
            SetupButtons();
            SetupKeyHandling();

            inputBuilder = new StringBuilder();
        }

        private void SetupButtons()
        {
            foreach (var child in ((Grid)Content).Children)
            {
                if (child is Button button) {
                    button.Clicked += OnButtonClicked;
                }
            }
        }

        private void SetupKeyHandling(object sender, KeyEventArgs e)
        {
            if (char.IsDigit(e.Key))
            {
                inputBuilder.Append(e.Key);
                UpdateResult(e.Key.ToString());
            }
        }

        private void OnButtonClicked(object sender, EventArgs e)
        {

            if (sender is Button button)
            {
                string text = button.Text;
                
                if (char.IsDigit(text[0]) || "+-x/-".Contains(text))
                {
                    inputBuilder.Append(text == "x" ? "*" : text); //Replacing 'x' with '*' for calculation purposes
                    UpdateResult();
                }

                if (text == "=" || text == "⌫") return; //These buttons don't do anything at this point
            }
        }

        private void UpdateResult()
        {
            ResLabel.Text = inputBuilder.ToString();
        }
    }
}
