

namespace PlayerDePobre.Model
{
    public class Musica
    {
        public ImageSource Cover { get; set; }
        public ImageSource CoverThumb { get; set; }
        public string Titulo { get; set; }
        public string Album { get; set; }
        public string Artista { get; set; }
        public string Extensao { get; set; }
        public string FullPath { get; set; }
        public string NomeOnDisk { get; set; }
        //public ushort Duraçao { get; set; }
    }
}
