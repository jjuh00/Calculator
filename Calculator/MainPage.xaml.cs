using System.Text;

namespace Calculator
{
    public partial class MainPage : ContentPage
    {
        private readonly StringBuilder input = new();
        public MainPage()
        {
            InitializeComponent();
            HandleButtons();
        }

        private void HandleButtons()
        {
            foreach (var child in BtnGrid.Children)
            {
                if (child is Button button)
                {
                    button.Clicked += OnButtonClicked;
                }
            }
        }

        private void OnButtonClicked(object sender, EventArgs e)
        {
            if (sender is  Button button)
            {
                string text = button.Text;

                if (text == "=" || text == "⌫") //These buttons don't do anything yet
                {
                    return;
                }

                input.Append(text);
                UpdateResult();
            }
        }

        private void UpdateResult()
        {
            ResLabel.Text = input.ToString();
        }
    }
}
