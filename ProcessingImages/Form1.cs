using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace ProcessingImages
{
    public partial class Form1 : Form
    {
        private List<Bitmap> _bitmapsList = new List<Bitmap>();
        private Random _random = new Random();

        public Form1()
        {
            InitializeComponent();
        }

        //  Открываем файл изображения
        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var sw = Stopwatch.StartNew();
                menuStrip1.Enabled = trackBar1.Enabled = false;
                pictureBox1.Image = null;
                _bitmapsList.Clear();
                var newBitmap = new Bitmap(openFileDialog1.FileName);
                await Task.Run(() => { RunProcessing(newBitmap); });
                menuStrip1.Enabled = trackBar1.Enabled = true;
                sw.Stop();
                Text = $"Processing time: {sw.Elapsed}";

                //  Всплывающая подсказка
                toolTip1.Show("Move it to change image", 
                    trackBar1, -50, trackBar1.Height-50);
            }
        }

        //  Обработка изображения
        private void RunProcessing(Bitmap bitmap)
        {
            var pixels = GetPixels(bitmap);
            var pixelsInStep = (bitmap.Height * bitmap.Width) / 100;
            var currentPixelsSet = new List<Pixel>(pixels.Count - pixelsInStep);

            //  Создание отдельного изображения для каждого значения трекбара
            for (int i = 1; i < trackBar1.Maximum; i++)
            {
                //  Обращение к UI из другого потока
                this.Invoke(new Action(() =>
                {
                    Text = $"Processing... {i} %";
                }));

                for (int j = 0; j < pixelsInStep; j++)
                {
                    var index = _random.Next(pixels.Count);
                    currentPixelsSet.Add(pixels[index]);
                    pixels.RemoveAt(index);
                }

                var currentBitmap = new Bitmap(bitmap.Width, bitmap.Height);

                foreach (var pixel in currentPixelsSet)
                    currentBitmap.SetPixel(pixel.CoordPixel.X, pixel.CoordPixel.Y, pixel.ColorPixel);

                _bitmapsList.Add(currentBitmap);
            }
            _bitmapsList.Add(bitmap);
        }

        //  Формируем список пикселей
        private List<Pixel> GetPixels(Bitmap bitmap)
        {
            var pixels = new List<Pixel>(bitmap.Width * bitmap.Height);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    pixels.Add(new Pixel() 
                    { 
                        ColorPixel = bitmap.GetPixel(x, y),
                        CoordPixel=new Point() { X = x, Y = y }
                    });
                }
            }
            return pixels;
        }

        //  Прокрутка трекбара
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Text = trackBar1.Value.ToString()+"%";
            if (_bitmapsList == null || _bitmapsList.Count == 0)
                return;

            pictureBox1.Image = _bitmapsList[trackBar1.Value-1];
            //  Уберает подсказку
            toolTip1.RemoveAll();
        }

        //  Сохранение нового изображения
        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _bitmapsList[trackBar1.Value - 1].Save(saveFileDialog1.FileName);
            }
        }
    }
}
