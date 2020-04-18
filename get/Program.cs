using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace HttpGet
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1) return;
            string sUrl = args[0];
            string[] sParams = sUrl.Split('?')[1].Split('&');
            for (int i = 0; i < sParams.Length; i++)
            {
                sParams[i] = sParams[i].Split('=')[0] + "=" + HttpUtility.UrlEncode(sParams[i].Split('=')[1]);
            }
            sUrl = sUrl.Split('?')[0] + "?" + string.Join("&", sParams);
            Request req = new Request();
            Request.Response rsp = req.Get(sUrl);
            Console.Write(rsp.ToString());
        }
    }
}
