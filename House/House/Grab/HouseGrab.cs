using House.Entity;
using House.Enum;
using House.Helper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace House.Grab
{
    class HouseGrab
    {
        public string Url { set; get; } = "https://m.lianjia.com";

        /// <summary>
        /// 房屋类型
        /// </summary>
        public HouseType HouseType { get; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { set; get; }

        /// <summary>
        /// 总共的页数
        /// </summary>
        public int TotalPagesCount { get; set; }

        /// <summary>
        /// 总共房源数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 进度发生变化
        /// </summary>
        public event Action<HouseGrab, double> ProgressChanged;

        /// <summary>
        /// 采集到了数据
        /// </summary>
        public event  Action<List<HouseDetailInfo>> GrabHouseDetailInfoChanged;
        /// <summary>
        /// 是否通知进度
        /// </summary>
        private bool isNoityProgress = true;

        public HouseGrab(HouseType houseType)
        {
            HouseType = houseType;
        }

        /// <summary>
        /// 请求头部
        /// </summary>
        private NameValueCollection Header = new NameValueCollection
        {
            {"User-Agent","Mozilla/5.0 (Windows NT 6.1; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0" },
            { "Referer","m.lianjia.com"},
            { "Host","m.lianjia.com"},
            { "Accept","text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"},
            { "Accept-Encoding","gzip, deflate"},
            { "Accept-Language","zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3"},
        };

        /// <summary>
        /// 抓取所有房源信息
        /// </summary>
        /// <returns></returns>
        public List<HouseDetailInfo> GrabAll()
        {
            List<HouseDetailInfo> houseDetailInfos = new List<HouseDetailInfo>();
            isNoityProgress = false;
            // 抓取第一页的数据获取总页数
            houseDetailInfos.AddRange(Grab(0, 1));
            isNoityProgress = true;
            houseDetailInfos.AddRange(Grab(1, TotalPagesCount - 1));
            return houseDetailInfos;
        }

        /// <summary>
        /// 抓取房源信息
        /// </summary>
        /// <param name="startPageIndex">开始的页码</param>
        /// <param name="pageCount">采集的页数</param>
        /// <returns></returns>
        public List<HouseDetailInfo> Grab(int startPageIndex, int pageCount)
        {
            List<HouseDetailInfo> detailInfos = new List<HouseDetailInfo>();
            string url = Url;
            Header["Referer"] = url;
            // 1.设置cookie
            string cookie;
            HttpHelper.DoGet(Url, out cookie, Header);
            Header.Add("Cookie", cookie);

            // 2.切换城市
            url = Url + City;
            Header["Referer"] = url;
            string html = HttpHelper.DoGet(url, out cookie, Header);
            Header["Cookie"] = cookie;

            // 3.按页获取房屋详情
            for (int i = 0; i < pageCount; i++)
            {
                int page = startPageIndex + i;
                switch (HouseType)
                {
                    case Enum.HouseType.NEW:
                        url = string.Format("{0}{1}loupan/fang/pg{2}/", Url, City, page);
                        break;
                    case Enum.HouseType.OLD:
                        url = string.Format("{0}{1}ershoufang/index/pg{2}/", Url, City, page);
                        break;
                    case Enum.HouseType.RENTING:
                        url = string.Format("{0}/chuzu{1}zufang/pg{2}/?ajax=1", Url, City, page);
                        break;
                }

                Header["Referer"] = url;
                html = HttpHelper.DoGet(url, out cookie, Header);
                if (isNoityProgress)
                {
                    ProgressChanged?.Invoke(this,((double)i) / pageCount);
                }
                if (html != null)
                {
                    var houseDetailInfos = GetDetailInfos(html);                
                    if (houseDetailInfos != null)
                    {
                        GrabHouseDetailInfoChanged?.Invoke(houseDetailInfos);
                        detailInfos.AddRange(houseDetailInfos);
                    }
                    System.Threading.Thread.Sleep(10);
                }
                else
                {
                    break;
                }

            }
            return detailInfos;
        }

        /// <summary>
        /// 获取房屋详细信息
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private List<HouseDetailInfo> GetDetailInfos(string html)
        {
            List<HouseDetailInfo> detailInfos = new List<HouseDetailInfo>();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            //  < div class="mod_cont">
            //<ul class="lists" data-mark="list_container" data-info="total=37088">
            //<li class="download_card arrow">
            // 获取总房源数
            TotalCount = 0;
            var mod_cont = doc.DocumentNode.SelectNodes("//div[@class='mod_cont']");
            if (mod_cont != null && mod_cont.Count > 0)
            {
                var lists = mod_cont[0].SelectNodes(".//ul[@class='lists']");
                if (lists != null && lists.Count > 0)
                {
                    var total = lists[0].Attributes["data-info"].Value;
                    if (total != null)
                    {
                        TotalCount = int.Parse(total.ToLower().Replace("total=", ""));
                    }
                }
            }

            //class="a_mask post_ulog post_ulog_action"
            HtmlNodeCollection lis = doc.DocumentNode.SelectNodes("//li[@class='pictext']");
            if (lis != null)
            {
                int pageSize = lis.Count;
                TotalPagesCount = TotalCount / pageSize + ((TotalCount % pageSize == 0) ? 0 : 1);
                foreach (HtmlNode li in lis)
                {
                    try
                    {
                        HouseDetailInfo detailInfo = new HouseDetailInfo();
                        detailInfo.HouseType = HouseType;
                        detailInfo.City = City;

                        // <a href = "/aq/ershoufang/103103467743.html" class="a_mask post_ulog post_ulog_action" data-evtid_action="13298" data-ulog_action="pid=lianjiamweb&event=mModuleClick&fb_query_id=" ></a>
                        var aNodes = li.SelectNodes(".//a[@class='a_mask post_ulog post_ulog_action']");
                        if (aNodes != null && aNodes.Count > 0)
                        {
                            detailInfo.Url = Url + aNodes[0].Attributes["href"].Value;
                            Regex regex = new Regex("[0-9]+(?=\\.html)");
                            detailInfo.PageID = regex.Match(detailInfo.Url).Value;
                        }
                        //   <div class="item_main">凯旋尊邸 高档小区  安保严格 成熟小区  配套齐*</div>
                        var tag = li.SelectNodes(".//div[@class='item_main']");
                        if (tag != null && tag.Count > 0)
                        {
                            detailInfo.Tag = tag[0].InnerHtml;
                        }
                        // <div class="item_other text_cut" title="凯旋尊邸二手房">4室2厅/141.73m²/南/凯旋尊邸</div>
                        var area = li.SelectNodes(".//div[@class='item_other text_cut']");
                        if (area != null && area.Count > 0)
                        {
                            string[] infos = area[0].InnerHtml.Split('/');
                            if (infos.Length > 3)
                            {
                                detailInfo.Room = infos[0];
                                detailInfo.Area = infos[1];
                                detailInfo.Direction = infos[2];
                                detailInfo.Name = infos[3];
                            }
                        }
                        //  <div class="item_minor"><span class="price_total"><em>160</em><span class="unit">万</span></span><span class="unit_price">11290元/平</span></div>
                        var minor = li.SelectNodes(".//div[@class='item_minor']");
                        if (minor != null && minor.Count > 0)
                        {
                            var span = minor[0].SelectNodes("./span/em");
                            if (span != null && span.Count > 0)
                            {
                                double.TryParse(span[0].InnerHtml, out double amount);
                                detailInfo.TotalAmount = amount;
                            }

                            var unit_price = li.SelectNodes(".//span[@class='unit_price']");
                            if (unit_price != null && unit_price.Count > 0)
                            {
                                detailInfo.Price = unit_price[0].InnerHtml;
                            }
                        }
                        detailInfos.Add(detailInfo);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message, ex);
                    }
                }
            }

            return detailInfos;
        }
    }
}

