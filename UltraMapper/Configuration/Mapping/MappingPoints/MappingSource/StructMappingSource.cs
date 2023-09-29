using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UltraMapper.Internals
{
    public class StructMappingSource : IMappingSource
    {
        private readonly MemberAccessPath _sourcePath;
        private readonly MemberInfo _memberInfo;
        private string _toString;
        private LambdaExpression _valueGetter;

        public StructMappingSource( MemberAccessPath sourcePath, LambdaExpression sourceMemberGetterExpression )
        {
            _sourcePath = sourcePath;
            _memberInfo = sourcePath.Last();
            var originalReturnType = sourcePath.ReturnType;
            ReturnType = GetNullableType( originalReturnType );
            _valueGetter = sourcePath.GetNullableGetterExpWithNullChecks();
        }

        public LambdaExpression ValueGetter => _valueGetter;

        public Type EntryType => _sourcePath.EntryInstance;

        public Type ReturnType { get; private set; }

        public Type MemberType => ReturnType;

        public MemberAccessPath MemberAccessPath => _sourcePath;

        public bool Ignore { get; set; }

        public override bool Equals( object obj )
        {
            if( obj is StructMappingSource mp )
            {
                return _memberInfo.Equals( mp._memberInfo ) &&
                    this.MemberAccessPath.EntryInstance.Equals( mp.MemberAccessPath.EntryInstance ) &&
                    this.MemberAccessPath.ReturnType.Equals( mp.MemberAccessPath.ReturnType );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _memberInfo.GetHashCode() ^
                this.MemberAccessPath.EntryInstance?.GetHashCode() ?? 0 ^
                this.MemberAccessPath.ReturnType?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            if( _toString == null )
            {
                string typeName = this.MemberType.GetPrettifiedName();
                _toString = this._sourcePath.Last() == this.MemberType ? typeName
                    : $"{typeName} {this._sourcePath.Last().Name}";
            }

            return _toString;
        }

        private Type GetNullableType( Type notNullableType )
        {
            var type = Nullable.GetUnderlyingType( notNullableType ) ?? notNullableType; // avoid type becoming null
            if( type.IsValueType )
                return typeof( Nullable<> ).MakeGenericType( type );
            else
                return type;
        }
    }
}
