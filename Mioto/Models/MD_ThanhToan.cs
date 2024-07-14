using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mioto.Models
{
    public class MD_ThanhToan
    {
        [Required]
        [StringLength(50)]
        public string PhuongThuc { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayTT { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal SoTien { get; set; }

        [StringLength(30)]
        public string TrangThai { get; set; }

        public int? IDMGG { get; set; }

        [ForeignKey("IDDT")]
        public virtual DonThueXe DonThueXe { get; set; }

        [ForeignKey("IDMGG")]
        public virtual MaGiamGia MaGiamGia { get; set; }
    }
}