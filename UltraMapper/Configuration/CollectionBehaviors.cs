namespace UltraMapper
{
    public enum CollectionBehaviors
    {
        /// <summary>
        /// Inherit this option
        /// </summary>
        INHERIT,

        /// <summary>
        /// Keep using the input collection (same reference). 
        /// The collection is cleared and then elements are added. 
        /// </summary>
        RESET,

        /// <summary>
        /// Keep using the input collection (same reference).
        /// The collection is untouched and elements are added.
        /// </summary>
        MERGE,

        /// <summary>
        /// Keep using the input collection (same reference).
        /// Each source item matching a target item is updated.
        /// Each source item non existing in the target collection is added.
        /// Each target item non existing in the source collection is removed.
        /// A way to compare two items must be provided.
        /// </summary>
        UPDATE
    }
}
