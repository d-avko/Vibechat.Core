using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Vibechat.Web.Extension_methods
{
    public static class ExpressionExtensions
    {
        public static string GetNestedMemberAccessString(this Expression expression)
        {
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)expression;
                string parentValue;

                //do not include lambda variable.
                if (memberExpression.Expression is ParameterExpression)
                {
                    parentValue = memberExpression.Member.Name;
                }
                else
                {
                    parentValue = GetNestedMemberAccessString(memberExpression.Expression) + "." + memberExpression.Member.Name;
                }

                return parentValue;
            }
            else
            {
                throw new ArgumentException("The expression must contain only member access calls.", "expression");
            }
        }
    }
}
