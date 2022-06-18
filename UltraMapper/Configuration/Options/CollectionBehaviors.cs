namespace UltraMapper
{
    public enum CollectionBehaviors
    {
        /// <summary>
        /// Inherits this option
        /// </summary>
        INHERIT,

        /// <summary>
        /// If <see cref="ReferenceBehaviors"/> is set to <see cref="ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL"/>
        /// keeps using the target collection (same reference) and then the target collection is cleared and then elements are added. 
        /// 
        /// If <see cref="ReferenceBehaviors"/> is set to <see cref="ReferenceBehaviors.CREATE_NEW_INSTANCE"/>
        /// a new instance is created for the target collection.
        /// </summary>
        RESET,

        /// <summary>
        /// If <see cref="ReferenceBehaviors"/> is set to <see cref="ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL"/>
        /// keeps using the target collection (same reference) and elements are added.
        /// 
        /// If <see cref="ReferenceBehaviors"/> is set to <see cref="ReferenceBehaviors.CREATE_NEW_INSTANCE"/>
        /// the target collection's items are added to the new instance and then source collection's items are added
        /// </summary>
        MERGE,

        /// <summary>
        /// Keeps using the target collection (same reference).
        /// If <see cref="ReferenceBehaviors"/> is set to <see cref="ReferenceBehaviors.CREATE_NEW_INSTANCE"/> it is ignored.
        /// Each target item matching a source item is updated.
        /// Each source item non existing in the target collection is added.
        /// Each target item non existing in the source collection is removed.
        /// A way to compare two items must be provided.
        /// </summary>
        UPDATE
    }
}
