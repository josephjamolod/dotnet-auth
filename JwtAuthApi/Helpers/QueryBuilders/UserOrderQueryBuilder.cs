using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Helpers.HelperObjects;
using JwtAuthApi.Models;

namespace JwtAuthApi.Helpers.QueryBuilders
{
    // public static class UserOrderQueryBuilder
    // {
    //     public static IQueryable<Order> ApplyFilters(IQueryable<Order> query, MyOrdersQuery queryObject)
    //     {
    //         if (!string.IsNullOrEmpty(queryObject.OrderNumber))
    //             query = query.Where(o => o.OrderNumber.Contains(queryObject.OrderNumber));


    //         if (queryObject.Total > 0)
    //             query = query.Where(o => o.Total <= queryObject.Total);

    //         if (queryObject.Status.HasValue)
    //             query = query.Where(o => o.Status == queryObject.Status.Value);

    //         if (queryObject.IsDescending)
    //         {
    //             query = query.OrderByDescending(f => f.CreatedAt);
    //         }
    //         else
    //         {
    //             query = query.OrderBy(f => f.CreatedAt);
    //         }
    //         return query;
    //     }
    // }
    public static class UserOrderQueryBuilder
    {
        public static IQueryable<Order> ApplyFilters(IQueryable<Order> query, MyOrdersQuery queryObject)
        {
            if (!string.IsNullOrEmpty(queryObject.OrderNumber))
                query = query.Where(o => o.OrderNumber.Contains(queryObject.OrderNumber));

            if (queryObject.Total > 0)
                query = query.Where(o => o.Total <= queryObject.Total);

            if (queryObject.Status.HasValue)
                query = query.Where(o => o.Status == queryObject.Status.Value);

            return query;
        }

        public static IQueryable<Order> ApplySorting(IQueryable<Order> query, MyOrdersQuery queryObject)
        {
            if (queryObject.IsDescending)
                return query.OrderByDescending(o => o.CreatedAt);

            return query.OrderBy(o => o.CreatedAt);
        }
    }
}