using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TPV.MODELOAK;

namespace TPV.BISTAK
{
    public partial class ZerbitzuaHasi : Form
    {
        private readonly HttpClient http;
        private List<Erreserba> erreserbak;
        private readonly int langileId;

        public ZerbitzuaHasi(int langileIdPasatua)
        {
            InitializeComponent();
            http = new HttpClient();
            langileId = langileIdPasatua;
            KargatuOrduFiltroa();
            _ = KargatuGaurkoErreserbak();
        }

        private async Task KargatuGaurkoErreserbak()
        {
            try
            {
                string gaur = DateTime.Now.ToString("yyyy-MM-dd");
                erreserbak = await http.GetFromJsonAsync<List<Erreserba>>("https://localhost:7236/api/Erreserbak");

                var gaurkoak = erreserbak
                    .Where(e => e.erreserbaData.ToString("yyyy-MM-dd") == gaur)
                    .OrderBy(e => e.erreserbaData)
                    .ToList();

                AzalduErreserbak(gaurkoak);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea: " + ex.Message);
            }
        }

        private void AzalduErreserbak(List<Erreserba> lista)
        {
            erreserbakPanel.Controls.Clear();

            if (!lista.Any())
            {
                var lbl = new Label
                {
                    Text = "Ez dago erreserbarik aukeratutako orduan.",
                    AutoSize = true,
                    Dock = DockStyle.Top
                };
                erreserbakPanel.Controls.Add(lbl);
                return;
            }

            foreach (var e in lista)
            {
                Button btn = new Button
                {
                    Width = 300,
                    Height = 80,
                    Text = $"{e.izena}\n{e.erreserbaData:HH:mm} - {e.pertsonaKop} pertsona",
                    Tag = e
                };
                btn.Click += (s, a) =>
                {
                    var erreserbaSel = (Erreserba)btn.Tag;
                    var kudeatu = new ZerbitzuaKudeatu(erreserbaSel.id, langileId, erreserbaSel.mahaiaId);
                    kudeatu.Show();
                };
                erreserbakPanel.Controls.Add(btn);
            }
        }

        private void KargatuOrduFiltroa()
        {
            hourFilter.Items.Clear();
            hourFilter.Items.Add("Guztiak");

            string[] orduak = { "13:30", "15:00", "20:00", "21:30", "23:00" };

            foreach (var o in orduak)
                hourFilter.Items.Add(o);

            hourFilter.SelectedIndex = 0;
        }

        private void btnBilatu_Click(object sender, EventArgs e)
        {
            if (erreserbak == null) return;

            string auk = hourFilter.SelectedItem.ToString();

            var filtratuta = auk == "Guztiak"
                ? erreserbak
                : erreserbak.Where(x => x.erreserbaData.ToString("HH:mm") == auk).ToList();

            AzalduErreserbak(filtratuta);
        }
    }
}
