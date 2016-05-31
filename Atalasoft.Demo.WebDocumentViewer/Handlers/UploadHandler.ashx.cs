using System;
using System.IO;
using System.Web;
using Atalasoft.Imaging;
using Atalasoft.Imaging.Codec;

namespace Atalasoft.Demo.WebDocumentViewer.Handlers
{
    /// <summary>
    /// Summary description for UploadHandler
    /// </summary>
    public class UploadHandler : IHttpHandler
    {
        private const string CacheFolder = "~/WebViewingDemoResources/TempSession/";
        private const string ContentType = "text/html";

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {

            var newFileName = Path.Combine(CacheFolder, string.Format("{0}.tif", Guid.NewGuid()));
            try
            {
                if (context.Request.Files.Count != 0 && context.Request.Files[0].ContentLength > 0)
                {
                    var msg = UploadFile(context, context.Request.MapPath(newFileName));
                    WriteResponse(context, string.IsNullOrEmpty(msg), newFileName);
                }
            }
            catch (Exception)
            {
                WriteResponse(context, false, newFileName);
            }

        }

        private static void WriteResponse(HttpContext context, bool success, string fileName)
        {
            context.Response.ContentType = ContentType;
           // context.Response.Write("<html><head></head><body>");
            context.Response.Write(string.Format("{{ \"success\":\"{0}\", \"file\": \"{1}\" }}", success.ToString().ToLowerInvariant(), fileName));
           // context.Response.Write("</body></html>");
        }

        private string UploadFile(HttpContext context, string filename)
        {
            var msg = string.Empty;
            var file = context.Request.Files[0];

            var fileBytes = new byte[file.ContentLength];
            file.InputStream.Read(fileBytes, 0, file.ContentLength);


            using (var outStream = new FileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                using (var ms = new MemoryStream())
                {
                    ms.Write(fileBytes, 0, fileBytes.Length);
                    ms.Seek(0, 0);
                    try
                    {
                        var decoder = RegisteredDecoders.GetDecoder(ms);
                        if (decoder is TiffDecoder)
                        {
                            outStream.Write(fileBytes, 0, fileBytes.Length);
                            outStream.Seek(0, 0);
                        }
                        else
                        {
                            var ic = new ImageCollection(ms, null);
                            var tiffEncoder = new TiffEncoder();
                            ic.Save(outStream, tiffEncoder, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        msg = ex.Message;
                    }

                    ms.Seek(0, 0);
                }
            }

            return msg;
        }
    }
}