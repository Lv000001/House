using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace House.Helper
{
    class HttpHelper
    {

        public static string DoGet(string url, out NameValueCollection responseHeader, NameValueCollection header = null, NameValueCollection paramInfos = null)
        {
            string returnValue = null;
            WebClient webClient = new WebClient();
            responseHeader = new NameValueCollection();
            try
            {
                webClient.Headers = new WebHeaderCollection();
                if (header != null)
                {
                    foreach (string key in header.Keys)
                    {
                        try
                        {
                            webClient.Headers.Set((HttpRequestHeader)System.Enum.Parse(typeof(HttpRequestHeader), key.Replace("-", "")), header[key]);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                if (paramInfos != null)
                {
                    foreach (string item in paramInfos.Keys)
                    {
                        url += string.Format("&{0}={1}", HttpUtility.UrlEncode(item), HttpUtility.UrlEncode(paramInfos[item]));
                    }
                }

                byte[] responseArray = webClient.DownloadData(url);
                responseHeader = webClient.ResponseHeaders;
                if (responseHeader["Content-Encoding"] != null && responseHeader["Content-Encoding"].ToLower().Contains("gzip"))
                {
                    responseArray = GZipHelper.Decompress(responseArray);
                }
                returnValue = Encoding.UTF8.GetString(responseArray);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex);
            }
            finally
            {
                webClient.Dispose();
                webClient = null;
            }
            return returnValue;
        }

        public static string DoPost(string url, out NameValueCollection responseHeader, NameValueCollection header = null, NameValueCollection paramInfos = null)
        {
            string returnValue = null;
            WebClient webClient = new WebClient();
            responseHeader = new NameValueCollection();
            try
            {
                if (header != null)
                {
                    foreach (string key in header.Keys)
                    {
                        try
                        {
                            webClient.Headers.Set((HttpRequestHeader)System.Enum.Parse(typeof(HttpRequestHeader), key.Replace("-", "")), header[key]);
                        }
                        catch (Exception)
                        {
                        };
                    }
                }
                byte[] responseArray = webClient.UploadValues(url, paramInfos);
                responseHeader = webClient.ResponseHeaders;
                if (responseHeader["Content-Encoding"] != null && responseHeader["Content-Encoding"].ToLower().Contains("gzip"))
                {
                    responseArray = GZipHelper.Decompress(responseArray);
                }
                returnValue = Encoding.UTF8.GetString(responseArray);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex);
            }
            finally
            {
                webClient.Dispose();
                webClient = null;
            }
            return returnValue;
        }

        public static string DoGet(string url, NameValueCollection header = null, NameValueCollection paramInfos = null)
        {
            NameValueCollection responseHeader = null;
            return DoGet(url, out responseHeader, header, paramInfos);
        }

        public static string DoPost(string url, NameValueCollection header = null, NameValueCollection paramInfos = null)
        {
            NameValueCollection responseHeader = null;
            return DoPost(url, out responseHeader, header, paramInfos);
        }

        public static string DoGet(string url, out string cookie, NameValueCollection header = null, NameValueCollection paramInfos = null)
        {
            cookie = "";
            NameValueCollection responseHeader = null;
            string result = DoGet(url, out responseHeader, header, paramInfos);
            foreach (string item in responseHeader.Keys)
            {
                if (item.ToLower().Equals("set-cookie"))
                {
                    cookie = responseHeader[item];
                    break;
                }
            }
            return result;
        }

        public static string DoPost(string url, out string cookie, NameValueCollection header = null, NameValueCollection paramInfos = null)
        {
            cookie = "";
            NameValueCollection responseHeader = null;
            string result = DoPost(url, out responseHeader, header, paramInfos);
            foreach (string item in responseHeader.Keys)
            {
                if (item.ToLower().Equals("set-cookie"))
                {
                    cookie = responseHeader[item];
                    break;
                }
            }
            return result;
        }
    }
}
