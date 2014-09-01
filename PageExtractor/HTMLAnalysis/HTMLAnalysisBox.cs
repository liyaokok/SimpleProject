using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTMLAnalysis
{
    public class HTMLAnalysisBox
    {
        private HtmlAgilityPack.HtmlDocument htmlDoc;

        public HTMLAnalysisBox()
        {
            htmlDoc = new HtmlAgilityPack.HtmlDocument();
            // There are various options, set as needed
            htmlDoc.OptionFixNestedTags = true;
        }

        public HtmlAgilityPack.HtmlDocument HTMLDocument
        {
            get
            {
                return htmlDoc;
            }
            set
            {
                htmlDoc = value;
            }
        }

        public bool Loan_FilePath(string _path)
        {
            if (htmlDoc == null)
                return false;
            else 
            {
                htmlDoc.Load(_path);
                return true;
            }
        }

        public bool Loan_PlainText(string _html_string)
        {
            if (htmlDoc == null)
                return false;
            else
            {
                htmlDoc.LoadHtml(_html_string);
                return true;
            }
        }

        public HtmlAgilityPack.HtmlNode Slect_SingleNode(string _xPATH)
        {
            if (htmlDoc == null)
                return null;
            else
            {
                if (htmlDoc.DocumentNode == null)
                    return null;
                else
                {
                    //ElementName[@attributeName='value']
                    HtmlAgilityPack.HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode(_xPATH);

                    if (bodyNode == null)
                        return null;
                    else
                    {
                        return bodyNode;
                    }
                }
            }
        }

        public HtmlAgilityPack.HtmlNodeCollection Slect_AllNode(string _xPATH)
        {
            if (htmlDoc == null)
                return null;
            else
            {
                if (htmlDoc.DocumentNode == null)
                    return null;
                else
                {
                    //ElementName[@attributeName='value']
                    HtmlAgilityPack.HtmlNodeCollection bodyNode = htmlDoc.DocumentNode.SelectNodes(_xPATH);

                    if (bodyNode == null)
                        return null;
                    else
                    {
                        return bodyNode;
                    }
                }
            }
        }

    }
}
