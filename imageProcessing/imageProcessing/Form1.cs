using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace imageProcessing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public int ResimKenari => trackBar1.Value;
        private Bitmap KamufleEt(in Bitmap image)
        {
            Bitmap copyImage = image.Clone() as Bitmap;

            List<Color> renkler = GetDifferentColorListInBitmap(copyImage, 10);
            List<BitmapSatirDetail> KenarListesi = GetBorderCoordinates(copyImage);

            RenkListesi(copyImage, KenarListesi, renkler);

            return copyImage;
        }
        private void RenkListesi(Bitmap image, List<BitmapSatirDetail> KenarListesi, List<Color> colors)
        {
            Random random = new Random();

            for (int i = 0; i < KenarListesi.Count; i++)
            {
                for (int j = KenarListesi[i].IlkKolon; j < KenarListesi[i].SonKolon; j++)
                {
                    var randomNumber = random.Next(0, colors.Count());

                    image.SetPixel(i, j, colors[randomNumber]);
                }
            }
        }
        private List<BitmapSatirDetail> GetBorderCoordinates(Bitmap image)
        {
            List<BitmapSatirDetail> KenarListesi = new List<BitmapSatirDetail>();

            for (int i = 1; i < image.Width - 1; i++)
            {
                BitmapSatirDetail SatirDetail = new BitmapSatirDetail();

                SatirDetail.Satir = i;

                for (int j = 1; j < image.Height - 1; j++)
                {
                    Color cr = image.GetPixel(i + 1, j);
                    Color cl = image.GetPixel(i - 1, j);
                    Color cu = image.GetPixel(i, j - 1);
                    Color cd = image.GetPixel(i, j + 1);
                    int dx = cr.R - cl.R;
                    int dy = cd.R - cu.R;
                    double formul = Math.Sqrt(dx * dx / 4 + dy * dy / 4);

                    if (formul > ResimKenari)
                    {
                        if (SatirDetail.IlkKolon == 0)
                        {
                            SatirDetail.IlkKolon = j;
                        }
                        else
                        {
                            SatirDetail.SonKolon = j;
                        }
                    }
                }

                KenarListesi.Add(SatirDetail);
            }

            return KenarListesi;
        }
        private List<Color> GetDifferentColorListInBitmap(Bitmap image, int count)
        {
            List<Color> colors = new List<Color>();

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var color = image.GetPixel(x, y);
                    colors.Add(color);
                }
            }
            return colors
                .GroupBy(i => i)
                .Select(group => new
                {
                    Key = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(i => i.Count)
                .Take(count)
                .Select(i => i.Key)
                .ToList();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
        }
        private Bitmap BinaryYap(Bitmap image)
        {
            Bitmap gri = griYap(image);
            int temp = 0;
            int esik = 155;
            Color renk;
            for (int i = 0; i < gri.Height - 1; i++)
            {
                for (int j = 0; j < gri.Width - 1; j++)
                {
                    temp = gri.GetPixel(j, i).R;

                    if (temp < esik)
                    {
                        renk = Color.FromArgb(0, 0, 0);
                        gri.SetPixel(j, i, renk);
                    }
                    else
                    {
                        renk = Color.FromArgb(255, 255, 255);
                        gri.SetPixel(j, i, renk);
                    }
                }
            }
            return gri;
        }
        private Bitmap griYap(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Height - 1; i++)
            {
                for (int j = 0; j < bmp.Width - 1; j++)
                {
                    int deger = (bmp.GetPixel(j, i).R + bmp.GetPixel(j, i).G + bmp.GetPixel(j, i).B) / 3;

                    Color renk;
                    renk = Color.FromArgb(deger, deger, deger);
                    bmp.SetPixel(j, i, renk);
                }
            }
            return bmp;
        }    
        OpenFileDialog file = new OpenFileDialog();
        private void button2_Click(object sender, EventArgs e)
        {          
            var dialogResult = file.ShowDialog();
           
            Bitmap image = new Bitmap(file.FileName);
            pictureBox1.Image = image;
            Bitmap newMaskAppliedImage = KamufleEt(in image);
            pictureBox2.Image = newMaskAppliedImage;
        }
        class BitmapSatirDetail
        {
            public int Satir { get; set; }
            public int IlkKolon { get; set; }
            public int SonKolon { get; set; }
        }
        private Bitmap sobelEdgeDetection(Bitmap image)
        {
            Bitmap gri = griYap(image);
            Bitmap buffer = new Bitmap(gri.Width, gri.Height);
            Color renk;
            int valX, valY, gradient;
            int[,] GX = new int[3, 3];
            int[,] GY = new int[3, 3];

            //Yatay yönde kenar
            GX[0, 0] = -1; GX[0, 1] = 0; GX[0, 2] = 1;
            GX[1, 0] = -2; GX[1, 1] = 0; GX[1, 2] = 2;
            GX[2, 0] = -1; GX[2, 1] = 0; GX[2, 2] = 1;

            //Düşey yönde kenar
            GY[0, 0] = -1; GY[0, 1] = -2; GY[0, 2] = -1;
            GY[1, 0] = 0; GY[1, 1] = 0; GY[1, 2] = 0;
            GY[2, 0] = 1; GY[2, 1] = 2; GY[2, 2] = 1;


            for (int i = 0; i < gri.Height; i++)
            {
                for (int j = 0; j < gri.Width; j++)
                {
                    if(i==0 || i==gri.Height-1 || j==0 || j == gri.Width - 1)
                    {
                        renk = Color.FromArgb(155, 155, 150);
                        buffer.SetPixel(j, i, renk);
                        valX = 0;
                        valY = 0;                           
                    }
                    else
                    {
                        valX = gri.GetPixel(j - 1, i - 1).R * GX[0, 0] +
                            gri.GetPixel(j, i-1).R * GX[0, 1] +
                            gri.GetPixel(j + 1, i - 1).R * GX[0, 2] +
                            gri.GetPixel(j - 1, i).R * GX[1, 0] +
                            gri.GetPixel(j, i).R * GX[1, 1] +
                            gri.GetPixel(j +1 , i).R * GX[1, 2] +
                            gri.GetPixel(j - 1, i + 1).R * GX[2, 0] +
                            gri.GetPixel(j, i +1).R * GX[2, 1] +
                            gri.GetPixel(j + 1, i + 1).R * GX[2, 2];

                        valY = gri.GetPixel(j - 1, i - 1).R * GY[0, 0] +
                           gri.GetPixel(j, i - 1).R * GY[0, 1] +
                           gri.GetPixel(j+1, i - 1).R * GY[0, 2] +
                           gri.GetPixel(j - 1, i).R * GY[1, 0] +
                           gri.GetPixel(j, i).R * GY[1, 1] +
                           gri.GetPixel(j + 1, i).R * GY[1, 2] +
                           gri.GetPixel(j - 1, i + 1).R * GY[2, 0] +
                           gri.GetPixel(j, i + 1).R * GY[2, 1] +
                           gri.GetPixel(j + 1, i + 1).R * GY[2, 2];

                        gradient = (int)(Math.Abs(valX) + Math.Abs(valY));

                        if (gradient < 0)
                        {
                            gradient = 0;
                        }
                        if (gradient > 255)
                        {
                            gradient = 255;
                        }

                        renk = Color.FromArgb(gradient, gradient, gradient);
                        buffer.SetPixel(j, i, renk);
                    }
                }
            }
            return buffer;
        }
        private void button3_Click(object sender, EventArgs e)
        {           
            Bitmap image = new Bitmap(pictureBox1.Image);
            Bitmap sobel = sobelEdgeDetection(image);
            pictureBox3.Image = sobel;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }
    }
}
