using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using vmPing.Classes;
using vmPing.Properties;

namespace vmPing.Views
{
  /// <summary>
  /// Interaction logic for ManageFavoritesWindow.xaml
  /// </summary>
  public partial class ManageAliasesWindow : Window
  {
    private const int GWL_STYLE      = -16;
    private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
    private const int WS_MINIMIZEBOX = 0x20000; //minimize button

    public static ManageAliasesWindow OpenWindow = null;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


    public ManageAliasesWindow()
    {
      InitializeComponent();

      RefreshAliasList();
    }

    public void RefreshAliasList()
    {
      AliasesDataGrid.ItemsSource = null;

      var aliasList = Alias.GetAliases().ToList();
      aliasList.Sort((pair1, pair2) => string.Compare(pair1.Value, pair2.Value, StringComparison.Ordinal));

      AliasesDataGrid.ItemsSource = aliasList;
    }

    private void Remove_Click(object sender, RoutedEventArgs e)
    {
      if (AliasesDataGrid.SelectedIndex < 0)
      {
        return;
      }

      var dialogWindow = new DialogWindow(
                                          DialogWindow.DialogIcon.Warning,
                                          Strings.DialogTitle_ConfirmDelete,
                                          $"{Strings.ManageAliases_Warn_DeleteA} {((KeyValuePair<string, string>)AliasesDataGrid.SelectedItem).Value} {Strings.ManageAliases_Warn_DeleteB}",
                                          Strings.DialogButton_Remove,
                                          true);
      dialogWindow.Owner = this;
      if (dialogWindow.ShowDialog() == true)
      {
        Alias.DeleteAlias(((KeyValuePair<string, string>)AliasesDataGrid.SelectedItem).Key);
        RefreshAliasList();
      }
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
      if (AliasesDataGrid.SelectedIndex < 0)
      {
        return;
      }

      var editAliasWindow = new EditAliasWindow(((KeyValuePair<string, string>)AliasesDataGrid.SelectedItem).Key, ((KeyValuePair<string, string>)AliasesDataGrid.SelectedItem).Value);
      editAliasWindow.Owner = this;

      if (editAliasWindow.ShowDialog() == true)
      {
        RefreshAliasList();
      }
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
      var newAliasWindow = new NewAliasWindow { Owner = this };
      if (newAliasWindow.ShowDialog() == true)
      {
        RefreshAliasList();
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      OpenWindow = this;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      OpenWindow = null;
    }

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
      HideMinimizeAndMaximizeButtons();
    }

    protected void HideMinimizeAndMaximizeButtons()
    {
      var windowHandle = new WindowInteropHelper(this).Handle;
      if (windowHandle == null)
      {
        return;
      }

      SetWindowLong(windowHandle, GWL_STYLE, GetWindowLong(windowHandle, GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
    }
  }
}
