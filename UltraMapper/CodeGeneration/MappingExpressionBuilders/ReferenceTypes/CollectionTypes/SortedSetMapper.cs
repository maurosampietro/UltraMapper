//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using UltraMapper.Internals;

//namespace UltraMapper.MappingExpressionBuilders
//{
//    //We only solve a problem starting from .NET5 
//    //Solving reported issue: https://github.com/dotnet/runtime/issues/71643
//    public class SortedSetMapper : CollectionMapper
//    {
//        public override bool CanHandle( Mapping mapping )
//        {
//            return base.CanHandle( mapping ) &&
//                //mapping.Source.EntryType.IsCollectionOfType( typeof( SortedSet<> ) ) &&
//                mapping.Target.EntryType == typeof( SortedSet<string> );
//        }

//        protected override Expression GetNewInstanceFromSourceCollection( MemberMappingContext context, CollectionMapperContext collectionContext )
//        {
//            var targetConstructor = context.TargetMember.Type.GetConstructor(
//                new[] { typeof( IComparer<> ).MakeGenericType( collectionContext.TargetCollectionElementType ) } );

//            if( targetConstructor == null ) return null;
//            return Expression.New( targetConstructor, Expression.Property( context.SourceMember, nameof( SortedSet<int>.Comparer ) ) );


//            // Expression.Constant( StringComparer.OrdinalIgnoreCase );


//        }
//    }
//}
