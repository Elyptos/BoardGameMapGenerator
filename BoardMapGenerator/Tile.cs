using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public Guid BackgroundImage { get; set; } = Guid.Empty;
        public Color BackgroundColor { get; set; } = Colors.LightBlue;

        private Grid canvas;
        private MainWindow windowRef;

        public Tile(MainWindow window)
        {
            windowRef = window;

            Form.MouseLeftButtonDown += OnLeftClick;
        }

        public void LoadTile(BinaryReader reader)
        {
            Vector pos = new Vector();
            pos.X = reader.ReadDouble();
            pos.Y = reader.ReadDouble();
            Position = pos;

            Rotation = (float)reader.ReadDouble();
            BackgroundImage = new Guid(reader.ReadBytes(16));

            Color c = new Color();

            c.A = 255;
            c.R = reader.ReadByte();
            c.G = reader.ReadByte();
            c.B = reader.ReadByte();

            BackgroundColor = c;
        }

        public void SaveTile(BinaryWriter writer)
        {
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write((double)Rotation);
            writer.Write(BackgroundImage.ToByteArray());
            writer.Write(BackgroundColor.R);
            writer.Write(BackgroundColor.G);
            writer.Write(BackgroundColor.B);
        }

        private void OnLeftClick(object sender, MouseButtonEventArgs e)
        {
            if(windowRef.InFillMode)
            {
                BackgroundColor = windowRef.SelectedColor != null ? windowRef.SelectedColor.Value : Colors.LightBlue;
            }

            if(windowRef.InSetImageMode)
            {
                BackgroundImage = windowRef.SelectedBitmap;
            }

            UpdateBackground();
        }

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

            UpdateBackground();
        }

        public void AddToCanvas(Grid c )
        {
            if (canvas != null)
                canvas.Children.Remove(Form);

            canvas = c;

            canvas.Children.Add(Form);
        }

        public void UpdateBackground()
        {
            if(BackgroundImage != Guid.Empty)
            {
                ImageBrush brush = GetBitmapBrush();

                if (brush != null)
                    Form.Fill = brush;
                else
                    Form.Fill = new SolidColorBrush(BackgroundColor);
            }
            else
            {
                Form.Fill = new SolidColorBrush(BackgroundColor);
            }
        }

        private ImageBrush GetBitmapBrush()
        {
            ImageBrush brush = new ImageBrush();

            if(BackgroundImage != Guid.Empty)
            {
                System.Drawing.Bitmap bgImage = windowRef.DataRepo.Images.Where(x => x.ID == BackgroundImage).Select(x => x.RawImage).FirstOrDefault();

                if(bgImage == null)
                {
                    return null;
                }

                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(bgImage.Width, bgImage.Height);
                System.Drawing.Color c;

                for(int x = 0; x < bgImage.Width; x++)
                {
                    for(int y = 0; y < bgImage.Height; y++)
                    {
                        c = bgImage.GetPixel(x, y);

                        float a = c.A / 255;
                        byte r = (byte)(BackgroundColor.R * (1f - a) + c.R);
                        byte g = (byte)(BackgroundColor.G * (1f - a) + c.G);
                        byte b = (byte)(BackgroundColor.B * (1f - a) + c.B);

                        bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(255, r, g, b));
                    }
                }

                brush.ImageSource = Utils.ConvertBitmap(bitmap);
            }

            return brush;
        }
    }
}
