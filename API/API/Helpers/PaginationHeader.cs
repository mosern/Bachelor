using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Routing;

namespace Api.Classes
{
    /// <summary>
    /// PaginationHeader object and static metode for creating it.
    /// </summary>
    public class PaginationHeader
    {
        public int page;
        public int pageSize;
        public int totalCount;
        public int totalPages;
        public string previousPageLink;
        public string nextPageLink;

        private PaginationHeader(int _page, int _pageSize, int _totalCount, string routeName, IDictionary<string, object> _routeValues, HttpRequestMessage request)
        {
            page = _page;
            pageSize = _pageSize;
            totalCount = _totalCount;
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize);


            UrlHelper urlHelper = new UrlHelper(request);

            ExpandoObject routeValues = new ExpandoObject();

            //TODO Check what this is about!
            //foreach (KeyValuePair<string, object> entry in _routeValues)
            //{
            //    _routeValues.Add(entry.Key, entry.Value);
            //}

            _routeValues.Add("page", page - 1);
            _routeValues.Add("pageSize", pageSize);

            previousPageLink = page > 1 ? urlHelper.Link(routeName, _routeValues) : "";

            _routeValues["page"] = page + 1;

            nextPageLink = page < totalPages ? urlHelper.Link(routeName, _routeValues) : "";
        }

        public static PaginationHeader Get(int page, int pageSize, int totalCount, string routeName, IDictionary<string, object> routeValues, HttpRequestMessage request)
        {
            return new PaginationHeader(page, pageSize, totalCount, routeName, routeValues, request);
        }
    }
}