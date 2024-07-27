using System;
using System.ComponentModel.DataAnnotations;

namespace Mioto.Models
{
    public class MD_MaGiamGia
    {
        [Required(ErrorMessage = "Mã giảm giá là bắt buộc.")]
        public string Ma { get; set; }

        [Required(ErrorMessage = "Phần trăm giảm là bắt buộc.")]
        [Range(0, 100, ErrorMessage = "Phần trăm giảm phải nằm trong khoảng từ 0 đến 100.")]
        public int PhanTramGiam { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày Bắt Đầu")]
        [CustomValidation(typeof(MD_MaGiamGia), "ValidateNgayBatDau")]
        public DateTime NgayBatDau { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày Kết Thúc")]
        [CustomValidation(typeof(MD_MaGiamGia), "ValidateNgayKetThuc")]
        public DateTime NgayKetThuc { get; set; }

        [Required(ErrorMessage = "Số lần sử dụng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lần sử dụng phải lớn hơn 0.")]
        public int SolanSuDung { get; set; }

        // Kiểm tra Ngày Bắt Đầu
        public static ValidationResult ValidateNgayBatDau(DateTime ngayBatDau, ValidationContext context)
        {
            if (ngayBatDau < DateTime.Today)
            {
                return new ValidationResult("Ngày bắt đầu phải lớn hơn hoặc bằng ngày hiện tại.");
            }
            return ValidationResult.Success;
        }

        // Kiểm tra Ngày Kết Thúc
        public static ValidationResult ValidateNgayKetThuc(DateTime ngayKetThuc, ValidationContext context)
        {
            var instance = context.ObjectInstance as MD_MaGiamGia;
            if (instance != null && ngayKetThuc <= instance.NgayBatDau)
            {
                return new ValidationResult("Ngày kết thúc phải sau ngày bắt đầu.");
            }
            return ValidationResult.Success;
        }
    }
}
