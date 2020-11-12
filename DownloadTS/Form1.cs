using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        System.Diagnostics.Process m_p = new System.Diagnostics.Process();
        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists($"{txtName.Text}.mp4"))
            {
                MessageBox.Show($"Le fichier {txtName.Text}.mp4 existe déjà.\r\nVeuillez changer de nom ou l'effacer.");
                return;
            }
            string[] items = txtURL.Text.Split('/');
            System.Net.Http.HttpClient c = new System.Net.Http.HttpClient();
            var r = await c.GetAsync(txtURL.Text);
            if (r.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string s = await r.Content.ReadAsStringAsync();
                r.Dispose();

                string[] tmp = s.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                List<string> parts = new List<string>();
                foreach (string s2 in tmp)
                {
                    if (!s2.StartsWith("#") && !parts.Contains(s2) && !string.IsNullOrEmpty(s2))
                        parts.Add(s2);
                }
                progressBar1.Maximum = parts.Count;
                string name = Guid.NewGuid().ToString();
                using (StreamWriter sw = new StreamWriter(name))
                {
                    int count = 0;
                    foreach (string s2 in parts)
                    {
                        progressBar1.Value = ++count;
                        lblInfos.Text = $"Téléchargement partie {count}/{parts.Count}";
                        Application.DoEvents();
                        items[items.Length - 1] = s2;
                        r = await c.GetAsync(string.Join("/", items));
                        if (r.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            await r.Content.CopyToAsync(sw.BaseStream);
                        }
                    }
                }
                lblInfos.Text = $"Optimisation de la taille de la vidéo";
                Application.DoEvents();

                m_p.StartInfo.FileName = "ffmpeg.exe";
                m_p.StartInfo.Arguments = $"-i \"{name}\" {txtName.Text}.mp4";
                m_p.StartInfo.UseShellExecute = false;
                m_p.EnableRaisingEvents = true;
                m_p.Exited += delegate (object sender2, EventArgs e2)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        lblInfos.Text = "Terminé";
                        System.IO.File.Delete(name);
                    });
                };
                m_p.Start();
            }
        }
    }
}
