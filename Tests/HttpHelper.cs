using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Tests
{
    public static class HttpHelper
    {
        public static async Task<T> GetDataFromAddress<T>(string address)
        {
            //todo: think if need to automaticaly close connection
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(address);

                Type desiredType = typeof(T);

                if (desiredType == typeof(string))
                {
                    return (T)Convert.ChangeType(await response.Content.ReadAsStringAsync(), desiredType);
                }

                throw new InvalidOperationException("unknown requested data type");
            }
        }

        
        public static string BuildRequestUrl(string baseAddress, string method, Dictionary<string, string> parameters = null)
        {
            if (parameters != null)
            {
                NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

                foreach (var pair in parameters)
                    queryString[pair.Key] = pair.Value;

                return string.Concat(baseAddress, method, "?", queryString.ToString());
            }
            else
                return string.Concat(baseAddress, method);
        }

    }
}
