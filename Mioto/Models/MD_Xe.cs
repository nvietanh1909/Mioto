using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mioto.Models
{
    public class MD_Xe
    {
        [Required(ErrorMessage = "Vui lòng nhập biển số xe.")]
        [StringLength(20, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        public string BienSoXe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập hãng xe.")]
        [StringLength(50, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        public string HangXe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập màu xe.")]
        [StringLength(50, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        public string MauXe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập năm sản xuất.")]
        public int NamSanXuat { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số ghế.")]
        public int SoGhe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tính năng.")]
        [StringLength(50, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        public string TinhNang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá thuê.")]
        [DataType(DataType.Currency)]
        public decimal GiaThue { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khu vực.")]
        [StringLength(20, ErrorMessage = "Khu vực không hợp lệ.")]
        public string KhuVuc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình ảnh xe.")]
        public List<string> HinhAnh { get; set; }

    }
}
