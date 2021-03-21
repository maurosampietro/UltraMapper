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
        /// Keeps using the target collection (same reference). 
        /// The target collection is cleared and then elements are added. 
        /// 
        /// If <see cref="ReferenceBehaviors"/> is set to <see cref="ReferenceBehaviors.CREATE_NEW_INSTANCE"/>
        /// A new instance is created the target collection.
        /// </summary>
        RESET,

        /// <summary>
        /// If <see cref="ReferenceBehaviors"/> is set to <see cref="ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL"/>
        /// Keep using the target collection (same reference).
        /// The target collection is untouched and elements are added.
        /// 
        /// If <see cref="ReferenceBehaviors"/> is set to <see cref="ReferenceBehaviors.CREATE_NEW_INSTANCE"/>
        /// First target collection's items are added to the new instance and then
        /// source collection's items are added
        /// </summary>
        MERGE,

        /// <summary>
        /// Keep using the target collection (same reference).
        /// If <see cref="ReferenceBehaviors"/> is set to <see cref="ReferenceBehaviors.CREATE_NEW_INSTANCE"/> it is ignored.
        /// Each target item matching a source item is updated.
        /// Each source item non existing in the target collection is added.
        /// Each target item non existing in the source collection is removed.
        /// A way to compare two items must be provided.
        /// </summary>
        UPDATE
    }
}
