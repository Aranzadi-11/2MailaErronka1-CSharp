using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TPV.DTOak;
using TPV.MODELOAK;

namespace TPV.BISTAK
{
    public class PlaterPrevioDto
    {
        public int PlateraId { get; set; }
        public int Kantitatea { get; set; }
    }

    public partial class ZerbitzuaKudeatu : Form
    {
        private readonly HttpClient bezeroa;
        private readonly int erreserbaId;
        private readonly int langileId;
        private readonly int mahaiaId;

        private List<Platerak> platerak;
        private List<PlaterenOsagaiak> platerenosagaiak;
        private List<Inbentarioa> inbentarioa;
        private List<Kategoria> kategoriak;

        private readonly Dictionary<int, int> aukerak = new();
        private readonly Dictionary<int, Panel> panelak = new();

        public ZerbitzuaKudeatu(int eId, int lId, int mId)
        {
            InitializeComponent();
            bezeroa = new HttpClient();
            erreserbaId = eId;
            langileId = lId;
            mahaiaId = mId;

            _ = DenaKargatu();
            btnEskaeraAmaitu.Click += Amaitu;
        }

        private async Task DenaKargatu()
        {
            platerak = await bezeroa.GetFromJsonAsync<List<Platerak>>("https://localhost:7236/api/Platerak");
            platerenosagaiak = await bezeroa.GetFromJsonAsync<List<PlaterenOsagaiak>>("https://localhost:7236/api/PlaterenOsagaiak");
            inbentarioa = await bezeroa.GetFromJsonAsync<List<Inbentarioa>>("https://localhost:7236/api/Inbentarioa");
            kategoriak = await bezeroa.GetFromJsonAsync<List<Kategoria>>("https://localhost:7236/api/Kategoria");

            try
            {
                var prevPlaterak = await bezeroa.GetFromJsonAsync<List<PlaterPrevioDto>>(
                    $"https://localhost:7236/api/Zerbitzuak/erreserba/{erreserbaId}/platerak"
                );

                foreach (var p in prevPlaterak)
                {
                    aukerak[p.PlateraId] = p.Kantitatea;
                }
            }
            catch { }

            KargatuPlaterak();
            await StockEgiaztatu();
        }

        private async Task StockEgiaztatu()
        {
            inbentarioa = await bezeroa.GetFromJsonAsync<List<Inbentarioa>>("https://localhost:7236/api/Inbentarioa");

            foreach (var p in platerak)
            {
                if (!panelak.ContainsKey(p.Id)) continue;

                var pnl = panelak[p.Id];
                var txt = pnl.Controls.Find("txtKop", true).FirstOrDefault() as Label;
                var btnPlus = pnl.Controls.Find("btnPlus", true).FirstOrDefault() as Button;
                var btnMinus = pnl.Controls.Find("btnMinus", true).FirstOrDefault() as Button;

                if (txt == null || btnPlus == null || btnMinus == null) continue;

                int max = GehienezkoKopurua(p);

                if (!aukerak.ContainsKey(p.Id)) aukerak[p.Id] = 0;
                if (aukerak[p.Id] > max) aukerak[p.Id] = max;

                txt.Text = aukerak[p.Id].ToString();
                btnPlus.Enabled = aukerak[p.Id] < max;
                btnMinus.Enabled = aukerak[p.Id] > 0;

                if (max == 0) pnl.BackColor = Color.Red;
                else if (max <= 5) pnl.BackColor = Color.Yellow;
                else pnl.BackColor = Color.White;
            }
        }

