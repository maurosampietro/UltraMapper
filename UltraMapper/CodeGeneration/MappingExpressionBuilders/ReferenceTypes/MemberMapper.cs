using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UltraMapper.Internals;
using UltraMapper.ReferenceTracking;

namespace UltraMapper.MappingExpressionBuilders
{
    public class MemberMapper : IMappingExpressionBuilder
    {
        private const string errorMsg = "Error mapping '{0}'. Value '{1}' (of type '{2}') cannot be assigned to the target (of type '{3}').";
        private static readonly Expression<Func<string, string, object, Type, Type, string>> _getErrorExp =
            ( error, mapping, sourceMemberValue, sourceType, targetType ) => String.Format( error, mapping,
                sourceMemberValue ?? "null", sourceType.GetPrettifiedName(), targetType.GetPrettifiedName() );

        public bool CanHandle( Mapping mapping )
        {
            return mapping is MemberMapping;
        }

        public LambdaExpression GetMappingExpression( Mapping mapping )
        {
            if( mapping is not MemberMapping memberMapping )
                throw new ArgumentException( $"Expected '{nameof( MemberMapping )}' as argument for parameter '{nameof( mapping )}'" );

            if( memberMapping.Mapper is ReferenceMapper )
                return GetComplexMemberExpression( memberMapping );

            return GetSimpleMemberExpression( memberMapping );
        }

        private LambdaExpression GetComplexMemberExpression( MemberMapping mapping )
        {
            var memberContext = new MemberMappingContext( mapping );

            if( mapping.CustomConverter != null )
            {
                var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
                var targetSetterValueParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

                var valueReaderExp = Expression.Invoke( mapping.CustomConverter, memberContext.SourceMemberValueGetter );

                var exp = mapping.TargetMember.ValueSetter.Body
                    .ReplaceParameter( memberContext.TargetInstance, targetSetterInstanceParamName )
                    .ReplaceParameter( valueReaderExp, targetSetterValueParamName );

                return ToActionWithReferenceTrackerLambda( exp, memberContext );
            }

            var memberAssignmentExp = ((IMemberMappingExpression)mapping.Mapper)
                .GetMemberAssignment( memberContext, out bool needsTrackingOrRecursion );

            if( !needsTrackingOrRecursion )
            {
                var exp = memberAssignmentExp
                    .ReplaceParameter( memberContext.SourceMemberValueGetter, "sourceValue" );

                //if a setter method was provided or resolved a target value getter may be missing
                if( memberContext.TargetMemberValueGetter != null )
                    exp = exp.ReplaceParameter( memberContext.TargetMemberValueGetter, "targetValue" );
                else // if( memberContext.TargetMemberValueSetter != null ) fails directly if not resolved/provided
                    exp = exp.ReplaceParameter( memberContext.TargetMemberValueSetter, "targetValue" );

                return ToActionWithReferenceTrackerLambda( exp, memberContext );
            }

            if( memberContext.Options.IsReferenceTrackingEnabled )
            {
                var parameters = new List<ParameterExpression>()
                {
                    memberContext.SourceMember,
                    memberContext.TargetMember,
                    memberContext.TrackedReference
                };

                var exp = Expression.Block
                (
                    parameters,

                    Expression.Assign( memberContext.SourceMember, memberContext.SourceMemberValueGetter ),

                    ReferenceTrackingExpression.GetMappingExpression
                    (
                        memberContext.ReferenceTracker, memberContext.SourceMember,
                        memberContext.TargetMember, memberAssignmentExp,
                        memberContext.Mapper, memberContext.MapperInstance, mapping,
                        Expression.Constant( mapping )
                    ),

                    memberContext.TargetMemberValueSetter
                );

                return ToActionWithReferenceTrackerLambda( exp, memberContext );
            }
            else
            {

                //non recursive
                //var exp = Expression.Block
                //(
                //    memberAssignmentExp
                //        .ReplaceParameter( memberContext.SourceMemberValueGetter, "sourceValue" )
                //        .ReplaceParameter( memberContext.TargetMemberValueGetter, "targetValue" ),

                //    Expression.Invoke( mapping.MappingExpression, memberContext.ReferenceTracker,
                //        memberContext.SourceMemberValueGetter, memberContext.TargetMemberValueGetter )
                //);

                //recursive
                var mapMethod = ReferenceMapperContext.RecursiveMapMethodInfo
                    .MakeGenericMethod( memberContext.SourceMember.Type, memberContext.TargetMember.Type );

                var exp = Expression.Block
                (
                    memberAssignmentExp
                        .ReplaceParameter( memberContext.SourceMemberValueGetter, "sourceValue" )
                        .ReplaceParameter( memberContext.TargetMemberValueGetter, "targetValue" ),

                    Expression.Call( memberContext.Mapper, mapMethod, memberContext.SourceMemberValueGetter,
                        memberContext.TargetMemberValueGetter,
                        memberContext.ReferenceTracker, Expression.Constant( mapping ) )
                );

                return ToActionWithReferenceTrackerLambda( exp, memberContext );
            }
        }

        private LambdaExpression GetSimpleMemberExpression( MemberMapping mapping )
        {
            var memberContext = new MemberMappingContext( mapping );

            var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
            var targetSetterValueParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

            var valueReaderExp = mapping.MappingExpression.Body.ReplaceParameter(
                memberContext.SourceMemberValueGetter, mapping.MappingExpression.Parameters[ 0 ].Name );

            if( mapping.MappingExpression.Parameters[ 0 ].Type == typeof( ReferenceTracker ) )
            {
                valueReaderExp = mapping.MappingExpression.Body.ReplaceParameter(
                    memberContext.SourceMemberValueGetter, mapping.MappingExpression.Parameters[ 1 ].Name );
            }

            var expression = mapping.TargetMember.ValueSetter.Body
                .ReplaceParameter( memberContext.TargetInstance, targetSetterInstanceParamName )
                .ReplaceParameter( valueReaderExp, targetSetterValueParamName );

            var exceptionParam = Expression.Parameter( typeof( Exception ), "exception" );
            var ctor = typeof( ArgumentException )
                .GetConstructor( new Type[] { typeof( string ), typeof( Exception ) } );

            var getErrorMsg = Expression.Invoke
            (
                _getErrorExp,
                Expression.Constant( errorMsg ),
                Expression.Constant( memberContext.Options.ToString() ),
                Expression.Convert( memberContext.SourceMemberValueGetter, typeof( object ) ),
                Expression.Constant( memberContext.SourceMember.Type ),
                Expression.Constant( memberContext.TargetMember.Type )
            );

            expression = Expression.TryCatch
            (
                Expression.Block( typeof( void ), expression ),

                Expression.Catch( exceptionParam, Expression.Throw
                (
                    Expression.New( ctor, getErrorMsg, exceptionParam ),
                    typeof( void )
                ) )
            );

            var delegateType = typeof( Action<,> ).MakeGenericType(
            memberContext.SourceInstance.Type, memberContext.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                memberContext.SourceInstance, memberContext.TargetInstance );
        }

        private LambdaExpression ToActionWithReferenceTrackerLambda( Expression expression, MemberMappingContext memberContext )
        {
            var delegateType = typeof( Action<,,> ).MakeGenericType(
                memberContext.ReferenceTracker.Type, memberContext.SourceInstance.Type, memberContext.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
               memberContext.ReferenceTracker, memberContext.SourceInstance, memberContext.TargetInstance );
        }
    }
}
