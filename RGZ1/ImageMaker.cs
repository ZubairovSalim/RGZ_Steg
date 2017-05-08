using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace RGZ1
{
    class ImageMaker//класс создания изображения
    {
        public Bitmap img;//объект для работы с растровыми изображениями
        public int Width;
        public int Height;
        public static Color[,] Colors;

        public ImageMaker(string URI)//конструктор
        {
            img = new Bitmap(URI);
            if (img.Height % 8 != 0)//если высота изображения в пикселях не кратна 8, то изменить высоту
            {
                img = HeightChanger(img);
            }
            if (img.Width % 8 != 0)//если ширина изображения в пикселях не кратна 8, то изменить ширину
            {
                img = WidthChanger(img);
            }
            Width = img.Width;
            Height = img.Height;
            Colors = ArrayRGB(img);
        }

        static Bitmap HeightChanger(Bitmap image)//метод, изменяющий значение высоты изображения на кратное 8
        {
            int HeightNew = image.Height;

            while (HeightNew % 8 != 0)//определение нужного количества столбцов
            {
                HeightNew++;
            }

            Color[,] colors = ArrayRGB(image);

            Bitmap imageNew = new Bitmap(image.Width, HeightNew);
            
            for(int i = 0;i<image.Width;i++)
                for(int j = 0;j<HeightNew;j++)
                {
                    if(j>=image.Height)
                    {
                        imageNew.SetPixel(i, j, colors[i, image.Height - 1]);
                        continue;
                    }
                    imageNew.SetPixel(i, j, colors[i, j]);
                }
            return imageNew;
        }

        static Bitmap WidthChanger(Bitmap image)//метод, изменяющий значение ширины иображения на кратное 8
        {
            int WidthNew = image.Width;

            while (WidthNew % 8 != 0)//определение нужного количества строк
            {
                WidthNew++;
            }

            Color[,] colors = ArrayRGB(image);

            Bitmap imageNew = new Bitmap(WidthNew, image.Height);

            for (int i = 0; i < WidthNew; i++)
                for (int j = 0; j < image.Height; j++)
                {
                    if (i >= image.Width)
                    {
                        imageNew.SetPixel(i, j, colors[image.Width-1, j]);
                        continue;
                    }
                    imageNew.SetPixel(i, j, colors[i, j]);
                }
            return imageNew;
        }

        

        static Color[,] ArrayRGB(Bitmap Example)//метод, получющий RGB модел каждого пикселя
        {
            Color[,] color = new Color[Example.Width, Example.Height];

            for(int i = 0;i<Example.Width;i++)
                for(int j = 0;j<Example.Height;j++)
                {
                    color[i, j] = Example.GetPixel(i, j);
                }
            return color;
        } 

        public byte[,] ArrayFilling()//метод, заполненяющий массив значениями цвета B-компоненты
        {
            byte[,] mas = new byte[Width, Height];

            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    mas[i, j] = Colors[i,j].B;
                }
            return mas;
        }

        public List<byte[,]> ArraySeparation(byte[,] mas)//метод, определяющий границы сегментов для разделения массива
        {
            List<byte[,]> Separated = new List<byte[,]>();//список сегментов 
            int SegDimension = 8;//размерность одного сегмента
            int SegCount = Width * Height / (SegDimension * SegDimension);//количество сегментов
            int y1 = 0;
            int y2 = SegDimension - 1;
            int x1;
            int x2;

            for(int s = 0; s < SegCount; s++)
            {
                x1 = SegDimension * s % Width;//формула для расчета начальной координаты по x
                x2 = x1 + SegDimension - 1;//формула для расчета конечной координаты по x
                Separated.Add(SegmentFilling(mas,x1,x2,y1,y2));
                if(x2==Width-1)//если по конечная координата по x равна ширине, то делаем спуск по y
                {
                    y1 += SegDimension;
                    y2 += SegDimension;
                }
            }
            return Separated;                    
        } 

        byte[,] SegmentFilling(byte[,] mas, int x1, int x2, int y1, int y2)//метод, заполненяющий сегменты
        {
            byte[,] matrix = new byte[8,8];
            for(int i = x1, a = 0; i <= x2; i++, a++)
                for(int j = y1, b = 0; j <= y2; j++, b++)
                {
                    matrix[a, b] = mas[i, j];
                }
            return matrix;
        }

        public Bitmap Introduction(byte[,] NewB)
        {
            Bitmap im_new = new Bitmap(Width, Height);

            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    im_new.SetPixel(i, j, Color.FromArgb(Colors[i,j].R, Colors[i, j].G, NewB[i, j]));
                }
            return im_new;
        }
    }
}
