using System.Configuration;
using Atalasoft.Imaging.Codec;
using Atalasoft.Imaging.Codec.CadCam;
using Atalasoft.Imaging.Codec.Dicom;
using Atalasoft.Imaging.Codec.Jbig2;
using Atalasoft.Imaging.Codec.Jpeg2000;
using Atalasoft.Imaging.Codec.Pdf;
using Atalasoft.Imaging.WebControls;
using System.Web;
using System.IO;
using Atalasoft.Imaging.WebControls.Text;
using System.Diagnostics;

namespace Atalasoft.Demo.WebDocumentViewer.Handlers
{
    public class WebDocumentViewerHandler : WebDocumentRequestHandler
    {
        static WebDocumentViewerHandler()
        {
            Licensing.AtalaLicense.SetAssemblyLicense(System.Web.HttpUtility.HtmlDecode(ConfigurationManager.AppSettings["AtalasoftLicenseString"]));

            RegisteredDecoders.Decoders.Add(new PdfDecoder { Resolution = 200, RenderSettings = new RenderSettings { AnnotationSettings = AnnotationRenderSettings.None } });
            RegisteredDecoders.Decoders.Add(new RawDecoder());
            RegisteredDecoders.Decoders.Add(new DwgDecoder());
            RegisteredDecoders.Decoders.Add(new Jb2Decoder());
            RegisteredDecoders.Decoders.Add(new Jp2Decoder());
            RegisteredDecoders.Decoders.Add(new DicomDecoder());
            RegisteredDecoders.Decoders.Add(new XpsDecoder());
        }

        public WebDocumentViewerHandler()
        {
            this.PageTextRequested += WebDocumentViewerHandler_PageTextRequested;
        }

        private void WebDocumentViewerHandler_PageTextRequested(object sender, PageTextRequestedEventArgs e)
        {
            var serverPath = HttpContext.Current.Server.MapPath(e.FilePath);
            if (File.Exists(serverPath))
            {
                using (var stream = File.OpenRead(serverPath))
                {
                    try
                    {
                        var decoder = RegisteredDecoders.GetDecoder(stream) as Atalasoft.Imaging.Text.ITextFormatDecoder;
                        if (decoder != null)
                        {
                            using (var extractor = new SegmentedTextTranslator(decoder.GetTextDocument(stream)))
                            {
                                extractor.BlockDetectionDistance = new System.Drawing.SizeF(2, 4);
                                extractor.RegionDetection = TextRegionDetectionMode.BlockDetection;
                                e.Page = extractor.ExtractPageText(e.Index);
                                return;
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