using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
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

namespace BoardMapGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Color? SelectedColor;

        public Guid SelectedBitmap;

        public bool InFillMode = false;
        public bool InSetImageMode = false;

        public DataRepo DataRepo { get; set; } = new DataRepo();

        public string OpenedFilePath;

        public MainWindow()
        {
            InitializeComponent();
            lImages.ItemsSource = DataRepo.Images;

            DataRepo.Images.Add(new ImageDescriptor() { ImageName = "None" });

            GenerateMap();
        }

        private void GenerateRow(Vector offset, bool flip, int count, int xIndex, int yIndex)
        {
            int lastTriangle = DataRepo.TriCount + DataRepo.TriCount - 2 - DataRepo.Trim + yIndex;
            int lastTriangleHeight = DataRepo.TriCount - (int)Math.Ceiling(DataRepo.Trim / 2d) - 1;

            if (yIndex > lastTriangleHeight)
                return;

            for (int i = 0; i < count; i++)
            {
                if (xIndex < DataRepo.Trim - yIndex || xIndex > lastTriangle)
                {
                    xIndex+=2;
                    continue;
                }

                Tile tile = new Tile(this);

                tile.Form.Fill = Brushes.LightBlue;
                tile.Form.Stroke = Brushes.Black;
                tile.Form.StrokeThickness = 1;
                tile.Size = DataRepo.TileSize;
                tile.Position = DataRepo.MapPos + new Vector(tile.Size * i, 0) + offset;
                tile.Rotation = flip ? 180f : 0f;

                tile.AddToCanvas(canvas);
                tile.Update();

                DataRepo.Map.Add(tile);

                xIndex+=2;
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            MapSettings dlg = new MapSettings();

            dlg.Owner = this;

            if(dlg.ShowDialog() == true)
            {
                DataRepo.TriCount = dlg.MapSize;
                DataRepo.TileSize = Utils.CentimetersToPixel(dlg.TileSize);
                DataRepo.Trim = dlg.Trim;

                GenerateMap();
            }
        }

        private void GenerateMap()
        {
            canvas.Children.Clear();
            DataRepo.Map.Clear();

            float triHeight = DataRepo.TileSize * 0.5f * (float)Math.Tan(Math.PI * 60 / 180);
            float midPointHeight = DataRepo.TileSize * 0.5f * (float)Math.Tan(Math.PI * 30 / 180);
            float radius = triHeight - midPointHeight;

            DataRepo.MapPos = new Vector(DataRepo.TileSize / 2, midPointHeight);
            zoomBox.Width = DataRepo.TileSize * DataRepo.TriCount;
            zoomBox.Height = (zoomBox.Width * 0.5f) * (float)Math.Tan(Math.PI * 60 / 180);

            int index = 0;
            float xIndex = 0;

            int triCount = DataRepo.TriCount;

            int trimIndexX = 0;
            int trimIndexY = 0;

            while (triCount > 0)
            {
                GenerateRow(new Vector(DataRepo.TileSize * .5f * index, triHeight * index), false, triCount, trimIndexX, trimIndexY);
                GenerateRow(new Vector(DataRepo.TileSize * .5f + DataRepo.TileSize * xIndex, radius - midPointHeight + triHeight * index), true, --triCount, trimIndexX + 1, trimIndexY);

                index++;
                xIndex += 0.5f;
                trimIndexX++;
                trimIndexY++;
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Map"; // Default file name
            dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "PNG (.png)|*.png"; // Filter files by extension

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;

                RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, Utils.DPI, Utils.DPI, System.Windows.Media.PixelFormats.Default);
                rtb.Render(canvas);

                //var crop = new CroppedBitmap(rtb, new Int32Rect(50, 50, 250, 250));

                BitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

                using (var fs = System.IO.File.OpenWrite(filename))
                {
                    pngEncoder.Save(fs);
                }
            }
        }

        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SelectedColor = colorPicker.SelectedColor;
        }

        private void btnFill_Click(object sender, RoutedEventArgs e)
        {
            InFillMode = !InFillMode;
        }

        private void btnAddImg_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            if(result == true)
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(dlg.FileName);

                if(img != null)
                {
                    BitmapImage rawImg = Utils.ConvertBitmap((System.Drawing.Bitmap)img);

                    DataRepo.Images.Add(new ImageDescriptor() { RawImage = (System.Drawing.Bitmap)img, ImageName = dlg.SafeFileName, Image = rawImg, ID = Guid.NewGuid() });
                }
            }
        }

        private void btnSetImg_Click(object sender, RoutedEventArgs e)
        {
            InSetImageMode = !InSetImageMode;
        }

        private void lImages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = lImages.SelectedIndex;

            if(index > 0)
            {

                SelectedBitmap = DataRepo.Images[index].ID;
            }
            else
            {
                SelectedBitmap = Guid.Empty;
            }
        }

        private void btnRemoveImg_Click(object sender, RoutedEventArgs e)
        {
            int index = lImages.SelectedIndex;

            if(index > 0)
            {
                Guid id = DataRepo.Images[index].ID;

                foreach(Tile t in DataRepo.Map)
                {
                    if(t.BackgroundImage == id)
                    {
                        t.BackgroundImage = Guid.Empty;

                        t.UpdateBackground();
                    }
                }

                DataRepo.Images.RemoveAt(index);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrWhiteSpace(OpenedFilePath))
            {
                ShowSaveDialog();
            }
            else
            {
                DataRepo.SaveToDisk(OpenedFilePath);
            }
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            ShowSaveDialog();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "MapFile (.mf)|*.mf";

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                SelectedBitmap = Guid.Empty;
                OpenedFilePath = dlg.FileName;

                DataRepo.LoadFromDisk(dlg.FileName, this);

                zoomBox.Width = DataRepo.TileSize * DataRepo.TriCount;
                zoomBox.Height = (zoomBox.Width * 0.5f) * (float)Math.Tan(Math.PI * 60 / 180);
            }
        }

        private void ShowSaveDialog()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Map"; // Default file name
            dlg.DefaultExt = ".mf"; // Default file extension
            dlg.Filter = "MapFile (.mf)|*.mf"; // Filter files by extension

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            if(result == true)
            {
                OpenedFilePath = dlg.FileName;

                DataRepo.SaveToDisk(OpenedFilePath);
            }
        }

        private void zoomBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int zoomVal = (e.Delta > 0) ? 10 : -10;

            if(zoomBox.Width >= 0 && zoomBox.Height >= 0)
            {
                zoomBox.Width += zoomVal;
                zoomBox.Height += zoomVal;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();

            

            if(printDialog.ShowDialog() == true)
            {
                FixedDocument doc = Utils.GetFixedDocument(canvas, printDialog);

                if(Utils.ShowPrintPreview(doc) == true)
                {
                    
                }
            }
        }
    }
}
