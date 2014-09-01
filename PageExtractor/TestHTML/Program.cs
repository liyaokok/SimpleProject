using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHTML
{
    class Program
    {
        public static void handle_ReadInConfigFile_msg(string _website, string _msg_line)
        {
 
        }

        public static void ReadInConfigFile(string _directory_path)
        {
            try
            {
                using (StreamReader _streamReader = new StreamReader(_directory_path))
                {
                    String line;

                    string WebsiteName = null;

                    while ((line = _streamReader.ReadLine()) != null)
                    {
                        if (line.Length == 0 || line[0] == ';')//comment line
                        {
                            continue;
                        }
                        else if (line[0] == '[' && line[line.Length - 1] == ']')//Website Title eg. [OfficeDepot]
                        {
                            WebsiteName = line;
                        }
                        else
                        {
                            handle_ReadInConfigFile_msg(WebsiteName, line);
                        }
                    }
                }
            }
            catch (Exception e)
            {
               
            } 
 
        }

        static void Main(string[] args)
        {
            const string const_sign_R = "&reg;";
            const string const_sign_TM = "&trade;";

            string filePath = @"C:\Users\Yao\Desktop\TestSpider\0.html";
            string config_file_path = @"C:\PageExtractor\PageExtractor\PageExtractor\bin\Debug\config.ini";

            PageExtractor.ConfigurationManagement config_manage_box = new PageExtractor.ConfigurationManagement(config_file_path);

            #region old code

            /*HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

            // There are various options, set as needed
            htmlDoc.OptionFixNestedTags = true;

            // filePath is a path to a file containing the html
            htmlDoc.Load(filePath);*/

            // Use:  htmlDoc.LoadHtml(xmlString);  to load from a string (was htmlDoc.LoadXML(xmlString)

            // ParseErrors is an ArrayList containing any errors from the Load statement
            /*if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
            {
                // Handle any parse errors as required

            }
            else
            {

                if (htmlDoc.DocumentNode != null)
                {
                    //ElementName[@attributeName='value']
                    HtmlAgilityPack.HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//td[@class='price_amount']");

                    if (bodyNode != null)
                    {
                        // Do something with bodyNode
                        Console.WriteLine(bodyNode.InnerText);
                    }
                }
            }*/


            /*if (htmlDoc.DocumentNode != null)
            {
                //ElementName[@attributeName='value']
                HtmlAgilityPack.HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//span[@itemprop='price']");

                if (bodyNode != null)
                {
                    // Do something with bodyNode
                    Console.WriteLine(bodyNode.InnerText.Trim());
                }
            }
            */
            #endregion

            //string webName = "OfficeDepot";
            string webName = "Wayfair";
            PageExtractor.WebAnalysisAttribute web_attribute = (PageExtractor.WebAnalysisAttribute)config_manage_box.hashtable_WebAttribute[webName];



            HTMLAnalysis.HTMLAnalysisBox HTMLBox = new HTMLAnalysis.HTMLAnalysisBox();
            HTMLBox.Loan_FilePath(filePath);

            //HtmlAgilityPack.HtmlNode product_name_node = HTMLBox.Slect_SingleNode("//span[@itemprop='name']");
            HtmlAgilityPack.HtmlNode product_name_node = HTMLBox.Slect_SingleNode(web_attribute.ProductName_XPath[0]);
            //HtmlAgilityPack.HtmlNode product_price_node = HTMLBox.Slect_SingleNode("//span[@itemprop='price']");
            HtmlAgilityPack.HtmlNode product_price_node = HTMLBox.Slect_SingleNode(web_attribute.ProductPrice_XPath[0]);

            int simple_counter = 1;
            if (product_price_node == null)
            {
                while (web_attribute.ProductPrice_XPath[simple_counter] != null)
                {
                    product_price_node = HTMLBox.Slect_SingleNode(web_attribute.ProductPrice_XPath[simple_counter]);
                    simple_counter++;

                    if (product_price_node != null)
                        break;
                }
            }

            //HtmlAgilityPack.HtmlNode product_brand_node = HTMLBox.Slect_SingleNode("//td[@id='attributebrand_namekey']");

            if (product_name_node != null && product_price_node != null)
            {
                string product_name = product_name_node.InnerText.Trim().Replace(const_sign_R, "").Replace(const_sign_TM, "");
                string product_price = product_price_node.InnerText.Trim();
                //string product_brand = product_brand_node.InnerText.Trim();

                Console.Write("Product Name:" + product_name + "\n" + "Product Price:" + product_price + "\n" + "Brand Name:" );
            }
            else
            {
                Console.Write("Error: Can't find specific node!");
            }


            Console.Read();
        }
    }
}
