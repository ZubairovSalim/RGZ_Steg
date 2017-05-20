using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGZ1
{
    class DCT
    {       
        const int N = 8;//размерность одного сегмента
        public static string binary_message = String.Empty;

        static double Coeffs(int k)
        {
            double c = 0.0;

            if (k == 0)//установление значений коэффициентов
            {
                c = 1.0 / Math.Sqrt(2.0);
            }
            else
            {
                c = 1.0;
            }

            return c; 
        }

        public List<double[,]> DisCosTrans(List<byte[,]> list)//метод, выполняющий дискретное косинусное преобразование всех сегментов
        {
            List<double[,]> listDCT = new List<double[,]>();//список для хранения новых массивов с коэффициентами дкп

            double per = 0;//переменная для хранения промежуточного значения
            
            for(int i = 0;i < list.Count;i++)
            {
                double[,] temp = new double[8, 8];

                for(int u = 0; u < 8; u++)
                {
                    for(int v = 0; v < 8; v++)
                    {                      
                        for(int x = 0; x < 8; x++)
                        {
                            for(int y = 0; y < 8; y++)
                            {
                                per += list[i][x, y] 
                                    * Math.Cos(Math.PI * u * (2 * x + 1) / 16.0)                                                      
                                    * Math.Cos(Math.PI * v * (2 * y + 1) / 16.0);
                            }
                        }
                        temp[u,v] = Coeffs(u)* Coeffs(v) * per / 4.0;
                        per = 0;
                    }

                }
                listDCT.Add(temp);
            }
            return listDCT;
        }

        public List<double[,]> Infiltration(List<double[,]> ListDCT,string message, int u1,int v1,int u2,int v2)
        {
            int P = 80;
            double w1;
            double w2;
            int z1;
            int z2;
            string bin = String.Empty;

            byte[] message_byte = Encoding.GetEncoding(1251).GetBytes(message);

            foreach (byte x in message_byte)
            {
                bin = Convert.ToString(x, 2);

                if(bin.Length==6)
                {
                    bin = "00" + bin;
                }

                if(bin.Length==7)
                {
                    bin = "0" + bin;
                }

                binary_message += bin;
                bin = String.Empty;
            }
                 
        
            for (int i = 0; i < binary_message.Length; i++)
            {
                w1 = Math.Abs(ListDCT[i][u1, v1]);
                w2 = Math.Abs(ListDCT[i][u2, v2]);

                if(ListDCT[i][u1, v1]>=0)
                {
                    z1 = 1;
                }
                else
                {
                    z1 = -1;
                }

                if (ListDCT[i][u2, v2] >= 0)
                {
                    z2 = 1;
                }
                else
                {
                    z2 = -1;
                }

                if(binary_message[i] == '0')
                {
                    if(w1-w2<=P)
                    {
                        w1 = P + w2 + 1;
                    }
                }
                else
                {
                    if (w1 - w2 >= -P)
                    {
                        w2 = P + w1 + 1;
                    }
                }

                ListDCT[i][u1, v1] = z1 * w1;
                ListDCT[i][u2, v2] = z2 * w2;
            }


            binary_message = String.Empty;
            return ListDCT;
        } 

        public string Extraction(List<double[,]> listDCT, int u1, int v1, int u2, int v2,int messageLength)
        {
            string Binary_message = String.Empty;

            for(int i = 0;i<messageLength*8;i++)
            {
                if(Math.Abs(listDCT[i][u1,v1])> Math.Abs(listDCT[i][u2, v2]))
                {
                    Binary_message += 0;
                }
                else
                {
                    Binary_message += 1; 
                }
            }

            string Binary_message_symbol = String.Empty;

            string Decimal_message = String.Empty;

            int count = 0;

            for (int i = 0,j = 0;i<messageLength*8;i++,j++)
            {
                Binary_message_symbol += Binary_message[i];

                if (j!=0 && j % 7 == 0)
                {
                    Decimal_message += Convert.ToByte(Binary_message_symbol, 2) + " ";
                    j = -1;
                    Binary_message_symbol = String.Empty;
                    count++;
                }
            }

            string[] message_string_byte = Decimal_message.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            byte[] message_byte = new byte[count];

            for (int i = 0; i < message_string_byte.Length; i++)
            {
                message_byte[i] = Convert.ToByte(message_string_byte[i]);
            }

            string message = Encoding.GetEncoding(1251).GetString(message_byte);

            return message;
        }

        public List<double[,]> RevDisCosTrans(List<double[,]> listDCT)
        {
            List<double[,]> listIDCT = new List<double[,]>();

            double per = 0;

            for(int i = 0;i<listDCT.Count;i++)
            {
                double[,] temp = new double[8, 8];

                for(int x = 0; x < 8; x++)
                {
                    for(int y = 0; y < 8; y++)
                    {
                        for(int u = 0; u < 8; u++)
                        {
                            for(int v = 0; v < 8; v++)
                            {                         
                                per += Coeffs(u) * Coeffs(v) * listDCT[i][u, v]
                                    * Math.Cos(Math.PI * u * (2 * x + 1) / 16.0)                                                                               
                                    * Math.Cos(Math.PI * v * (2 * y + 1) / 16.0);
                            }
                        }
                        temp[x, y] = Math.Round(per / 4.0);
                        per = 0;
                    }
                }
                listIDCT.Add(temp);
            }
            return listIDCT;
        }

        public List<byte[,]> Normalize(List<double[,]> IDCT)
        {
            double MIN = IDCT[0][0, 0];
            for(int c = 0;c< IDCT.Count;c++)
            {
                for(int i = 0;i< N;i++)
                    for(int j = 0;j< N;j++)
                    {
                        if (IDCT[c][i, j] < MIN)
                            MIN = IDCT[c][i, j];
                    }
            }

            double MAX = IDCT[0][0, 0];
            for (int c = 0; c < IDCT.Count; c++)
            {
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                    {
                        if (IDCT[c][i, j] > MAX)
                            MAX = IDCT[c][i, j];
                    }
            }

            List<byte[,]> Normal = new List<byte[,]>();

            for(int c = 0;c<IDCT.Count;c++)
            {
                byte[,] temp = new byte[N, N];

                for(int i = 0;i< N;i++)
                    for(int j = 0;j< N;j++)
                    {
                        if(IDCT[c][i,j] < 0 || IDCT[c][i, j]>255)
                        {
                            temp[i,j] = (byte)(255 * (IDCT[c][i, j] + Math.Abs(MIN)) / (MAX + Math.Abs(MIN)));                               
                        }
                        else
                        {
                            temp[i, j] = (byte)IDCT[c][i, j];
                        }
                    }
                Normal.Add(temp);
            }

            return Normal;
        }

        public byte[,] Add(List <byte[,]> list,int Width,int Height)//изменить
        {

            byte[,] mas = new byte[Width, Height];
            int s = 0;
            bool temp = false;
            int del = 1;

            for (int i = 0, a = 0; i < Width; i++, a++)
                for (int j = 0, b = 0; j < Height; j++, b++)
                {
                    if (b == 8)
                    {
                        b = 0;
                        s += Width / 8;
                    }
                    if (a == 8)
                    {
                        a = 0;
                        s += 1;
                    }

                    mas[i, j] = list[s][a, b];

                    if (j == Height - 1)
                    {
                        if (temp == false)
                        {
                            del = s;
                            temp = true;
                        }
                        s = s % del;
                    }
                }

            return mas;
        }

    }

    //class dct 
    //{
    //    public static double[,] _dct(byte[,] mas)
    //    {
    //        double[,] C = new double[8, 8];
    //        for(int i = 0;i<8;i++)
    //            for(int j = 0;j<8;j++)
    //            {
    //                if(i==0)
    //                {
    //                    C[i, j] = 1 / Math.Sqrt(8);
    //                }
    //                if(i>0)
    //                {
    //                    C[i, j] = 0.5 * Math.Cos((2 * j + 1) * i * Math.PI);
    //                }
    //            }

    //        double[,] Ct = new double[8, 8];
             
    //        for (int i = 0; i < 8; i++)
    //            for (int j = 0; j < 8; j++)
    //            {
    //                Ct[i, j] = C[j, i];
    //            }

    //        double[,] temp = new double[8, 8];
    //        for (int i = 0;i<8;i++)
    //            for(int j = 0;j<8;j++)
    //                for(int r = 0;r<8;r++)
    //                {
    //                    temp[i, j]+= mas[i, r] * Ct[r, j];
    //                }

    //        double[,] temp2 = new double[8, 8];
    //        for (int i = 0; i < 8; i++)
    //            for (int j = 0; j < 8; j++)
    //                for (int r = 0; r < 8; r++)
    //                {
    //                    temp2[i, j] += C[i, r] * temp[r, j];
    //                }

    //        return temp2;

    
}
