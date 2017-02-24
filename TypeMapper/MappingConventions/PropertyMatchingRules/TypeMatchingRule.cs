﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.MappingConventions
{
    /// <summary>
    /// Two properties match if the source type is of the same type 
    /// or (optionally) implicitly convertible to the target type.
    /// </summary>
    public class TypeMatchingRule : PropertyMatchingRuleBase
    {
        public bool AllowImplicitConversions { get; set; } = true;
        public bool AllowExplicitConversions { get; set; } = true;
        public bool AllowNullableUnwrappings { get; set; } = true;

        public override bool IsCompliant( MemberInfo source, MemberInfo target )
        {
            var sourceType = source.GetMemberType();
            var targetType = target.GetMemberType();

            var isCompliant = this.CanHandle( sourceType, targetType );
            //if( !isCompliant && this.AllowNullableUnwrappings )
            //{
            //    isCompliant = targetType.IsAssignableFrom( sourceType );

            //    if( sourceType.IsNullable() && !targetType.IsNullable() )
            //    {
            //        var underlyingSourceType = Nullable.GetUnderlyingType( sourceType );
            //        isCompliant = this.CanHandle( underlyingSourceType, targetType );
            //    }
            //}

            return isCompliant;
        }

        private bool CanHandle( Type source, Type target )
        {
            //PrimitiveType -> Nullable<PrimitiveType> always possible. No flag to disable that.
            Lazy<bool> primitiveToNullablePrimitive = new Lazy<bool>( () =>
            {
                return !source.IsNullable() && target.IsNullable() && target.IsAssignableFrom( source );
            } );

            return source == target || primitiveToNullablePrimitive.Value ||
                (this.AllowExplicitConversions && source.IsImplicitlyConvertibleTo( target )) ||
                (this.AllowExplicitConversions && source.IsExplicitlyConvertibleTo( target ));
        }
    }
}