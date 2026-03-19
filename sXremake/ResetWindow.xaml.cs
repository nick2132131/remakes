using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Synapse_UI_WPF.Interfaces;

namespace Synapse_UI_WPF
{
    public partial class ResetWindow
    {
        public ResetWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private void MiniButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = WebInterface.RandomString(WebInterface.Rnd.Next(10, 32));
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void ResetEmailButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Password reset is not available in this build.",
                "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Password reset is not available in this build.",
                "Synapse X", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}