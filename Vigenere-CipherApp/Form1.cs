using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Vigenere_CipherApp
{
    public partial class MainThread : Form
    {
        BackgroundWorker worker;
        string plaintext = "";
        string key = "";
        Thread slaveThread;

        public MainThread()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
        }

        private void InitializeBackgroundWorker()
        {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        private void EncryptVigenere(string plaintext, string key, BackgroundWorker bgWorker)
        {
            int keyIndex = 0;
            int keyLength = key.Length;

            StringBuilder encryptedText = new StringBuilder();

            for (int i = 0; i < plaintext.Length; i++)
            {
                if (bgWorker.CancellationPending)
                {
                    return;
                }

                char plainChar = plaintext[i];
                char keyChar = key[keyIndex];
                char encryptedChar = (char)(((plainChar + keyChar) % 26) + 'A');

                encryptedText.Append(encryptedChar);

                keyIndex = (keyIndex + 1) % keyLength;

                int progressPercentage = (i + 1) * 100 / plaintext.Length;
                bgWorker.ReportProgress(progressPercentage, encryptedText.ToString());

                // Menjalankan operasi slave thread secara eksplisit
                if (i == plaintext.Length / 2)
                {
                    slaveThread = new Thread(SlaveThreadFunction);
                    slaveThread.Start();
                }

                // Simulasi tugas yang memakan waktu
                Thread.Sleep(100);
            }

            // Mengembalikan hasil enkripsi
            bgWorker.ReportProgress(100, encryptedText.ToString());
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker = sender as BackgroundWorker;
            EncryptVigenere(plaintext, key, bgWorker);
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            progressBar1.Step = 1; // Tentukan ukuran langkah kemajuan
            progressBar1.PerformStep();
            textBox4.Text = e.UserState as string;
            textBox6.Text = $"{progressBar1.Value}% Complete";
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Operation was cancelled.");
            }
            else if (e.Error != null)
            {
                MessageBox.Show("An error occurred: " + e.Error.Message);
            }
        }
        private void SlaveThreadFunction()
        {
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"Slave Thread: Iteration {i}");
                Thread.Sleep(1000);
            }
        }

        // Metode untuk tombol Start
        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (!worker.IsBusy)
            {
                key = textBox1.Text;
                progressBar1.Value = 0;

                if (key.Length < plaintext.Length)
                {
                    key = ExtendKey(key);
                }

                textBox3.Text = key;
                worker.RunWorkerAsync();
            }
        }
        private void buttonBrowse_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    plaintext = File.ReadAllText(openFileDialog.FileName);
                    textBox2.Text = plaintext;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }
        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (worker.IsBusy)
            {
                worker.CancelAsync();
            }
            // Menghentikan slave thread jika sedang berjalan
            if (slaveThread != null && slaveThread.IsAlive)
            {
                slaveThread.Abort();
            }
        }

        // Metode untuk menambahkan kunci baru
        private void buttonTambah_Click(object sender, EventArgs e)
        {
            // Ambil teks asli dari textBox2 dan kunci baru dari textBox1
            string newKey = textBox1.Text;

            // Lakukan enkripsi ulang dengan kunci baru
            StringBuilder reencryptedText = new StringBuilder();
            int keyIndex = 0;
            int keyLength = newKey.Length;

            for (int i = 0; i < plaintext.Length; i++)
            {
                char plainChar = plaintext[i];
                char keyChar = newKey[keyIndex];
                char encryptedChar = (char)(((plainChar + keyChar) % 26) + 'A');
                reencryptedText.Append(encryptedChar);
                keyIndex = (keyIndex + 1) % keyLength;
            }

            // Tampilkan chipertext hasil enkripsi ulang di textBox5
            textBox5.Text = reencryptedText.ToString();
        }
        private string ExtendKey(string key)
        {
            StringBuilder extendedKey = new StringBuilder();
            for (int i = 0; i < plaintext.Length; i++)
            {
                extendedKey.Append(key[i % key.Length]);
            }
            return extendedKey.ToString();
        }
    }
}
