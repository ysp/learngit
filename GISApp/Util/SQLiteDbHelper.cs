using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace GISApp
{
    class SQLiteDbHelper
    {
        //数据库文件名 OMDB.db
        private readonly static string DbPath = StorageManager.GetDataPath("OMDB.db");

        public static SQLiteConnection GetDbConnection()
        {
            // 连接数据库，如果数据库文件不存在则创建一个空数据库
            return new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);
        }

        public static void CreateTable(Type type)
        {
            // 创建 模型对应的表，如果已存在，则忽略该操作
            using (var conn = GetDbConnection())
            {
                conn.CreateTable(type);
            }
        }

        public static int Insert<T>(T obj)
        {
            int count;
            using (var conn = GetDbConnection())
            {
                count = conn.Insert(obj);
            }
            return count;
        }

        public static int Insert<T>(List<T> objList)
        {
            int count = 0;
            using (var conn = GetDbConnection())
            {
                count = conn.InsertAll(objList);
            }
            return count;
        }

        public static void Delete<T>(T obj)
        {
            using (var conn = GetDbConnection())
            {
                conn.Delete(obj);
            }
        }

        public static void DeleteAll(Type type)
        {
            using (var conn = GetDbConnection())
            {
                conn.DeleteAll(type);
            }
        }

        //DbHelper.Delete<Student>("select * from student where id < 24");
        public static void Delete<T>(string query) where T : class
        {
            using (var conn = GetDbConnection())
            {
                var list = conn.Query<T>(query);
                foreach (var item in list)
                {
                    conn.Delete(item);
                }
            }
        }

        public static int Update<T>(T obj)
        {
            using (var conn = GetDbConnection())
            {
                return conn.Update(obj);
            }
        }

        public static int Update<T>(List<T> objList)
        {
            using (var conn = GetDbConnection())
            {
                return conn.Update(objList);
            }
        }

        //DbHelper.Find<Student>("select * from student where id < 24");
        public static T FindByPrimaryKey<T>(object pk) where T : class
        {
            using (var conn = GetDbConnection())
            {
                return conn.Find<T>(pk);
            }
        }

        //DbHelper.Find<Student>("select * from student where id < 24");
        public static List<T> Query<T>(string query) where T : class
        {
            using (var conn = GetDbConnection())
            {
                return conn.Query<T>(query);
            }
        }

        public static List<T> Query<T>(string query, object[] args) where T : class
        {
            using (var conn = GetDbConnection())
            {
                return conn.Query<T>(query, args);
            }
        }

        public static int Execute(string query, params object[] args)
        {
            using (var conn = GetDbConnection())
            {
                return conn.Execute(query, args);
            }
        }
    }
}
