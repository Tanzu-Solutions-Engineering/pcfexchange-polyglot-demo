using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Client
{
    public static class UriExtensions
    {
        public static string MatchCurrentScheme(this string uri, HttpContext context)
        {
            string result;
            StringValues scheme;
            // when running on CF, the container is always going to be http as ssl termination happens before
            // however, original scheme will be carried in the incoming header
            if (!context.Request.Headers.TryGetValue("x-forwarded-proto", out scheme))
                scheme = new StringValues("http");
            if (scheme.FirstOrDefault() == "http")
                result = uri.Replace("https://", "http://");
            else
                result = uri.Replace("https://", "http://");
            return result;
        }
    }
}
