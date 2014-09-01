using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PageExtractor;
using System.IO;
using System.Threading;

namespace MainWindow
{
    public partial class MainWinfow : Form
    {
        private Spider _spider = null;

        //Web settings
        private ConfigurationManagement config_manage_box = null;
        public WebAnalysisAttribute web_attribute = null;
        string config_file_path = @"C:\PageExtractor\PageExtractor\PageExtractor\bin\Debug\config.ini";
        string webName = "OfficeDepot";


        public MainWinfow()
        {
            InitializeComponent();
            _spider = new Spider(this);
            _spider.ContentsSaved += new Spider.ContentsSavedHandler(Spider_ContentsSaved);
            _spider.DownloadFinish += new Spider.DownloadFinishHandler(Spider_DownloadFinish);
            button_Cancel.Enabled = false;
        }

        private void button_FilePath_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fdlg = new System.Windows.Forms.FolderBrowserDialog();
            fdlg.RootFolder = Environment.SpecialFolder.Desktop;
            fdlg.Description = "Contents Root Folder";
            var result = fdlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string path = fdlg.SelectedPath;
                textBox_FilePath.Text = path;
            }
        }

        private void button_BaseURL_Click(object sender, EventArgs e)
        {
            PropertySettingWindow setting_window = new PropertySettingWindow()
            {
                MaxDepth = _spider.MaxDepth,
                MaxConnection = _spider.MaxConnection,
            };

            if (setting_window.ShowDialog() == DialogResult.OK)
            {
                _spider.MaxDepth = setting_window.MaxDepth;
                _spider.MaxConnection = setting_window.MaxConnection;
            }
        }

        delegate void Spider_DownloadFinish_callback(int _integer);
        void Spider_DownloadFinish(int count)
        {
            if(button_Download.InvokeRequired)
            {
                Spider_DownloadFinish_callback delegate_Spider_DownloadFinish_callback = new Spider_DownloadFinish_callback(Spider_DownloadFinish);
                button_Download.Invoke(delegate_Spider_DownloadFinish_callback, new object[] { count });
            }
            else
            {
                _spider.Abort();
            
                button_Download.Enabled = true;
            
                button_Download.Text = "Download";
            
                button_Cancel.Enabled = false;
            
                MessageBox.Show("Finished");
            }
        }


        delegate void write_richTextBox_prompt_callback(string _contents_string);
        void write_to_prompt(string _contents)
        {
            if (richTextBox_prompt.InvokeRequired)
            {
                write_richTextBox_prompt_callback delegate_write_richTextBox_prompt_callback = new write_richTextBox_prompt_callback(write_to_prompt);
                richTextBox_prompt.Invoke(delegate_write_richTextBox_prompt_callback, new object[] { _contents });
            }
            else
            {
                DateTime dt = DateTime.Now;
                richTextBox_prompt.AppendText(dt.ToString() + "\t" +_contents + "\n");
            }
        }

        void Spider_ContentsSaved(string path, string url)
        {
            Log_URL_LocalFile(url, path);
        }

        private bool first_time_log = true;
        public void Log_URL_LocalFile(string _url, string _path)
        {
            string path = Environment.CurrentDirectory + "\\URL_LocalFile.txt";

            try
            {
                using (StreamWriter w = File.AppendText(path))
                {
                    if (first_time_log)
                    {
                        first_time_log = false;
                        w.WriteLine("-------------------------------------------");
                    }

                    DateTime dt = DateTime.Now;
                    w.WriteLine(_url.ToString() + "\t" + _path + "\n");
                }
            }
            catch (Exception)
            { }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _spider.Abort();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox_BaseURL.Text = "www.officedepot.com";
            textBox_FilePath.Text = "C:\\Users\\Yao\\Desktop\\TestSpider";
        }

        private void button_Download_Click(object sender, EventArgs e)
        {
            config_manage_box = new ConfigurationManagement(config_file_path);
            web_attribute = (WebAnalysisAttribute)config_manage_box.hashtable_WebAttribute[webName];

            _spider.RootUrl = textBox_BaseURL.Text;
            Thread thread = new Thread(new ParameterizedThreadStart(Download));
            thread.Start(textBox_FilePath.Text);
            button_Download.Enabled = false;
            button_Download.Text = "Downloading...";
            button_Cancel.Enabled = true;
        }

        private void Download(object param)
        {
            _spider.Download((string)param);
        }

        private void Download_test(object param)
        {
            _spider.Download_test((string)param);
        }


        private void button_Cancel_Click(object sender, EventArgs e)
        {
            _spider.Abort();
            button_Download.Enabled = true;
            button_Download.Text = "Download";
            button_Cancel.Enabled = false;
        }

        private void button1_ReadFile_Click(object sender, EventArgs e)
        {
            config_manage_box = new ConfigurationManagement(config_file_path);
            web_attribute = (WebAnalysisAttribute)config_manage_box.hashtable_WebAttribute[webName];

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
                            if (_spider.RootUrl == null)
                                _spider.RootUrl = _line;
                            else
                            {
                                _spider.AddUrls(new string[1] { _line }, 0);
                            }
                        }
                    }
                }
            }
            catch(Exception)
            {
                MessageBox.Show("error!");
            }

            Thread thread = new Thread(new ParameterizedThreadStart(Download_test));

            thread.Start(textBox_FilePath.Text);

            button_Download.Enabled = false;

            button_Download.Text = "Downloading...";

            button_Cancel.Enabled = true;

        }













    }
}
