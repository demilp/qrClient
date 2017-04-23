using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bypass;
using ZXing;

namespace QrClient
{
    public class QrClient
    {
        IBarcodeReader reader;
        FileSystemWatcher watcher;
        BypassClient client;
        public QrClient()
        {
            bool exit = false;
            if (!Directory.Exists(ConfigurationManager.AppSettings["sourceFolder"]))
            {
                Console.WriteLine("Directory "+ ConfigurationManager.AppSettings["sourceFolder"] + "does not exist");
                exit = true;
            }
            if (!Directory.Exists(ConfigurationManager.AppSettings["destinationFolder"]))
            {
                Console.WriteLine("Directory " + ConfigurationManager.AppSettings["destinationFolder"] + "does not exist");
                exit = true;
            }
            if(exit)
                return;
            client = new BypassClient(ConfigurationManager.AppSettings["ip"], int.Parse(ConfigurationManager.AppSettings["port"]), ConfigurationManager.AppSettings["delimiter"], "qrClient", "tool");
            reader = new BarcodeReader();
            watcher = new FileSystemWatcher();
            watcher.Filter = "*.*";
            watcher.Created += new FileSystemEventHandler(watcher_FileCreated);
            watcher.Path = ConfigurationManager.AppSettings["sourceFolder"];
            watcher.EnableRaisingEvents = true;
        }
        private void watcher_FileCreated(object sender, FileSystemEventArgs e)
        {
            string strFileExt = Path.GetExtension(e.FullPath);
            
            if (Regex.IsMatch(strFileExt, @"\.jpg|\.png", RegexOptions.IgnoreCase))
            {
                var b = (Bitmap)Bitmap.FromFile(e.FullPath);
                var result = reader.Decode(b);
                if (result != null)
                {
                    
                    if (result.ResultPoints[0].X < result.ResultPoints[2].X)
                    {
                        if (result.ResultPoints[0].Y < result.ResultPoints[2].Y)
                        {
                            b.RotateFlip(System.Drawing.RotateFlipType.Rotate270FlipNone);
                        }
                    }
                    else
                    {
                        if (result.ResultPoints[0].Y < result.ResultPoints[2].Y)
                        {
                            b.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
                        }
                        else
                        {
                            b.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone);
                        }
                    }
                    string fileName = result.Text + DateTime.Now.ToString("yyyyMMdd-HHmmss") + Path.GetExtension(e.FullPath);
                    b.Save(Path.Combine(ConfigurationManager.AppSettings["destinationFolder"], fileName));
                    client.SendData(result.Text+"|"+fileName, "qrListener");
                }
                b.Dispose();
                File.Delete(e.FullPath);
            }
        }

        private Bitmap RotateImage(Bitmap bmp, float angle)
        {
            Bitmap rotatedImage = new Bitmap(bmp.Width, bmp.Height);
            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                g.TranslateTransform(bmp.Width / 2, bmp.Height / 2); //set the rotation point as the center into the matrix
                g.RotateTransform(angle); //rotate
                g.TranslateTransform(-bmp.Width / 2, -bmp.Height / 2); //restore rotation point into the matrix
                g.DrawImage(bmp, new Point(0, 0)); //draw the image on the new bitmap
            }
            return rotatedImage;
        }

        public void Close()
        {
            if(client != null)
                client.Close();
        }
    }
}