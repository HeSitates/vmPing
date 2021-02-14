using System.Windows;
using System.Windows.Input;
using vmPing.Classes;
using vmPing.Properties;

namespace vmPing.Views
{
    /// <summary>
    /// EditAliasWindow is used to rename an alias.
    /// </summary>
    public partial class EditAliasWindow : Window
    {
        private readonly string _hostname;

        public EditAliasWindow(Probe pingItem) : this(pingItem.Hostname, pingItem.Alias)
        {
        }

        public EditAliasWindow(string hostname, string alias)
        {
            InitializeComponent();

            Header.Text = $"{Strings.EditAlias_AliasFor} {hostname}";
            MyAlias.Text = alias;
            MyAlias.SelectAll();
            _hostname = hostname;

            // Set initial focus to text box.
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MyAlias.Text))
            {
              Alias.DeleteAlias(_hostname);
            }
            else
            {
              Alias.AddAlias(_hostname, MyAlias.Text);
            }

            DialogResult = true;
        }
    }
}
