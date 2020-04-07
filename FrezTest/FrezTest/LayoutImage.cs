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
    class LayoutImage
    {
        private WriteableBitmap bitmap;
        private WriteableBitmap bitmapCopy;
        private Image image;

        private DispatcherTimer timer;

        private int previousProcessX = 0;
        private int previousProcessY = 0;

        private int processX = 0;
        private int processY = 0;

        private bool processFilling = false;
        private Queue<Point> processQueue = new Queue<Point>();
        private Stack<Point> processStack = new Stack<Point>();

        private Color surfaceColor = Colors.Gray;
        private Color voidColor = Colors.Black;
        private Color processColor = Colors.Blue;
        private Color processedColor = Colors.YellowGreen;
        private Color memedColor = Colors.Yellow;
        private Color drillColor = Colors.Red;

        private bool cloneLayoutOnStartProcessing = true;

        public LayoutImage()
        {
            Initialize();
        }

        private void Initialize()
        {
            InitializeBitmap();
            InitializeImage();
            InitializeTimer();
        }

        public Image GetWidget()
        {
            return image;
        }

        public void StartProcessing()
        {
            if (cloneLayoutOnStartProcessing) CloneBitmap();

            if (!MarkDrillPoints())
            {
                MessageBox.Show("No drill points found!");
                return;
            }

            timer.Start();

            cloneLayoutOnStartProcessing = false;
        }

        private bool MarkDrillPoints()
        {
            var drillPoints = new List<Point>();

            for (var r = 0; r < image.ActualHeight; ++r)
                for (var c = 0; c < image.ActualWidth; c++)
                    if (GetPixel(bitmap, c, r) == processColor && WillFrezFit(c, r))
                        drillPoints.Add(new Point(c, r));

            if (drillPoints.Count == 0) return false;

            foreach (var point in drillPoints)
                SetPixel(bitmap, drillColor, (int) point.X, (int) point.Y);

            return true;
        }

        public void PauseProcessing()
        {
            timer.Stop();
        }

        public void CreateNewLayout()
        {
            timer.Stop();
            processX = 0;
            processY = 0;
            processFilling = false;
            processQueue.Clear();
            processStack.Clear();

            bitmapCopy = null;

            InitializeBitmap();
            InitializeImage();
            InitializeTimer();
        }

        public void ResetLayout()
        {
            timer.Stop();
            processX = 0;
            processY = 0;
            processFilling = false;
            processQueue.Clear();
            processStack.Clear();

            if (bitmapCopy != null) bitmap = bitmapCopy;
            image.Source = bitmap;

            InitializeTimer();

            cloneLayoutOnStartProcessing = true;
        }

        private void CloneBitmap()
        {
            bitmapCopy = bitmap.Clone();
        }

        public void Draw(int x, int y)
        {
            if (!DrawingSettings.enable) return;
            switch (DrawingSettings.shape)
            {
                case DrawingSettings.Shape.Rectangle:
                    DrawFilledRectangle(x, y, DrawingSettings.rectWidth, DrawingSettings.rectHeight, processColor);
                    break;
                case DrawingSettings.Shape.Circle:
                    DrawFilledCircle(x, y, DrawingSettings.circleRadius, processColor);
                    break;
            }
        }

        public void Erase(int x, int y)
        {
            if (!DrawingSettings.enable) return;
            switch (DrawingSettings.shape)
            {
                case DrawingSettings.Shape.Rectangle:
                    DrawFilledRectangle(x, y, DrawingSettings.rectWidth, DrawingSettings.rectHeight, surfaceColor);
                    break;
                case DrawingSettings.Shape.Circle:
                    DrawFilledCircle(x, y, DrawingSettings.circleRadius, surfaceColor);
                    break;
            }
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Tick += dispatcherTimer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 0);
        }

        private void InitializeBitmap()
        {
            var pf = PixelFormats.Bgra32;
            var w = GlobalSettings.defaultLayoutWidth;
            var h = GlobalSettings.defaultLayoutHeight;
            var rawStride = (w * pf.BitsPerPixel + 7) / 8;
            var rawImage = new byte[rawStride * h];

            var c = surfaceColor;

            for (var i = 0; i < rawStride * h; i += 4)
            {
                rawImage[i] = c.B;
                rawImage[i + 1] = c.G;
                rawImage[i + 2] = c.R;
                rawImage[i + 3] = c.A;
            }

            var bs = BitmapSource.Create(w, h, 96, 96, pf, null, rawImage, rawStride);
            bitmap = new WriteableBitmap(bs);
        }

        private void InitializeImage()
        {
            image = new Image
            {
                Source = bitmap,
                Width = bitmap.PixelWidth,
                Height = bitmap.PixelHeight
            };
        }

        private void SetPixel(WriteableBitmap wb, Color color, int column, int row)
        {
            if (column < 0 || column >= (int) image.ActualWidth ||
                row < 0 || row >= (int) image.ActualHeight) return;

            try
            {
                wb.Lock();

                unsafe
                {
                    var pBackBuffer = wb.BackBuffer;
                    pBackBuffer += row * wb.BackBufferStride;
                    pBackBuffer += column * 4;

                    var colorData = color.A << 24;
                    colorData |= color.R << 16;
                    colorData |= color.G << 8;
                    colorData |= color.B << 0;

                    *((int*) pBackBuffer) = colorData;
                }

                wb.AddDirtyRect(new Int32Rect(column, row, 1, 1));
            }
            finally
            {
                wb.Unlock();
            }
        }

        private Color GetPixel(WriteableBitmap wb, int column, int row)
        {
            if (column < 0 || column >= image.ActualWidth ||
                row < 0 || row >= image.ActualHeight) return default(Color);

            var retColor = new Color();

            try
            {
                wb.Lock();

                unsafe
                {
                    var pBackBuffer = wb.BackBuffer;
                    pBackBuffer += row * wb.BackBufferStride;
                    pBackBuffer += column * 4;

                    var tmpColor = *((int*) pBackBuffer);

                    retColor.A = (byte) (tmpColor >> 24);
                    retColor.R = (byte) (tmpColor >> 16);
                    retColor.G = (byte) (tmpColor >> 8);
                    retColor.B = (byte) (tmpColor >> 0);
                }
            }
            finally
            {
                wb.Unlock();
            }

            return retColor;
        }

        private void DrawFilledRectangle(int left, int top, int width, int height, Color color)
        {
            left = left - width / 2;
            top = top - height / 2;
            if (left < 0 || left + width > (int) bitmap.Width || top < 0 || top + height > (int) bitmap.Height) return;

            var rawStride = (width * PixelFormats.Bgra32.BitsPerPixel + 7) / 8;
            var rawImage = new byte[rawStride * height];

            for (var i = 0; i < rawStride * height; i += 4)
            {
                rawImage[i] = color.B;
                rawImage[i + 1] = color.G;
                rawImage[i + 2] = color.R;
                rawImage[i + 3] = color.A;
            }

            var rect = new Int32Rect(left, top, width, height);
            bitmap.WritePixels(rect, rawImage, rawStride, 0);
        }

        private void DrawFilledCircle(int left, int top, int r, Color color)
        {
            for (var i = -r; i < r; i++)
            {
                var j = (int) Math.Sqrt(r * r - i * i);
                DrawFilledRectangle(left + i, top, 1, 2 * j, color);
            }
        }

        private void Process()
        {
            if (processFilling)
            {
                if (processStack.Count == 0)
                //if (processQueue.Count == 0)
                {
                    processFilling = false;
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 0);
                    SentCoordinates(processX, processY, 0);
                    return;
                }

                FillDFS();
                //FillBFS();
                return;
            }

            if (processX >= image.ActualWidth)
            {
                processX = 0;
                processY++;
            }

            if (processY >= image.ActualHeight)
            {
                timer.Stop();
                return;
            }

            //if (Math.Abs(processX - previousProcessX) > 2 || Math.Abs(processY - previousProcessY) > 2)
            //    Console.WriteLine(Math.Abs(processX - previousProcessX) + @", " + Math.Abs(processY - previousProcessY));

            var color = GetPixel(bitmap, processX, processY);
            if (color == drillColor)
            {
                processFilling = true;
                processStack.Push(new Point(processX, processY));
                //processQueue.Enqueue(new Point(processX, processY));
                timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
                SentCoordinates(processX, processY, 0);
            }
            else if (color != voidColor && color != processColor && color != drillColor)
            {
                SetPixel(bitmap, processedColor, processX, processY);
            }

            previousProcessX = processX;
            previousProcessY = processY;

            processX++;
        }

        private bool WillFrezFit(int x, int y)
        {
            return GetPixel(bitmap, x - GlobalSettings.frezRadius, y) == processColor &&
                   GetPixel(bitmap, x, y - GlobalSettings.frezRadius) == processColor &&
                   GetPixel(bitmap, x + GlobalSettings.frezRadius, y) == processColor &&
                   GetPixel(bitmap, x, y + GlobalSettings.frezRadius) == processColor;
        }

        private void FillDFS()
        {
            var curr = processStack.Pop();
            var x = (int) curr.X;
            var y = (int) curr.Y;

            SentCoordinates(x, y, 0);

            SetPixel(bitmap, voidColor, x, y);

            if (GetPixel(bitmap, x, y - 1) == drillColor)
            {
                processStack.Push(new Point(x, y - 1));
                SetPixel(bitmap, memedColor, x, y - 1);
            }

            if (GetPixel(bitmap, x + 1, y - 1) == drillColor)
            {
                processStack.Push(new Point(x + 1, y - 1));
                SetPixel(bitmap, memedColor, x + 1, y - 1);
            }

            if (GetPixel(bitmap, x + 1, y) == drillColor)
            {
                processStack.Push(new Point(x + 1, y));
                SetPixel(bitmap, memedColor, x + 1, y);
            }

            if (GetPixel(bitmap, x + 1, y + 1) == drillColor)
            {
                processStack.Push(new Point(x + 1, y + 1));
                SetPixel(bitmap, memedColor, x + 1, y + 1);
            }

            if (GetPixel(bitmap, x, y + 1) == drillColor)
            {
                processStack.Push(new Point(x, y + 1));
                SetPixel(bitmap, memedColor, x, y + 1);
            }

            if (GetPixel(bitmap, x - 1, y + 1) == drillColor)
            {
                processStack.Push(new Point(x - 1, y + 1));
                SetPixel(bitmap, memedColor, x - 1, y + 1);
            }

            if (GetPixel(bitmap, x - 1, y) == drillColor)
            {
                processStack.Push(new Point(x - 1, y));
                SetPixel(bitmap, memedColor, x - 1, y);
            }

            if (GetPixel(bitmap, x - 1, y - 1) == drillColor)
            {
                processStack.Push(new Point(x - 1, y - 1));
                SetPixel(bitmap, memedColor, x - 1, y - 1);
            }
        }

        private void FillBFS()
        {
            var curr = processQueue.Dequeue();
            var x = (int)curr.X;
            var y = (int)curr.Y;

            SentCoordinates(x, y, 0);

            SetPixel(bitmap, voidColor, x, y);

            if (GetPixel(bitmap, x, y - 1) == drillColor)
            {
                processQueue.Enqueue(new Point(x, y - 1));
                SetPixel(bitmap, memedColor, x, y - 1);
            }

            if (GetPixel(bitmap, x + 1, y - 1) == drillColor)
            {
                processQueue.Enqueue(new Point(x + 1, y - 1));
                SetPixel(bitmap, memedColor, x + 1, y - 1);
            }

            if (GetPixel(bitmap, x + 1, y) == drillColor)
            {
                processQueue.Enqueue(new Point(x + 1, y));
                SetPixel(bitmap, memedColor, x + 1, y);
            }

            if (GetPixel(bitmap, x + 1, y + 1) == drillColor)
            {
                processQueue.Enqueue(new Point(x + 1, y + 1));
                SetPixel(bitmap, memedColor, x + 1, y + 1);
            }

            if (GetPixel(bitmap, x, y + 1) == drillColor)
            {
                processQueue.Enqueue(new Point(x, y + 1));
                SetPixel(bitmap, memedColor, x, y + 1);
            }

            if (GetPixel(bitmap, x - 1, y + 1) == drillColor)
            {
                processQueue.Enqueue(new Point(x - 1, y + 1));
                SetPixel(bitmap, memedColor, x - 1, y + 1);
            }

            if (GetPixel(bitmap, x - 1, y) == drillColor)
            {
                processQueue.Enqueue(new Point(x - 1, y));
                SetPixel(bitmap, memedColor, x - 1, y);
            }

            if (GetPixel(bitmap, x - 1, y - 1) == drillColor)
            {
                processQueue.Enqueue(new Point(x - 1, y - 1));
                SetPixel(bitmap, memedColor, x - 1, y - 1);
            }
        }

        private void SentCoordinates(int x, int y, int z)
        {
            timer.Stop();

            // Call function to send coord. to frez
            // this func will block current fred untill
            // respons from fres or timeout

            timer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Process();
        }

        public void ImportLayout(string fileName)
        {
            timer.Stop();
            processX = 0;
            processY = 0;
            processFilling = false;
            processQueue.Clear();
            processStack.Clear();

            bitmapCopy = null;
            InitializeTimer();

            BitmapSource bSource = new BitmapImage(new Uri(fileName));
            bitmap = new WriteableBitmap(bSource);

            image.Source = bitmap;
            image.Width = bitmap.PixelWidth;
            image.Height = bitmap.PixelHeight;
        }
    }
}
