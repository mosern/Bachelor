using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http.Routing;

namespace Api.Classes
{
    public class PaginationHeader
    {
        int currentPage;
        int pageSize;
        int totalCount;
        int totalPages;
        string previousPageLink;
        string nextPageLink;

        private PaginationHeader(int _currentPage, int _pageSize, int _totalCount, string routeName, IDictionary<string, object> _routeValues)
        {
            currentPage = _currentPage;
            pageSize = _pageSize;
            totalCount = _totalCount;
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize);


            UrlHelper urlHelper = new UrlHelper();

            ExpandoObject routeValues = new ExpandoObject();

            foreach(KeyValuePair<string, object> entry in _routeValues)
            {
                ((IDictionary<string, object>)routeValues).Add(entry.Key, entry.Value);
            }

            ((IDictionary<string, object>)routeValues).Add("currentPage", currentPage - 1);
            ((IDictionary<string, object>)routeValues).Add("pageSize", pageSize);

            previousPageLink = currentPage > 1 ? urlHelper.Link(routeName, routeValues) : "";
        }

        public static PaginationHeader Get(int currentPage, int pageSize, int totalCount, string routeName, IDictionary<string, object> routeValues)
        {
            return new PaginationHeader(currentPage, pageSize, totalCount, routeName, routeValues);
        }
    }
}