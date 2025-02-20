namespace Dateiverwerwaltung.Models
{
    public class ImgFile
    {
        public string? Filepath { get; set; }
        public string? Filename { get; set; }
        public string? SourceDirectory { get; set; }
        public DateTime CreationDate { get; set; }
        public string? ImageDecoded { get; set; }
    }
}
