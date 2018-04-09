using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BoardMapGenerator
{
    public static class Utils
    {
        public static readonly int DPI = 96;

        /// <summary>
        /// Takes a bitmap and converts it to an image that can be handled by WPF ImageBrush
        /// </summary>
        /// <param name="src">A bitmap image</param>
        /// <returns>The image as a BitmapImage for WPF</returns>
        public static BitmapImage ConvertBitmap(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        public static int CentimetersToPixel(float cent)
        {
            float inches = cent / 2.54f;

            return (int)Math.Round(inches * DPI);
        }

        public static float PixelToCentimeters(int pixel)
        {
            float inches = pixel / DPI;

            return inches * 2.54f;
        }

        public static FixedDocument GetFixedDocument(FrameworkElement toPrint, PrintDialog printDialog)
        {
            PrintCapabilities capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
            System.Windows.Size pageSize = new System.Windows.Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
            System.Windows.Size visibleSize = new System.Windows.Size((int)capabilities.PageImageableArea.ExtentWidth, (int)capabilities.PageImageableArea.ExtentHeight);
            FixedDocument fixedDoc = new FixedDocument();
            //If the toPrint visual is not displayed on screen we neeed to measure and arrange it  
            toPrint.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            toPrint.Arrange(new Rect(new System.Windows.Point(0, 0), toPrint.DesiredSize));
            //  
            System.Windows.Size size = toPrint.DesiredSize;
            //Will assume for simplicity the control fits horizontally on the page  
            double yOffset = 0;
            double xOffset = 0;

            while (yOffset < size.Height)
            {
                while(xOffset < size.Width)
                {
                    VisualBrush vb = new VisualBrush(toPrint);
                    vb.Stretch = Stretch.None;
                    vb.AlignmentX = AlignmentX.Left;
                    vb.AlignmentY = AlignmentY.Top;
                    vb.ViewboxUnits = BrushMappingMode.Absolute;
                    vb.TileMode = TileMode.None;
                    vb.Viewbox = new Rect(xOffset, yOffset, visibleSize.Width, visibleSize.Height);
                    PageContent pageContent = new PageContent();
                    FixedPage page = new FixedPage();
                    ((IAddChild)pageContent).AddChild(page);
                    fixedDoc.Pages.Add(pageContent);
                    page.Width = pageSize.Width;
                    page.Height = pageSize.Height;
                    Canvas canvas = new Canvas();
                    FixedPage.SetLeft(canvas, capabilities.PageImageableArea.OriginWidth);
                    FixedPage.SetTop(canvas, capabilities.PageImageableArea.OriginHeight);
                    canvas.Width = visibleSize.Width;
                    canvas.Height = visibleSize.Height;
                    canvas.Background = vb;
                    page.Children.Add(canvas);
                    xOffset += visibleSize.Width;
                }

                xOffset = 0;
                yOffset += visibleSize.Height;
            }
            return fixedDoc;
        }

        public static bool? ShowPrintPreview(FixedDocument fixedDoc)
        {
            Window wnd = new Window();
            DocumentViewer viewer = new DocumentViewer();
            viewer.Document = fixedDoc;
            wnd.Content = viewer;
            return wnd.ShowDialog();
        }
    }
}
