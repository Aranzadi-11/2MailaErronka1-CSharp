using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Forms;
using TPV.MODELOAK;

namespace TPV
{
    public partial class Login : Form
    {
        private int saiakerak = 3;
        private readonly string apiUrl = "https://localhost:7236/api/Langileak";

        public Login()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(500, 420);
            this.AutoScaleMode = AutoScaleMode.Font;
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string erabiltzailea = txtErabiltzailea.Text.Trim();
            string pasahitza = txtPasahitza.Text.Trim();

            if (string.IsNullOrEmpty(erabiltzailea) || string.IsNullOrEmpty(pasahitza))
            {
                MessageBox.Show("Mesedez, bete bi eremuak.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using HttpClient client = new HttpClient();
                var langileak = await client.GetFromJsonAsync<List<Langileak>>(apiUrl);

                if (langileak == null)
                {
                    MessageBox.Show("Ezin izan da APIarekin konektatu.", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var langile = langileak.FirstOrDefault(l =>
                    l.Erabiltzailea == erabiltzailea &&
                    l.Pasahitza == pasahitza
                );

                if (langile != null)
                {
                    if (langile.Aktibo == "Bai")
                    {
                        int langileId = langile.LangileId;

                        if (langile.RolaId == 1)
                        {
                            new SukaldariMenua(langileId).ShowDialog();
                        }
                        else
                        {
                            new ZerbitzariMenua(langileId).ShowDialog();
                        }

                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Erabiltzailea bajan emanda dago.\nJarri harremanetan administratzailearekin.", "Erabiltzailea ez aktiboa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    saiakerak--;
                    MessageBox.Show($"Erabiltzailea edo pasahitza okerra.\nSaiakerak: {saiakerak}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    if (saiakerak <= 0)
                    {
                        MessageBox.Show("Saiakera kopurua gaindituta.\nAplikazioa itxiko da.", "Blokeoa", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea APIarekin konektatzean:\n" + ex.Message, "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
