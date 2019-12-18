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


namespace File_Transfer
{
    public partial class Form1 : Form
    {

        private static string shortFileName = "";
        private static string fileName = "";
        public Form1()
        {
            InitializeComponent();

            txtIPAddress.Text = GetLocalIP();
        }

        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach(IPAddress ip in host.AddressList)
            {
                if(ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Btn_Browse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Dosya Aktarımı";
            dialog.ShowDialog();
            txtFile.Text = dialog.FileName;
            fileName = dialog.FileName;
            shortFileName = dialog.SafeFileName;

            
        }

        private void Btn_Send_Click(object sender, EventArgs e)
        {
            string ipAddress = txtIPAddress.Text;
            int port = int.Parse(txtHost.Text);
            string fileName = txtFile.Text;
            Task.Factory.StartNew(() => SendFile(ipAddress, port, fileName, shortFileName));
            MessageBox.Show("Dosya gönderiliyor...");
        }

        public void SendFile(string remoteHostIP, int remoteHostPort, string longFileName, string shortFileName)

        {
            try
            {
                if(!string.IsNullOrEmpty(remoteHostIP))
                {
                    byte[] fileNameByte = Encoding.ASCII.GetBytes(shortFileName);
                    byte[] fileData = File.ReadAllBytes(longFileName);
                    byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];
                    byte[] filenameLen = BitConverter.GetBytes(fileNameByte.Length);
                    filenameLen.CopyTo(clientData, 0);
                    fileNameByte.CopyTo(clientData, 4);
                    fileData.CopyTo(clientData, 4 + fileNameByte.Length);
                    TcpClient clientSocket = new TcpClient(remoteHostIP, remoteHostPort);
                    NetworkStream networkstream = clientSocket.GetStream();
                    networkstream.Write(clientData, 0, clientData.GetLength(0));
                    networkstream.Close();

                }
            }
            catch
            {

            }
        }

        private void TxtIPAddress_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void TxtHost_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
