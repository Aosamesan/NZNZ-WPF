using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NZNZWPF
{
    class WebClientWithTimeOut : WebClient
    {
        public static readonly string USER_AGENT_STR
            = @"Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

        private int timeout;

        public int Timeout
        {
            get { return timeout; }
        }

        public WebClientWithTimeOut(int timeout = 2000)
        {
            this.timeout = timeout;
            this.Headers[HttpRequestHeader.UserAgent] = USER_AGENT_STR;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            request.Timeout = timeout;

            return request;
        }
    }
}
