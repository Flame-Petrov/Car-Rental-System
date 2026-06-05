using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CarRentalSystem_WPF.Views
{
    /// <summary>
    /// Interaction logic for AddCarWindow.xaml
    /// </summary>
    public partial class AddCarWindow : Window
    {
        public AddCarWindow()
        {
            InitializeComponent();
        }

        // ── Year: whole numbers only (digits, no sign, no decimal point) ──
        private void IntegerOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValidInteger(ProspectiveText((TextBox)sender, e.Text));
        }

        private void IntegerOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!TryGetPastedText(e, out var pasted) ||
                !IsValidInteger(ProspectiveText((TextBox)sender, pasted)))
            {
                e.CancelCommand();
            }
        }

        // ── Price: non-negative decimal (digits + a single decimal separator) ──
        private void DecimalOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValidDecimal(ProspectiveText((TextBox)sender, e.Text));
        }

        private void DecimalOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!TryGetPastedText(e, out var pasted) ||
                !IsValidDecimal(ProspectiveText((TextBox)sender, pasted)))
            {
                e.CancelCommand();
            }
        }

        private static bool IsValidInteger(string text) => text.All(char.IsDigit);

        private static bool IsValidDecimal(string text)
        {
            var separator = Regex.Escape(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            // Allows partial entry like "", "12", "12." or ".5"; blocks letters, signs and extra separators.
            return Regex.IsMatch(text, $@"^\d*({separator}\d*)?$");
        }

        // The text the field would contain if the input were accepted (honours the current selection).
        private static string ProspectiveText(TextBox textBox, string input)
        {
            return textBox.Text.Substring(0, textBox.SelectionStart)
                 + input
                 + textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength);
        }

        private static bool TryGetPastedText(DataObjectPastingEventArgs e, out string text)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                text = (string)e.DataObject.GetData(DataFormats.Text);
                return true;
            }
            text = string.Empty;
            return false;
        }
    }
}
