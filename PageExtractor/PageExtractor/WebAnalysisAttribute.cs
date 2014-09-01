using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PageExtractor
{
    public class WebAnalysisAttribute
    {
        public WebAnalysisAttribute()
        {
 
        }

        public string Name
        {
            get;
            set;
        }
        public string URL
        {
            get;
            set;
        }
        public string[] Filter_out
        {
            get;
            set;
        }
        public string[] Filter_in
        {
            get;
            set;
        }
        public string[] ProductName_XPath
        {
            get;
            set;
        }
        public string[] ProductPrice_XPath
        {
            get;
            set;
        }
        public string[] BrandName_XPath
        {
            get;
            set;
        }

    }
}
