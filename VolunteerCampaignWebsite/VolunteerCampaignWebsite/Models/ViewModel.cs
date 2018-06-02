using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VolunteerCampaignWebsite.Models
{
    public class ViewModel
    {
        public IEnumerable<SINHVIEN> SINHVIEN { get; set; }
        public IEnumerable<GIANGVIEN> GIANGVIEN { get; set; }
        public IEnumerable<CHIENDICH> CHIENDICH { get; set; }
        public IEnumerable<CONGVIEC> CONGVIEC { get; set; }
        public IEnumerable<DOI> DOI { get; set; }
        public IEnumerable<CTCONGVIEC> CTCONGVIEC { get; set; }
        public IEnumerable<CHIPHIPHATSINH> CHIPHIPHATSINH { get; set; }
        public IEnumerable<PHIEUTHANHTOAN> PHIEUTHANHTOAN { get; set; }
        public IEnumerable<CHUCVU> CHUCVU { get; set; }
        public IEnumerable<NHANVIEN> NHANVIEN { get; set; }

    }
}