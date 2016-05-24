using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Atalasoft.Annotate;
using Atalasoft.Annotate.Formatters;
using Atalasoft.Annotate.UI;
using Atalasoft.Imaging.Codec;

#region Annotation Converter

    namespace Atalasoft.Demo.WebDocumentViewer
    {
        public class AnnotationConverter
        {
            private AnnotationUIFactoryCollection _factories;

            public AnnotationConverter()
            {
                _factories = new AnnotationUIFactoryCollection();
            }

            public AnnotationConverter(AnnotationUIFactoryCollection factories)
            {
                _factories = factories;
            }

            public AnnotationUIFactoryCollection Factories
            {
                get { return _factories; }
                set { _factories = value; }
            }

            public LayerData[] ToWebDocumentViewer(LayerCollection layerCollection, string docPath)
            {
                Size[] pageSizes = new Size[0];

                if (File.Exists(docPath))
                {
                    using (FileStream fs = new FileStream(docPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        pageSizes = new PageSizeAggregate(new DocumentPageSizes(RegisteredDecoders.GetDecoder(fs), fs, false)).ToArray();
                    }
                }

                return ToWebDocumentViewer(layerCollection, pageSizes);
            }

            public LayerData[] ToWebDocumentViewer(LayerCollection layerCollection, Size[] pageSizes)
            {
                LayerData[] layers = new LayerData[0];
                if (layerCollection != null)
                {
                    layers = LoadAnnotationData(layerCollection);
                }

                ConvertToWDV(layers, pageSizes);

                return layers;
            }

            public LayerCollection ToAnnotationUI(string xmpPath, string docPath)
            {
                LayerData[] layers = new LayerData[0];

                if (File.Exists(xmpPath))
                {
                    using (FileStream fs = new FileStream(xmpPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        layers = LoadAnnotationData(fs);
                    }
                }

                return ToAnnotationUI(layers, docPath);
            }

            public LayerCollection ToAnnotationUI(LayerData[] layers, string docPath)
            {
                Size[] pageSizes = new Size[0];

                if (File.Exists(docPath))
                {
                    using (FileStream fs = new FileStream(docPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        pageSizes = new PageSizeAggregate(new DocumentPageSizes(RegisteredDecoders.GetDecoder(fs), fs, false)).ToArray();
                    }
                }

                return ToAnnotationUI(layers, pageSizes);
            }

            public LayerCollection ToAnnotationUI(LayerData[] layers, Size[] pageSizes)
            {
                LayerCollection retLayers = new LayerCollection();

                if (layers != null)
                {
                    ConvertFromWDV(layers, pageSizes);

                    foreach (LayerData layer in layers)
                    {
                        AnnotationUI outAnnot = _factories.GetAnnotationFromData(layer);
                        LayerAnnotation layerAnnot = outAnnot as LayerAnnotation;

                        retLayers.Add(layerAnnot);
                    }
                }

                return retLayers;
            }


            #region Annotation Loading

            private LayerData[] LoadAnnotationData(object data)
            {
                LayerData[] layers = null;

                LayerCollection lc = data as LayerCollection;
                if (lc != null && lc.Count != 0)
                {
                    layers = new LayerData[lc.Count];

                    for (int i = 0; i < layers.Length; i++)
                    {
                        layers[i] = (LayerData)lc[i].Data;
                    }
                }

                LayerData[] lda = data as LayerData[];
                if (lda != null)
                {
                    layers = lda;
                }

                LayerAnnotation la = data as LayerAnnotation;
                if (la != null)
                {
                    layers = new LayerData[1];
                    layers[0] = (LayerData)la.Data;
                }

                LayerData ld = data as LayerData;
                if (ld != null)
                {
                    layers = new LayerData[1];
                    layers[0] = ld;
                }

                AnnotationUI ann = data as AnnotationUI;
                if (ann != null)
                {
                    layers = new LayerData[1];
                    layers[0].Items.Add(ann.Data);
                }

                AnnotationData ad = data as AnnotationData;
                if (ad != null)
                {
                    layers = new LayerData[1];
                    layers[0].Items.Add(ad);
                }

                return layers;
            }

            private LayerData[] LoadAnnotationData(System.IO.Stream stream)
            {
                if (stream == null)
                    throw new ArgumentNullException("stream");

                XmpFormatter xmp = new XmpFormatter();
                object data = xmp.Deserialize(stream);

                return LoadAnnotationData(data);
            }

            #endregion

            #region Annotation Conversion

            private void ConvertToWDV(LayerData[] layers, Size[] pageSizes)
            {
                if (pageSizes.Length >= layers.Length)
                {
                    // all pages are bestfit to the first page
                    Size destSize = pageSizes[0];

                    for (int i = 0; i < pageSizes.Length && i < layers.Length; i++)
                    {
                        float zoom = ScaleBestFit(pageSizes[i], destSize);
                        SizeF zoomSize = new SizeF(pageSizes[i].Width * zoom, pageSizes[i].Height * zoom);
                        PointF offset = new PointF((destSize.Width - zoomSize.Width) / 2, (destSize.Height - zoomSize.Height) / 2);

                        foreach (AnnotationData data in layers[i].Items)
                        {
                            // can't use data.Transform because it stays with the annotation, need to modify data directly
                            data.Location = new PointF((data.Location.X) * zoom + offset.X, (data.Location.Y) * zoom + offset.Y);
                            data.Size = new SizeF(data.Size.Width * zoom, data.Size.Height * zoom);
                        }
                    }
                }
            }

            private void ConvertFromWDV(LayerData[] layers, Size[] pageSizes)
            {
                if (pageSizes.Length >= layers.Length)
                {
                    // all pages are bestfit to the first page
                    Size destSize = pageSizes[0];

                    for (int i = 0; i < pageSizes.Length && i < layers.Length; i++)
                    {
                        float zoom = ScaleBestFit(pageSizes[i], destSize);
                        SizeF zoomSize = new SizeF(pageSizes[i].Width * zoom, pageSizes[i].Height * zoom);
                        PointF offset = new PointF((destSize.Width - zoomSize.Width) / 2, (destSize.Height - zoomSize.Height) / 2);

                        EmbeddedImageData embeddedImage;
                        AnnotationData data;

                        for (int j = 0; j < layers[i].Items.Count; j++)
                        {
                            data = layers[i].Items[j];

                            if (layers[i].Items[j] is ReferencedImageData)
                            {
                                embeddedImage = new EmbeddedImageData();
                                embeddedImage.Location = layers[i].Items[j].Location;
                                embeddedImage.Size = layers[i].Items[j].Size;
                                try
                                {
                                    Uri referencedImage = new Uri(((ReferencedImageData)layers[i].Items[j]).FileName);
                                    if (!referencedImage.IsAbsoluteUri)
                                    {
                                        if (System.Web.HttpContext.Current != null)
                                            embeddedImage.Image = new AnnotationImage(new global::Atalasoft.Imaging.AtalaImage(System.Web.HttpContext.Current.Request.MapPath(((ReferencedImageData)layers[i].Items[j]).FileName)));
                                        else
                                            embeddedImage.Image = new AnnotationImage(new global::Atalasoft.Imaging.AtalaImage(((ReferencedImageData)layers[i].Items[j]).FileName));
                                    }

                                    layers[i].Items[j] = embeddedImage;
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                                }

                            }

                            // can't use data.Transform because it stays with the annotation, need to modify data directly
                            data.Location = new PointF((data.Location.X - offset.X) / zoom, (data.Location.Y - offset.Y) / zoom);
                            data.Size = new SizeF(data.Size.Width / zoom, data.Size.Height / zoom);
                        }
                    }
                }
            }

            #endregion

            #region Page Size Helpers

            private float ScaleBestFit(SizeF origSize, SizeF destSize)
            {
                float scale = 1;

                if (origSize.Width / destSize.Width > origSize.Height / destSize.Height)
                {	//size to the width
                    scale = destSize.Width / origSize.Width;
                }
                else
                {	//size to the height
                    scale = destSize.Height / origSize.Height;
                }

                return scale;
            }

            public class SingleMultiple<T>
            {
                private List<T> _coll = new List<T>();
                protected T Single { get { return _coll[0]; } set { _coll.Clear(); _coll.Add(value); } }
                protected IList<T> Multiple { get { return _coll; } }
            }

            public class PageSizeAggregate : SingleMultiple<Size>
            {
                private int _count;

                public PageSizeAggregate(Size size)
                    : base()
                {
                    this.Single = size;
                    this._count = 1;
                }

                public PageSizeAggregate(IEnumerable<Size> sizes)
                    : base()
                {
                    this._count = 0;

                    foreach (Size s in sizes)
                    {
                        Multiple.Add(s);
                        this._count++;
                    }

                    if (this._count <= 0)
                        throw new ArgumentOutOfRangeException("sizes", "sizes must have at least one entry");
                }

                public IEnumerable<Size> GetEnumerator()
                {
                    foreach (Size s in Multiple)
                    {
                        yield return s;
                    }
                }

                public int Count
                {
                    get { return this._count; }
                }

                public Size[] ToArray()
                {
                    Size[] sizes = new Size[this._count];
                    Multiple.CopyTo(sizes, 0);
                    return sizes;
                }
            }

            public class DocumentPageSizes : IEnumerable<Size>
            {
                #region IEnumerable<Size> Members

                Stream _stm;
                ImageDecoder _dec;
                int _count;
                bool _timeoutPages = true;

                public DocumentPageSizes(ImageDecoder dec, Stream stm, bool timeoutPages)
                {
                    _stm = stm;
                    _dec = dec;
                    _count = -1;
                    _timeoutPages = timeoutPages;
                }

                public int Count
                {
                    get { return _count; }
                }

                public IEnumerator<Size> GetEnumerator()
                {
                    ImageInfo info = _dec.GetImageInfo(_stm);
                    _count = info.FrameCount;

                    MultiFramedImageDecoder mpdec = _dec as MultiFramedImageDecoder;
                    if (mpdec != null)
                    {
                        double timeoutSeconds = (_timeoutPages) ? 2 : 300;
                        DateTime timeout = DateTime.Now.AddSeconds(timeoutSeconds);

                        for (int i = 0; i < _count && timeout > DateTime.Now; i++)
                        {
                            _stm.Seek(0, SeekOrigin.Begin);
                            ImageInfo pginfo = mpdec.GetImageInfo(_stm, i);
                            yield return pginfo.Size;
                        }
                    }
                    else
                    {
                        yield return info.Size;
                    }
                }

                #endregion

                #region IEnumerable Members

                System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }

                #endregion
            }

            #endregion
        }
    }
    #endregion
