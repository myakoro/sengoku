using System;
using System.Windows;

namespace SengokuSLG;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            InitializeComponent();
            DataContext = new ViewModels.MainViewModel();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize MainWindow:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }
}
