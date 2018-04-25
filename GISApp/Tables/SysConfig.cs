using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GISApp.Tables
{
    [Table("SYSCONFIG")]
    class SysConfig
    {
        [Column("key"), PrimaryKey, NotNull]
        public string Key { get; set; }

        [Column("value"), NotNull]
        public string Value { get; set; }

        [Column("description")]
        public string Description { get; set; }
    }
}
