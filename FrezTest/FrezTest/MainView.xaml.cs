using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Annotations;
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
using Microsoft.Win32;
using Label = System.Windows.Controls.Label;

namespace FrezTest
{
    public partial class MainView : UserControl
    {
        private Label positionLabel;
        private Line positionVLine;
        private Line positionHLine;

        private Rectangle positionRect;
        private Ellipse positionCircle;

        private LayoutImage image;

        private int zoom = 0;

        public MainView()
        {
            InitializeComponent();

            Initialize();

            InitializeCanvas(GlobalSettings.defaultLayoutWidth, GlobalSettings.defaultLayoutHeight);

            InitializePositionElements();

            InitializeLayoutImage();
        }

        public void StatProcessing()
        {
            image.StartProcessing();
        }

        public void PauseProcessing()
        {
            image.PauseProcessing();
        }

        public void CreateNewLayout()
        {
            image.CreateNewLayout();

            InitializeCanvas(GlobalSettings.defaultLayoutWidth, GlobalSettings.defaultLayoutHeight);

            InitializePositionElements();

            InitializeLayoutImage();
        }

        public void ResetLayout()
        {
            image.ResetLayout();
        }

        private void Initialize()
        {
            Background = new SolidColorBrush(Colors.Black);
        }

        private void InitializeLayoutImage()
        {
            image = new LayoutImage();
            MyCanvas.Children.Clear();
            MyCanvas.Children.Add(image.GetWidget());
        }

        private void InitializePositionElements()
        {
            positionLabel = new Label
            {
                Foreground = new SolidColorBrush(Colors.White),
                Height = 15,
                Padding = new Thickness(0, 0, 0, 0)
            };

            positionVLine = new Line
            {
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = MyCanvas.Height,
                Stroke = Brushes.White,
                StrokeThickness = 0.5,
            };

            positionHLine = new Line
            {
                X1 = 0,
                Y1 = 0,
                X2 = MyCanvas.Width,
                Y2 = 0,
                Stroke = Brushes.White,
                StrokeThickness = 0.5,
            };

            positionRect = new Rectangle
            {
                Stroke = Brushes.White,
                StrokeThickness = 0.5
            };

            positionCircle = new Ellipse()
            {
                Stroke = Brushes.White,
                StrokeThickness = 0.5
            };
        }

        private void InitializeCanvas(int width, int height)
        {
            MyCanvas.Background = new SolidColorBrush(Colors.Black);
            MyCanvas.Width = width;
            MyCanvas.Height = height;
            ResetZoom();
        }

        private void MyCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            MyCanvas.Children.Add(positionLabel);
            MyCanvas.Children.Add(positionVLine);
            MyCanvas.Children.Add(positionHLine);
            if (DrawingSettings.enable)
            {
                if (DrawingSettings.shape == DrawingSettings.Shape.Rectangle) MyCanvas.Children.Add(positionRect);
                else if (DrawingSettings.shape == DrawingSettings.Shape.Circle) MyCanvas.Children.Add(positionCircle);
            }
        }

        private void MyCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            MyCanvas.Children.Remove(positionLabel);
            MyCanvas.Children.Remove(positionVLine);
            MyCanvas.Children.Remove(positionHLine);
            if (DrawingSettings.enable)
            {
                if(DrawingSettings.shape == DrawingSettings.Shape.Rectangle) MyCanvas.Children.Remove(positionRect);
                else if(DrawingSettings.shape == DrawingSettings.Shape.Circle) MyCanvas.Children.Remove(positionCircle);
            }
        }

        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(MyCanvas);

            positionLabel.Content = "(" + (int) pos.X + ", " + (int) pos.Y + ")";
            positionLabel.Margin = new Thickness(pos.X + 5, pos.Y - positionLabel.Height, 0, 0);
            positionVLine.X1 = positionVLine.X2 = pos.X;
            positionHLine.Y1 = positionHLine.Y2 = pos.Y;
            positionRect.Margin = new Thickness((int) pos.X - DrawingSettings.rectWidth / 2,
                (int) pos.Y - DrawingSettings.rectHeight / 2, 0, 0);
            positionRect.Width = DrawingSettings.rectWidth;
            positionRect.Height = DrawingSettings.rectHeight;
            positionCircle.Margin = new Thickness(pos.X - DrawingSettings.circleRadius,
                pos.Y - DrawingSettings.circleRadius, 0, 0);
            positionCircle.Width = DrawingSettings.circleRadius * 2;
            positionCircle.Height = DrawingSettings.circleRadius * 2;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                image.Draw((int) pos.X, (int) pos.Y);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                image.Erase((int) pos.X, (int) pos.Y);
            }
        }

        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(MyCanvas);
            image.Draw((int)pos.X, (int)pos.Y);
        }

        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(MyCanvas);
            image.Erase((int)pos.X, (int)pos.Y);
        }

        private void MyCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var m = MyCanvas.RenderTransform.Value;
            if (e.Delta > 0)
            {
                m.ScaleAt(1.1, 1.1, 0, 0);
                MyCanvas.Height *= 1.1;
                MyCanvas.Width *= 1.1;
                zoom++;
            }
            else
            {
                m.ScaleAt(1.0 / 1.1, 1.0 / 1.1, 0, 0);
                MyCanvas.Height *= 1.0 / 1.1;
                MyCanvas.Width *= 1.0 / 1.1;
                zoom--;
            }
            MyCanvas.RenderTransform = new MatrixTransform(m);
        }

        public void ImportLayout()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) image.ImportLayout(openFileDialog.FileName);

            InitializeCanvas((int) image.GetWidget().Width, (int) image.GetWidget().Height);

            InitializePositionElements();
        }

        private void ResetZoom()
        {
            var m = MyCanvas.RenderTransform.Value;
            if (zoom > 0)
                for (var i = 0; i < zoom; ++i)
                    m.ScaleAt(1.0 / 1.1, 1.0 / 1.1, 0, 0);
            else if (zoom < 0)
                for (var i = 0; i < -zoom; ++i)
                    m.ScaleAt(1.1, 1.1, 0, 0);
            zoom = 0;
            MyCanvas.RenderTransform = new MatrixTransform(m);
        }
    }
}

