using System;
using System.ComponentModel.DataAnnotations;

namespace Mioto.Models
{
    public class MD_BookingCar
    {
        // Đơn thuê xe
        [Required(ErrorMessage = "Ngày thuê là bắt buộc")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(MD_BookingCar), "ValidateNgayThue")]
        public DateTime NgayThue { get; set; }

        [Required(ErrorMessage = "Ngày trả là bắt buộc")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(MD_BookingCar), "ValidateNgayTra")]
        public DateTime NgayTra { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime BDT { get; set; } = DateTime.Now;

        public string TrangThai { get; set; }

        // Kiểm tra NgayThue
        public static ValidationResult ValidateNgayThue(DateTime ngayThue, ValidationContext context)
        {
            if (ngayThue < DateTime.Today)
            {
                return new ValidationResult("Ngày thuê phải lớn hơn hoặc bằng ngày hiện tại");
            }
            return ValidationResult.Success;
        }

        // Kiểm tra NgayTra
        public static ValidationResult ValidateNgayTra(DateTime ngayTra, ValidationContext context)
        {
            var instance = context.ObjectInstance as MD_BookingCar;
            if (instance != null && instance.NgayThue != DateTime.MinValue && ngayTra <= instance.NgayThue)
            {
                return new ValidationResult("Ngày trả phải sau ngày thuê");
            }
            return ValidationResult.Success;
        }

        // Lấy ra Xe/ChuXe/ThanhToan
        public Xe Xe { get; set; }
        public ChuXe ChuXe { get; set; }
    }
}
