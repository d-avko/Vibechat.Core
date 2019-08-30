using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Vibechat.DataLayer.Repositories.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        List<string> IncludeStrings { get; }
        Expression<Func<T, object>> OrderBy { get; }
        Expression<Func<T, object>> OrderByDescending { get; }
        Expression<Func<T, object>> GroupBy { get; }

        //Expression<Func<T, object>> Union { get; }

        int Take { get; }
        int Skip { get; }
        bool IsPagingEnabled { get; }

        bool IsAsNoTracking { get; }
    }
}
