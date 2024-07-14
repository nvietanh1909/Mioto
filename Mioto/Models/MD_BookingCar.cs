using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mioto.Models
{
    public class MD_BookingCar
    {
        // Đơn thuê xe
        [Required(ErrorMessage = "Ngày thuê là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime NgayThue { get; set; }

        [Required(ErrorMessage = "Ngày trả là bắt buộc")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(MD_BookingCar), "ValidateNgayTra")]
        public DateTime NgayTra { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime BDT { get; set; } = DateTime.Now;

        public string TrangThai { get; set; }

        // Kiểm tra NgayThue và NgayTra
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
        public ThanhToan ThanhToan { get; set; }
    }
}
