using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;

namespace File_Transfer_Server
{
    public partial class Form1 : Form
    {

        public delegate void FileReceivedEventHandler(object source, string fileName);
        public event FileReceivedEventHandler NewFileReceived;

        //asenkron uygulamada kullanabilmek adına delegate kullandım.
        //event bir olay tanımlaması olduğu için FileReceivedEventHandler şeklinde bir event tanımladım.

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.NewFileReceived += new FileReceivedEventHandler(Form1_NewFileReceived);
            //Yeni dosya geldiğinde file received olayını tekrar çağırdım.
        }

        private void Form1_NewFileReceived(object sender, string fileName)
        {
            this.BeginInvoke(new Action(
                delegate ()
                {
                    MessageBox.Show("Yeni Dosya Alındı... \n" + fileName);
                    System.Diagnostics.Process.Start("explorer", @"C:\Users\user\Documents\Dosyalar");
                }));
        }

        //Bu kısımda yeni dosya alındığında messagebox ile yeni dosyanın alındığını bildirdim.
        //Alınan dosyanın adının yazdırılmasını sağladım.
        //Dosya alındıktan sonra nereye kaydedilmesi gerektiğini ayarladım.

        private void Button1_Click(object sender, EventArgs e)
        {
            int port = int.Parse(txtHost.Text);
            Task.Factory.StartNew(() => HandleIncomingFile(port));
            MessageBox.Show("Port Dinleniyor..." + port);

        }

        //Bu kısımda Start Listening butonuna basıldığında yapılması gereken işlemleri yazdım.
        //txthost kısmına girilen yazıyı int tipine dönüştürüp port değişkenine atadım.
        //Girilen port numarasını message box ile port dinleniyor şeklinde gösterdim.
        public void HandleIncomingFile(int port)
        {
            try
            {
                TcpListener tcpListener = new TcpListener(IPAddress.Any ,port);  //Tcp listener oluşturdum.
                tcpListener.Start();        //Tcp listenerı başlattım.
                while(true)
                {
                    Socket handlerSocket = tcpListener.AcceptSocket();   //Gelen soket bağlantısını kabul ettim.
                    if(handlerSocket.Connected)         //soket bağlantısı kurulduysa...
                    {
                        string fileName = string.Empty;     //filename kısmını empty hale getirdim.
                        NetworkStream networkStream = new NetworkStream(handlerSocket); //networkstream tanımladım. veri gönderip alabilmek için.
                        int thisRead = 0; //thisread değişkenini sıfırladım. 
                        int BlockSize = 1024;       //blocksize'ı 1024 olarak ayarladım.
                        byte[] dataByte = new byte[BlockSize];      //databyte ı blocksize kadar byte şeklinde oluşturdum.
                        lock (this)
                        {
                            string folderPath = @"C:\Users\user\Documents\Dosyalar";
                            handlerSocket.Receive(dataByte);

                            // alınan dosyayı soket üzerinden databyte kadar okunmasını sağladım.
                            
                            int fileNameLen = BitConverter.ToInt32(dataByte, 0);        //dosya ismi uzunluğunu integer tipinde aldım.
                            fileName = Encoding.ASCII.GetString(dataByte, 4, fileNameLen);     //dosya adını string şeklinde aldım.
                            Stream fileStream = File.OpenWrite(folderPath + fileName);      //yeni file stream oluşturup dosya adını yazdım.
                            File.WriteAllBytes(Path.Combine(folderPath, fileName), dataByte);       //dosya içine tüm byteları yazdırdım.

                           fileStream.Write(dataByte, 4 + fileNameLen, (1024 - (4 + fileNameLen))); 
                            while(true)
                            {
                                thisRead = networkStream.Read(dataByte, 0, BlockSize);  //dosya içeriğini okudum.
                                fileStream.Write(dataByte, 0, thisRead);        //okunan içeriği yazdırdım.
                                if(thisRead == 0)       //okuma sıfıra eşitse yani boş dosya ise çıkış yaptım.
                                {
                                    break;
                                } 
                            }
                            fileStream.Close();     //filestreami kapattım.
                        }
                        if (NewFileReceived != null)
                        {
                            NewFileReceived(this, fileName);    

                            //Yeni dosya gelmesi boşa eşit değilse yeni dosya geldi metodunu çağırdım.
                        }
                        handlerSocket = null;
                    }
                }
            }
            catch(Exception ex)
            {
                

            }
        }
    }
}
