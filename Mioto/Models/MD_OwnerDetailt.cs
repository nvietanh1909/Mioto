using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mioto.Models
{
    public class MD_OwnerDetailt
    {
        public string BienSoXe { get; set; }
        public DateTime NgayThue { get; set; }
        public DateTime NgayTra { get; set; }
        public decimal TongTien { get; set; }
        public int TrangThai { get; set; }
        public ChuXe ChuXe { get; set; }
    }
}