using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mioto.Models
{
    public class MD_Payment
    {
        public int IDTT { get; set; } 

        public int IDDT { get; set; }

        [Required]
        [StringLength(50)]
        public string PhuongThuc { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NgayTT { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal SoTien { get; set; }

        [Required]
        [StringLength(30)]
        public string TrangThai { get; set; } = "Yes";

        public string MaGiamGia { get; set; }
    }
}