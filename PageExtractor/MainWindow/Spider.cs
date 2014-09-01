using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
//using System.Windows.Threading;
using System.Threading;
using System.Threading;
using System.Collections;
using MainWindow;
using HTMLAnalysis;
using TestDatabase;
using System.Data;

namespace PageExtractor
{
    public class Spider
    {
        #region private type
        private class RequestState
        {
            private const int BUFFER_SIZE = 131072;
            private byte[] _data = new byte[BUFFER_SIZE];
            private StringBuilder _sb = new StringBuilder();
            
            public HttpWebRequest Req { get; private set; }
            public string Url { get; private set; }
            public int Depth { get; private set; }
            public int Index { get; private set; }
            public Stream ResStream { get; set; }
            public StringBuilder Html
            {
                get
                {
                    return _sb;
                }
            }
            
            public byte[] Data
            {
                get
                {
                    return _data;
                }
            }

            public int BufferSize
            {
                get
                {
                    return BUFFER_SIZE;
                }
            }

            public RequestState(HttpWebRequest req, string url, int depth, int index)
            {
                Req = req;
                Url = url;
                Depth = depth;
                Index = index;
            }
        }

        private class WorkingUnitCollection
        {
            private int _count;
            //private AutoResetEvent[] _works;
            private bool[] _busy;

            public WorkingUnitCollection(int count)
            {
                _count = count;
                //_works = new AutoResetEvent[count];
                _busy = new bool[count];

                for (int i = 0; i < count; i++)
                {
                    //_works[i] = new AutoResetEvent(true);
                    _busy[i] = true;
                }
            }

            public void StartWorking(int index)
            {
                if (!_busy[index])
                {
                    _busy[index] = true;
                    //_works[index].Reset();
                }
            }

            public void FinishWorking(int index)
            {
                if (_busy[index])
                {
                    _busy[index] = false;
                    //_works[index].Set();
                }
            }

            public bool IsFinished()
            {
                bool notEnd = false;
                foreach (var b in _busy)
                {
                    notEnd |= b;
                }
                return !notEnd;
            }

            public void WaitAllFinished()
            {
                while (true)
                {
                    if (IsFinished())
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
                //WaitHandle.WaitAll(_works);
            }

            public void AbortAllWork()
            {
                for (int i = 0; i < _count; i++)
                {
                    _busy[i] = false;
                }
            }
        }
        #endregion

        #region private fields
        private static Encoding GB18030 = Encoding.GetEncoding("GB18030");   // GB18030兼容GBK和GB2312
        private static Encoding UTF8 = Encoding.UTF8;
        //private string _userAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0)";
        private string _userAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0; BOIE9;ZHCN)";
        private string _accept = "text/html";
        private string _method = "GET";
        private Encoding _encoding = GB18030;
        private Encodings _enc = Encodings.GB;
        private int _maxTime = 2 * 60 * 1000;
        //private int _maxTime = 10 * 1000;

        private int _index;
        private string _path = null;
        private int _maxDepth = 1;
        private int _maxExternalDepth = 0;
        private string _rootUrl = null;
        private string _baseUrl = null;
        private Dictionary<string, int> _urlsLoaded = new Dictionary<string, int>();
        private Dictionary<string, int> _urlsUnload = new Dictionary<string, int>();

        //private Hashtable Already_Searched_URL = new Hashtable();

        private bool _stop = true;
        private Timer _checkTimer = null;
        private readonly object _locker = new object();
        private bool[] _reqsBusy = null;
        private int _reqCount = 1;
        private WorkingUnitCollection _workingSignals;
        #endregion

        System.Windows.Forms.RichTextBox _richTextBox;
        MainWinfow _mainWinform;
        private Int64 global_database_index = 0;

        #region constructors
        /// <summary>
        /// 创建一个Spider实例
        /// </summary>

        public Spider()
        {
        }


        public Spider(MainWinfow _MW)
        {
            _mainWinform = _MW;
            _richTextBox = _MW.richTextBox_prompt;
        }

       
        #endregion

