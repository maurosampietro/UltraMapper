namespace UltraMapper
{
    public enum ReferenceBehaviors
    {
        /// <summary>
        /// Creates a new instance, but only if the reference has not been mapped and tracked yet.
        /// If the reference has been mapped and tracked, the tracked object is assigned.
        /// This is the default.
        /// </summary>
        CREATE_NEW_INSTANCE,

        /// <summary>
        /// The instance of the target is used in one particular case, following this table:
        /// SOURCE (NULL) -> TARGET = NULL
        /// 
        /// SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET (NULL) = ASSIGN NEW OBJECT 
        /// SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET (NOT NULL) = KEEP USING INSTANCE OR CREATE NEW INSTANCE
        /// 
        /// SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NULL) = ASSIGN TRACKED OBJECT
        /// SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NOT NULL) = ASSIGN TRACKED OBJECT (the priority is to map identically the source to the target)
        /// </summary>
        USE_TARGET_INSTANCE_IF_NOT_NULL
    }
}
