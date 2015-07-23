namespace Examine
{
    /// <summary>
    /// Represents an indexing operation (either add/remove)
    /// </summary>
    public class IndexOperation
    {
        public IndexOperation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public IndexOperation(IndexItem item, IndexOperationType operation)
        {
            Item = item;
            Operation = operation;
        }

        /// <summary>
        /// Gets the Index item
        /// </summary>
        public IndexItem Item { get; set; }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public IndexOperationType Operation { get; set; }
    }
}