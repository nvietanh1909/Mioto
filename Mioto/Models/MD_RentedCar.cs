using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mioto.Models
{
    public class MD_RentedCar
    {
        public int IDDT { get; set; }  
        public string BienSoXe { get; set; }  
        public string HangXe { get; set; }        
        public string MauXe { get; set; }         
        public DateTime NgayThue { get; set; }   
        public DateTime NgayTra { get; set; }     
        public decimal TongTien { get; set; }     
        public int TrangThai { get; set; }   
        public DonThueXe DonThueXe { get; set; }
        public Xe Xe { get; set; }
    }
}