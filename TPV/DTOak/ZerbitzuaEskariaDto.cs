namespace TPV.DTOak
{
    public class PlateraEskariaDto
    {
        public int PlateraId { get; set; }
        public int Kantitatea { get; set; }
    }

    public class ZerbitzuaEskariaDto
    {
        public int LangileId { get; set; }
        public int MahaiaId { get; set; }
        public List<PlateraEskariaDto> Platerak { get; set; } = new();
    }
}
