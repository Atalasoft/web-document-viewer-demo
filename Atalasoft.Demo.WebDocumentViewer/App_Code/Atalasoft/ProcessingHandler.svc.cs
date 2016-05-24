using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Activation;
using System.IO;
using System.Web;
using Atalasoft.Imaging;
using Atalasoft.Imaging.Codec.Pdf;
using System.ServiceModel.Web;
using System.Configuration;
using Atalasoft.Annotate.UI;
using Atalasoft.Imaging.Codec;
using Atalasoft.Annotate;
using Atalasoft.Imaging.ImageProcessing;

    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class ProcessingHandler
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        public AjaxDictionary<string, string> InitializeDocument(string document)
        {
            AjaxDictionary<string, string> returnDict = new AjaxDictionary<string, string>();

            try
            {
                string saveDocPath = NewTempFileName(".tif");
                string mappedDocPath = HttpContext.Current.Request.MapPath(document);
                string mappedSaveDocPath = HttpContext.Current.Request.MapPath(saveDocPath);

                saveDocPath = saveDocPath.Replace('\\', '/');

                File.Copy(mappedDocPath, mappedSaveDocPath, true);
                File.SetAttributes(mappedSaveDocPath, FileAttributes.Normal);

                returnDict.Add("success", "true");
                returnDict.Add("file", saveDocPath);

                return returnDict;
            }
            catch (Exception ex)
            {
                returnDict.Add("success", "false");
                returnDict.Add("error", ex.ToString());

                return returnDict;
            }
        }

        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare)]
        public void MakePrintPdf(string document, string annotationFolder)
        {
            string newfile = NewTempFileName(".pdf");

            string savedDocDir = Path.GetDirectoryName(document).Replace("\\", "/");

            string tempBurnedDoc = null;

            try
            {
                AjaxDictionary<string, string> burnDict = BurnImage(document, annotationFolder);

                if (burnDict["success"] != "true")
                    throw new Exception("Burn failed, cannot produce print Pdf.");

                tempBurnedDoc = burnDict["file"];
            }
            catch (Exception ex)
            {
                //Probably want to log the exception.
                throw;
            }

            
            using (Stream documentOut = File.Create(HttpContext.Current.Request.MapPath(newfile)))
            using (FileSystemImageSource burnedDocument = new FileSystemImageSource(HttpContext.Current.Request.MapPath(tempBurnedDoc), true))
            {
                PdfEncoder encoder = new PdfEncoder();
                HttpResponse response = HttpContext.Current.Response;
                response.Clear();
                response.ContentType = "application/pdf";
                    
                try
                {
                    response.Headers.Add("Content-Disposition", "attachment; filename=Document.pdf");
                }
                catch(PlatformNotSupportedException e)
                { /*This is thrown if we're not running under IIS/IIS-express. Just means we can't force the user to download instead of view.  */ }
                    
                using (MemoryStream stream = new MemoryStream())
                {
                    encoder.Save(stream, burnedDocument, null);
                    stream.Seek(0, SeekOrigin.Begin);
                    response.BinaryWrite(stream.ToArray());
                    response.Flush();
                    response.Close();
                }
                    
            }
        }
        
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        public AjaxDictionary<string, string> BurnImage(string document, string annotationFolder)
        {
            AjaxDictionary<string, string> returnDict = new AjaxDictionary<string, string>();

            string newfile = NewTempFileName(".tif");

            string xmpFile = Path.Combine(annotationFolder, Path.GetFileNameWithoutExtension(document) + ".xmp");
            if (!File.Exists(HttpContext.Current.Request.MapPath(xmpFile)))
            {
                returnDict.Add("success", "false");
                returnDict.Add("error", "Please save your annotations before burning them to the document.");
                return returnDict;
            }

            try
            {
                using (Stream documentIn = File.OpenRead(HttpContext.Current.Request.MapPath(document)))
                using (Stream annotations = File.OpenRead(HttpContext.Current.Request.MapPath(xmpFile)))
                using (Stream documentOut = File.Create(HttpContext.Current.Request.MapPath(newfile)))
                using (AnnotateViewer av = new AnnotateViewer())
                {
                    AnnotationController ac = new AnnotationController();

                    AnnotationConverter annoConverter = new AnnotationConverter(ac.Factories);
                    
                    ac.Layers = annoConverter.ToAnnotationUI(HttpContext.Current.Request.MapPath(xmpFile), HttpContext.Current.Request.MapPath(document));

                    AtalaImage image;
                    TiffEncoder encoder = new TiffEncoder();
                    encoder.Append = false;

                    int frameCount = RegisteredDecoders.GetImageInfo(documentIn).FrameCount;

                    AtalaImage myOverlay = new AtalaImage(HttpContext.Current.Request.MapPath(@"~/WebViewingDemoResources/small_logo.png"));
                    OverlayCommand cmd = new OverlayCommand(myOverlay, new System.Drawing.Point(0, 0));

                    for (int i = 0; i < frameCount; i++)
                    {
                        documentIn.Position = 0;
                        documentOut.Position = 0;

                        image = new AtalaImage(documentIn, i, null);
                        if (image.PixelFormat != PixelFormat.Pixel24bppBgr)
                            image = image.GetChangedPixelFormat(PixelFormat.Pixel24bppBgr);

                        if (ac.Layers.Count > i && ac.Layers[i] != null && ac.Layers[i].Items.Count > 0)
                        {
                            av.Annotations.RenderAnnotations(new RenderDevice(), image.GetGraphics(), ac.Layers[i]);
                        }

                        image = cmd.Apply(image).Image;

                        image.Save(documentOut, encoder, null);

                        encoder.Append = true;
                    }
                }

                returnDict.Add("file", newfile);
                returnDict.Add("success", "true");
            }
            catch (Exception ex)
            {
                returnDict.Add("success", "false");
                returnDict.Add("error", ex.ToString());
            }

            return returnDict;
        }
        
        
        private string NewTempFileName(string ext = "")
        {
            string tempDir = string.Empty;

            try
            {
                //tempDir = ConfigurationManager.AppSettings["TempDirectory"];
				tempDir = "~/WebViewingDemoResources/TempSession/";
                Directory.CreateDirectory(HttpContext.Current.Request.MapPath(tempDir));
            }
            catch (Exception)
            {
                //Don't do anything.
            }

            return Path.Combine(tempDir, Guid.NewGuid().ToString() + ext);
        }

        //This helps us return the ajax we want without having to use custom structs.
        //If you use a regular Dictionary, it returns {"Key":"key1", "Value":"value1"} instead of {"key1":"value1"}
        [Serializable]
        public class AjaxDictionary<TKey, TValue> : ISerializable
        {
            private Dictionary<TKey, TValue> _Dictionary;
            public AjaxDictionary()
            {
                _Dictionary = new Dictionary<TKey, TValue>();
            }
            public AjaxDictionary(SerializationInfo info, StreamingContext context)
            {
                _Dictionary = new Dictionary<TKey, TValue>();
            }
            public TValue this[TKey key]
            {
                get { return _Dictionary[key]; }
                set { _Dictionary[key] = value; }
            }
            public void Add(TKey key, TValue value)
            {
                _Dictionary.Add(key, value);
            }
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                foreach (TKey key in _Dictionary.Keys)
                    info.AddValue(key.ToString(), _Dictionary[key]);
            }
        }

    }

