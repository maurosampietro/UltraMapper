using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class ReferenceMapperContext : MapperContext
    {
        public ConstantExpression SourceInstanceNullValue { get; protected set; }
        public ConstantExpression TargetInstanceNullValue { get; protected set; }

        public ParameterExpression ReferenceTracker { get; protected set; }
        public ParameterExpression Mapper { get; private set; }

        public static MethodInfo RecursiveMapMethodInfo { get; protected set; }
        public Mapper MapperInstance { get; private set; }

        static ReferenceMapperContext()
        {
            RecursiveMapMethodInfo = GetUltraMapperMapGenericMethodMemberMapping();
        }

        public ReferenceMapperContext( Mapping mapping )
            : this( mapping.Source.EntryType, mapping.Target.EntryType, (IMappingOptions)mapping ) { }

        public ReferenceMapperContext( Type source, Type target, IMappingOptions options )
             : base( source, target, options )
        {
            ReferenceTracker = Expression.Parameter( typeof( ReferenceTracker ), "referenceTracker" );

            if( !SourceInstance.Type.IsValueType )
                SourceInstanceNullValue = Expression.Constant( null, SourceInstance.Type );

            if( !TargetInstance.Type.IsValueType )
                TargetInstanceNullValue = Expression.Constant( null, TargetInstance.Type );

            Mapper = Expression.Variable( typeof( Mapper ), "mapper" );
            MapperInstance = new Mapper( MapperConfiguration );
        }

        private static MethodInfo GetUltraMapperMapGenericMethodMemberMapping()
        {
            return typeof( Mapper )
                .GetMethods( BindingFlags.Instance | BindingFlags.NonPublic )
                .Where( m => m.Name == nameof( UltraMapper.Mapper.Map ) )
                .Select( m => new
                {
                    Method = m,
                    Params = m.GetParameters(),
                    GenericArgs = m.GetGenericArguments()
                } )
                .Where
                (
                    x =>
                    x.Params.Length == 4 && x.GenericArgs.Length == 2 &&
                    x.Params[ 0 ].ParameterType == x.GenericArgs[ 0 ] &&
                    x.Params[ 1 ].ParameterType == x.GenericArgs[ 1 ] &&
                    x.Params[ 2 ].ParameterType == typeof( ReferenceTracker ) &&
                    x.Params[ 3 ].ParameterType == typeof( IMapping )
                 )
                .Select( x => x.Method )
                .First();
        }
    }
}
