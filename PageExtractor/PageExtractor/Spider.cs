using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Collections;

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
        private int _maxDepth = 4;
        private int _maxExternalDepth = 0;
        private string _rootUrl = null;
        private string _baseUrl = null;
        private Dictionary<string, int> _urlsLoaded = new Dictionary<string, int>();
        private Dictionary<string, int> _urlsUnload = new Dictionary<string, int>();

        private Hashtable Already_Searched_URL = new Hashtable();

        private bool _stop = true;
        private Timer _checkTimer = null;
        private readonly object _locker = new object();
        private bool[] _reqsBusy = null;
        private int _reqCount = 4;
        private WorkingUnitCollection _workingSignals;
        #endregion

        System.Windows.Forms.RichTextBox _richTextBox;


        #region constructors
        /// <summary>
        /// 创建一个Spider实例
        /// </summary>

        public Spider()
        {
        }


        public Spider(System.Windows.Forms.RichTextBox _temp_richTextBox)
        {
            _richTextBox = _temp_richTextBox;
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
            _urlsLoaded.Clear();
            _urlsUnload.Clear();
            AddUrls(new string[1] { RootUrl }, 0);
            _index = 0;
            _reqsBusy = new bool[_reqCount];
            _workingSignals = new WorkingUnitCollection(_reqCount);
            _stop = false;
        }

        int test_counter = 0;

        private void RequestResource(int index)
        {
            test_counter++;
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
                    depth = _urlsUnload.First().Value;
                    url = _urlsUnload.First().Key;

                    _urlsLoaded.Add(url, depth);
                    _urlsUnload.Remove(url);
                }

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = _method; //请求方法
                req.Accept = _accept; //接受的内容
                req.UserAgent = _userAgent; //用户代理
                RequestState rs = new RequestState(req, url, depth, index);

                //log 
                Log_add_searched_html(url);

                var result = req.BeginGetResponse(new AsyncCallback(ReceivedResource), rs);
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle,
                        TimeoutCallback, rs, _maxTime, true);
            }
            catch (WebException we)
            {
                //MessageBox.Show("RequestResource " + we.Message + url + we.Status);
                Log_error("RequestResource " + we.Message + url + we.Status + "[RequestResource]");
            }
            catch (Exception e)
            {
                Log_error("RequestResource " + e.Message + url + e.ToString() + "[RequestResource]");
            }
        }

        private void ReceivedResource(IAsyncResult ar)
        {
            RequestState rs = (RequestState)ar.AsyncState;
            HttpWebRequest req = rs.Req;
            string url = rs.Url;


            //if (url.Contains("cleaning-and-breakroom/N=5+549384+588406"))
            //{
            //    int asdf = 0;
           //     asdf++;
            //}


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

            if (url.Contains("a/browse/cleaning-and-breakroom/N=5+549384+588406"))
            {
                int test_i = 0;
                test_i++;
            }
            else
            {
                int test_i = 0;
                test_i++;
            }

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
                SaveContents(html, url);
                string[] links = GetLinks(html);
                 if(links != null)
                AddUrls(links, depth + 1);

                _reqsBusy[index] = false;
                DispatchWork();
            }
            catch (WebException we)
            {
                MessageBox.Show("ReceivedData Web " + we.Message + url + we.Status);
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

                if (temp_s.Contains("N=5+530448"))
                {
                    int testtest = 0;
                    testtest++;
                }



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

                    if (RootUrl.ToLower().Contains("http://")) //get real root. for example. http://www.officedepot.com/product/a/12345.html real root is www.officedepot.com
                    {
                        int slash_index = RootUrl.IndexOf('/', 8);

                        _original_root_url = RootUrl.Substring(0, slash_index);
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


            
            string target_str0 = RootUrl .Split('.')[1];

            //restrict links to this website ONLY
            for (int i = 0; i < links.Length; i++)
            {
                if (links[i] == null)
                    continue;

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

        private void AddUrls(string[] urls, int depth)
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
                    if (Already_Searched_URL.ContainsKey(cleanUrl))//This URL has been searched before
                        continue;

                    if (cleanUrl.Contains(_baseUrl))
                    {
                        _urlsUnload.Add(cleanUrl, depth);
                        Already_Searched_URL.Add(cleanUrl, depth);
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
                MessageBox.Show("SaveContents IO" + ioe.Message + " path=" + path);
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
        private void Log_add_searched_html(string url)
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
                    w.WriteLine(counter_searched_url.ToString() + ". " + dt.ToString() + "\t" + url + "\n");

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

        
        #endregion
    }
}
