using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TPV.MODELOAK;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using TPV.DTOak;
{
    
}

namespace TPV.BISTAK
{
    public partial class FakturaSortu : Form
    {
        private readonly HttpClient bezeroa;
        private readonly int langileId;
        private const string ApiOinarria = "http://192.168.1.117:5001";

        private List<Zerbitzuak> zerbitzuak = new();

        public FakturaSortu(int langileIdPasatua)
        {
            InitializeComponent();
            bezeroa = new HttpClient();
            langileId = langileIdPasatua;
            _ = KargatuZerbitzatuak();
        }

        private async Task KargatuZerbitzatuak()
        {
            try
            {
                var denak = await bezeroa.GetFromJsonAsync<List<Zerbitzuak>>($"{ApiOinarria}/api/Zerbitzuak");

                zerbitzuak = (denak ?? new List<Zerbitzuak>())
                    .Where(z => string.Equals(z.Egoera, "Zerbitzatuta", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(z => z.Id)
                    .ToList();

                AzalduZerbitzuak(zerbitzuak);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea: " + ex.Message);
            }
        }

        private void AzalduZerbitzuak(List<Zerbitzuak> lista)
        {
            zerbitzuakPanel.Controls.Clear();

            if (lista == null || !lista.Any())
            {
                lblMezua.Text = "Ez dago 'Zerbitzatuta' egoerako zerbitzurik.";
                zerbitzuakPanel.Controls.Add(lblMezua);
                return;
            }

            foreach (var z in lista)
            {
                zerbitzuakPanel.Controls.Add(SortuZerbitzuTxartela(z));
            }
        }

        private Control SortuZerbitzuTxartela(Zerbitzuak z)
        {
            var txartela = new Panel
            {
                Width = 360,
                Height = 180,
                BackColor = Color.White,
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            var etiketa = new Label
            {
                Dock = DockStyle.Top,
                Height = 75,
                Font = new System.Drawing.Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10),
                Text = $"Zerbitzua ID: {z.Id}\nEgoera: {z.Egoera}"
            };

            var btnTicketDeskargatu = new Button
            {
                Dock = DockStyle.Top,
                Height = 45,
                Text = "Ticket-a ireki",
                BackColor = Color.Goldenrod,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            btnTicketDeskargatu.FlatAppearance.BorderSize = 0;

            var btnOrdaindu = new Button
            {
                Dock = DockStyle.Top,
                Height = 45,
                Text = "Ordaindu",
                BackColor = Color.Black,
                ForeColor = Color.Goldenrod,
                FlatStyle = FlatStyle.Flat
            };
            btnOrdaindu.FlatAppearance.BorderSize = 0;

            btnTicketDeskargatu.Click += async (_, __) => await TicketSortuEtaIreki(z.Id);
            btnOrdaindu.Click += async (_, __) => await OrdainduMarkatu(z.Id);

            txartela.Controls.Add(btnOrdaindu);
            txartela.Controls.Add(btnTicketDeskargatu);
            txartela.Controls.Add(etiketa);

            return txartela;
        }

        private async Task TicketSortuEtaIreki(int zerbitzuId)
        {
            var erantzuna = MessageBox.Show(
                "Zerbitzuaren ticket-a sortu eta irekiko da. Seguru?",
                "Baieztatu",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (erantzuna != DialogResult.Yes) return;

            try
            {
                var xehetasunak = await bezeroa.GetFromJsonAsync<List<ZerbitzuXehetasunak>>($"{ApiOinarria}/api/ZerbitzuXehetasunak");

                var lerroak = (xehetasunak ?? new List<ZerbitzuXehetasunak>())
                    .Where(x => x.ZerbitzuaId == zerbitzuId)
                    .ToList();

                if (!lerroak.Any())
                {
                    MessageBox.Show("Ez dago zerbitzu honetako daturik.");
                    return;
                }

                var platerak = await bezeroa.GetFromJsonAsync<List<Platerak>>($"{ApiOinarria}/api/Platerak");
                var platerMapa = (platerak ?? new List<Platerak>())
                    .GroupBy(p => p.Id)
                    .ToDictionary(g => g.Key, g => g.First());

                var itemak = lerroak
                    .GroupBy(l => l.PlateraId)
                    .Select(g =>
                    {
                        var plateraId = g.Key;
                        var kopurua = g.Sum(x => x.Kantitatea);

                        platerMapa.TryGetValue(plateraId, out var p);

                        return new TicketDTO
                        {
                            PlateraId = plateraId,
                            Izena = p?.Izena ?? $"Platera {plateraId}",
                            Kantitatea = kopurua,
                            PrezioaUnitatea = p?.Prezioa ?? 0m
                        };
                    })
                    .OrderBy(i => i.Izena)
                    .ToList();

                var mahaigaina = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                var karpetaNagusia = System.IO.Path.Combine(mahaigaina, "Ticket-ak");
                var egunKarpeta = System.IO.Path.Combine(karpetaNagusia, DateTime.Now.ToString("dd-MM-yyyy"));
                System.IO.Directory.CreateDirectory(egunKarpeta);

                var fitxIzena = $"ID{zerbitzuId}.pdf";
                var bidea = System.IO.Path.Combine(egunKarpeta, fitxIzena);

                var modua = "Eskudirua";
                decimal jasotakoa = 0m;

                SortuPdfTicket(bidea, zerbitzuId, itemak, modua, jasotakoa);

                try
                {
                    Process.Start(new ProcessStartInfo(bidea) { UseShellExecute = true });
                }
                catch
                {
                }

                MessageBox.Show($"Ticket-a sortuta:\n{bidea}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea: " + ex.Message);
            }
        }

        private async Task OrdainduMarkatu(int zerbitzuId)
        {
            var erantzuna = MessageBox.Show(
                "Zerbitzua ordainduta bezala markatuko da. Seguru?",
                "Baieztatu",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (erantzuna != DialogResult.Yes) return;

            try
            {
                var patchEginDa = await EgoeraPatch(zerbitzuId, "Ordainduta");

                if (!patchEginDa)
                {
                    var zerbitzua = await bezeroa.GetFromJsonAsync<Zerbitzuak>($"{ApiOinarria}/api/Zerbitzuak/{zerbitzuId}");
                    if (zerbitzua == null)
                    {
                        MessageBox.Show("Ezin izan da zerbitzua aurkitu.");
                        return;
                    }

                    zerbitzua.Egoera = "Ordainduta";
                    var put = await bezeroa.PutAsJsonAsync($"{ApiOinarria}/api/Zerbitzuak/{zerbitzuId}", zerbitzua);
                    put.EnsureSuccessStatusCode();
                }

                await KargatuZerbitzatuak();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errorea: " + ex.Message);
            }
        }

        private async Task<bool> EgoeraPatch(int zerbitzuId, string egoeraBerria)
        {
            try
            {
                var payload = JsonSerializer.Serialize(new { egoera = egoeraBerria });
                var edukia = new StringContent(payload, Encoding.UTF8, "application/json");

                var eskaera = new HttpRequestMessage(new HttpMethod("PATCH"), $"{ApiOinarria}/api/Zerbitzuak/{zerbitzuId}")
                {
                    Content = edukia
                };

                var erantzuna = await bezeroa.SendAsync(eskaera);
                return erantzuna.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private void SortuPdfTicket(string bidea, int zerbitzuId, List<TicketDTO> itemak, string modua, decimal jasotakoa)
        {
            const string izenburua = "ABEJ JATETXEA";
            const string cif = "CIF: P0801900B";
            const string herria = "Madrid";
            const string tel = "Tel: 666 777 888";

            const decimal ivaTasa = 0.10m;

            decimal subtotal = 0m;
            var lineas = new List<(int kop, string prod, decimal ud, decimal tot)>();

            foreach (var it in itemak)
            {
                var tot = it.Kantitatea * it.PrezioaUnitatea;
                subtotal += tot;
                lineas.Add((it.Kantitatea, it.Izena, it.PrezioaUnitatea, tot));
            }

            decimal iva = Math.Round(subtotal * ivaTasa, 2, MidpointRounding.AwayFromZero);
            decimal guztira = subtotal + iva;

            decimal itzulia = 0m;
            bool pagoInformado = jasotakoa > 0m;
            if (pagoInformado) itzulia = Math.Max(0m, jasotakoa - guztira);

            var dokumentua = new PdfDocument();
            dokumentua.Info.Title = $"Ticket ID{zerbitzuId}";

            var orria = dokumentua.AddPage();
            orria.Width = 220;

            int baseLines = 4 + 3 + 2 + 1 + lineas.Count + 5 + 3 + 2;
            int extraWrap = lineas.Sum(l => (l.prod?.Length ?? 0) > 16 ? 1 : 0);

            double lineH = 11.5;
            orria.Height = Math.Max(420, (baseLines + extraWrap) * lineH + 40);

            var g = XGraphics.FromPdfPage(orria);

            var fTitle = new XFont("Courier New", 12, XFontStyle.Bold);
            var fText = new XFont("Courier New", 9, XFontStyle.Regular);
            var fBold = new XFont("Courier New", 9, XFontStyle.Bold);
            var fBig = new XFont("Courier New", 14, XFontStyle.Bold);

            double y = 14;
            double left = 10;
            double width = orria.Width - 20;

            void PrintLeft(string t, XFont f)
            {
                g.DrawString(t, f, XBrushes.Black, new XRect(left, y, width, 12), XStringFormats.TopLeft);
                y += 12;
            }

            void PrintCenter(string t, XFont f)
            {
                g.DrawString(t, f, XBrushes.Black, new XRect(left, y, width, 12), XStringFormats.TopCenter);
                y += 12;
            }

            void Sep()
            {
                PrintLeft(new string('-', 32), fText);
            }

            PrintCenter(izenburua, fTitle);
            PrintCenter(cif, fText);
            PrintCenter(herria, fText);
            PrintCenter(tel, fText);

            y += 4;

            PrintLeft($"Tiket: {zerbitzuId}", fText);
            PrintLeft($"Data: {DateTime.Now:dd/MM/yyyy HH:mm}", fText);
            PrintLeft($"Langilea: {langileId}", fText);

            y += 2;
            Sep();

            int prodW = 16;
            string HeaderRow() => $"{PadL("Kop", 3)} {PadR("Prod", prodW)} {PadL("Ud.", 6)} {PadL("Tot", 6)}";
            PrintLeft(HeaderRow(), fBold);

            foreach (var l in lineas)
            {
                var name = l.prod ?? "";
                var first = name.Length > prodW ? name.Substring(0, prodW) : name;
                var rest = name.Length > prodW ? name.Substring(prodW) : "";

                string row =
                    $"{PadL(l.kop.ToString(), 3)} " +
                    $"{PadR(first, prodW)} " +
                    $"{PadL(l.ud.ToString("0.00"), 6)} " +
                    $"{PadL(l.tot.ToString("0.00"), 6)}";

                PrintLeft(row, fText);

                if (!string.IsNullOrWhiteSpace(rest))
                {
                    PrintLeft($"{new string(' ', 4)}{rest.Trim()}", fText);
                }
            }

            y += 2;
            Sep();

            void PrintRightPair(string label, string value)
            {
                g.DrawString(label, fText, XBrushes.Black, new XRect(left, y, width, 12), XStringFormats.TopLeft);
                g.DrawString(value, fText, XBrushes.Black, new XRect(left, y, width, 12), XStringFormats.TopRight);
                y += 12;
            }

            PrintRightPair("Subtotala:", subtotal.ToString("0.00").Replace('.', ','));
            PrintRightPair($"IVA ({(int)(ivaTasa * 100)}%):", iva.ToString("0.00").Replace('.', ','));

            y += 2;

            g.DrawString("GUZTIRA:", fBig, XBrushes.Black, new XRect(left, y, width * 0.55, 18), XStringFormats.TopLeft);
            g.DrawString($"{guztira:0.00}".Replace('.', ',') + " €", fBig, XBrushes.Black, new XRect(left, y, width, 18), XStringFormats.TopRight);
            y += 20;

            Sep();

            PrintLeft($"Modua: {modua}", fText);

            if (pagoInformado)
            {
                PrintLeft($"Jasotakoa: {jasotakoa:0.00}".Replace('.', ','), fText);
                PrintLeft($"Itzulia: {itzulia:0.00}".Replace('.', ','), fText);
            }
            else
            {
                PrintLeft("Jasotakoa: -", fText);
                PrintLeft("Itzulia: -", fText);
            }

            y += 8;
            PrintCenter("Eskerrik asko!", fBold);

            dokumentua.Save(bidea);
        }

        private static string PadL(string s, int w)
        {
            s ??= "";
            if (s.Length >= w) return s.Substring(0, w);
            return s.PadLeft(w);
        }

        private static string PadR(string s, int w)
        {
            s ??= "";
            if (s.Length >= w) return s.Substring(0, w);
            return s.PadRight(w);
        }

    }
}
