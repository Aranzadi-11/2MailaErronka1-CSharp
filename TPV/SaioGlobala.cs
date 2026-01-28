namespace TPV
{
    public static class SaioGlobala
    {
        public static int LangileId { get; private set; }
        public static int RolaId { get; private set; }
        public static string ErabiltzaileIzena { get; private set; } = "";
        public static string? Tokena { get; private set; }

        public static void Ezarri(int langileId, int rolaId, string erabiltzaileIzena, string? tokena = null)
        {
            LangileId = langileId;
            RolaId = rolaId;
            ErabiltzaileIzena = erabiltzaileIzena ?? "";
            Tokena = tokena;
        }

        public static void Garbitu()
        {
            LangileId = 0;
            RolaId = 0;
            ErabiltzaileIzena = "";
            Tokena = null;
        }
    }
}
