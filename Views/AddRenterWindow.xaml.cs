using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CarRentalSystem_WPF.Views
{
    /// <summary>
    /// Interaction logic for AddRenterWindow.xaml
    /// </summary>
    public partial class AddRenterWindow : Window
    {
        public AddRenterWindow()
        {
            InitializeComponent();
        }

        // ── Driver license & phone: whole numbers only (digits, no letters/signs) ──
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

        private static bool IsValidInteger(string text) => text.All(char.IsDigit);

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
