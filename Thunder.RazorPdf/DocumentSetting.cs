using iTextSharp.text;

namespace Thunder.RazorPdf
{
    /// <summary>
    ///     Pdf settings
    /// </summary>
    public class DocumentSetting
    {
        /// <summary>
        ///     New instance of class <see cref="DocumentSetting" />
        /// </summary>
        public DocumentSetting()
        {
            PageSize = iTextSharp.text.PageSize.A4;
            Margin = new DocumentMargin();
        }

        /// <summary>
        ///     Get or set page size
        /// </summary>
        public Rectangle PageSize { get; set; }

        /// <summary>
        ///     Get or set margin of document
        /// </summary>
        public DocumentMargin Margin { get; set; }
    }
}