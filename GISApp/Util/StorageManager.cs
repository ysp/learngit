using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GISApp
{
    public static class StorageManager
    {
        public static string GetTPKPath(string name)
        {
            string filePath = GetPathByNameAndExtension(name, ".tpk");
            return filePath;
        }

        public static List<string> GetAllTPKNames()
        {
            List<string> filenames = GetAllFilesWithExtension(".tpk");
            //做排序
            filenames.Sort();
            return filenames;
        }

        public static string GetGeodatabasePath(string name)
        {
            string filePath = GetPathByNameAndExtension(name, ".geodatabase");
            return filePath;
        }

        public static List<string> GetAllGeodatabaseNames()
        {
            List<string> filenames = GetAllFilesWithExtension(".geodatabase");
            //做排序
            filenames.Sort();
            return filenames;
        }

        //TODO: 为了获取额外目录中的数据
        public static List<string> GetExtraGeodatabaseNames()
        {
            List<string> filenames = new List<string>();
            string dir = GetExtraPath();

            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            FileSystemInfo[] fileInfos = dirInfo.GetFileSystemInfos();
            foreach (FileSystemInfo fsinfo in fileInfos)
            {
                //判断是否为文件夹
                if (fsinfo is DirectoryInfo)
                    continue;//不做处理

                string fileName = fsinfo.Name;
                string fileExt = Path.GetExtension(fileName);
                if (fileExt.Equals(".geodatabase", StringComparison.OrdinalIgnoreCase))
                    filenames.Add(fileName);
            }
            filenames.Sort();
            return filenames;
        }

        private static List<string> GetAllFilesWithExtension(string extension)
        {
            List<string> filenames = new List<string>();
            string dir = GetDataFolder();

            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            FileSystemInfo[] fileInfos = dirInfo.GetFileSystemInfos();
            foreach (FileSystemInfo fsinfo in fileInfos)
            {
                //判断是否为文件夹
                if (fsinfo is DirectoryInfo)
                    continue;//不做处理

                string fileName = fsinfo.Name;
                string fileExt = Path.GetExtension(fileName);
                if (fileExt.Equals(extension, StringComparison.OrdinalIgnoreCase))
                    filenames.Add(fileName);
            }
            return filenames;
        }

        public static string GetPathByNameAndExtension(string fileName, string ext)
        {
            string filePath = GetDataPath(fileName);
            if (File.Exists(filePath))
                return filePath;
            filePath = GetDataPath(fileName + ext);
            if (File.Exists(filePath))
                return filePath;
            return String.Empty;
        }

        public static string GetExtraPath()
        {
            return Path.Combine(GetDataFolder(), "extra");
        }
        public static string GetExtraDataPath(string fileName)
        {
            return Path.Combine(GetExtraPath(), fileName);
        }

        public static string GetDataPath(string fileName)
        {
            return Path.Combine(GetDataFolder(), fileName);
        }

        public static string GetDataFolder()
        {
            string appDataFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            string userDataFolder = Path.Combine(appDataFolder, "LJUserData");
            if (!Directory.Exists(userDataFolder)) { Directory.CreateDirectory(userDataFolder); }
            return userDataFolder;
        }
    }
}
