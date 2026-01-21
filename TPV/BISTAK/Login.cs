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
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(500, 420);
            AutoScaleMode = AutoScaleMode.Font;
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

                if (langile == null)
                {
                    saiakerak--;
                    MessageBox.Show($"Erabiltzailea edo pasahitza okerra.\nSaiakerak: {saiakerak}", "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    if (saiakerak <= 0)
                    {
                        MessageBox.Show("Saiakera kopurua gaindituta.\nAplikazioa itxiko da.", "Blokeoa", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        Application.Exit();
                    }

                    return;
                }

                if (langile.Aktibo != "Bai")
                {
                    MessageBox.Show("Erabiltzailea bajan emanda dago.\nJarri harremanetan administratzailearekin.", "Erabiltzailea ez aktiboa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int langileId = langile.Id;
                int rolaId = langile.RolaId ?? 0;

                if (rolaId == 1)
                {
                    new SukaldariMenua(langileId).ShowDialog();
                    Close();
                    return;
                }

                if (rolaId == 2)
                {
                    new ZerbitzariMenua(langileId).ShowDialog();
                    Close();
                    return;
                }

                if (rolaId == 3 || rolaId == 4)
                {
                    bool sukaldariModuan = false;

                    while (true)
                    {
                        DialogResult emaitza = !sukaldariModuan
                            ? new ZerbitzariMenua(langileId, true).ShowDialog()
                            : new SukaldariMenua(langileId, true).ShowDialog();

                        if (emaitza == DialogResult.Retry)
                        {
                            sukaldariModuan = !sukaldariModuan;
                            continue;
                        }

                        break;
                    }

                    Close();
                    return;
                }

                new ZerbitzariMenua(langileId).ShowDialog();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea APIarekin konektatzean:\n" + ex.Message, "Errorea", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
