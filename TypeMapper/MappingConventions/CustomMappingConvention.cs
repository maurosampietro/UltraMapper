﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.MappingConventions
{
    /// <summary>
    /// Inizialize a blank mapping convention.
    /// No property matching rule is applied by default.
    /// </summary>
    public class CustomMappingConvention : IMappingConvention
    {
        public PropertyMatchingConfiguration PropertyMatchingRules { get; set; }

        public CustomMappingConvention()
        {
            this.PropertyMatchingRules = new PropertyMatchingConfiguration();
        }

        public bool IsMatch( MemberInfo source, MemberInfo target )
        {
            if( source is PropertyInfo && target is PropertyInfo )
            {
                return this.PropertyMatchingRules.MatchingEvaluator(
                    (PropertyInfo)source, (PropertyInfo)target );
            }

            throw new ArgumentException( $"Cannot handle {source} and {target}" );
        }
    }
}
