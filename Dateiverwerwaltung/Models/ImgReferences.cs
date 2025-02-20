namespace Dateiverwerwaltung.Models
{
    public class ImgReferences
    {
        public List<ImgFile> ImgFiles { get; set; }
        public int ResultLength { get; set; }
        public List<DateTime>? sessions { get; set; }
        public List<DateTime>? sessionsAll { get; set; }
    }
}
