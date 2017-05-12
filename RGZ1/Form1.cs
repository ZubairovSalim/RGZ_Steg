using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Threading;

namespace RGZ1
{
    public partial class Form1 : Form
    {
        string filter = "Image Files (*.jpg)|*.jpg|Image Files (.bmp)|*.bmp";
        string adress = String.Empty;
        Bitmap resultImage;
        DCT transforms = new DCT();
        string message;
        int mes_length;
        byte[,] mas_byte;
        List<byte[,]> mas_sepparated;
        List<double[,]> mas_dct;
        List<double[,]> mas_infiltrated;
        List<double[,]> mas_idct;
        List<byte[,]> mas_new_sepparated;
        byte[,] mas_new_byte;


        byte[,] mas2_byte;
        List<byte[,]> mas2_sepparated;
        List<double[,]> mas2_dct;

        List<Thread> threads = new List<Thread>();
        
        public Form1()
        {
            InitializeComponent();
            btn_Encrypt.Enabled = false;
            btn_Decrypt.Enabled = false;
            btn_Save.Enabled = false;
        }

        public void DoEncrypt()
        {
            try
            {
                ImageMaker image = new ImageMaker(adress);
                mas_byte = image.ArrayFilling();
                mas_sepparated = image.ArraySeparation(mas_byte);

                if (mas_sepparated.Count < message.Length * 8)
                {
                    progressBar1.Visible = false;
                    label4.Visible = false;
                    throw new ContainerFlaw("The message length is too large for the container");
                }

                mas_dct = transforms.DisCosTrans(mas_sepparated);
                mas_infiltrated = transforms.Infiltration(mas_dct, message, 4, 5, 5, 4);
                mas_idct = transforms.RevDisCosTrans(mas_infiltrated);
                mas_new_sepparated = transforms.Normalize(mas_idct);
                mas_new_byte = transforms.Add(mas_new_sepparated, image.Width, image.Height);
                resultImage = image.Introduction(mas_new_byte);
                progressBar1.Visible = false;
                label4.Visible = false;
                btn_Share.Enabled = true;
                btn_Encrypt.Enabled = true;
                btn_Decrypt.Enabled = true;
                btn_Save.Enabled = true;
            }
            catch (ContainerFlaw ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        public void DoDecrypt()
        {
            ImageMaker image1 = new ImageMaker(adress);
            mes_length = Convert.ToInt32(txt_Number.Text);
            mas2_byte = image1.ArrayFilling();
            mas2_sepparated = image1.ArraySeparation(mas2_byte);
            mas2_dct = transforms.DisCosTrans(mas2_sepparated);
            string mes = transforms.Extraction(mas2_dct, 4, 5, 5, 4, mes_length);
            txt_Decrypt.Text = mes;
            progressBar1.Visible = false;
            label4.Visible = false;
            btn_Share.Enabled = true;
            btn_Save.Enabled = false;
            btn_Encrypt.Enabled = true;
            btn_Decrypt.Enabled = true;
        }

        private void btn_Share_Click(object sender, EventArgs e)
        {
            OpenFileDialog opn = new OpenFileDialog();
            opn.Filter = filter;
            if (opn.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox1.Image = new Bitmap(opn.FileName);
                adress = opn.FileName;
            }
            if (adress != String.Empty)
            {
                btn_Encrypt.Enabled = true;
                btn_Decrypt.Enabled = true;
            }
        }

        private void btn_Encrypt_Click(object sender, EventArgs e)
        {
            try
            {
                if (txt_Encrypt.Text == String.Empty)
                {
                    throw new ContainerFlaw("Please, enter a hidding message");
                }
                progressBar1.Visible = true;
                label4.Visible = true;
                btn_Share.Enabled = false;
                btn_Encrypt.Enabled = false;
                btn_Decrypt.Enabled = false;
                message = txt_Encrypt.Text;
                ThreadStart deleg = new ThreadStart(DoEncrypt);
                Thread thr = new Thread(deleg);
                threads.Add(thr);
                thr.Start();
            }
            catch(ContainerFlaw ex)
            {
                MessageBox.Show(ex.Message);
            }
          
        }

        private void btn_Decrypt_Click(object sender, EventArgs e)
        {
            try
            {
                if(txt_Number.Text == String.Empty)
                {
                    throw new ContainerFlaw("Please, enter number of symbols");
                }

                else if(Convert.ToInt32(txt_Number.Text) == 0)
                {
                    throw new ContainerFlaw("Length of message is too small");
                }

                progressBar1.Visible = true;
                label4.Visible = true;
                btn_Share.Enabled = false;
                btn_Save.Enabled = false;
                btn_Encrypt.Enabled = false;
                btn_Decrypt.Enabled = false;
                ThreadStart deleg1 = new ThreadStart(DoDecrypt);
                Thread thr1 = new Thread(deleg1);
                threads.Add(thr1);
                thr1.Start();
            }
            catch(ContainerFlaw ex)
            {
                MessageBox.Show(ex.Message);
            }
          
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();

            string result_adress;
            save.DefaultExt = ".jpg";

            if (save.ShowDialog() == DialogResult.OK)
            {
                result_adress = save.FileName;

                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

                //System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                //System.Drawing.Imaging.Encoder myEncoder2 = System.Drawing.Imaging.Encoder.Compression;

                //EncoderParameters myEncoderParameters = new EncoderParameters(2);

                //EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
                //EncoderParameter myEncoderParameter2 = new EncoderParameter(myEncoder2,(long)EncoderValue.CompressionNone);

                //myEncoderParameters.Param[0] = myEncoderParameter;
                //myEncoderParameters.Param[1] = myEncoderParameter2;


                //resultImage.Save(result_adress, jpgEncoder, myEncoderParameters);
                resultImage.Save(result_adress);
            }
        }

        private void btn_Reset_Click(object sender, EventArgs e)
        {
            foreach (Thread t in threads)
            {
                t.Abort();
            }

            progressBar1.Visible = false;
            btn_Encrypt.Enabled = false;
            btn_Decrypt.Enabled = false;
            btn_Save.Enabled = false;
            pictureBox1.Image = null;
            txt_Encrypt.Text = String.Empty;
            txt_Number.Text = String.Empty;
            txt_Decrypt.Text = String.Empty;
            label4.Hide();
        }

        public class ContainerFlaw : Exception//исключение
        {
            public ContainerFlaw(string message) : base(message)
            {

            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        bool non = false;

        private void txt_Number_KeyDown(object sender, KeyEventArgs e)
        {            
            char symb = (char)e.KeyData;
            if (e.KeyCode == Keys.OemMinus)
                non = true;
            if (char.IsNumber(symb) == false)
                non = true;
            if (e.KeyCode == Keys.Back)
                non = false;          
        }

        private void txt_Number_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (non == true)
            {
                e.Handled = true;
                non = false;
            }

        }

        private void txt_Encrypt_TextChanged(object sender, EventArgs e)
        {
            label5.Text = txt_Encrypt.Text.Length.ToString() + " symbols";
        }

        private void txt_Encrypt_MouseEnter(object sender, EventArgs e)
        {
            tip_Encrypt.SetToolTip(txt_Encrypt, "Enter there text for hiding");
        }

        private void txt_Number_MouseEnter(object sender, EventArgs e)
        {
            tip_Number.SetToolTip(txt_Number, "Enter there number of symbols in the hidden message");
        }

        private void btn_Share_MouseEnter(object sender, EventArgs e)
        {
            tip_Share.SetToolTip(btn_Share, "Click there for choosing image in directory");
        }

        private void btn_Save_MouseEnter(object sender, EventArgs e)
        {
            tip_Save.SetToolTip(btn_Save, "CLick there for saving image in directory");
        }

        private void btn_Encrypt_MouseEnter(object sender, EventArgs e)
        {
            tip_Encrypt.SetToolTip(btn_Encrypt, "Click there for hiding message in choosen image");
        }

        private void btn_Decrypt_MouseEnter(object sender, EventArgs e)
        {
            tip_Decrypt.SetToolTip(btn_Decrypt, "Click there for extracting message from choosen image");
        }

        private void btn_Reset_MouseEnter(object sender, EventArgs e)
        {
            tip_Reset.SetToolTip(btn_Reset, "Click there for reset settings");
        }
    }
}
