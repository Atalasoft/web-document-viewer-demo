using System;
using System.Web;
using System.IO;
using Atalasoft.Imaging.Codec;
using Atalasoft.Imaging;

/// <summary>
/// Summary description for UploadHandler
/// </summary>
public class UploadHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string cachefolder = "TempSession/";
        string newfile = Path.Combine(cachefolder, Guid.NewGuid().ToString() + ".tif");
        try
        {
            if (context.Request.Files.Count != 0 && context.Request.Files[0].ContentLength > 0)
            {
                string msg = UploadFile(context, context.Request.MapPath(newfile));
                if (msg == "")
                {
                    context.Response.ContentType = "text/html";
                    context.Response.Write("<html><head></head><body>");
                    context.Response.Write("{ \"success\":\"true\", \"file\": \"" + newfile + "\" }");
                    context.Response.Write("</body></html>");
                }
                else
                {
                    context.Response.ContentType = "text/html";
                    context.Response.Write("<html><head></head><body>");
                    context.Response.Write("{ \"success\":\"false\", \"file\": \"" + newfile + "\" }");
                    context.Response.Write("</body></html>");
                }
            }
        }
        catch (Exception)
        {
            context.Response.ContentType = "text/html";
            context.Response.Write("<html><head></head><body>");
            context.Response.Write("{ \"success\":\"false\", \"file\": \"" + newfile + "\" }");
            context.Response.Write("</body></html>");
        }

    }

    private string UploadFile(HttpContext context, string filename)
    {
        string msg = "";
        HttpPostedFile file = context.Request.Files[0];

        byte[] fileBytes = new byte[file.ContentLength];
        file.InputStream.Read(fileBytes, 0, file.ContentLength);


        using (FileStream outStream = new FileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite))
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(fileBytes, 0, fileBytes.Length);
                ms.Position = 0;
                try
                {
                    ImageDecoder decoder = RegisteredDecoders.GetDecoder(ms);
                    if (decoder is TiffDecoder)
                    {
                        outStream.Write(fileBytes, 0, fileBytes.Length);
                        outStream.Position = 0;
                    }
                    else
                    {
                        ImageCollection ic = new ImageCollection(ms, null);
                        TiffEncoder tiffEncoder = new TiffEncoder();
                        ic.Save(outStream, tiffEncoder, null);
                    }
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                }

                ms.Position = 0;
            }
        }

        return msg;

    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}