namespace UltraMapper.Internals
{
    //Cannot be a struct cause assignment to null is needed
    public class ObjectPair
    {
        public readonly object Source;
        public readonly object Target;

        public ObjectPair( object source, object target )
        {
            this.Source = source;
            this.Target = target;
        }
    }
}
