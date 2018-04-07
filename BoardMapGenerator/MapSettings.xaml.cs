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
using System.Windows.Shapes;

namespace BoardMapGenerator
{
    /// <summary>
    /// Interaktionslogik für MapSettings.xaml
    /// </summary>
    public partial class MapSettings : Window
    {
        public int MapSize;
        public float TileSize;

        public MapSettings()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtSize.Text, out MapSize) || (!float.TryParse(txtTileSize.Text, out TileSize)))
            {
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
