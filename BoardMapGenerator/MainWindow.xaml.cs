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
        public Tile[,] Map = new Tile[20,20];

        public Vector MapPos = new Vector(100f, 100f);

        Tile tile;

        public MainWindow()
        {
            InitializeComponent();

            bool offset = false;

            Map[0, 0] = new Tile();

            //Tile tile = Map[0, 0];

            //tile.Form.Fill = Brushes.LightBlue;
            //tile.Form.Stroke = Brushes.Black;
            //tile.Form.StrokeThickness = 1;
            //tile.Size = 50f;
            //tile.Position = MapPos;
            //tile.Rotation = 0f;

            //tile.AddToCanvas(canvas);
            //tile.Update();
            for(int x = 0; x < Map.GetLength(0); x++)
            {
                for (int y = 0; y < Map.GetLength(1); y++)
                {
                    Map[x, y] = new Tile();

                    Tile tile = Map[x, y];

                    tile.Form.Fill = Brushes.LightBlue;
                    tile.Form.Stroke = Brushes.Black;
                    tile.Form.StrokeThickness = 1;
                    tile.Size = 50f;
                    tile.Position = MapPos + new Vector((tile.Size * 0.5f + tile.Size) * x, offset ? tile.TriHeight * 2f * y + tile.TriHeight : tile.TriHeight * 2f * y);
                    tile.Rotation = 0f;

                    tile.AddToCanvas(canvas);
                    tile.Update();
                }

                offset = !offset;
            }
        }
    }
}
