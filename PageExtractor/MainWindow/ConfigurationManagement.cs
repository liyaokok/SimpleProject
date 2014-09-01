using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PageExtractor
{
    public class ConfigurationManagement
    {
        private const char CONFIG_INI_SPLITTER = '~';

        private string CONFIG_INI_PATH = null; // Different websites has different XPath for name, price, etc.

        public Hashtable hashtable_WebAttribute;

        public ConfigurationManagement(string _config_file_path)
        {
            CONFIG_INI_PATH = _config_file_path;

            hashtable_WebAttribute = new Hashtable();

            ReadInConfigFile();
        }

        private void ReadInConfigFile()
        {
            try
            {
                using (StreamReader _streamReader = new StreamReader(CONFIG_INI_PATH))
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
                            WebsiteName = line.Substring(1,line.Length-2);
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

        private void handle_ReadInConfigFile_msg(string _website, string _msg_line)
        {
            if (!hashtable_WebAttribute.ContainsKey(_website))
            {
                WebAnalysisAttribute _temp_WAA = new WebAnalysisAttribute();
                hashtable_WebAttribute.Add(_website, _temp_WAA);
            }
            
            WebAnalysisAttribute current_WebAnalysisAttribute = (WebAnalysisAttribute)hashtable_WebAttribute[_website];

            //handle config.ini message line
            int _temp_position = _msg_line.IndexOf('=');

            string _temp_string1 = _msg_line.Substring(0, _temp_position).Trim();
            string _temp_string2 = _msg_line.Substring(_temp_position + 1, _msg_line.Length - _temp_position - 1).Trim();

            //no valid attribute
            if (_temp_string1 == null || _temp_string1.Trim().Equals("") == true)
                return;

            string attribute_name = _temp_string1;
            string attribute_value_choices = _temp_string2.Trim();

            string[] split_attribute_value_choices = attribute_value_choices.Split(CONFIG_INI_SPLITTER);

            //no valid attribute
            if (split_attribute_value_choices.Length < 1)
                return;

            switch (attribute_name)
            {
                case "Website_URL":
                    current_WebAnalysisAttribute.URL = split_attribute_value_choices[0];
                    break;
                case "ProductName_XPath":
                    current_WebAnalysisAttribute.ProductName_XPath = split_attribute_value_choices;
                    break;
                case "ProductPrice_XPath":
                    current_WebAnalysisAttribute.ProductPrice_XPath = split_attribute_value_choices;
                    break;
                case "ProductBrandName_XPath":
                    current_WebAnalysisAttribute.BrandName_XPath = split_attribute_value_choices;
                    break;
                case "Filter_out"://the keywork which you don't wanna it appear in the URL
                    current_WebAnalysisAttribute.Filter_out = split_attribute_value_choices;
                    break;
                case "Filter_in"://the keywork which must appear in the URL
                    current_WebAnalysisAttribute.Filter_in = split_attribute_value_choices;
                    break;
                case "ProductImageURL_XPath":
                    current_WebAnalysisAttribute.ProuctImageURL_XPath = split_attribute_value_choices;
                    break;
                case "ProductDescription_XPath":
                    current_WebAnalysisAttribute.ProductDescription_XPath = split_attribute_value_choices;
                    break;
                case "ModelName_XPath":
                    current_WebAnalysisAttribute.ModelName_XPath = split_attribute_value_choices;
                    break;
                case "ManufactureName_XPath":
                    current_WebAnalysisAttribute.ManufactureName_XPath = split_attribute_value_choices;
                    break;
                case "ManufactureNumber_XPath":
                    current_WebAnalysisAttribute.ManufactureNumber_XPath = split_attribute_value_choices;
                    break;
                case "UPCNumber_XPath":
                    current_WebAnalysisAttribute.UPCNumber_XPath = split_attribute_value_choices;
                    break;




                default:
                    break;
            }

            
        }
    }
}
