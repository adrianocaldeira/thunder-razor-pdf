using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using CsQuery;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;

namespace Thunder.RazorPdf
{
    /// <summary>
    /// Pdf result
    /// </summary>
    public class PdfResult : ActionResult
    {
        /// <summary>
        /// New instance of class <see cref="PdfResult"/>
        /// </summary>
        /// <param name="viewName"></param>
        public PdfResult(string viewName)
        {
            if (string.IsNullOrEmpty(viewName)) throw new ArgumentNullException(nameof(viewName), "ViewName is null or empty");

            StyleSheets = new List<string>();
            Settings = new DocumentSetting();
            Download = true;
            ViewName = viewName;
            Model = null;
            FileName = string.Concat(Guid.NewGuid().ToString("D").ToUpper(), ".pdf");
        }

        /// <summary>
        /// New instance of class <see cref="PdfResult"/>
        /// </summary>
        /// <param name="viewName">View Name</param>
        /// <param name="model">Model</param>
        public PdfResult(string viewName, object model)
            : this(viewName)
        {
            Model = model;
        }

        /// <summary>
        /// New instance of class <see cref="PdfResult"/>
        /// </summary>
        /// <param name="viewName">View Name</param>
        /// <param name="model">Model</param>
        /// <param name="fileName">File Name</param>
        public PdfResult(string viewName, object model, string fileName)
            : this(viewName, model)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName), "File name is null or empty");

            FileName = fileName;
        }

        /// <summary>
        /// New instance of class <see cref="PdfResult"/>
        /// </summary>
        /// <param name="viewName">View Name</param>
        /// <param name="model">Model</param>
        /// <param name="download">Force download</param>
        public PdfResult(string viewName, object model, bool download)
            : this(viewName, model)
        {
            Download = download;
        }

        /// <summary>
        /// New instance of class <see cref="PdfResult"/>
        /// </summary>
        /// <param name="viewName">View Name</param>
        /// <param name="model">Model</param>
        /// <param name="fileName">File Name</param>
        /// <param name="download">Force download</param>
        public PdfResult(string viewName, object model, string fileName, bool download)
            : this(viewName, model, fileName)
        {
            Download = download;
        }

        /// <summary>
        /// Get html
        /// </summary>
        public string Html { get; private set; }
        /// <summary>
        /// Get view name
        /// </summary>
        public string ViewName { get; }
        /// <summary>
        /// Get model
        /// </summary>
        public object Model { get; }
        /// <summary>
        /// Get file name
        /// </summary>
        public string FileName { get; }
        /// <summary>
        /// Get or set style sheets
        /// </summary>
        public IList<string> StyleSheets { get; set; }
        /// <summary>
        /// Get or set document setting
        /// </summary>
        public DocumentSetting Settings { get; set; }

        /// <summary>
        /// Get if force download
        /// </summary>
        public bool Download { get; private set; }

        /// <summary>
        /// ExecuteResult
        /// </summary>
        /// <param name="context"></param>
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.ContentType = "application/pdf";

            if (Download)
                context.HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + FileName);

            Html = RenderRazorViewToString(context);

            Format(context.HttpContext);

            using (var document = new Document(Settings.PageSize, Settings.Margin.Left, Settings.Margin.Right, Settings.Margin.Top, Settings.Margin.Bottom))
            {
                var memoryStream = new MemoryStream();
                TextReader textReader = new StringReader(Html);
                var pdfWriter = PdfWriter.GetInstance(document, memoryStream);
                var htmlPipelineContext = new HtmlPipelineContext(null);
                var cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(false);

                document.Open();

                FontFactory.RegisterDirectories();

                htmlPipelineContext.SetTagFactory(Tags.GetHtmlTagProcessorFactory());

                foreach (var styleSheet in StyleSheets)
                {
                    cssResolver.AddCssFile(context.HttpContext.Server.MapPath(styleSheet), true);
                }

                var pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlPipelineContext, new PdfWriterPipeline(document, pdfWriter)));
                var worker = new XMLWorker(pipeline, true);
                var xmlParse = new XMLParser(true, worker);

                xmlParse.Parse(textReader);
                xmlParse.Flush();

                document.Close();
                document.Dispose();

                context.HttpContext.Response.BinaryWrite(memoryStream.ToArray());
            }

            context.HttpContext.Response.End();
            context.HttpContext.Response.Flush();
        }

        /// <summary>
        /// Format html
        /// </summary>
        /// <param name="httpContext"></param>
        private void Format(HttpContextBase httpContext)
        {
            var cq = CQ.Create(Html);

            foreach (var img in cq.Select("img"))
            {
                var src = img.Attributes["src"];

                if (!src.StartsWith("http://") && !src.StartsWith("https://"))
                {
                    img.Attributes["src"] = string.Concat(httpContext.Request.Url?.Scheme, "://",
                        httpContext.Request.Url?.Authority, src);
                }
            }

            Html = cq.Render();
        }

        /// <summary>
        /// Render razor view to string
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string RenderRazorViewToString(ControllerContext context)
        {
            context.Controller.ViewData.Model = Model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(context, ViewName);
                var viewContext = new ViewContext(context, viewResult.View, context.Controller.ViewData, context.Controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(context, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}