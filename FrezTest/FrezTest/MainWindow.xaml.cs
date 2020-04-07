using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using FrezTest.Common;
using FrezTest.COM;
using Microsoft.Win32;

namespace FrezTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainView mw;
        private MainMenu mm;
        private SideMenu sm;

        public MainWindow()
        {
            InitializeComponent();

            mw = new MainView();
            MainGrid.Children.Add(mw);
            Grid.SetRow(mw, 1);
            Grid.SetColumn(mw, 0);

            mm = new MainMenu();
            MainGrid.Children.Add(mm);
            Grid.SetRow(mm, 0);
            Grid.SetColumn(mm, 0);
            mm.StartProcessing_btn.Click += StartProcessingBtnOnClick;
            mm.PauseProcessing_btn.Click += PauseProcessingBtnOnClick;
            mm.CerateNewLayout_btn.Click += CerateNewLayoutBtnOnClick;
            mm.ResetLayout_btn.Click += ResetLayoutBtnOnClick;
            mm.ImportLayout_btn.Click += ImportLayoutBtnOnClick;
            mm.PauseProcessing_btn.IsEnabled = false;
            mm.ResetLayout_btn.IsEnabled = false;

            sm = new SideMenu();
            MainGrid.Children.Add(sm);
            Grid.SetRow(sm, 1);
            Grid.SetColumn(sm, 1);
            sm.Draw_cb.Click += DrawCbOnClick;
            sm.Rectangle_rad.Checked += RectangleRadOnChecked;
            sm.Circle_rad.Checked += CircleRadOnChecked;
            sm.RectHeight_tb.TextChanged += RectHeightTbOnTextChanged;
            sm.RectWidth_tb.TextChanged += RectWidthTbOnTextChanged;
            sm.CircleRadius_tb.TextChanged += CircleRadiusTbOnTextChanged;

            MinWidth = 700;
            MinHeight = 700;
        }

        private void CircleRadiusTbOnTextChanged(object sender, TextChangedEventArgs e)
        { 
            try
            {
                var input = ((TextBox)sender).Text;
                DrawingSettings.circleRadius = Int32.Parse(input);
            }
            catch (FormatException)
            {
                ((TextBox)sender).Text = DrawingSettings.circleRadius.ToString();
            }
        }

        private void RectWidthTbOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var input = ((TextBox) sender).Text;
                DrawingSettings.rectWidth = Int32.Parse(input);
            }
            catch (FormatException)
            {
                ((TextBox)sender).Text = DrawingSettings.rectWidth.ToString();
            }
        }

        private void RectHeightTbOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var input = ((TextBox)sender).Text;
                DrawingSettings.rectHeight = Int32.Parse(input);
            }
            catch (FormatException)
            {
                ((TextBox) sender).Text = DrawingSettings.rectHeight.ToString();
            }
        }

        private void CircleRadOnChecked(object sender, RoutedEventArgs e)
        {
            DrawingSettings.shape = DrawingSettings.Shape.Circle;
        }

        private void RectangleRadOnChecked(object sender, RoutedEventArgs e)
        {
            DrawingSettings.shape = DrawingSettings.Shape.Rectangle;
        }

        private void DrawCbOnClick(object sender, RoutedEventArgs e)
        {
            DrawingSettings.enable = ((CheckBox) sender).IsChecked == true;
        }

        private void ImportLayoutBtnOnClick(object sender, RoutedEventArgs e)
        {
            mm.StartProcessing_btn.IsEnabled = true;
            mm.StartProcessing_btn.Content = "Start";
            mm.PauseProcessing_btn.IsEnabled = false;
            mm.ResetLayout_btn.IsEnabled = false;
            mw.ImportLayout();
        }

        private void ResetLayoutBtnOnClick(object sender, RoutedEventArgs e)
        {
            mm.StartProcessing_btn.IsEnabled = true;
            mm.StartProcessing_btn.Content = "Start";
            mm.PauseProcessing_btn.IsEnabled = false;
            mm.ResetLayout_btn.IsEnabled = false;
            mw.ResetLayout();
        }

        private void StartProcessingBtnOnClick(object sender, RoutedEventArgs e)
        {
            mm.StartProcessing_btn.IsEnabled = false;
            mm.PauseProcessing_btn.IsEnabled = true;
            mm.ResetLayout_btn.IsEnabled = true;
            mw.StatProcessing();
        }

        private void PauseProcessingBtnOnClick(object sender, RoutedEventArgs e)
        {
            mm.StartProcessing_btn.IsEnabled = true;
            mm.StartProcessing_btn.Content = "Resume";
            mm.PauseProcessing_btn.IsEnabled = false;
            mw.PauseProcessing();
        }

        private void CerateNewLayoutBtnOnClick(object sender, RoutedEventArgs e)
        {
            mm.StartProcessing_btn.IsEnabled = true;
            mm.StartProcessing_btn.Content = "Start";
            mm.PauseProcessing_btn.IsEnabled = false;
            mm.ResetLayout_btn.IsEnabled = false;
            mw.CreateNewLayout();
        }
    }
}