        #region properties
        /// <summary>
        /// 下载根Url
        /// </summary>
        public string RootUrl
        {
            get
            {
                return _rootUrl;
            }
            set
            {
                if (!value.Contains("http://"))
                {
                    _rootUrl = "http://" + value;
                }
                else
                {
                    _rootUrl = value;
                }
                _baseUrl = _rootUrl.Replace("www.", "");
                _baseUrl = _baseUrl.Replace("http://", "");
                _baseUrl = _baseUrl.TrimEnd('/');

                int first_slash_index = -1;
                first_slash_index = _baseUrl.IndexOf('/');
                if (first_slash_index != -1)
                {
                    _baseUrl = _baseUrl.Substring(0, first_slash_index);
                }
            }
        }

        /// <summary>
        /// 网页编码类型
        /// </summary>
        public Encodings PageEncoding
        {
            get
            {
                return _enc;
            }
            set
            {
                _enc = value;
                switch (value)
                {
                    case Encodings.GB:
                        _encoding = GB18030;
                        break;
                    case Encodings.UTF8:
                        _encoding = UTF8;
                        break;
                }
            }
        }

        /// <summary>
        /// 最大下载深度
        /// </summary>
        public int MaxDepth
        {
            get
            {
                return _maxDepth;
            }
            set
            {
                _maxDepth = Math.Max(value, 1);
            }
        }

        /// <summary>
        /// 下载最大连接数
        /// </summary>
        public int MaxConnection
        {
            get
            {
                return _reqCount;
            }
            set
            {
                _reqCount = value;
            }
        }
        #endregion

        #region public type
        public delegate void ContentsSavedHandler(string path, string url);

        public delegate void DownloadFinishHandler(int count);

        public enum Encodings
        {
            UTF8,
            GB
        }

        public string [] Filter_out
        {
            get;
            set;
        }
        public string [] Filter_in
        {
            get;
            set;
        }
        
        #endregion

        #region events
        /// <summary>
        /// 正文内容被保存到本地后触发
        /// </summary>
        public event ContentsSavedHandler ContentsSaved = null;

        /// <summary>
        /// 全部链接下载分析完毕后触发
        /// </summary>
        public event DownloadFinishHandler DownloadFinish = null;
        #endregion

        #region public methods
        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="path">保存本地文件的目录</param>
        public void Download(string path)
        {
            if (string.IsNullOrEmpty(RootUrl))
            {
                return;
            }
            _path = path;
            Init();
            StartDownload();
        }

        public void Download_test(string path)
        {
            if (string.IsNullOrEmpty(RootUrl))
            {
                return;
            }
            _path = path;
            Init_test();
            StartDownload();
        }


        /// <summary>
        /// 终止下载
        /// </summary>
        public void Abort()
        {
            _stop = true;
            if (_workingSignals != null)
            {
                _workingSignals.AbortAllWork();
            }
        }
        #endregion

        #region private methods
        private void StartDownload()
        {
            _checkTimer = new Timer(new TimerCallback(CheckFinish), null, 0, 300);
            DispatchWork();
        }

        private void CheckFinish(object param)
        {
            if (_workingSignals.IsFinished())
            {
                _checkTimer.Dispose();
                _checkTimer = null;
                if (DownloadFinish != null)
                {
                    DownloadFinish(_index);
                }
            }
        }

        private void DispatchWork()
        {
            if (_stop)
            {
                return;
            }
            for (int i = 0; i < _reqCount; i++)
            {
                if (!_reqsBusy[i])
                {
                    RequestResource(i);
                }
            }
        }

        private void Init()
        {
            //Already_Searched_URL = new Hashtable();
            _urlsLoaded.Clear();
            _urlsUnload.Clear();
            AddUrls(new string[1] { RootUrl }, 0);
            _index = 0;
            _reqsBusy = new bool[_reqCount];
            _workingSignals = new WorkingUnitCollection(_reqCount);
            _stop = false;
        }

