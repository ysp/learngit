using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GISApp
{
    public interface IUserMapTool
    {
        //返回当前操作名称
        string GetToolName();

        //初始化地图工具，绑定用户操作事件
        void InitTool();

        //销毁工具，注销用户操作事件，清除必要数据
        void DestroyTool();
    }
}
