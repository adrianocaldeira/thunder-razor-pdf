namespace Thunder.RazorPdf
{
    /// <summary>
    ///     Page
    /// </summary>
    public class DocumentMargin
    {
        /// <summary>
        ///     Initializa new instance of class <see cref="DocumentMargin" />
        /// </summary>
        public DocumentMargin()
        {
            Top = 40;
            Left = 40;
            Right = 40;
            Bottom = 40;
        }

        /// <summary>
        ///     Get or set top margin
        /// </summary>
        public float Top { get; set; }

        /// <summary>
        ///     Get or set bottom margin
        /// </summary>
        public float Bottom { get; set; }

        /// <summary>
        ///     Get or set left margin
        /// </summary>
        public float Left { get; set; }

        /// <summary>
        ///     Get or set right margin
        /// </summary>
        public float Right { get; set; }
    }
}