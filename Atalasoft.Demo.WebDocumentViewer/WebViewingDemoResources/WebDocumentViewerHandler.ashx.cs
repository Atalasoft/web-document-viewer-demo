using System.Configuration;
using Atalasoft.Imaging.Codec;
using Atalasoft.Imaging.Codec.CadCam;
using Atalasoft.Imaging.Codec.Dicom;
using Atalasoft.Imaging.Codec.Jbig2;
using Atalasoft.Imaging.Codec.Jpeg2000;
using Atalasoft.Imaging.Codec.Pdf;
using Atalasoft.Imaging.WebControls;

public class WebDocumentViewerHandler : WebDocumentRequestHandler
{
    static WebDocumentViewerHandler()
    {
        Atalasoft.Licensing.AtalaLicense.SetAssemblyLicense(System.Web.HttpUtility.HtmlDecode(ConfigurationManager.AppSettings["AtalasoftLicenseString"]));

        RegisteredDecoders.Decoders.Add(new PdfDecoder { Resolution = 200, RenderSettings = new RenderSettings { AnnotationSettings = AnnotationRenderSettings.None } });
        RegisteredDecoders.Decoders.Add(new RawDecoder());
        RegisteredDecoders.Decoders.Add(new DwgDecoder());
        RegisteredDecoders.Decoders.Add(new Jb2Decoder());
        RegisteredDecoders.Decoders.Add(new Jp2Decoder());
        RegisteredDecoders.Decoders.Add(new DicomDecoder());
        RegisteredDecoders.Decoders.Add(new XpsDecoder());
    }
}