using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GISApp.Tables
{
    [Table("MAPPOI")]
    class MapPoi
    {
        [Column("ID"), PrimaryKey, AutoIncrement, NotNull]
        public int ID { get; set; }

        [Column("POINAME"), NotNull]
        public string PoiName { get; set; }

        [Column("X"), NotNull]
        public double X { get; set; }

        [Column("Y"), NotNull]
        public double Y { get; set; }

        [Column("TAG")]
        //TODO: Tag类型不适合用string
        public string Tag { get; set; }

        public override string ToString()
        {
            return this.PoiName;
        }
    }
}
