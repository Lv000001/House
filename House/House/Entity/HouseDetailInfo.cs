using House.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace House.Entity
{
    public class HouseDetailInfo
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long ID { set; get; }
        /// <summary>
        /// 貌似能唯一确定房源信息
        /// </summary>
        public string PageID { set; get; }
        /// <summary>
        /// 房屋类型
        /// </summary>
        public HouseType HouseType { set; get; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { set; get; }
        /// <summary>
        /// 单价
        /// </summary>
        public string Price { set; get; }
        /// <summary>
        /// 总价
        /// </summary>
        public double TotalAmount { set; get; }
        /// <summary>
        /// 几房几厅
        /// </summary>
        public string Room { set; get; }
        /// <summary>
        /// 总面积
        /// </summary>
        public string Area { set; get; }
        /// <summary>
        /// 实用面积
        /// </summary>
        public string AvailableArea { set; get; }
        /// <summary>
        /// 朝向
        /// </summary>
        public string Direction { set; get; }
        /// <summary>
        /// 小区名称
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// 标记
        /// </summary>
        public string Tag { set; get; }
        /// <summary>
        /// 详情页
        /// </summary>
        public string Url { set; get; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { set; get; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { set; get; }
    }
}
