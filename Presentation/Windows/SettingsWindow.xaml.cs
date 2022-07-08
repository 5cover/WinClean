﻿using System.Windows;

namespace Scover.WinClean.Presentation.Windows;

public partial class SettingsWindow : Window
{
    public SettingsWindow() => InitializeComponent();

    private void ButtonOK_Click(object sender, RoutedEventArgs e) => Close();

    private void ButtonReset_Click(object sender, RoutedEventArgs e) => App.Settings.Reset();
}