﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using vmPing.Classes;
using vmPing.Properties;

namespace vmPing.Views
{
    /// <summary>
    /// NewFavoriteWindow provides an interface for creating a favorite.  A favorite is a
    /// collection of hosts that can be recalled later.
    /// </summary>
    public partial class NewFavoriteWindow
    {
        private readonly List<string> _hostList;
        private readonly int          _columnCount;

        public NewFavoriteWindow(List<string> hostList, int columnCount)
        {
            InitializeComponent();

            Contents.ItemsSource = hostList;

            _hostList = hostList;
            _columnCount = columnCount;

            // Set initial focus to text box.
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validate favorite name.
            if (Favorite.IsTitleInvalid(MyTitle.Text))
            {
                var errorWindow = DialogWindow.ErrorWindow(Strings.NewFavorite_Error_InvalidName);
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                MyTitle.Focus();
                MyTitle.SelectAll();
                return;
            }

            // Check if favorite title already exists.
            if (Favorite.TitleExists(MyTitle.Text))
            {
                var warningWindow = DialogWindow.WarningWindow(message: $"{MyTitle.Text} {Strings.NewFavorite_Warn_AlreadyExists}", confirmButtonText: Strings.DialogButton_Overwrite);
                warningWindow.Owner = this;
                if (warningWindow.ShowDialog() == true)
                // User opted to overwrite existing favorite entry.
                {
                  SaveFavorite();
                }
            }
            else
            // Checks passed.  Saving.
            {
              SaveFavorite();
            }
        }

        private void SaveFavorite()
        {
            Favorite.Save(MyTitle.Text, _hostList, _columnCount);
            DialogResult = true;
        }
    }
}
