using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pivotal.Discovery.Client;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index([FromServices]IDiscoveryClient discoveryClient)
        {
            var omsUrl = discoveryClient.GetInstances("ORDERMANAGER")?.FirstOrDefault()?.Uri?.ToString() ?? "http://localhost:8080";
            omsUrl = omsUrl.Replace("https://", "http://"); // need to force http due to self signed cert
            ViewBag.OMS =  omsUrl ;
            ViewBag.MDS = discoveryClient.GetInstances("MDS")?.FirstOrDefault()?.Uri?.ToString() ?? "http://localhost:53809";
            return View();
        }
        
    }
}
