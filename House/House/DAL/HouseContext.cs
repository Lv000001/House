using House.Entity;
using SQLite.CodeFirst;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;

namespace House.DAL
{
    public class HouseContext : DbContext
    {
        public HouseContext() : base("Name=House")
        {
        }

        /// <summary>
        /// 房屋详细信息
        /// </summary>
        public DbSet<HouseDetailInfo> HouseDetailInfo { set; get; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //创建表
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<HouseDetailInfo>();


            // ---关闭所有级联删除
            modelBuilder.Conventions.Remove();

            // ---由于SQLite的EntityFramework库没有代码优先模式下自动创建数据库的功能,所以使用SQLiteCodeFirst 库自动创建数据库,
            // 当数据库不存在时创建
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<HouseContext>(modelBuilder);

            Database.SetInitializer(sqliteConnectionInitializer);
            base.OnModelCreating(modelBuilder);
        }
    }
}
