using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BoardMapGenerator
{
    //enum ETileType
    //{
    //    TILE_TYPE_TRI = 0,
    //    TILE_TYPE_RECT = 1,
    //    TILE_TYPE_PENT = 2
    //}

    public class Tile
    {
        public Polygon Form { get; set; } = new Polygon();
        public float Size { get; set; } = 50f;
        public Vector Position { get; set; } = new Vector();
        public float Rotation { get; set; } = 0f;
        //public ETileType Type { get { return _tileType; } set { _tileType = value; } }

        public float TriHeight { get { return (Size * 0.5f * (float)Math.Tan(Math.PI * 60 / 180)); } }
        public float MidPointHeight { get { return (Size * 0.5f * (float)Math.Tan(Math.PI * 30 / 180)); } }
        public float Radius { get { return (TriHeight - MidPointHeight); } }

        private Grid canvas;

        public void Update()
        {
            PointCollection points = new PointCollection();

            Point p = new Point(0.0f, Radius);
            Matrix rotMatrix = new Matrix();

            points.Add(p);

            for(int i = 1; i <= 2; i++)
            {
                rotMatrix = new Matrix();
                rotMatrix.Rotate(120 * i);

                points.Add(p * rotMatrix);
            }

            rotMatrix = new Matrix();
            rotMatrix.Rotate(Rotation);

            for(int i = 0; i < points.Count; i++)
            {
                points[i] = points[i] * rotMatrix;
                points[i] = points[i] + Position;
            }

            Form.Points = points;
        }

        public void AddToCanvas(Grid c )
        {
            if (canvas != null)
                canvas.Children.Remove(Form);

            canvas = c;

            canvas.Children.Add(Form);
        }
    }
}
