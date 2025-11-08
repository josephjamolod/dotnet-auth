using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Models;
using JwtAuthApi.Repository.Models;

namespace JwtAuthApi.Repository.QueryBuilders
{
    public class PendingSellerQueryBuilder
    {
        public static IQueryable<AppUser> ApplyPendingSellerFilters(IQueryable<AppUser> query, PendingSellerQueryObj queryObject)
        {
            if (!string.IsNullOrEmpty(queryObject.FirstName))
                query = query.Where(u => u.FirstName != null && u.FirstName.Contains(queryObject.FirstName));

            if (!string.IsNullOrEmpty(queryObject.LastName))
                query = query.Where(u => u.LastName != null && u.LastName.Contains(queryObject.LastName));

            if (!string.IsNullOrEmpty(queryObject.Email))
                query = query.Where(u => u.Email!.Contains(queryObject.Email));

            if (!string.IsNullOrEmpty(queryObject.PhoneNumber))
                query = query.Where(u => u.PhoneNumber!.Contains(queryObject.PhoneNumber));

            return query;
        }

        public static IQueryable<AppUser> ApplyPendingSellerSorting(IQueryable<AppUser> query, PendingSellerQueryObj queryObject)
        {
            switch (queryObject.SortBy)
            {
                case SortByOption.Name:
                    if (queryObject.IsDescending)
                        query = query.OrderByDescending(u => u.FirstName);
                    else
                        query = query.OrderBy(u => u.FirstName);
                    break;

                case SortByOption.LastName:
                    if (queryObject.IsDescending)
                        query = query.OrderByDescending(u => u.LastName);
                    else
                        query = query.OrderBy(u => u.LastName);
                    break;

                default:
                    query = query.OrderByDescending(u => u.CreatedAt);
                    break;
            }
            return query;
        }
    }
}