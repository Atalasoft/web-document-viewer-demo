using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Caching;
using Atalasoft.Imaging.Codec;
using Atalasoft.Imaging.Codec.CadCam;
using Atalasoft.Imaging.Codec.Dicom;
using Atalasoft.Imaging.Codec.Jbig2;
using Atalasoft.Imaging.Codec.Jpeg2000;
using Atalasoft.Imaging.Codec.Office;
using Atalasoft.Imaging.Codec.Pdf;
using Atalasoft.Imaging.Text;
using Atalasoft.Imaging.WebControls;
using Atalasoft.Imaging.WebControls.Text;

namespace Atalasoft.Demo.WebDocumentViewer.Handlers
{
    public class WebDocumentViewerHandler : WebDocumentRequestHandler
    {
        private static CacheItemRemovedCallback _onCacheRemove;
        private const int CacheMonitorInterval = 5;
        public const string GuardFileName = "keep.me";
        public const string CacheFolder = "WebViewingDemoResources/TempSession/";

        static WebDocumentViewerHandler()
        {
            Licensing.AtalaLicense.SetAssemblyLicense(System.Web.HttpUtility.HtmlDecode(ConfigurationManager.AppSettings["AtalasoftLicenseString"]));
            
            RegisteredDecoders.Decoders.Add(new PdfDecoder { Resolution = 200, RenderSettings = new RenderSettings { AnnotationSettings = AnnotationRenderSettings.None } });
            RegisteredDecoders.Decoders.Add(new OfficeDecoder { Resolution = 200 });
            RegisteredDecoders.Decoders.Add(new RawDecoder());
            RegisteredDecoders.Decoders.Add(new DwgDecoder());
            RegisteredDecoders.Decoders.Add(new Jb2Decoder());
            RegisteredDecoders.Decoders.Add(new Jp2Decoder());
            RegisteredDecoders.Decoders.Add(new DicomDecoder());
            RegisteredDecoders.Decoders.Add(new XpsDecoder());

            // simple stub to monitor files both locally and on azure role.
            StartCacheMonitor();
        }

        private static void StartCacheMonitor()
        {
            _onCacheRemove = ClearExpiredFiles;
            HttpRuntime.Cache.Insert("atala_upload_cache", 0, null, DateTime.Now.AddHours(CacheMonitorInterval), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, _onCacheRemove);

        }

        private static void ClearExpiredFiles(string key, object value, CacheItemRemovedReason reason)
        {
            foreach (var filePath in Directory.EnumerateFiles(Path.Combine(HttpRuntime.AppDomainAppPath, CacheFolder)))
            {
                try
                {
                    // Skip guard file
                    if (Path.GetFileName(filePath).Equals(GuardFileName))
                        continue;

                    if (File.GetLastAccessTimeUtc(filePath) < DateTime.UtcNow.AddHours(-CacheMonitorInterval))
                        File.Delete(filePath);
                }
                catch (Exception)
                {
                }
            }

            StartCacheMonitor();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDocumentViewerHandler"/> class.
        /// </summary>
        public WebDocumentViewerHandler()
        {
            PageTextRequested += OnPageTextRequested;
        }

        private void OnPageTextRequested(object sender, PageTextRequestedEventArgs pageTextRequestedEventArgs)
        {
            var serverPath = HttpContext.Current.Server.MapPath(pageTextRequestedEventArgs.FilePath);
            if (File.Exists(serverPath))
            {
                using (var stream = File.OpenRead(serverPath))
                {
                    try
                    {
                        var decoder = RegisteredDecoders.GetDecoder(stream) as ITextFormatDecoder;
                        if (decoder != null)
                        {
                            using (var extractor = new SegmentedTextTranslator(decoder.GetTextDocument(stream)))
                            {
                                // for documents that have comlicated structure, i.e. consist from the isolated pieces of text, or table structure
                                // it's possible to configure nearby text blocks are combined into text segments(text containers that provide 
                                // selection isolated from other document content)
                                extractor.RegionDetection = TextRegionDetectionMode.BlockDetection;

                                // each block boundaries inflated to one average character width and two average character height 
                                // and all intersecting blocks are combined into single segment.
                                // Having vertical ratio bigger then horizontal behaves better on column-layout documents.
                                extractor.BlockDetectionDistance = new System.Drawing.SizeF(1, 2);
                                pageTextRequestedEventArgs.Page = extractor.ExtractPageText(pageTextRequestedEventArgs.Index);
                            }
                        }
                    }
                    catch (ImageReadException imagingException)
                    {
                        Debug.WriteLine("Text extraction: image type is not recognized. {0}", imagingException);
                    }
                }
            }
        }
    }
}