        private void Init_test()
        {
            //Already_Searched_URL = new Hashtable();
            _urlsLoaded.Clear();
            _urlsUnload.Clear();

            AddUrls(new string[1] { RootUrl }, 0);

            
            string _file_path = Environment.CurrentDirectory + "//url_list.ini";

            try
            {
                using (StreamReader sr = new StreamReader(_file_path))
                {
                    string _line;

                    while ((_line = sr.ReadLine()) != null)
                    {
                        if (_line.Length >= 1 && _line[0] != ';')
                        {
                            if (RootUrl == null)
                                RootUrl = _line;
                            else
                            {
                                AddUrls(new string[1] { _line }, 0);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                
            }

            

            _index = 0;
            _reqsBusy = new bool[_reqCount];
            _workingSignals = new WorkingUnitCollection(_reqCount);
            _stop = false;
        }



        private void RequestResource(int index)
        {
            int depth;
            string url = "";
            try
            {
                lock (_locker)
                {
                    if (_urlsUnload.Count <= 0)
                    {
                        _workingSignals.FinishWorking(index);
                        return;
                    }
                    _reqsBusy[index] = true;
                    _workingSignals.StartWorking(index);
                    depth = _urlsUnload.FirstOrDefault().Value;
                    url = _urlsUnload.FirstOrDefault().Key;

                    if (!_urlsLoaded.ContainsKey(url))
                        _urlsLoaded.Add(url, depth);
                    _urlsUnload.Remove(url);
                }

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = _method; //请求方法
                req.Accept = _accept; //接受的内容
                req.UserAgent = _userAgent; //用户代理
                RequestState rs = new RequestState(req, url, depth, index);

                //log 
                Log_add_searched_html(url,depth);

                var result = req.BeginGetResponse(new AsyncCallback(ReceivedResource), rs);
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle,
                        TimeoutCallback, rs, _maxTime, true);
            }
            catch (WebException we)
            {
                //MessageBox.Show("RequestResource " + we.Message + url + we.Status);
                Log_error("RequestResource " + we.Message + url + we.Status + "[RequestResource]");

                _reqsBusy[index] = false;
                DispatchWork();
            }
            catch (Exception e)
            {
                Log_error("RequestResource " + e.Message + url + e.ToString() + "[RequestResource]");

                _reqsBusy[index] = false;
                DispatchWork();
            }
        }

        private void ReceivedResource(IAsyncResult ar)
        {
            RequestState rs = (RequestState)ar.AsyncState;
            HttpWebRequest req = rs.Req;
            string url = rs.Url;

            try
            {
                HttpWebResponse res = (HttpWebResponse)req.EndGetResponse(ar);
                if (_stop)
                {
                    res.Close();
                    req.Abort();
                    return;
                }
                if (res != null && res.StatusCode == HttpStatusCode.OK)
                {
                    Stream resStream = res.GetResponseStream();
                    rs.ResStream = resStream;
                    var result = resStream.BeginRead(rs.Data, 0, rs.BufferSize,
                        new AsyncCallback(ReceivedData), rs);
                }
                else
                {
                    res.Close();
                    rs.Req.Abort();
                    _reqsBusy[rs.Index] = false;
                    DispatchWork();
                }
            }
            catch (WebException we)
            {
                //MessageBox.Show("ReceivedResource " + we.Message + url + we.Status);
                Log_error("ReceivedResource " + we.Message + url + "\t" + we.Status + "[ReceivedResource]");
                _reqsBusy[rs.Index] = false;
                DispatchWork();
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                Log_error(e.Message + "[ReceivedResource]");
                _reqsBusy[rs.Index] = false;
                DispatchWork();
            }
        }

        private void ReceivedData(IAsyncResult ar)
        {
            RequestState rs = (RequestState)ar.AsyncState;
            HttpWebRequest req = rs.Req;
            Stream resStream = rs.ResStream;
            string url = rs.Url;
            int depth = rs.Depth;
            string html = null;
            int index = rs.Index;
            int read = 0;

            try
            {
                read = resStream.EndRead(ar);
                if (_stop)
                {
                    rs.ResStream.Close();
                    req.Abort();
                    return;
                }
                if (read > 0)
                {
                    MemoryStream ms = new MemoryStream(rs.Data, 0, read);
                    StreamReader reader = new StreamReader(ms, _encoding);
                    string str = reader.ReadToEnd();
                    rs.Html.Append(str);
                    var result = resStream.BeginRead(rs.Data, 0, rs.BufferSize,
                        new AsyncCallback(ReceivedData), rs);
                    return;
                }
                html = rs.Html.ToString();
                //SaveContents(html, url);

                //Write a file print info into it

                test_write_info_to_file(html, url);
                




                string[] links = GetLinks(html);
                 if(links != null)
                AddUrls(links, depth + 1);

                _reqsBusy[index] = false;
                DispatchWork();
            }
            catch (WebException we)
            {
                //MessageBox.Show("ReceivedData Web " + we.Message + url + we.Status);
                Log_error(we.ToString() + "[ReceivedData]");
                _reqsBusy[rs.Index] = false;
                DispatchWork();
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.GetType().ToString() + e.Message);
                Log_error(e.GetType().ToString() + e.Message +"[ReceivedData]");
                _reqsBusy[rs.Index] = false;
                DispatchWork();
            }
         }

        private void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                RequestState rs = state as RequestState;
                if (rs != null)
                {
                    rs.Req.Abort();
                }
                _reqsBusy[rs.Index] = false;
                DispatchWork();
            }
        }



