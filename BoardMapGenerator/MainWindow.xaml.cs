using System;
using System.Collections.Generic;
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
        public List<Tile> Map = new List<Tile>();

        public Vector MapPos = new Vector(100f, 100f);

        public int triCount = 10;

        public float tileSize = 50f;

        public MainWindow()
        {
            InitializeComponent();

            GenerateMap();
        }

        private void GenerateRow(Vector offset, bool flip, int count)
        {

            for (int i = 0; i < count; i++)
            {
                Tile tile = new Tile();

                tile.Form.Fill = Brushes.LightBlue;
                tile.Form.Stroke = Brushes.Black;
                tile.Form.StrokeThickness = 1;
                tile.Size = tileSize;
                tile.Position = MapPos + new Vector(tile.Size * i, 0) + offset;
                tile.Rotation = flip ? 180f : 0f;

                tile.AddToCanvas(canvas);
                tile.Update();

                Map.Add(tile);
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            MapSettings dlg = new MapSettings();

            dlg.Owner = this;

            if(dlg.ShowDialog() == true)
            {
                triCount = dlg.MapSize;
                tileSize = dlg.TileSize;

                GenerateMap();
            }
        }

        private void GenerateMap()
        {
            canvas.Children.Clear();
            Map.Clear();

            float triHeight = tileSize * 0.5f * (float)Math.Tan(Math.PI * 60 / 180);
            float midPointHeight = tileSize * 0.5f * (float)Math.Tan(Math.PI * 30 / 180);
            float radius = triHeight - midPointHeight;

            int index = 0;
            float xIndex = 0;

            while (triCount > 0)
            {
                GenerateRow(new Vector(tileSize * .5f * index, triHeight * index), false, triCount);
                GenerateRow(new Vector(tileSize * .5f + tileSize * xIndex, radius - midPointHeight + triHeight * index), true, --triCount);

                index++;
                xIndex += 0.5f;
            }
        }
    }
}