        private void KargatuPlaterak()
        {
            kategoriakPanel.Controls.Clear();

            foreach (var k in kategoriak.OrderBy(x => x.Id))
            {
                var lblKat = new Label
                {
                    Text = k.Izena,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    Margin = new Padding(10, 20, 10, 5)
                };

                kategoriakPanel.Controls.Add(lblKat);

                var pnlKat = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                    WrapContents = true,
                    Margin = new Padding(20, 5, 20, 25)
                };

                foreach (var p in platerak.Where(x => x.KategoriaId == k.Id))
                {
                    var pnl = new Panel
                    {
                        Width = 200,
                        Height = 260,
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle,
                        Margin = new Padding(10)
                    };

                    var lblIzena = new Label
                    {
                        Text = p.Izena,
                        Dock = DockStyle.Top,
                        Height = 35,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold)
                    };

                    var lblPrezio = new Label
                    {
                        Text = p.Prezioa.ToString("0.00") + " €",
                        Dock = DockStyle.Top,
                        Height = 25,
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    var pnlKontrol = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Bottom,
                        Height = 45
                    };

                    var btnMinus = new Button { Text = "-", Width = 50, Name = "btnMinus" };
                    var txtKop = new Label
                    {
                        Text = aukerak.ContainsKey(p.Id) ? aukerak[p.Id].ToString() : "0",
                        Width = 50,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Name = "txtKop"
                    };
                    var btnPlus = new Button { Text = "+", Width = 50, Name = "btnPlus" };

                    pnlKontrol.Controls.Add(btnMinus);
                    pnlKontrol.Controls.Add(txtKop);
                    pnlKontrol.Controls.Add(btnPlus);

                    pnl.Controls.Add(pnlKontrol);
                    pnl.Controls.Add(lblPrezio);
                    pnl.Controls.Add(lblIzena);

                    if (!aukerak.ContainsKey(p.Id)) aukerak[p.Id] = 0;
                    panelak[p.Id] = pnl;

                    btnPlus.Click += async (_, __) =>
                    {
                        await StockEgiaztatu();
                        if (aukerak[p.Id] < GehienezkoKopurua(p))
                            aukerak[p.Id]++;
                        await StockEgiaztatu();
                    };

                    btnMinus.Click += async (_, __) =>
                    {
                        await StockEgiaztatu();
                        if (aukerak[p.Id] > 0)
                            aukerak[p.Id]--;
                        await StockEgiaztatu();
                    };

                    pnlKat.Controls.Add(pnl);
                }

                kategoriakPanel.Controls.Add(pnlKat);
            }
        }

        private int GehienezkoKopurua(Platerak p)
        {
            var osa = platerenosagaiak.Where(o => o.PlateraId == p.Id);
            int max = int.MaxValue;

            foreach (var o in osa)
            {
                var s = inbentarioa.FirstOrDefault(i => i.Id == o.InbentarioaId);
                if (s == null || s.Kantitatea <= 0) return 0;

                int balioa = (int)Math.Floor(s.Kantitatea / o.Kantitatea);
                if (balioa < max) max = balioa;
            }

            return max;
        }

        private async void Amaitu(object sender, EventArgs e)
        {
            if (aukerak.Values.All(v => v == 0))
            {
                MessageBox.Show("Ez duzu ezer eskatu.");
                return;
            }

            var eskaria = new ZerbitzuaEskariaDto
            {
                LangileId = langileId,
                MahaiaId = mahaiaId,
                ErreserbaId = erreserbaId,
                Platerak = aukerak
                    .Where(x => x.Value > 0)
                    .Select(x => new PlateraEskariaDto
                    {
                        PlateraId = x.Key,
                        Kantitatea = x.Value
                    })
                    .ToList()
            };

            var res = await bezeroa.PostAsJsonAsync(
                "https://localhost:7236/api/Zerbitzuak/egin",
                eskaria
            );

            var emaitza = await res.Content.ReadFromJsonAsync<ZerbitzuaEmaitzaDto>();

            if (!emaitza.Ondo)
            {
                var mezua = string.Join(
                    "\n",
                    emaitza.Erroreak.Select(e => $"{e.PlateraIzena} ez dago eskuragarri")
                );

                MessageBox.Show("Eskaera ezin izan da egin:\n" + mezua);
                await StockEgiaztatu();
                return;
            }

            MessageBox.Show("Eskaera ondo egin da!");
            Close();
        }
    }
}
