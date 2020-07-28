using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UltraMapper.Internals
{
    public static class ExpressionExtensions
    {
        public static MemberAccessPath ExtractMember( this Expression lambdaExpression )
        {
            var memberAcessPath = new MemberAccessPath();

            if( !(lambdaExpression is LambdaExpression lambda) )
                throw new InvalidCastException( "Invalid lambda expression" );

            var stack = new Stack<Expression>();
            Expression exp = lambda.Body;

            //we are always only interested in the left part of an assignment expression.
            if( exp.NodeType == ExpressionType.Assign )
                exp = ((BinaryExpression)exp).Left;

            //if the expression is a constant, we just return the type of the constant
            else if( exp.NodeType == ExpressionType.Constant )
            {
                memberAcessPath.Add( ((ConstantExpression)exp).Type );
                return memberAcessPath;
            }

            //break the expression down member by member
            while( !(exp is ParameterExpression) )
            {
                stack.Push( exp );

                if( exp.NodeType == ExpressionType.Convert )
                    exp = ((UnaryExpression)exp).Operand as Expression;

                else if( exp.NodeType == ExpressionType.MemberAccess )
                    exp = ((MemberExpression)exp).Expression;

                else if( exp.NodeType == ExpressionType.Call )
                    exp = ((MethodCallExpression)exp).Object;
            }

            //instance parameter
            stack.Push( exp );


            //If the instance on which we call is a derived class and the property
            //we select is defined in the base class, we will notice that
            //the PropertyInfo is retrieved through the base class; hence
            //DeclaredType and ReflectedType are equal and we basically
            //lose information about the ReflectedType (which should be the derived class)...
            //..to fix that we do another search. 
            //We search the member we are accessing by name in the actual type we meant to use for the invocation
            //Since we support deep member accessing, things get a little more complex here
            //but we basically just follow each member access starting from the passed lambda parameter.

            memberAcessPath = new MemberAccessPath();

            //Follow member accesses starting from the lambda input parameter.
            var lambdaMember = (stack.Pop() as ParameterExpression);
            var member = lambda.Parameters.First(
                p => p.Name == lambdaMember.Name ).Type as MemberInfo;

            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach( var item in stack )
            {
                string memberName = null;

                if( item is MemberExpression )
                    memberName = ((MemberExpression)item).Member.Name;
                else if( item is MethodCallExpression )
                    memberName = ((MethodCallExpression)item).Method.Name;

                member = member.GetMemberType().GetMember( memberName, bindingFlags )[ 0 ];
                memberAcessPath.Add( member );
            }

            return memberAcessPath;
        }

        public static Expression ReplaceParameter( this Expression expression, Expression newExpression, string name )
        {
            return new ExpressionParameterReplacer( newExpression, name ).Visit( expression );
        }
    }
}