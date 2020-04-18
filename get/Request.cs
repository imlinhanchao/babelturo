using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace HttpGet
{
    public class Request
    {
        public class Header
        {
            public string sContentType;
            public string sAccept;
            public string sUserAgent;
            public string sReferer;
            public Dictionary<string, string> defineHeaders;

            public Header(Header header)
            {
                sReferer = header.sReferer;
                sContentType = header.sContentType;
                sAccept = header.sAccept;
                sUserAgent = header.sUserAgent;
                defineHeaders = new Dictionary<string, string>(header.defineHeaders);
            }
            
            public Header()
            {
                sReferer = null;
                sContentType = "application/x-www-form-urlencoded";
                sAccept = "*/*";
                sUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36";
                defineHeaders = new Dictionary<string, string>();
            }

            /// <summary>
            /// 添加自定義請求頭，可以按如下使用：
            /// Header.Extend(new Dictionary<string, string>(){
            ///     { "key1", "value1" },
            ///     { "key2", "value2" }
            /// })
            /// </summary>
            /// <param name="header">請求頭字典</param>
            /// <returns>添加后的請求頭</returns>
            public Header Extend(Dictionary<string, string> header)
            {
                if (header == null) return this;
                foreach (string sKey in header.Keys)
                {
                    switch (sKey.ToLower())
                    {
                        case "referer": sReferer = header[sKey]; break;
                        case "content-type":
                        case "contenttype": sContentType = header[sKey]; break;
                        case "accept": sAccept = header[sKey]; break;
                        case "useragent": 
                        case "user-agent": sUserAgent = header[sKey]; break;
                        default: if (defineHeaders.ContainsKey(sKey))
                                defineHeaders[sKey] = header[sKey];
                            else
                                defineHeaders.Add(sKey, header[sKey]);
                    	break;
                    }
                }
                return this;
            }
        };

        public class Authorization
        {
            public string sUsername;
            public string sPassword;
            public string sType;

            public Authorization()
            {
                sUsername = "";
                sPassword = "";
                sType = "Basic";
            }

            public Authorization(string sUser, string sPasswd)
            {
                sUsername = sUser;
                sPassword = sPasswd;
                sType = "Basic";
            }

            public Authorization(string sUser, string sPasswd, string type)
            {
                sUsername = sUser;
                sPassword = sPasswd;
                sType = type;
            }
        }

        public class Options
        {
            public string sBaseUrl;
            public string sMethod;
            public string sData;
            public Header Headers;
            public CookieContainer Cookies;
            public Authorization AuthOption;
            public WebProxy Proxy;
            public int nTimeout;

            public Options()
            {
                sBaseUrl = "";
                sMethod = "GET";
                sData = "";
                Headers = new Header();
                Cookies = new CookieContainer();
                AuthOption = null;
                nTimeout = 30000;
                Proxy = null;
            }
        }

        public class Response
        {
            string m_sLastError = "";

            Dictionary<string, List<string>> m_Headers = new Dictionary<string,List<string>>();
            CookieCollection m_Cookies;
            HttpStatusCode m_Status;
            byte[] m_Body;
            string m_ContentType;
            string m_CharacterSet;
            string m_ContentEncoding;

            public HttpStatusCode Status { get { return m_Status; } }

            public byte[] Body { get { return m_Body; } }

            public Dictionary<string, Cookie> Cookies {  get { return Request.CookiesParse(m_Cookies); }  }

            public string ContentType { get { return m_ContentType; } }

            public string CharacterSet { get { return m_CharacterSet; } }

            public string ContentEncoding { get { return m_ContentEncoding; } }

            public string LastError { get { return m_sLastError; } }

            public Dictionary<string, List<string>> Headers { get { return m_Headers; } }

            /// <summary>
            /// 將 Response 解析為 Json 對象
            /// </summary>
            /// <returns>Json 對象</returns>
            public object ToJson()
            {
                return ToJson(Encoding.UTF8);
            }

            /// <summary>
            /// 將 Response 解析為 Json 對象
            /// </summary>
            /// <param name="encoding">指定編碼，可以解決解析后亂碼問題</param>
            /// <returns>Json 對象</returns>
            public object ToJson(Encoding encoding)
            { 
                try
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    object data = serializer.DeserializeObject(encoding.GetString(m_Body));
                    return data;
                }
                catch (System.Exception e)
                {
                    m_sLastError = e.Message;
                    return null;
                }
            }

            /// <summary>
            /// 將 Response 解析為 HTML 源代碼
            /// </summary>
            /// <returns>HTML 源代碼 字符串</returns>
            public string ToHtml()
            {
                return ToHtml(Encoding.UTF8);
            }

            /// <summary>
            /// 將 Response 解析為 HTML 源代碼
            /// </summary>
            /// <param name="encoding">指定編碼，可以解決解析后亂碼問題</param>
            /// <returns>HTML 源代碼 字符串</returns>
            public string ToHtml(Encoding encoding)
            {
                try
                {
                    return CorrectEncode(m_Body, encoding);
                }
                catch (System.Exception e)
                {
                    m_sLastError = e.Message;
                    return "";
                }
            }

            public override string ToString()
            {
                return CorrectEncode(m_Body, Encoding.UTF8);
            }

            public string ToString(Encoding encoding)
            {
                try
                {
                    return CorrectEncode(m_Body, encoding);
                }
                catch (System.Exception e)
                {
                    m_sLastError = e.Message;
                    return "";
                }
            }

            public Response() { }

            public Response(HttpWebResponse rsp)
            {
                Stream stream = rsp.GetResponseStream();
                m_Body = StreamToBytes(stream);

                m_Status = rsp.StatusCode;
                m_Cookies = rsp.Cookies;
                m_ContentType = rsp.ContentType;
                m_CharacterSet = rsp.CharacterSet;
                m_ContentEncoding = rsp.ContentEncoding;

                for (int i = 0; i < rsp.Headers.Count; ++i)
                {
                    List<string> lstValue = new List<string>();
                    string header = rsp.Headers.GetKey(i);
                    foreach (string value in rsp.Headers.GetValues(i))
                    {
                        lstValue.Add(value);
                    }
                    m_Headers[header] = lstValue;
                }
            }

            private byte[] StreamToBytes(Stream stream)
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read = 0;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }

                    return ms.ToArray();
                }
            }

            private string CorrectEncode(byte[] btWebHtml, Encoding encoding)
            {
                string sWebHtml = encoding.GetString(btWebHtml);
                string pattern = @"(?i)\bcharset=(?<charset>[-a-zA-Z_0-9]+)";
                Match mat = Regex.Match(sWebHtml, pattern);
                if (mat != null) 
                {
                    string sCharset = mat.Groups["charset"].Value;
                    if (string.IsNullOrEmpty(sCharset)) sCharset = "utf-8";
                    sWebHtml = Encoding.GetEncoding(sCharset).GetString(btWebHtml);
                }
                return sWebHtml;
            }
        }

        string _sLastError = "";
        public Options options = new Options();

        /// <summary>
        /// 獲取最後抓取網頁錯誤信息
        /// </summary>
        public string GetLastError()
        {
            return _sLastError;
        }

        #region Reset
        /// <summary>
        /// 重置保存的 Cookies
        /// </summary>
        public void ResetCookie()
        {
            options.Cookies = new CookieContainer();
        }

        /// <summary>
        /// 重置設置的 Header
        /// </summary>
        public void ResetHeader()
        {
            options.Headers = new Header();
        }

        /// <summary>
        /// 重置設置的 Options 到默認狀態
        /// </summary>
        public void Reset()
        {
            options = new Options();
        }
        #endregion

        #region Static Funtion
        /// <summary>
        /// 轉換字典為 Form String，可以按如下使用：
        /// Request.toFormString(new Dictionary<string, string>(){
        ///     { "key1", "value1" },
        ///     { "key2", "value2" }
        /// })
        /// value 將會自動轉 UrlEncode，編碼為 UTF-8
        /// </summary>
        /// <param name="data">要轉換的數據</param>
        /// <returns>轉換後的字符串</returns>
        public static string ToFormString(Dictionary<string, string> data)
        {
            return ToFormString(data, Encoding.UTF8);
        }

        /// <summary>
        /// 轉換字典為 Form String，可以按如下使用：
        /// Request.toFormString(new Dictionary<string, string>(){
        ///     { "key1", "value1" },
        ///     { "key2", "value2" }
        /// }, Encoding.UTF8)
        /// </summary>
        /// <param name="data">要轉換的數據</param>
        /// <param name="encoding">URL Encode 編碼</param>
        /// <returns>轉換後的字符串</returns>
        public static string ToFormString(Dictionary<string, string> data, Encoding encoding)
        {
            string sFormData = "";

            if (data == null) return sFormData;

            foreach (string sKey in data.Keys)
            {
                MatchCollection mc = Regex.Matches(data[sKey], "^\\w+$", RegexOptions.IgnoreCase);
                string sValue = mc != null && mc.Count > 0 ? 
                    data[sKey] : HttpUtility.UrlEncode(data[sKey], encoding);
                sFormData += "&" + sKey + "=" + sValue;
            }
            return sFormData.Trim('&');
        }

        /// <summary>
        /// 將 Object 序列化為字符串，用在 Post Json 上
        /// </summary>
        /// <param name="data">Json 數據</param>
        /// <returns>轉換後的字符串</returns>
        public static string ToJsonString(object data)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(data);
        }

        /// <summary>
        /// 解析 Cookies 對象
        /// </summary>
        /// <param name="Cookies">Cookies 對象</param>
        /// <param name="sDomain">指定 Cookie域名</param>
        /// <returns>返回字典Cookies 對象</returns>
        public static Dictionary<string, Cookie> CookiesParse(CookieContainer Cookies, string sDomain)
        {
            Dictionary<string, Cookie> dictCookies = new Dictionary<string, Cookie>();
            Uri uri = new Uri(sDomain);

            foreach (Cookie cookie in Cookies.GetCookies(uri))
            {
                dictCookies[cookie.Name] = cookie;
            }

            return dictCookies;
        }

        /// <summary>
        /// 解析 Cookies 對象
        /// </summary>
        /// <param name="Cookies">Cookies 對象</param>
        /// <returns>返回字典Cookies 對象</returns>
        public static Dictionary<string, Cookie> CookiesParse(CookieCollection Cookies)
        {
            Dictionary<string, Cookie> dictCookies = new Dictionary<string, Cookie>();

            foreach (Cookie cookie in Cookies)
            {
                dictCookies[cookie.Name] = cookie;
            }

            return dictCookies;
        }
        #endregion

        #region Get
        /// <summary>
        /// 通過Get方式下載網頁源碼
        /// </summary>
        /// <param name="sWebUrl">網頁地址</param>
        /// <returns>相應内容信息，null表示請求失敗, 失敗時調用LastError()查看錯誤信息</returns>
        public Response Get(string sWebUrl)
        {
            return Get(sWebUrl, options.Cookies);
        }

        /// <summary>
        /// 通過Get方式下載網頁源碼
        /// </summary>
        /// <param name="sWebUrl">網頁地址</param>
        /// <param name="cookieContainer">網頁Cookies, 獲取或設置抓取網頁中所需Cookies信息</param>
        /// <returns>相應内容信息，null表示請求失敗, 失敗時調用LastError()查看錯誤信息</returns>
        public Response Get(string sWebUrl, CookieContainer cookieContainer)
        {
            options.sBaseUrl = sWebUrl;
            options.sMethod = "GET";
            if (cookieContainer != null) options.Cookies = cookieContainer;
            return Send(options);
        }

        /// <summary>
        /// 通過Get方式下載網頁源碼
        /// </summary>
        /// <param name="sWebUrl">網頁地址</param>
        /// <param name="headers">自定義請求頭</param>
        /// <returns>相應内容信息，null表示請求失敗, 失敗時調用LastError()查看錯誤信息</returns>
        public Response Get(string sWebUrl, Dictionary<string, string> headers)
        {
            Header oldHeaders = options.Headers;
            options.Headers = new Header(oldHeaders);
            options.Headers.Extend(headers);
            options.sBaseUrl = sWebUrl;
            options.sMethod = "GET";

            Response rsp = Send(options);

            options.Headers = oldHeaders;

            return rsp;
        }

        /// <summary>
        /// 通過Get方式下載網頁源碼
        /// </summary>
        /// <param name="sWebUrl">網頁地址</param>
        /// <param name="headers">成功時返回網頁源碼</param>
        /// <param name="cookieContainer">網頁Cookies, 獲取或設置抓取網頁中所需Cookies信息</param>
        /// <returns>相應内容信息，null表示請求失敗, 失敗時調用LastError()查看錯誤信息</returns>
        public Response Get(string sWebUrl, Dictionary<string, string> headers, CookieContainer cookieContainer)
        {
            Header oldHeaders = options.Headers;
            options.Headers = new Header(oldHeaders);
            options.Headers.Extend(headers);
            options.sBaseUrl = sWebUrl;
            options.sMethod = "GET";
            options.Cookies = cookieContainer;

            Response rsp = Send(options);

            options.Headers = oldHeaders;

            return rsp;
        }
        #endregion

        #region Post

        /// <summary>
        /// 通過Post方式下載網頁源碼
        /// </summary>
        /// <param name="sWebUrl">網頁地址</param>
        /// <param name="sPostData">抓取網頁時需POST的數據</param>
        /// <returns>相應内容信息，null表示請求失敗, 失敗時調用LastError()查看錯誤信息</returns>
        public Response Post(string sWebUrl, string sPostData)
        {
            return Post(sWebUrl, sPostData, null, ref options.Cookies);
        }

        /// <summary>
        /// 通過Post方式下載網頁源碼
        /// </summary>
        /// <param name="sWebUrl">網頁地址</param>
        /// <param name="sPostData">抓取網頁時需POST的數據</param>
        /// <param name="headers">自定義請求頭</param>
        /// <param name="sWebCode">成功時返回網頁源碼</param>
        /// <returns>相應内容信息，null表示請求失敗, 失敗時調用LastError()查看錯誤信息</returns>
        public Response Post(string sWebUrl, string sPostData, Dictionary<string, string> headers)
        {
            return Post(sWebUrl, sPostData, headers, ref options.Cookies);
        }


        /// <summary>
        /// 通過Post方式下載網頁源碼
        /// </summary>
        /// <param name="sWebUrl">網頁地址</param>
        /// <param name="sPostData">抓取網頁時需POST的數據</param>
        /// <param name="cookieContainer">網頁Cookies, 獲取或設置抓取網頁中所需Cookies信息</param>
        /// <returns>相應内容信息，null表示請求失敗, 失敗時調用LastError()查看錯誤信息</returns>
        public Response Post(string sWebUrl, string sPostData, ref CookieContainer cookieContainer)
        {
            return Post(sWebUrl, sPostData, null, ref cookieContainer);
        }

        /// <summary>
        /// 通過Post方式下載網頁源碼
        /// </summary>
        /// <param name="sWebUrl">網頁地址</param>
        /// <param name="sPostData">抓取網頁時需POST的數據</param>
        /// <param name="headers">自定義請求頭</param>
        /// <param name="cookieContainer">網頁Cookies, 獲取或設置抓取網頁中所需Cookies信息</param>
        /// <returns>相應内容信息，null表示請求失敗, 失敗時調用LastError()查看錯誤信息</returns>
        public Response Post(string sWebUrl, string sPostData, Dictionary<string, string> headers, ref CookieContainer cookieContainer)
        {
            Header oldHeaders = options.Headers;
            options.Headers = new Header(oldHeaders);
            options.Headers.Extend(headers);
            options.sMethod = "POST";
            options.sBaseUrl = sWebUrl;
            options.sData = sPostData;
            if (cookieContainer != null) options.Cookies = cookieContainer;

            Response rsp = Send(options);

            options.Headers = oldHeaders;
            options.sData = "";
            options.sMethod = "GET";
            return rsp;
        }

        #endregion

        public Response Send(Options options)
        {
            HttpWebRequest  httpRequest = null;
            HttpWebResponse httpResponse = null;
            Response response = null;
            if (!string.IsNullOrEmpty(options.sBaseUrl))
            {
                try {
                    httpRequest = (HttpWebRequest)HttpWebRequest.Create(options.sBaseUrl);
                    httpRequest.CookieContainer = options.Cookies;
                    httpRequest.ContentType = options.Headers.sContentType;
                    httpRequest.ServicePoint.ConnectionLimit = 300;
                    httpRequest.Referer = string.IsNullOrEmpty(options.Headers.sReferer) ? options.sBaseUrl : options.Headers.sReferer;
                    httpRequest.Accept = options.Headers.sAccept;
                    httpRequest.UserAgent = options.Headers.sUserAgent;
                    httpRequest.Method = options.sMethod;
                    httpRequest.Timeout = options.nTimeout;

                    if (options.Proxy != null)
                        httpRequest.Proxy = options.Proxy;

                    if (options.Headers.defineHeaders != null)
                    {
                        foreach( string sKey in options.Headers.defineHeaders.Keys ) 
                        {
                            httpRequest.Headers.Add(sKey, options.Headers.defineHeaders[sKey]);
                        }
                    }

                    if (options.AuthOption != null)
                    {
                        string sAuth = System.Convert.ToBase64String(
                            System.Text.Encoding.UTF8.GetBytes(
                                options.AuthOption.sUsername + ":" + options.AuthOption.sPassword
                            ));
                        httpRequest.Headers.Add("Authorization", options.AuthOption.sType + " " + sAuth);
                    }

                    if (!string.IsNullOrEmpty(options.sData)) // Post
                    {
                        if (options.sMethod == "GET") options.sMethod = "POST";
                        byte[] btDataArray = Encoding.UTF8.GetBytes(options.sData);
                        httpRequest.ContentLength = btDataArray.Length;

                        Stream dataStream = null;
                        dataStream = httpRequest.GetRequestStream();
                        dataStream.Write(btDataArray, 0, btDataArray.Length);
                        dataStream.Close();

                        httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    }
                    else
                    {
                        httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    }

                    response = new Response(
                        httpResponse
                    );
                    httpResponse.Close();
                }
                catch (Exception ex)
                {
                    _sLastError = "抓取失敗: " + ex.Message;
                    Console.WriteLine(ex.Message);
                }
            }
            else 
            {
                _sLastError = "Base Url 是空的。";
            }
            return response;
        }
    }
}
