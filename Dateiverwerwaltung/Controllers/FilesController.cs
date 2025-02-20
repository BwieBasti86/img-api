using Dateiverwerwaltung.Models;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace Dateiverwerwaltung.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FilesController : ControllerBase
    {


        [HttpGet("GetFileReferences")]
        public ImgReferences GetFileReferences([FromQuery] string directory, DateTime? from = null, DateTime? to = null)
        {
            List<ImgFile> resultList = new List<ImgFile>();
            ImgReferences result = new ImgReferences();
            try
            {
                var imgFiles = (from != null && to != null) ? Directory.EnumerateFiles(directory, "*.JPG").Where(file => System.IO.File.GetCreationTime(file) >= from && System.IO.File.GetCreationTime(file) <= to) : Directory.EnumerateFiles(directory, "*.JPG");


                foreach (string currentFile in imgFiles)
                {
                    resultList.Add(new ImgFile
                    {
                        Filename = Path.GetFileName(currentFile).Substring(0, Path.GetFileName(currentFile).IndexOf(".")),
                        Filepath = currentFile,
                        SourceDirectory = directory,
                        CreationDate = System.IO.File.GetCreationTime(currentFile),
                    });
                }
                result.ImgFiles = resultList;
                result.ResultLength = resultList.Count();
                result.sessions = resultList
                    .Select(fileref => fileref.CreationDate.Date)
                    .Distinct()
                    .OrderBy(date => date)
                    .ToList();

            }
            catch
            {

            }

            return result;
        }

        [HttpGet("GetImages")]
        public ImgReferences GetImages([FromQuery] string directory, DateTime? from = null, DateTime? to = null, int? firstIndex = null, int? itemsPerRequest = null)
        {
            List<ImgFile> resultList = new List<ImgFile>();
            ImgReferences result = new ImgReferences();
            try
            {
                var imgFilePathsAll = (from != null && to != null) ? Directory.EnumerateFiles(directory, "*.JPG").Where(file => System.IO.File.GetCreationTime(file) >= from && System.IO.File.GetCreationTime(file) <= to) : Directory.EnumerateFiles(directory, "*.JPG");

                var imgFilePaths = (firstIndex != null && itemsPerRequest != null) ? imgFilePathsAll.Where((file, index) => index >= firstIndex && index <= firstIndex + (itemsPerRequest - 1)) : imgFilePathsAll;

                var count = 0;

                foreach (string currentFilePath in imgFilePaths)
                {
                    if (count >= 5) continue;
                    if (System.IO.File.Exists(currentFilePath))
                    {
                        using (var inputStream = System.IO.File.OpenRead(currentFilePath))
                        {
                            using (var original = SKBitmap.Decode(inputStream))
                            {
                                var resizedImage = original.Resize(new SKImageInfo(120, 80), SKFilterQuality.Low); // Skalieren auf 800x600
                                using (var image = SKImage.FromBitmap(resizedImage))
                                {
                                    using (var outputStream = new MemoryStream())
                                    {
                                        var data = image.Encode(SKEncodedImageFormat.Jpeg, 25); // Komprimieren mit 75% Qualität
                                        data.SaveTo(outputStream);
                                        var base64String = Convert.ToBase64String(outputStream.ToArray());
                                        resultList.Add(new ImgFile
                                        {
                                            Filename = Path.GetFileName(currentFilePath).Substring(0, Path.GetFileName(currentFilePath).IndexOf(".")),
                                            Filepath = currentFilePath,
                                            SourceDirectory = directory,
                                            CreationDate = System.IO.File.GetCreationTime(currentFilePath),
                                            ImageDecoded = base64String,
                                        });
                                    }
                                }
                            }
                        }
                        count++;

                    }
                }
                result.ImgFiles = resultList;
                result.ResultLength = resultList.Count();
                result.sessions = resultList
                      .Select(fileref => fileref.CreationDate.Date)
                      .Distinct()
                      .OrderBy(date => date)
                      .ToList();
                //result.sessionsAll = imgFilePaths.

            }
            catch
            {

            }
            return result;
        }

        [HttpGet("GetFullImage")]
        public ImgFile GetFullImage([FromQuery] string imgFilePath)
        {
            ImgFile result = new ImgFile();
            try
            {
                if (System.IO.File.Exists(imgFilePath))
                {
                    var imageBytes = System.IO.File.ReadAllBytes(imgFilePath);
                    var base64String = Convert.ToBase64String(imageBytes);
                    result.Filename = Path.GetFileName(imgFilePath).Substring(0, Path.GetFileName(imgFilePath).IndexOf("."));
                    result.Filepath = imgFilePath;
                    result.SourceDirectory = imgFilePath; //imgFilePath.Substring(imgFilePath.LastIndexOf("\n"), imgFilePath.Length - 1);
                    result.CreationDate = System.IO.File.GetCreationTime(imgFilePath);
                    result.ImageDecoded = base64String;
                }
            }
            catch
            {

            }
            return result;
        }

        [HttpPost("CopyRawFiles")]
        public bool CopyRawFiles([FromBody] CopyFilesArgument args)
        {
            try
            {
                foreach (ImgFile currentFile in args.Files)
                {
                    System.IO.File.Copy(Path.Combine(currentFile.SourceDirectory, currentFile.Filename + ".CR3"), Path.Combine(args.TargetDirectory, currentFile.Filename + ".CR3"));
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
