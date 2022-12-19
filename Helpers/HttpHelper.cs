using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialLibraryBot.Helpers
{
    public class HttpHelper
    {
        public static async Task<HttpResponseMessage> GetAsync(string uri)
        {
            var response = (HttpResponseMessage?)null;

            using (var client = new HttpClient())
            {
                response = await client.GetAsync(uri);
            }

            return response;
        }
    }
}
