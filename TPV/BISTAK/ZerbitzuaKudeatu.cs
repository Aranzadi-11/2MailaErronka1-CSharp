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
    public partial class ZerbitzuaKudeatu : Form
    {
        private readonly HttpClient client;
        private readonly int erreserbaId;
        private readonly int langileId;
        private readonly int mahaiaId;
        private List<Platerak> platerakLista;
        private List<Plateren_Osagaiak> platerOsagaiakLista;
        private List<Inbentarioa> inbentarioaLista;
        private List<Kategoria> kategoriaLista;
        private Dictionary<int, int> platerKopuru = new Dictionary<int, int>();

        public ZerbitzuaKudeatu(int erreserbaIdPasatua, int langileIdPasatua, int mahaiaIdPasatua)
        {
            InitializeComponent();
            client = new HttpClient();
            erreserbaId = erreserbaIdPasatua;
            langileId = langileIdPasatua;
            mahaiaId = mahaiaIdPasatua;
            _ = KargatuDatuak();
            btnEskaeraAmaitu.Click += BtnEskaeraAmaitu_Click;
        }

        private async Task KargatuDatuak()
        {
            try
            {
                platerakLista = await client.GetFromJsonAsync<List<Platerak>>("https://localhost:7236/api/Platerak");
                platerOsagaiakLista = await client.GetFromJsonAsync<List<Plateren_Osagaiak>>("https://localhost:7236/api/PlaterenOsagaiak");
                inbentarioaLista = await client.GetFromJsonAsync<List<Inbentarioa>>("https://localhost:7236/api/Inbentarioa");
                kategoriaLista = await client.GetFromJsonAsync<List<Kategoria>>("https://localhost:7236/api/Kategoria");
                PlaterakErakutsi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea datuak kargatzean: " + ex.Message);
            }
        }

        private void PlaterakErakutsi()
        {
            categoriasPanel.Controls.Clear();
            foreach (var k in kategoriaLista.OrderBy(k => k.id))
            {
                var kategoriaLabel = new Label
                {
                    Text = k.izena,
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    ForeColor = System.Drawing.Color.Black
                };
                categoriasPanel.Controls.Add(kategoriaLabel);

                var platoakPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                    WrapContents = true,
                    Dock = DockStyle.Top
                };

                var kategoriaPlaterak = platerakLista.Where(p => p.kategoriaId == k.id);
                foreach (var plato in kategoriaPlaterak)
                {
                    var btnPlato = PlaterakSortu(plato);
                    platoakPanel.Controls.Add(btnPlato);
                }

                categoriasPanel.Controls.Add(platoakPanel);
            }
        }

        private Button PlaterakSortu(Platerak plato)
        {
            var btn = new Button
            {
                Width = 180,
                Height = 120,
                Text = $"{plato.izena}\n0 +",
                Tag = plato,
                BackColor = System.Drawing.Color.White,
                TextAlign = System.Drawing.ContentAlignment.TopCenter
            };

            platerKopuru[plato.id] = 0;

            if (!PlatoEskuragarri(plato))
            {
                btn.BackColor = System.Drawing.Color.LightGray;
                btn.Enabled = false;
            }

            btn.Click += (s, e) =>
            {
                var platoSel = (Platerak)btn.Tag;
                if (platerKopuru[platoSel.id] == 0)
                {
                    if (PlatoEskuragarri(platoSel))
                        GehituPlato(platoSel);
                }
                else
                {
                    var res = MessageBox.Show("Gehiago gehitu (+) edo kendu (-)?", "Plato aukeraketa", MessageBoxButtons.YesNo);
                    if (res == DialogResult.Yes) GehituPlato(platoSel);
                    else KenPlato(platoSel);
                }

                btn.Text = $"{platoSel.izena}\n{platerKopuru[platoSel.id]} +";
                AktualizatuPlaterak();
            };

            return btn;
        }

        private bool PlatoEskuragarri(Platerak plato)
        {
            var osagaiak = platerOsagaiakLista.Where(po => po.plateraId == plato.id);
            foreach (var o in osagaiak)
            {
                var stock = inbentarioaLista.FirstOrDefault(i => i.id == o.osagaiaId)?.kantitatea ?? 0;
                if (stock < o.kantitatea) return false;
            }
            return true;
        }

        private void GehituPlato(Platerak plato)
        {
            var osagaiak = platerOsagaiakLista.Where(po => po.plateraId == plato.id);
            foreach (var o in osagaiak)
            {
                var stock = inbentarioaLista.FirstOrDefault(i => i.id == o.osagaiaId);
                if (stock != null) stock.kantitatea -= o.kantitatea;
            }
            platerKopuru[plato.id]++;
        }

        private void KenPlato(Platerak plato)
        {
            if (platerKopuru[plato.id] == 0) return;
            var osagaiak = platerOsagaiakLista.Where(po => po.plateraId == plato.id);
            foreach (var o in osagaiak)
            {
                var stock = inbentarioaLista.FirstOrDefault(i => i.id == o.osagaiaId);
                if (stock != null) stock.kantitatea += o.kantitatea;
            }
            platerKopuru[plato.id]--;
        }

        private void AktualizatuPlaterak()
        {
            foreach (FlowLayoutPanel panel in categoriasPanel.Controls.OfType<FlowLayoutPanel>())
            {
                foreach (Button btn in panel.Controls.OfType<Button>())
                {
                    var plato = (Platerak)btn.Tag;
                    if (PlatoEskuragarri(plato))
                    {
                        btn.Enabled = true;
                        btn.BackColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        if (platerKopuru[plato.id] == 0)
                        {
                            btn.Enabled = false;
                            btn.BackColor = System.Drawing.Color.LightGray;
                        }
                    }
                    btn.Text = $"{plato.izena}\n{platerKopuru[plato.id]} +";
                }
            }
        }

        private async void BtnEskaeraAmaitu_Click(object sender, EventArgs e)
        {
            try
            {
                decimal total = 0;
                var eskaerak = new List<Zerbitzu_Xehetasunak>();

                foreach (var p in platerKopuru)
                {
                    if (p.Value > 0)
                    {
                        var plato = platerakLista.First(pl => pl.id == p.Key);
                        total += plato.prezioa * p.Value;

                        eskaerak.Add(new Zerbitzu_Xehetasunak
                        {
                            zerbitzuaId = erreserbaId,
                            plateraId = plato.id,
                            kantitatea = p.Value,
                            prezioUnitarioa = plato.prezioa
                        });
                    }
                }

                var zerbitzu = new Zerbitzu
                {
                    langileId = langileId,
                    mahaiaId = mahaiaId,
                    eskaeraData = DateTime.Now,
                    egoera = "Eskatuta",
                    guztira = total
                };

                var res = await client.PostAsJsonAsync("https://localhost:7236/api/Zerbitzuak", zerbitzu);
                if (!res.IsSuccessStatusCode) MessageBox.Show("Errorea Zerbitzuan");

                foreach (var z in eskaerak)
                {
                    var r = await client.PostAsJsonAsync("https://localhost:7236/api/ZerbitzuXehetasunak", z);
                    if (!r.IsSuccessStatusCode) MessageBox.Show("Errorea Zerbitzu_Xehetasunetan");
                }

                MessageBox.Show("Eskaera amaitu da!");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea: " + ex.Message);
            }
        }
    }
}
