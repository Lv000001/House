using House.DAL;
using House.Entity;
using House.Grab;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace House
{
    public partial class FormMain : Form
    {
        public Dictionary<string, string> Citys;

        private object lockObject = new object();

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            CityGrab cityGrab = new CityGrab();
            Citys = cityGrab.Citys;

            foreach (var item in Citys.Keys)
            {
                grid.Rows.Add(item, Citys[item], "0%", "0", "未采集");
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < grid.Rows.Count; i++)
            {
                HouseGrab houseGrab = new HouseGrab(Enum.HouseType.OLD);
                houseGrab.City = Citys[grid.Rows[i].Cells[0].Value.ToString()];
                houseGrab.ProgressChanged += (s, p) =>
                {
                    Invoke(new Action(() =>
                    {
                        for (int j = 0; j < grid.RowCount; j++)
                        {
                            if (grid.Rows[j].Cells[1].Value.ToString() == s.City)
                            {
                                grid.Rows[j].Cells[2].Value = string.Format("{0}%", p * 100);
                                grid.Rows[j].Cells[3].Value = s.TotalCount;
                                grid.Rows[j].Cells[4].Value = "采集中...";
                                break;
                            }
                        }
                    }));
                };

                houseGrab.GrabHouseDetailInfoChanged += (detial) =>
                {
                    lock (lockObject)
                    {
                        using (HouseContext db = new HouseContext())
                        {
                            db.HouseDetailInfo.AddRange(detial);
                            db.SaveChanges();
                        }
                    }                   
                };

                Action action = new Action(() =>
                {
                    houseGrab.GrabAll();
                });
                action.BeginInvoke(null, null);


            }
        }
    }
}
