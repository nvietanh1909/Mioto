using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace Mioto.Models
{
    public class MD_KhachHang
    {

        [Required(ErrorMessage = "Vui lòng nhập tên của bạn.")]
        [StringLength(255, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        public string Ten { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ của bạn.")]
        [StringLength(255, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        public string DiaChi { get; set; }

        [Remote("IsEmailAvailable", "Home", HttpMethod = "POST", ErrorMessage = "Email đã tồn tại.")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email của bạn.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [StringLength(255, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại của bạn.")]
        [StringLength(20, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Số điện thoại chỉ chứa chữ số.")]
        public string SDT { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày sinh của bạn.")]
        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính của bạn.")]
        [StringLength(10, ErrorMessage = "Giới tính không hợp lệ.")]
        public string GioiTinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu của bạn.")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu của bạn.")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("MatKhau", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
        public string ConfirmMatKhau { get; set; }
    }
}
