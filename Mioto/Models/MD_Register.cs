using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mioto.Models
{
    public class MD_Register
    {
        [Required(ErrorMessage = "Vui lòng nhập tên của bạn.")]
        [StringLength(255, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        public string Ten { get; set; }

        [Remote("IsEmailAvailable", "Home", HttpMethod = "POST", ErrorMessage = "Email đã tồn tại.")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email của bạn.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [StringLength(255, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại của bạn.")]
        [StringLength(20, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Số điện thoại chỉ chứa chữ số.")]
        public string SDT { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày sinh.")]
        [CustomValidation(typeof(MD_Register), "ValidateAge")]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính của bạn.")]
        [StringLength(10, ErrorMessage = "Giới tính không hợp lệ.")]
        public string GioiTinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
        public string DiaChi { get; set; } 

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu của bạn.")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu của bạn.")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("MatKhau", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
        public string ConfirmMatKhau { get; set; }

        public static ValidationResult ValidateAge(DateTime date, ValidationContext context)
        {
            var today = DateTime.Today;
            var age = today.Year - date.Year;
            if (date > today.AddYears(-age)) age--;

            return age >= 18 ? ValidationResult.Success : new ValidationResult("Bạn phải ít nhất 18 tuổi để đăng ký.");
        }
    }
}
