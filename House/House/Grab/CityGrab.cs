using House.Helper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace House.Grab
{
    /// <summary>
    /// 抓取城市列表
    /// </summary>
    class CityGrab
    {
        /// <summary>
        /// 获取城市列表的url
        /// </summary>
        public string Url { set; get; } = "https://m.lianjia.com/city/";

        /// <summary>
        /// 城市列表  key:城市名称  value:城市代码
        /// </summary>
        public Dictionary<string, string> Citys
        {
            get
            {
                if (citys == null)
                {
                    citys = new Dictionary<string, string>();
                    GrabCitys();
                }             
                return citys;
            }
        }

        /// <summary>
        /// 城市列表  key:城市名称  value:城市代码
        /// </summary>
        private Dictionary<string, string> citys = null; 

        /// <summary>
        /// 获取城市列表 
        /// </summary>
        private void GrabCitys()
        {
            var response = HttpHelper.DoGet(Url);
            var doc = new HtmlDocument();
            doc.LoadHtml(response);
            //更加xpath获取总的对象，如果不为空，就继续选择dl标签
            var divs = doc.DocumentNode.SelectNodes("//div[@class='block city_block']");
            foreach (var div in divs)
            {
                var nodes = div.SelectNodes("a");
                foreach (var node in nodes)
                {
                    string city = node.InnerHtml;
                    string code = node.Attributes["href"].Value;
                    citys.Add(city, code);
                }              
            }
        }
    }
}
