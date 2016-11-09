using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Atalasoft.Imaging;
using Atalasoft.Imaging.Codec;
using Atalasoft.Imaging.Codec.Office;
using Atalasoft.Imaging.Codec.Pdf;

namespace Atalasoft.Demo.WebDocumentViewer.Handlers
{
    /// <summary>
    /// Summary description for UploadHandler
    /// </summary>
    public class UploadHandler : IHttpHandler
    {
        public const string CacheFolder = "WebViewingDemoResources/TempSession/";
        private const string ContentType = "application/json";
        private const int CacheMonitorInterval = 8;
        private readonly List<string> _blacklistedExtensions = new List<string> {".exe", ".bat", ".com"};
        private static CacheItemRemovedCallback _onCacheRemove;

        /// <summary>
        /// Initializes the <see cref="UploadHandler"/> class.
        /// </summary>
        static UploadHandler()
        {
            // simple stub to monitor files both locally and on azure role.
            StartCacheMonitor();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Files.Count > 0 && context.Request.Files[0].ContentLength > 0)
            {
                var ext = Path.GetExtension(context.Request.Files[0].FileName);
                // files are converted with encoder and not saved as is, but we still don't want such trash on filesystem.
                if (!_blacklistedExtensions.Any(e => e.Equals(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    var newFileName = Path.Combine("~/", CacheFolder, string.Format("{0}{1}", Guid.NewGuid(), ext));
                    try
                    {
                        UploadFile(context, context.Request.MapPath(newFileName));
                        WriteResponse(context, true, newFileName);
                        return;
                    }
                    catch (Exception)
                    {
                        if (File.Exists(newFileName))
                            File.Delete(newFileName);
                    }
                }

                WriteResponse(context, false, null);
            }
        }

        private static void WriteResponse(HttpContext context, bool success, string fileName)
        {
            context.Response.ContentType = ContentType;
            context.Response.Write(string.Format("{{ \"success\":{0}, \"file\": \"{1}\" }}", success.ToString().ToLowerInvariant(), fileName));
        }

        private void UploadFile(HttpContext context, string filename)
        {
            var file = context.Request.Files[0];
            using (var outStream = new FileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                using (var ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    ms.Seek(0, 0);

                    var decoder = RegisteredDecoders.GetDecoder(ms);
                    if (decoder is TiffDecoder || decoder is PdfDecoder || decoder is OfficeDecoder)
                    {
                        ms.CopyTo(outStream);
                    }
                    else
                    {
                        var ic = new ImageCollection(ms, null);
                        var tiffEncoder = new TiffEncoder();
                        ic.Save(outStream, tiffEncoder, null);
                    }
                }
            }
        }

        public static void StartCacheMonitor()
        {
            _onCacheRemove = ClearExpiredFiles;
            HttpRuntime.Cache.Insert("atala_upload_cache", 0, null, DateTime.Now.AddHours(CacheMonitorInterval), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, _onCacheRemove);
        }

        public static void ClearExpiredFiles(string key, object value, CacheItemRemovedReason reason)
        {
            foreach (var filePath in Directory.EnumerateFiles(Path.Combine(HttpRuntime.AppDomainAppPath, CacheFolder)))
            {
                try
                {
                    if (File.GetLastAccessTimeUtc(filePath) < DateTime.UtcNow.AddHours(-CacheMonitorInterval))
                        File.Delete(filePath);
                }
                catch (Exception)
                {
                }
            }

            StartCacheMonitor();
        }
    }
}