        private string[] GetLinks(string html)
        {
            //const string pattern = @"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
            //Yao
            //const string pattern = @"href=[\s\S]*?[;\\]+?";
            //const string pattern = "href *?= *?[\\w\"\\.:/;=\\+]+";
            const string pattern = "href *?= *?[\\\"][\\s\\S]*?[;\\\"]";

            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection m = r.Matches(html);
            string[] links = new string[m.Count];

            for (int i = 0; i < m.Count; i++)
            {
                //links[i] = m[i].ToString();
                //Yao
                string temp_s = m[i].ToString();
                int temp_length = temp_s.Length;
                //Yao:Safe Guard
                if (temp_length < 7)
                    continue;

                temp_s = temp_s.Substring(6, temp_length - 7);

                if (temp_s == null || temp_s.Length < 2)//url too short
                {
                    continue;
                }

                if (temp_s[0] == '/' && temp_s[1] == '/')
                {
                    continue;
                }

                if (temp_s[0] == '/')
                {
                    string _original_root_url = "";

                    if (RootUrl.Length >= 7 && RootUrl.ToLower().Substring(0,7).Equals("http://")) //get real root. for example. http://www.officedepot.com/product/a/12345.html real root is www.officedepot.com
                    {
                        if (RootUrl.Length >= 8)
                        {
                            string _str1 = RootUrl.Substring(0, 7); // "http://"
                            string _str2 = RootUrl.Substring(7, RootUrl.Length - 7);

                            if (_str2.Contains('/'))
                            {
                                int _RootUrl_index = _str2.IndexOf('/');

                                _original_root_url = _str1 + _str2.Substring(0, _RootUrl_index);
                            }
                            else
                                _original_root_url = _str1 + _str2;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (RootUrl.Length >= 8 && RootUrl.ToLower().Substring(0, 7).Equals("https://"))
                    {
                        if (RootUrl.Length >= 9)
                        {
                            string _str1 = RootUrl.Substring(0, 8); // "https://"
                            string _str2 = RootUrl.Substring(8, RootUrl.Length - 8);

                            if (_str2.Contains('/'))
                            {
                                int _RootUrl_index = _str2.IndexOf('/');

                                _original_root_url = _str1 + _str2.Substring(0, _RootUrl_index);
                            }
                            else
                                _original_root_url = _str1 + _str2;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        _original_root_url = RootUrl.Split('/')[0];
                    }

                    if (_original_root_url[_original_root_url.Length - 1] == '/') //delete last '/' in root url
                        _original_root_url = _original_root_url.Substring(0, _original_root_url.Length - 1);

                    temp_s = _original_root_url + temp_s; 


                    /*if (RootUrl[RootUrl.Length - 1] == '/') //delete last '/' in root url
                        RootUrl = RootUrl.Substring(0, RootUrl.Length - 1);

                    temp_s = RootUrl + temp_s; */
                }

                links[i] = temp_s;
            }


            
            //string target_str0 = RootUrl .Split('.')[1];

            //restrict links to this website ONLY
            for (int i = 0; i < links.Length; i++)
            {
                if (links[i] == null) // safe guard
                    continue;

                //filter out
                for (int k = 0; k < _mainWinform.web_attribute.Filter_out.Length; k++)
                {
                    if (links[i] != null)
                    {
                        if (links[i].Contains(_mainWinform.web_attribute.Filter_out[k]))
                        {
                            links[i] = null;
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                
                //filter in
                for (int k = 0; k < _mainWinform.web_attribute.Filter_in.Length; k++)
                {
                    if (links[i] != null)
                    {
                        if (!links[i].Contains(_mainWinform.web_attribute.Filter_in[k]))
                        {
                            links[i] = null;
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                #region old code
                /*
                string str0 = null;

                if (links[i].Split('.').Length >= 2)
                    str0 = links[i].Split('.')[1];
                else
                {
                    links[i] = null;
                    continue;
                }

                
                if (!links[i].Contains("." + target_str0 + ".")) //URL contains ".staples."
                {
                    links[i] = null;
                    continue;
                }

                if (!str0.Equals(target_str0))//URL has to follow http(s)://***.staples.***
                {
                    links[i] = null;
                    continue;
                }

                if (links[i].Contains("customerservice") || links[i].Contains("storelocator") || links[i].Contains("search"))//link can't contain "customerservice" "storelocator" "search"
                {
                    links[i] = null;
                    continue;
                }
                */
                #endregion
            }


            //filter link
            string [] filter_links = new string[links.Length];
            int j = 0;

            for (int i = 0; i < links.Length; i++)
            {
                if (links[i] == null)
                {
                    continue;
                }
                else
                {
                    filter_links[j] = links[i];
                    j++;
                }
            }

            if (j == 0)
                return null;

            string[] final_links = new string[j];

            for (int i = 0; i < j; i++)
                final_links[i] = filter_links[i];

            return final_links;
            //return links;
        }

        private bool UrlExists(string url)
        {
            bool result = _urlsUnload.ContainsKey(url);
            result |= _urlsLoaded.ContainsKey(url);
            return result;
        }

        private bool UrlAvailable(string url)
        {
            if (UrlExists(url))
            {
                return false;
            }
            if (url.Contains(".jpg") || url.Contains(".gif")
                || url.Contains(".png") || url.Contains(".css")
                || url.Contains(".js") || url.Contains(".do"))
            {
                return false;
            }
            return true;
        }

        public void AddUrls(string[] urls, int depth)
        {
            if (depth >= _maxDepth)
            {
                return;
            }
            foreach (string url in urls)
            {
                //if (url.Contains("cleaning-and-breakroom/N=5+549384+588406"))
                //{
                //    int asdf = 0;
                //    asdf++;
                //}


                string cleanUrl = url.Trim();
                int end = cleanUrl.IndexOf(' ');
                if (end > 0)
                {
                    cleanUrl = cleanUrl.Substring(0, end);
                }
                //cleanUrl = cleanUrl.TrimEnd('/');
                if (UrlAvailable(cleanUrl))
                {
                    //if (Already_Searched_URL.ContainsKey(cleanUrl))//This URL has been searched before
                    //    continue;

                    if (cleanUrl.Contains(_baseUrl))
                    {
                        _urlsUnload.Add(cleanUrl, depth);
                        //Already_Searched_URL.Add(cleanUrl, depth);
                    }
                    else
                    {
                        // 外链
                    }
                }
            }
        }

        private void SaveContents(string html, string url)
        {
            if (string.IsNullOrEmpty(html))
            {
                return;
            }
            string path = "";
            lock (_locker)
            {
                path = string.Format("{0}\\{1}.html", _path, _index++);
            }

            try
            {
                using (StreamWriter fs = new StreamWriter(path))
                {
                    fs.Write(html);
                }
            }
            catch (IOException ioe)
            {
                //MessageBox.Show("SaveContents IO" + ioe.Message + " path=" + path);
            }
            
            if (ContentsSaved != null)
            {
                ContentsSaved(path, url);
            }
        }

        private bool first_time_error_log = true;

        private void Log_error(string contents)
        {
            string path = Environment.CurrentDirectory + "\\error_log.txt";

            try
            {
                using (StreamWriter w = File.AppendText(path))
                {
                    if (first_time_error_log)
                    {
                        first_time_error_log = false;
                        w.WriteLine("-------------------------------------------");
                    }

                    DateTime dt = DateTime.Now;
                    w.WriteLine(dt.ToString() + "\t" + contents + "\n");

                    if (_richTextBox != null)
                        write_to_prompt(contents);
                }
            }
            catch (Exception)
            { }

        }

        private int counter_searched_url = 0;
        private void Log_add_searched_html(string url,int _depth)
        {
            string path = Environment.CurrentDirectory + "\\SearchedURL.txt";

            try
            {
                using (StreamWriter w = File.AppendText(path))
                {
                    if (first_time_error_log)
                    {
                        first_time_error_log = false;
                        w.WriteLine("-------------------------------------------");
                    }

                    DateTime dt = DateTime.Now;
                    w.WriteLine(counter_searched_url.ToString() + ". " + dt.ToString() + "\t" + url + "\tDepth:"+_depth.ToString()+"\n");

                    counter_searched_url++;
                }
            }
            catch (Exception)
            { } 
        }

        delegate void write_richTextBox_prompt_callback(string _contents_string);
        void write_to_prompt(string _contents)
        {
            if (_richTextBox.InvokeRequired)
            {
                write_richTextBox_prompt_callback delegate_write_richTextBox_prompt_callback = new write_richTextBox_prompt_callback(write_to_prompt);
                _richTextBox.Invoke(delegate_write_richTextBox_prompt_callback, new object[] { _contents });
            }
            else
            {
                DateTime dt = DateTime.Now;
                _richTextBox.AppendText(dt.ToString() + "\t" + _contents + "\n");
            }
        }

        private string Trim_HTML_Text(string _rawString)
        {
            string _temp_raw_string = _rawString.Replace("&nbsp","").Trim();

            return _temp_raw_string;
        }

        private HtmlAgilityPack.HtmlNode obtain__single_node_XPath(HTMLAnalysisBox HTMLBox, string[] XPath_StringArray)
        {
            int simple_counter;

            HtmlAgilityPack.HtmlNode product_node = HTMLBox.Select_SingleNode(XPath_StringArray[0]);

            if (product_node == null)
            {
                simple_counter = 1;

                while (XPath_StringArray.Length > simple_counter && XPath_StringArray[simple_counter] != null)
                {
                    product_node = HTMLBox.Select_SingleNode(XPath_StringArray[simple_counter]);
                    simple_counter++;

                    if (product_node != null)
                        break;
                }
            }

            return product_node;
        }


        void test_write_info_to_file(string html, string url)
        {
            HTMLAnalysisBox HTMLBox = new HTMLAnalysisBox();
            HTMLBox.Load_PlainText(html);

            string product_name = null, product_price = null, product_image_URL = null, product_description = null, product_brandname = null, product_modelname = null, product_manufacture_name = null, product_manufacture_number=null, product_upc_number = null;

            #region Product Name

            HtmlAgilityPack.HtmlNode product_name_node = obtain__single_node_XPath(HTMLBox, _mainWinform.web_attribute.ProductName_XPath);
            if (product_name_node != null)
            {
                product_name = product_name_node.InnerText;
                product_name = Trim_HTML_Text(product_name);
            }
            /*HtmlAgilityPack.HtmlNode product_name_node = HTMLBox.Select_SingleNode(_mainWinform.web_attribute.ProductName_XPath[0]);

            if (product_name_node == null)
            {
                simple_counter = 1;

                while (_mainWinform.web_attribute.ProductName_XPath.Length > simple_counter && _mainWinform.web_attribute.ProductName_XPath[simple_counter] != null)
                {
                    product_name_node = HTMLBox.Select_SingleNode(_mainWinform.web_attribute.ProductName_XPath[simple_counter]);
                    simple_counter++;

                    if (product_name_node != null)
                        break;
                }
            }

            if (product_name_node != null)
                product_name = Trim_HTML_Text(product_name_node.InnerText.Trim());*/

            #endregion

            #region Product Price

            HtmlAgilityPack.HtmlNode product_price_node = obtain__single_node_XPath(HTMLBox, _mainWinform.web_attribute.ProductPrice_XPath);

            if (product_price_node != null)
            {
                product_price = product_price_node.InnerText;
                product_price = Trim_HTML_Text(product_price);
            }
            /*HtmlAgilityPack.HtmlNode product_price_node = HTMLBox.Select_SingleNode(_mainWinform.web_attribute.ProductPrice_XPath[0]);

            if (product_price_node == null)
            {
                simple_counter = 1;

                while (_mainWinform.web_attribute.ProductPrice_XPath.Length > simple_counter && _mainWinform.web_attribute.ProductPrice_XPath[simple_counter] != null)
                {
                    product_price_node = HTMLBox.Select_SingleNode(_mainWinform.web_attribute.ProductPrice_XPath[simple_counter]);
                    simple_counter++;

                    if (product_price_node != null)
                        break;
                }
            }

            if (product_price_node != null)
                product_price = Trim_HTML_Text(product_price_node.InnerText.Trim());*/
            #endregion

            //Verify both product name and price are got
            if (product_name == null || product_price == null)
                return;
            
            #region Product Image URL

            HtmlAgilityPack.HtmlNode product_image_URL_node = obtain__single_node_XPath(HTMLBox, _mainWinform.web_attribute.ProuctImageURL_XPath);

            /*HtmlAgilityPack.HtmlNode product_image_URL_node = HTMLBox.Select_SingleNode(_mainWinform.web_attribute.ProuctImageURL_XPath[0]);

            if (product_image_URL_node == null)
            {
                simple_counter = 1;

                while (_mainWinform.web_attribute.ProuctImageURL_XPath.Length >simple_counter && _mainWinform.web_attribute.ProuctImageURL_XPath[simple_counter] != null)
                {
                    product_image_URL_node = HTMLBox.Select_SingleNode(_mainWinform.web_attribute.ProuctImageURL_XPath[simple_counter]);
                    simple_counter++;

                    if (product_image_URL_node != null)
                        break;
                } 
            }*/

            if (product_image_URL_node != null)
            {
                HtmlAgilityPack.HtmlAttribute html_attribute_product_image_URL = product_image_URL_node.Attributes["src"];

                if (html_attribute_product_image_URL != null) 
                {
                    product_image_URL = html_attribute_product_image_URL.Value.ToString();
                }
            }
            #endregion

            #region product description
            HtmlAgilityPack.HtmlNodeCollection product_desctiption_node_collection = HTMLBox.Select_AllNode(_mainWinform.web_attribute.ProductDescription_XPath[0]);

            if (product_desctiption_node_collection != null)
            {
                foreach (HtmlAgilityPack.HtmlNode product_desctiption_node in product_desctiption_node_collection)
                {
                    if (product_description==null)
                        product_description = "";

                    //HTML uses '&nbsp' representing white space
                    if (product_desctiption_node != null)
                        product_description = product_description + Trim_HTML_Text(product_desctiption_node.InnerText.Trim()) + "\n";
                }
            }

            #endregion

            #region Brand Name

            HtmlAgilityPack.HtmlNode html_node_BrandName = obtain__single_node_XPath(HTMLBox, _mainWinform.web_attribute.BrandName_XPath);

            if (html_node_BrandName != null)
            {
                product_brandname = html_node_BrandName.InnerText;
                product_brandname = Trim_HTML_Text(product_brandname);
            }

            #endregion

            #region Manufacture Number

            HtmlAgilityPack.HtmlNode html_node_ManufactureNumber = obtain__single_node_XPath(HTMLBox, _mainWinform.web_attribute.ManufactureNumber_XPath);

            if (html_node_ManufactureNumber != null)
            {

                product_manufacture_number = html_node_ManufactureNumber.InnerText;
                product_manufacture_number = Trim_HTML_Text(product_manufacture_number);
            }

            #endregion

            #region Manufacture Name

            HtmlAgilityPack.HtmlNode html_node_ManufactureName = obtain__single_node_XPath(HTMLBox, _mainWinform.web_attribute.ManufactureName_XPath);

            if (html_node_ManufactureName != null)
            {
                product_manufacture_name = html_node_ManufactureName.InnerText;
                product_manufacture_name = Trim_HTML_Text(product_manufacture_name);
            }

            #endregion

            #region UPC Number

            HtmlAgilityPack.HtmlNode html_node_UPCNumber = obtain__single_node_XPath(HTMLBox, _mainWinform.web_attribute.UPCNumber_XPath);

            if (html_node_UPCNumber != null)
            {
                product_upc_number = html_node_UPCNumber.InnerText;
                product_upc_number = Trim_HTML_Text(product_upc_number);
            }

            #endregion

            #region Model Name

            HtmlAgilityPack.HtmlNode html_node_ModelName = obtain__single_node_XPath(HTMLBox, _mainWinform.web_attribute.ModelName_XPath);

            if (html_node_ModelName != null)
            {
                product_modelname = html_node_ModelName.InnerText;
                product_modelname = Trim_HTML_Text(product_modelname);
            }
            #endregion



            #region //write result to file
           /*
            string path = Environment.CurrentDirectory + "\\test_write_info_to_file.txt";

            try
            {
                using (StreamWriter w = File.AppendText(path))
                {
                    w.WriteLine(url);
                    //Name & Price
                    w.Write("Product Name:\n" + product_name + "\n\nPrice:\n" + product_price); 
                    //Image URL
                    if (product_image_URL != null)
                        w.Write("\n\nImage URL:\n" + product_image_URL);
                    else
                        w.Write("\n\nImage URL:\n" + "null");
                    //description
                    if (product_description != null)
                        w.Write("\n\nDescription:\n" + product_description);
                    else
                        w.Write("\n\nDescription:\n" );
                    //Brand
                    if (product_brandname != null)
                        w.Write("\n\nBrand Name:\n" + product_brandname);
                    else
                        w.Write("\n\nBrand Name:\n");
                    //Manufacture Number
                    if (product_manufacture_number != null)
                        w.Write("\n\nManufacture #:\n" + product_manufacture_number);
                    else
                        w.Write("\n\nManufacture #:\n");
                    //Manufacture Name
                    if (product_manufacture_name != null)
                        w.Write("\n\nManufacture Name:\n" + product_manufacture_name);
                    else
                        w.Write("\n\nManufacture Name:");
                    //Model Name
                    if(product_modelname != null)
                        w.Write("\n\nModel:\n" + product_modelname);
                    else
                        w.Write("\n\nModel:\n");
                    //UPC Number
                    if (product_upc_number != null)
                        w.Write("\n\nUPC #:\n" + product_upc_number);
                    else
                        w.Write("\n\nUPC #:\n");

                    w.WriteLine();
                    w.WriteLine("---------------------------------------------------");
                }
            }
            catch (Exception)
            { }
            */
            #endregion

            //write resule to databse
            //SQLDatabase my_sql_database = new SQLDatabase("98.239.198.3,1433\\SQLEXPRESS", "TestDatabase", "sa", "ly23909475");
            SQLDatabase my_sql_database = new SQLDatabase("127.0.0.1,1433\\SQLEXPRESS", "TestDatabase", "sa", "ly23909475");

            //get table size
            string sql_query_table_size = "select count(*) from TestDatabase.dbo.OfficeDepot";

            DataSet dataset_table_size = my_sql_database.Execute_select_with_DataSet(sql_query_table_size);

            string table_size = dataset_table_size.Tables[0].Rows[0][0].ToString();


            string sql_query = "INSERT INTO OfficeDepot ([index],URL,ProductName,CurrentPrice,ImageURL,ProductDescription,BrandName,ManufacturerNumber,ManufacturerName,UPCNumber,Model) VALUES (" + table_size.ToString() + ",'" + url + "','" + product_name + "'," + product_price + ",'" + product_image_URL + "','" + product_description + "','" + product_brandname + "','" + product_manufacture_number + "','" + product_manufacture_name + "','" + product_upc_number + "','" + product_modelname + "')";

            int number_affected = my_sql_database.ExecuteNonQuery(sql_query);

            Console.WriteLine(number_affected.ToString() + " row(s) affected");

            //Console.Read();

        }



        
        #endregion
    }
}
