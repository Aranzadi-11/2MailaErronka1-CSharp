using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TPV
{
    public sealed class TxatBezeroa
    {
        public static TxatBezeroa Instantzia { get; } = new TxatBezeroa();
        private TxatBezeroa() { }

        TcpClient? tcp;
        StreamReader? irakurlea;
        StreamWriter? idazlea;
        CancellationTokenSource? cts;

        public event Action<string>? RawJasota;
        public event Action<bool, string>? EgoeraAldatu;

        public bool Konektatuta => tcp?.Connected == true;

        public async Task KonektatuAsync(string ip, int portua, int erabiltzaileId, int rolId, string erabiltzaileIzena, string? tokena = null)
        {
            if (Konektatuta) return;

            tcp = new TcpClient();
            await tcp.ConnectAsync(ip, portua);

            var stream = tcp.GetStream();
            stream.ReadTimeout = 10000;
            stream.WriteTimeout = 10000;

            irakurlea = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            idazlea = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

            cts = new CancellationTokenSource();

            var login = new
            {
                Mota = "login",
                Edukia = new { ErabiltzaileId = erabiltzaileId, RolId = rolId, ErabiltzaileIzena = erabiltzaileIzena, Tokena = tokena }
            };

            await idazlea.WriteLineAsync(JsonSerializer.Serialize(login));

            var erantzuna = await IrakurriLerroaTimeoutarekin(8000);
            if (erantzuna is null) throw new Exception("Zerbitzariak ez du erantzun (timeout).");

            RawJasota?.Invoke(erantzuna);

            using (var doc = JsonDocument.Parse(erantzuna))
            {
                var mota = doc.RootElement.GetProperty("Mota").GetString();
                if (mota == "error")
                {
                    var mezua = doc.RootElement.GetProperty("Edukia").GetProperty("mezua").GetString() ?? "Errorea";
                    throw new Exception(mezua);
                }

                if (mota != "ok")
                    throw new Exception("Zerbitzariaren erantzuna ez da ok.");
            }

            EgoeraAldatu?.Invoke(true, "Konektatuta");
            _ = Task.Run(() => JasotzeBegizta(cts.Token));
        }

        public Task BidaliAsync(string testua)
        {
            if (idazlea is null || !Konektatuta) throw new InvalidOperationException("Ez dago konektatuta.");

            var mezua = new
            {
                Mota = "msg",
                Edukia = new { Testua = testua }
            };

            return idazlea.WriteLineAsync(JsonSerializer.Serialize(mezua));
        }

        public void Deskonektatu()
        {
            try { cts?.Cancel(); } catch { }
            try { tcp?.Close(); } catch { }
            tcp = null;
            irakurlea = null;
            idazlea = null;
            cts = null;
            EgoeraAldatu?.Invoke(false, "Deskonektatuta");
        }

        async Task JasotzeBegizta(CancellationToken tokena)
        {
            try
            {
                while (!tokena.IsCancellationRequested && irakurlea is not null)
                {
                    var lerroa = await irakurlea.ReadLineAsync(tokena);
                    if (lerroa is null) break;
                    RawJasota?.Invoke(lerroa);
                }
            }
            catch { }
            finally
            {
                Deskonektatu();
            }
        }

        async Task<string?> IrakurriLerroaTimeoutarekin(int ms)
        {
            if (irakurlea is null) return null;

            using var timeoutCts = new CancellationTokenSource(ms);
            try
            {
                return await irakurlea.ReadLineAsync(timeoutCts.Token);
            }
            catch
            {
                return null;
            }
        }
    }
}
