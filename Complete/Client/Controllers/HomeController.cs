using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Pivotal.Discovery.Client;
using Steeltoe.Common.Discovery;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index([FromServices]IDiscoveryClient discoveryClient)
        {
            var omsUrl = discoveryClient.GetInstances("ORDERMANAGER")?.FirstOrDefault()?.Uri?.ToString() ?? "http://localhost:8080";
            var mdsUrl = discoveryClient.GetInstances("MDS")?.FirstOrDefault()?.Uri?.ToString() ?? "http://localhost:53809";
            
            ViewBag.OMS =  omsUrl.MatchCurrentScheme(HttpContext) ;
            ViewBag.MDS = mdsUrl.MatchCurrentScheme(HttpContext);
            var features = new Features()
            {
                TickerEnabled = true
            };
            return View(features);
        }
        
    }
    
}
