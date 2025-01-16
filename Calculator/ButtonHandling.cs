namespace Calculator
{
    public class ButtonHandling
    {
        private readonly Action<string> updateAction;

        public ButtonHandling(Action<string> _updateAction)
        {
            updateAction = _updateAction;
        }

        public void HandleSinBtn(Button sinBtn, Picker sinPicker)
        {
            sinBtn.IsVisible = false;
            sinPicker.IsVisible = true;
        }

        public void HandleSinPicker(Picker sinPicker, Button sinBtn, ref string input)
        {
            if (sinPicker.SelectedIndex != 1)
            {
                string option = sinPicker.Items[sinPicker.SelectedIndex];

                input += $" {option} ";

                updateAction.Invoke(input);

                sinPicker.IsVisible = false;
                sinBtn.IsVisible = true;
            }
        }

        public void HandleCosBtn(Button cosBtn, Picker cosPicker)
        {
            cosBtn.IsVisible = false;
            cosPicker.IsVisible = true;
        }

        public void HandleCosPicker(Picker cosPicker, Button cosBtn, ref string input)
        {
            if (cosPicker.SelectedIndex != 1)
            {
                string option = cosPicker.Items[cosPicker.SelectedIndex];

                input += $" {option} ";

                updateAction.Invoke(input);

                cosPicker.IsVisible = false;
                cosBtn.IsVisible = true;
            }
        }

        public void HandleTanBtn(Button tanBtn, Picker tanPicker)
        {
            tanBtn.IsVisible = false;
            tanPicker.IsVisible = true;
        }

        public void HandleTanPicker(Picker tanPicker, Button tanBtn, ref string input)
        {
            if (tanPicker.SelectedIndex != 1)
            {
                string option = tanPicker.Items[tanPicker.SelectedIndex];

                input += $" {option} ";

                updateAction.Invoke(input);

                tanPicker.IsVisible = false;
                tanBtn.IsVisible = true;
            }
        }

        public void HandlePiBtn(Button piBtn, Picker constPicker)
        {
            piBtn.IsVisible = false;
            constPicker.IsVisible = true;
        }

        public void HandleConstantPicker(Picker constPicker, Button piBtn, ref string input)
        {
            if (constPicker.SelectedIndex != 1)
            {
                string option = constPicker.Items[constPicker.SelectedIndex];

                input += $" {option} ";

                constPicker.IsVisible = false;
                piBtn.IsVisible = true;
            }
        }

        public void HandleLogPicker(Picker logPicker, Button logBtn, ref string input)
        {
            if (logPicker.SelectedIndex != 1)
            {
                string option = logPicker.Items[logPicker.SelectedIndex];

                input += $" {option} ";

                updateAction.Invoke(input);

                logPicker.IsVisible = false;
                logBtn.IsVisible = true;
            }
        }

        public void HandleBracketBtn(Button bracketBtn, Picker bracketPicker)
        {
            bracketBtn.IsVisible = false;
            bracketPicker.IsVisible = true;
        }

        public void HandleBracketPicker(Picker bracketPicker, Button bracketBtn, ref string input)
        {
            if (bracketPicker.SelectedIndex != 1)
            {
                string option = bracketPicker.Items[bracketPicker.SelectedIndex];

                input += $" {option} ";

                bracketPicker.IsVisible = false;
                bracketBtn.IsVisible = true;
            }
        }
        public void HandleBackspaceBtn(Button backSpaceBtn, Picker clearPicker)
        {
            backSpaceBtn.IsVisible = false;
            clearPicker.IsVisible = true;
        }

        public void HandleClearPicker(Picker clearPicker, Button backSpaceBtn, ref string input)
        {
            if (clearPicker.SelectedIndex != 1)
            {
                string option = clearPicker.Items[clearPicker.SelectedIndex];

                input += $" {option} ";

                clearPicker.IsVisible = false;
                backSpaceBtn.IsVisible = true;
            }
        }

        public void HandleLogBtn(Button logBtn, Picker logPicker)
        {
            logBtn.IsVisible = false;
            logPicker.IsVisible = true;
        }

        public void ButtonClicked(string text, ref string input)
        {
            input += text;

            updateAction.Invoke(input);
        }
    }
}