using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BoardMapGenerator
{
    public class ImageDescriptor
    {
        public System.Drawing.Bitmap RawImage { get; set; }
        public ImageSource Image { get; set; }
        public string ImageName { get; set; }
        public Guid ID { get; set; }
    }

    public class DataRepo
    {
        public ObservableCollection<ImageDescriptor> Images = new ObservableCollection<ImageDescriptor>();
        public List<Tile> Map = new List<Tile>();
        public Vector MapPos = new Vector(100f, 100f);
        public int TriCount = 10;
        public float TileSize = 50f;

        public void SaveToDisk(string filePath)
        {
            string dirPath = File.Exists(filePath) ? Path.GetDirectoryName(filePath) : Path.GetDirectoryName(filePath) + "/" + Path.GetFileNameWithoutExtension(filePath);
            string nFilePath = dirPath + "/" + Path.GetFileName(filePath);
            string imagePath = dirPath + "/Images";

            if(!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
                Directory.CreateDirectory(imagePath);
            }

            using (BinaryWriter writer = new BinaryWriter(File.Open(nFilePath, FileMode.Create)))
            {
                writer.Write(MapPos.X);
                writer.Write(MapPos.Y);
                writer.Write(TriCount);
                writer.Write((double)TileSize);
                writer.Write(Images.Count);
                
                for(int i = 1; i < Images.Count; i++)
                {
                    writer.Write(Images[i].ImageName);
                    writer.Write(Images[i].ID.ToByteArray());

                    if (!File.Exists(imagePath + "/" + Images[i].ID.ToString() + ".png"))
                        Images[i].RawImage.Save(imagePath + "/" + Images[i].ID.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                }

                writer.Write(Map.Count);

                for(int i = 0; i < Map.Count; i++)
                {
                    Map[i].SaveTile(writer);
                }
            }
        }

        public void LoadFromDisk(string filePath, MainWindow windowRef)
        {
            string dirPath = Path.GetDirectoryName(filePath);
            string imagePath = dirPath + "/Images";

            windowRef.canvas.Children.Clear();

            Map.Clear();
            Images.Clear();

            Images.Add(new ImageDescriptor() { ImageName = "None" });

            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                Vector mapPos = new Vector();

                mapPos.X = reader.ReadDouble();
                mapPos.Y = reader.ReadDouble();

                MapPos = mapPos;

                TriCount = reader.ReadInt32();
                TileSize = (float)reader.ReadDouble();

                int imageCount = reader.ReadInt32();

                for(int i = 1; i < imageCount; i++)
                {
                    ImageDescriptor desc = new ImageDescriptor();
                    desc.ImageName = reader.ReadString();
                    desc.ID = new Guid(reader.ReadBytes(16));

                    string imgPath = imagePath + "/" + desc.ID.ToString() + ".png";

                    if (!File.Exists(imgPath))
                        continue;

                    System.Drawing.Image img = System.Drawing.Image.FromFile(imgPath);

                    if (img != null)
                    {
                        System.Windows.Media.Imaging.BitmapImage rawImg = Utils.ConvertBitmap((System.Drawing.Bitmap)img);
                        desc.RawImage = (System.Drawing.Bitmap)img;
                        desc.Image = rawImg;

                        Images.Add(desc);
                    }
                }

                int mapCount = reader.ReadInt32();

                for (int i = 0; i < mapCount; i++)
                {
                    Tile t = new Tile(windowRef);
                    t.LoadTile(reader);

                    t.Form.Stroke = Brushes.Black;
                    t.Form.StrokeThickness = 1;
                    t.Size = TileSize;

                    t.AddToCanvas(windowRef.canvas);
                    t.Update();

                    Map.Add(t);
                }
            }
        }
    }
}
