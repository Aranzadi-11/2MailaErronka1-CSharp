using System;
using System.Globalization;
using System.Text.Json;
using System.Windows.Forms;

namespace TPV
{
    public partial class TxatKontrola : UserControl
    {
        DateTime? azkenData = null;

        public TxatKontrola()
        {
            InitializeComponent();
            TxatBezeroa.Instantzia.RawJasota += RawKudeatu;
            TxatBezeroa.Instantzia.EgoeraAldatu += EgoeraKudeatu;
        }

        void EgoeraKudeatu(bool konektatuta, string mezua)
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() =>
            {
                lblEgoera.Text = konektatuta ? "Konektatuta" : "Deskonektatuta";
            }));
        }

        void RawKudeatu(string raw)
        {
            if (IsDisposed) return;

            BeginInvoke(new Action(() =>
            {
                try
                {
                    using var doc = JsonDocument.Parse(raw);
                    var mota = doc.RootElement.GetProperty("Mota").GetString();

                    if (mota == "history")
                    {
                        var edukia = doc.RootElement.GetProperty("Edukia");
                        if (edukia.TryGetProperty("mezuak", out var arr) && arr.ValueKind == JsonValueKind.Array)
                        {
                            mezuakPanel.SuspendLayout();
                            foreach (var it in arr.EnumerateArray())
                            {
                                var norIzena = it.GetProperty("NorIzena").GetString() ?? "Ezezaguna";
                                var testua = it.GetProperty("Testua").GetString() ?? "";
                                var utc = it.GetProperty("Utc").GetDateTime();
                                GehituMezua(norIzena, testua, utc.ToLocalTime());
                            }
                            mezuakPanel.ResumeLayout();
                            BeheraJoan();
                        }
                        return;
                    }

                    if (mota == "msg")
                    {
                        var edukia = doc.RootElement.GetProperty("Edukia");
                        var norIzena = edukia.GetProperty("norIzena").GetString() ?? "Ezezaguna";
                        var testua = edukia.GetProperty("testua").GetString() ?? "";
                        var utc = edukia.GetProperty("utc").GetDateTime();
                        GehituMezua(norIzena, testua, utc.ToLocalTime());
                        BeheraJoan();
                        return;
                    }

                    if (mota == "system")
                    {
                        var edukia = doc.RootElement.GetProperty("Edukia");
                        var mezua = edukia.GetProperty("mezua").GetString() ?? "";
                        GehituSistema(mezua);
                        BeheraJoan();
                        return;
                    }

                    if (mota == "error")
                    {
                        var edukia = doc.RootElement.GetProperty("Edukia");
                        var mezua = edukia.GetProperty("mezua").GetString() ?? "Errorea";
                        GehituSistema("[ERROREA] " + mezua);
                        BeheraJoan();
                        return;
                    }
                }
                catch
                {
                    GehituSistema(raw);
                    BeheraJoan();
                }
            }));
        }

        void GehituMezua(string norIzena, string testua, DateTime dataLokala)
        {
            if (azkenData is null || azkenData.Value.Date != dataLokala.Date)
            {
                azkenData = dataLokala.Date;
                GehituDataBereizlea(dataLokala);
            }

            var mezuaPanela = new TableLayoutPanel();
            mezuaPanela.ColumnCount = 2;
            mezuaPanela.RowCount = 2;
            mezuaPanela.AutoSize = true;
            mezuaPanela.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            mezuaPanela.Margin = new Padding(0, 0, 0, 10);
            mezuaPanela.Padding = new Padding(10);
            mezuaPanela.BackColor = System.Drawing.Color.White;
            mezuaPanela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mezuaPanela.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            mezuaPanela.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mezuaPanela.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mezuaPanela.Width = mezuakPanel.ClientSize.Width - 30;

            var lblTestua = new Label();
            lblTestua.AutoSize = true;
            lblTestua.MaximumSize = new System.Drawing.Size(mezuaPanela.Width - 160, 0);
            lblTestua.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular);
            lblTestua.Text = testua;

            var lblNor = new Label();
            lblNor.AutoSize = true;
            lblNor.TextAlign = System.Drawing.ContentAlignment.TopRight;
            lblNor.Dock = DockStyle.Fill;
            lblNor.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            lblNor.Text = norIzena;

            var lblOrdua = new Label();
            lblOrdua.AutoSize = true;
            lblOrdua.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lblOrdua.Dock = DockStyle.Fill;
            lblOrdua.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            lblOrdua.Text = dataLokala.ToString("HH:mm", CultureInfo.InvariantCulture);

            mezuaPanela.Controls.Add(lblTestua, 0, 0);
            mezuaPanela.SetColumnSpan(lblTestua, 1);

            mezuaPanela.Controls.Add(lblNor, 1, 0);
            mezuaPanela.Controls.Add(new Panel(), 0, 1);
            mezuaPanela.Controls.Add(lblOrdua, 1, 1);

            mezuakPanel.Controls.Add(mezuaPanela);
        }

        void GehituDataBereizlea(DateTime dataLokala)
        {
            var lbl = new Label();
            lbl.AutoSize = false;
            lbl.Width = mezuakPanel.ClientSize.Width - 30;
            lbl.Height = 36;
            lbl.Margin = new Padding(0, 10, 0, 10);
            lbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            lbl.ForeColor = System.Drawing.Color.Goldenrod;
            lbl.Text = dataLokala.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            mezuakPanel.Controls.Add(lbl);
        }

        void GehituSistema(string testua)
        {
            var lbl = new Label();
            lbl.AutoSize = false;
            lbl.Width = mezuakPanel.ClientSize.Width - 30;
            lbl.Height = 28;
            lbl.Margin = new Padding(0, 0, 0, 10);
            lbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lbl.ForeColor = System.Drawing.Color.DimGray;
            lbl.Text = testua;
            mezuakPanel.Controls.Add(lbl);
        }

        void BeheraJoan()
        {
            if (mezuakPanel.Controls.Count == 0) return;
            var azkena = mezuakPanel.Controls[mezuakPanel.Controls.Count - 1];
            mezuakPanel.ScrollControlIntoView(azkena);
        }

        async void btnBidali_Click(object sender, EventArgs e)
        {
            var t = txtMezua.Text.Trim();
            if (t.Length == 0) return;

            try
            {
                txtMezua.Clear();
                await TxatBezeroa.Instantzia.BidaliAsync(t);
            }
            catch (Exception ex)
            {
                GehituSistema("[ERROREA] " + ex.Message);
                BeheraJoan();
            }
        }

        void txtMezua_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnBidali.PerformClick();
            }
        }
    }
}
