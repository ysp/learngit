using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GISApp.Tables
{
    [Table("User")]
    class User
    {
        [Column("UserName"), NotNull]
        public string UserName { get; set; }

        [Column("PassWord")]
        public string PassWord { get; set; }

        [Column("Role")]
        public string Role { get; set; }
    }
}
