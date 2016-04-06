using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Dynamic;

namespace ExpenseTracker.API.Helpers
{
    /// <summary>
    /// Written by Kevin Dockx as a part of the course "Building and Securing a RESTful API for Multiple Clients in ASP.NET" on pluralsight.
    /// https://app.pluralsight.com/library/courses/building-securing-restful-api-aspdotnet/description
    /// </summary>
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string sort)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (sort == null)
            {
                return source;
            }

            // split the sort string
            var lstSort = sort.Split(',');

            // run through the sorting options and apply them - in reverse
            // order, otherwise results will come out sorted by the last 
            // item in the string first!
            foreach (var sortOption in lstSort.Reverse())
            {
                // if the sort option starts with "-", we order
                // descending, ortherwise ascending

                if (sortOption.StartsWith("-"))
                {
                    source = source.OrderBy(sortOption.Remove(0, 1) + " descending");
                }
                else
                {
                    source = source.OrderBy(sortOption);
                }

            }

            return source;
        }
    }
}