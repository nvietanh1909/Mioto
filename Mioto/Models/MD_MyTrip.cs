using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mioto.Models
{
    public class MD_MyTrip
    {
        [Required]
        public int IDDT { get; set; }  

        [Required]
        [StringLength(50)]
        public string BienSoXe { get; set; }

        [Required]
        public DateTime NgayThue { get; set; }

        [Required]
        public DateTime NgayTra { get; set; }

        public DateTime BDT { get; set; }
        public int TrangThai { get; set; }

        [Required]
        public decimal TongTien { get; set; }

        public ThanhToan ThanhToan { get; set; } 
        public ChuXe ChuXe { get; set; } 
    }
}
