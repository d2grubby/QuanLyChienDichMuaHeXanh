using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.UI.HtmlControls;
using VolunteerCampaignWebsite.Models;  


namespace VolunteerCampaignWebsite.Controllers
{
    public class AdminController : Controller
    {
        CampaignDBDataContext db = new CampaignDBDataContext();
        
        
        // GET: Admin
        [HttpGet]
        public ActionResult Index()
        {
            if (Session["Taikhoan"] == null || Session["Taikhoan"].ToString() == "")
            {
                return RedirectToAction("LogIn");
            }
            NHANVIEN nv = (NHANVIEN)Session["Taikhoan"];
            GetUserRoles();
            return View(nv);
        }

        #region Login
        [HttpGet]
        public ActionResult LogIn()
        {
            var chucvu = db.CHUCVUs.ToList().OrderBy(n => n.Id);
            return View(chucvu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogIn(FormCollection collection, string submitButton)
        {
            var userid = collection["userID"];
            var userpwd = collection["userPWD"];
            var chucvu = collection["chucvu"];
            var name = collection["name"];
            var mail = collection["mail"];
            var phone = collection["phone"];
            var id = collection["username"];
            var pwd = collection["password"];
         
            NHANVIEN nv = db.NHANVIENs.SingleOrDefault(n => n.UserName == userid && n.Pwrd == userpwd);
            NHANVIEN nvregister = db.NHANVIENs.FirstOrDefault(n => n.UserName == id && n.Pwrd == pwd);
            var cvu = db.CHUCVUs.ToList().OrderBy(n => n.Id);

            switch (submitButton)
            {
                case "Login":
                    if (nv != null)
                    {
                        Session["Taikhoan"] = nv;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.Thongbao = "Ten dang nhap hoac mat khau khong dung";
                        return View(cvu);
                    }
                case "Register":
                    if (nvregister != null)
                    {
                        ViewBag.Thongbao = "Tai khoan da ton tai";
                        return View(cvu);

                    }
                    else
                    {
                        ViewBag.ThongBao = chucvu;
                        if (ModelState.IsValid)
                        {
                            CHUCVU cv = db.CHUCVUs.FirstOrDefault(n => n.Ten == chucvu);

                            NHANVIEN nvadd = new NHANVIEN();
                            nvadd.IdChucVu = cv.Id;
                            nvadd.Ten = name;
                            nvadd.Mail = mail;
                            nvadd.SDT = phone;
                            nvadd.UserName = id;
                            nvadd.Pwrd = pwd;

                            db.NHANVIENs.InsertOnSubmit(nvadd);
                            db.SubmitChanges();
                        }
                        return View(cvu);
                    }
                default:
                    return View(cvu);
            }
        }
        #endregion

        

        public void GetUserRoles()
        {
            NHANVIEN nv = (NHANVIEN)Session["Taikhoan"];
            if (nv != null)
            {
                var cv = db.CHUCVUs.SingleOrDefault(n => n.Id == nv.IdChucVu);

                this.ViewBag.Name = nv.Ten;
                if (cv.Ten == "Admin")
                {
                    this.ViewBag.IsAdmin = "Admin";
                    
                }
                else if (cv.Ten == "QTSV")
                {
                    this.ViewBag.IsAdmin = "QTSV";
                }
                else
                {
                    this.ViewBag.IsAdmin = "QTGV";
                }
            }
        }


        #region CHIENDICH
        [HttpGet]
        public ActionResult QLChiendich()
        {
            GetUserRoles();

            return View(db.CHIENDICHes.ToList().OrderBy(n => n.Id));       
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QLChiendich(string submitButton, CHIENDICH chiendich, FormCollection collection)
        {
            var id = collection["campaignID"];
            var name = collection["campaignName"];
            var reservation = collection["reservation"];
            var startday = reservation.Split('-')[0];
            var endday = reservation.Split('-')[1];
            var time = DateTime.Parse(endday) - DateTime.Parse(startday);
            var pneed = collection["campaignPneeded"];
            var pjoin = collection["campaignPjoined"];
            var meaning = collection["campaignMeaning"];
            var moreinfo = collection["campaignMoreinfo"];

            var numofteam = (int)(int.Parse(pneed) / 10);
            switch (submitButton)
            {
                case "Add":
                    if (ModelState.IsValid)
                    {
                        chiendich.Id = id;
                        chiendich.Ten = name;
                        chiendich.SoLuongNguoiCan = int.Parse(pneed);
                        chiendich.SoLuongNguoiThamGia = int.Parse(pjoin);
                        chiendich.NgayBatDau = DateTime.Parse(startday);
                        chiendich.ThoiGian = time.Days;
                        chiendich.YNghia = meaning;
                        chiendich.MoreInfo = moreinfo;

                        db.CHIENDICHes.InsertOnSubmit(chiendich);
                        db.SubmitChanges();

                        DOI doi = new DOI();
                        doi.Id = id + "0";
                        doi.IdCD = id;
                        doi.SoLuongThanhVien = 0;
                        doi.SoLuongThanhVienThamGia = 0;

                        db.DOIs.InsertOnSubmit(doi);
                        db.SubmitChanges();

                        PHIEUTHANHTOAN ptt = new PHIEUTHANHTOAN();
                        ptt.IdCD = id;
                        ptt.TongChiPhi = 0;
                        ptt.CHiPhiPhatSinh = 0;
                        ptt.ChiPhiDuDinh = 0;

                        db.PHIEUTHANHTOANs.InsertOnSubmit(ptt);
                        db.SubmitChanges();

                    }
                    return RedirectToAction("QLChiendich");
                case "Edit":
                    var cd = db.CHIENDICHes.SingleOrDefault(n => n.Id == id);
                    if (ModelState.IsValid)
                    {
                        
                        cd.Ten = name;
                        cd.SoLuongNguoiCan = int.Parse(pneed);
                        cd.SoLuongNguoiThamGia = int.Parse(pjoin);
                        cd.NgayBatDau = DateTime.Parse(startday);
                        cd.ThoiGian = time.Days;
                        cd.YNghia = meaning;
                        cd.MoreInfo = moreinfo;


                        UpdateModel(cd);
                        db.SubmitChanges();
                    }
                    return RedirectToAction("QLChiendich");
                case "Delete":
                    var cdDel = db.CHIENDICHes.SingleOrDefault(n => n.Id == id);
                    db.CHIENDICHes.DeleteOnSubmit(cdDel);
                    db.SubmitChanges();
                    return RedirectToAction("QLChiendich");
                default:
                    return View("QLChiendich");
            }
        }
        #endregion

        #region CONGVIEC

       
        [HttpGet]
        public ActionResult QLCongviec()
        {
            GetUserRoles();

            var cd = db.CHIENDICHes.ToList().OrderBy(n => n.Id);
            var cv = db.CONGVIECs.ToList().OrderBy(n => n.Id);
            var tuple = new Tuple<IEnumerable<CHIENDICH>, IEnumerable<CONGVIEC>>(cd, cv);
            return View(tuple);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QLCongviec(string submitButton, string showButton, CONGVIEC congviec, FormCollection collection)
        {
            var id = collection["jobID"];
            var idCD = collection["campaignID"];
            var name = collection["jobName"];
            var place = collection["jobPlace"];
            var detail = collection["jobDetails"];
            var pneed = collection["jobPneeded"];
            var reservation = collection["reservation"];
            var startday = reservation.Split('-')[0];
            var endday = reservation.Split('-')[1];
            var time = DateTime.Parse(endday) - DateTime.Parse(startday);
            var cost = collection["jobCost"];
            var moreinfo = collection["jobMoreinfo"];
            var numofteam = (int)(int.Parse(pneed) / 10);
            var totalFee = (float)(int.Parse(pneed) * float.Parse(cost));

            switch (submitButton)
            {
                case "Add":
                    if (ModelState.IsValid)
                    {
                        congviec.Id = id;
                        congviec.IdCD = idCD;
                        congviec.TenCV = name;
                        congviec.DiaDiem = place;
                        congviec.SoLuongNguoi = int.Parse(pneed);
                        congviec.NgayBatDau = DateTime.Parse(startday);
                        congviec.ThoiGian = time.Days;
                        congviec.ChiPhiMotNguoi = decimal.Parse(cost);
                        congviec.MoreInfo = moreinfo;
                        congviec.NoiDung = detail;

                        db.CONGVIECs.InsertOnSubmit(congviec);
                        db.SubmitChanges();

                        
                        for (int i = 1; i <= numofteam; i++)
                        {
                            DOI doi = new DOI();
                            doi.Id = id + "-" + i.ToString();
                            doi.IdCD = idCD;
                            doi.SoLuongThanhVienThamGia = 0;
                            if (i == numofteam)
                            {
                                doi.SoLuongThanhVien = int.Parse(pneed) - (10 * numofteam) + 10;
                            }
                            else { doi.SoLuongThanhVien = 10; }
                            db.DOIs.InsertOnSubmit(doi);
                            db.SubmitChanges();

                            CTCONGVIEC ctcv = new CTCONGVIEC();
                            ctcv.IdCV = id;
                            ctcv.IdDoi = id + "-" + i.ToString();
                            ctcv.KetQuaCV = '0';
                            db.CTCONGVIECs.InsertOnSubmit(ctcv);
                            db.SubmitChanges();                        
                        }

                        PHIEUTHANHTOAN ptt = db.PHIEUTHANHTOANs.FirstOrDefault(n => n.IdCD == idCD);
                        ptt.ChiPhiDuDinh += totalFee;
                        ptt.TongChiPhi = ptt.ChiPhiDuDinh + ptt.CHiPhiPhatSinh;
                        UpdateModel(ptt);
                        db.SubmitChanges();
                    }
                    return RedirectToAction("QLCongviec");
                case "Edit":
                    var cv = db.CONGVIECs.SingleOrDefault(n => n.Id == id);
                    if (ModelState.IsValid)
                    {
                        cv.IdCD = idCD;
                        cv.TenCV = name;
                        cv.DiaDiem = place;
                        cv.SoLuongNguoi = int.Parse(pneed);
                        cv.NgayBatDau = DateTime.Parse(startday);
                        cv.ThoiGian = time.Days;
                        cv.ChiPhiMotNguoi = decimal.Parse(cost);
                        cv.MoreInfo = moreinfo;
                        cv.NoiDung = detail;


                        UpdateModel(cv);
                        db.SubmitChanges();
                    }
                    return RedirectToAction("QLCongviec");
                case "Delete":
                    var cvDel = db.CONGVIECs.SingleOrDefault(n => n.Id == id);
                    db.CONGVIECs.DeleteOnSubmit(cvDel);
                    db.SubmitChanges();
                    return RedirectToAction("QLCongviec");
                default:
                    return RedirectToAction("QLCongviec"); 
            }
        }
        #endregion

        #region GIANGVIEN
        [HttpGet]
        public ActionResult QLGiangvien()
        {
            GetUserRoles();

            ViewModel vm = new ViewModel();
            vm.CHIENDICH = db.CHIENDICHes.ToList().OrderBy(n => n.Id);
            vm.GIANGVIEN = db.GIANGVIENs.ToList().OrderBy(n => n.Id);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QLGiangvien(string submitButton, GIANGVIEN giangvien, FormCollection collection)
        {
            var id = collection["participaterID"];
            var campaignid = collection["campaignID"];
            CHIENDICH cd = db.CHIENDICHes.FirstOrDefault(n => n.Id == campaignid);
            var sumofteam = (int)(cd.SoLuongNguoiCan / 10);

            var reservation = collection["reservation"];
            var name = collection["participaterName"];
            var birthday = collection["participaterBirthday"];
            var gender = collection["gender"];
            var phone = collection["participaterPhone"];

            var teacherFaculty = collection["facultySelect"];
            var teachertest = collection["teachertest"];

            var chiendich = db.CHIENDICHes.SingleOrDefault(n => n.Id == campaignid);

            switch (submitButton)
            {
                case "Add":
                    if (ModelState.IsValid)
                    {
                        giangvien.Id = id;
                        giangvien.IdCD = campaignid;
                        giangvien.IdDoi = campaignid + "0";
                        giangvien.Ten = name;
                        giangvien.NgaySinh = DateTime.Parse(birthday);
                        giangvien.GioiTinh = char.Parse(gender);
                        giangvien.SDT = phone;
                        giangvien.Khoa = teacherFaculty;


                        db.GIANGVIENs.InsertOnSubmit(giangvien);
                        db.SubmitChanges();

                        chiendich.SoLuongNguoiThamGia += 1;
                        UpdateModel(chiendich);
                        db.SubmitChanges();
                    }
                    return RedirectToAction("QLGiangvien");
                case "Edit":
                    if (ModelState.IsValid)
                    {
                        var gv = db.GIANGVIENs.SingleOrDefault(n => n.Id == id);

                        gv.Ten = name;
                        gv.NgaySinh = DateTime.Parse(birthday);
                        gv.GioiTinh = char.Parse(gender);
                        gv.SDT = phone;
                        gv.Khoa = teacherFaculty;

                        UpdateModel(gv);
                        db.SubmitChanges();
                        return RedirectToAction("QLGiangvien");
                    }
                    return RedirectToAction("QLGiangvien");
                case "Delete":
                    var gvdel = db.GIANGVIENs.SingleOrDefault(n => n.Id == id);
                    db.GIANGVIENs.DeleteOnSubmit(gvdel);
                    db.SubmitChanges();

                    chiendich.SoLuongNguoiThamGia -= 1;
                    UpdateModel(chiendich);
                    db.SubmitChanges();
                    return RedirectToAction("QLGiangvien");
                default:
                    return RedirectToAction("QLGiangvien");
            }
        }
        #endregion

        #region SINHVIEN
        [HttpGet]
        public ActionResult QLSinhvien()
        {
            GetUserRoles();

            ViewModel vm = new ViewModel();
            vm.CHIENDICH = db.CHIENDICHes.ToList().OrderBy(n => n.Id);
            vm.SINHVIEN = db.SINHVIENs.ToList().OrderBy(n => n.Id);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QLSinhvien(string submitButton, SINHVIEN sinhvien, FormCollection collection)
        {
            var id = collection["participaterID"];
            var campaignid = collection["campaignID"];
            CHIENDICH cd = db.CHIENDICHes.FirstOrDefault(n => n.Id == campaignid);
            var sumofteam = (int)(cd.SoLuongNguoiCan / 10);

            var reservation = collection["reservation"];
            var name = collection["participaterName"];
            var birthday = collection["participaterBirthday"];
            var gender = collection["gender"];
            var phone = collection["participaterPhone"];

            var studentclass = collection["studentClass"];
            var studentPjoin = collection["studentPartyJoinDay"];
            var studentdtb = collection["studentMark"];

            switch (submitButton)
            {
                case "Add":
                    if (ModelState.IsValid)
                    {
                        sinhvien.Id = id;
                        sinhvien.IdCD = campaignid;
                        sinhvien.IdDoi = campaignid + "0";
                        sinhvien.Ten = name;
                        sinhvien.NgaySinh = DateTime.Parse(birthday);
                        sinhvien.GioiTinh = char.Parse(gender);
                        sinhvien.SDT = phone;
                        sinhvien.Lop = studentclass;
                        sinhvien.NgayVaoDoan = DateTime.Parse(studentPjoin);
                        sinhvien.DiemTB = float.Parse(studentdtb);

                        db.SINHVIENs.InsertOnSubmit(sinhvien);
                        db.SubmitChanges();
                    }
                    return RedirectToAction("QLSinhvien");
                case "Edit":
                    if (ModelState.IsValid)
                    {
                        var sv = db.SINHVIENs.SingleOrDefault(n => n.Id == id);

                        sv.Ten = name;
                        sv.NgaySinh = DateTime.Parse(birthday);
                        sv.GioiTinh = char.Parse(gender);
                        sv.SDT = phone;
                        sv.Lop = studentclass;
                        sv.NgayVaoDoan = DateTime.Parse(studentPjoin);
                        sv.DiemTB = float.Parse(studentdtb);

                        UpdateModel(sv);
                        db.SubmitChanges();
                        return RedirectToAction("QLSinhvien");
                    }
                    return RedirectToAction("QLSinhvien");
                case "Delete":
                    var svDel = db.SINHVIENs.SingleOrDefault(n => n.Id == id);
                    db.SINHVIENs.DeleteOnSubmit(svDel);
                    db.SubmitChanges();
                    return RedirectToAction("QLSinhvien");
                default:
                    return RedirectToAction("QLSinhvien");
            }
        }
        #endregion

        #region NGUOITHAMGIA
        [HttpGet]
        public ActionResult QLNguoithamgia()
        {
            GetUserRoles();
            var cd = db.CHIENDICHes.ToList().OrderBy(n => n.Id);
            var sv = db.SINHVIENs.ToList().OrderBy(n => n.Id);
            var gv = db.GIANGVIENs.ToList().OrderBy(n => n.Id);
            var tuple = new Tuple<IEnumerable<CHIENDICH>, IEnumerable<SINHVIEN>, IEnumerable<GIANGVIEN>>(cd, sv, gv);
            return View(tuple);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QLNguoithamgia(string submitButton, SINHVIEN sinhvien, GIANGVIEN giangvien, FormCollection collection)
        {
            var id = collection["participaterID"];
            var campaignid = collection["campaignID"];
            CHIENDICH cd = db.CHIENDICHes.FirstOrDefault(n => n.Id == campaignid);
            var sumofteam = (int)(cd.SoLuongNguoiCan / 10);
           
            var chucvu = collection["positionSelect"];
            var reservation = collection["reservation"];
            var name = collection["participaterName"];
            var birthday = collection["participaterBirthday"];
            var gender = collection["gender"];
            var phone = collection["participaterPhone"];
                                        
            var teacherFaculty = collection["facultySelect"];
            var teachertest = collection["teachertest"];
           
            var studentclass = collection["studentClass"];
            var studentPjoin = collection["studentPartyJoinDay"];
            var studentdtb = collection["studentMark"];

            ViewBag.ThongBao = gender + " " + chucvu;
            switch (submitButton)
            {
                case "Add":
                    if (ModelState.IsValid)
                    {
                        if (chucvu == "Giáo viên")
                        {
                            giangvien.Id = id;
                            giangvien.IdCD = campaignid;
                            giangvien.IdDoi = campaignid + "0";
                            giangvien.Ten = name;
                            giangvien.NgaySinh = DateTime.Parse(birthday);
                            giangvien.GioiTinh = char.Parse(gender);
                            giangvien.SDT = phone;
                            giangvien.Khoa = teacherFaculty;
                           

                            db.GIANGVIENs.InsertOnSubmit(giangvien);
                            db.SubmitChanges();

                        }
                        if (chucvu == "Sinh viên")
                        {
                            sinhvien.Id = id;
                            sinhvien.IdCD = campaignid;
                            sinhvien.IdDoi = campaignid + "0";
                            sinhvien.Ten = name;
                            sinhvien.NgaySinh = DateTime.Parse(birthday);
                            sinhvien.GioiTinh = char.Parse(gender);
                            sinhvien.SDT = phone;
                            sinhvien.Lop = studentclass;
                            sinhvien.NgayVaoDoan = DateTime.Parse(studentPjoin);
                            sinhvien.DiemTB = float.Parse(studentdtb);
                        
                            db.SINHVIENs.InsertOnSubmit(sinhvien);
                            db.SubmitChanges();
                         
                        }          
                    }
                    return RedirectToAction("QLNguoithamgia");
                case "Edit":
                    if (ModelState.IsValid)
                    {
                        if (chucvu == "Giáo viên")
                        {
                            var gv = db.GIANGVIENs.SingleOrDefault(n => n.Id == id);

                            gv.Ten = name;
                            gv.NgaySinh = DateTime.Parse(birthday);
                            gv.GioiTinh = char.Parse(gender);
                            gv.SDT = phone;
                            gv.Khoa = teacherFaculty;

                            UpdateModel(gv);
                            db.SubmitChanges();
                            return RedirectToAction("QLNguoithamgia");
                        }
                        if (chucvu == "Sinh viên")
                        {
                            var sv = db.SINHVIENs.SingleOrDefault(n => n.Id == id);

                            sv.Ten = name;
                            sv.NgaySinh = DateTime.Parse(birthday);
                            sv.GioiTinh = char.Parse(gender);
                            sv.SDT = phone;
                            sv.Lop = studentclass;
                            sv.NgayVaoDoan = DateTime.Parse(studentPjoin);
                            sv.DiemTB = float.Parse(studentdtb);

                            UpdateModel(sv);
                            db.SubmitChanges();
                            return RedirectToAction("QLNguoithamgia");
                        }

                    }
                    return RedirectToAction("QLNguoithamgia");
                case "Delete":
                    if (chucvu == "Giáo viên")
                    {
                        var gvdel = db.GIANGVIENs.SingleOrDefault(n => n.Id == id);
                        db.GIANGVIENs.DeleteOnSubmit(gvdel);
                        db.SubmitChanges();
                        return RedirectToAction("QLNguoithamgia");
                    }
                    if (chucvu == "Sinh viên")
                    {
                        var svDel = db.SINHVIENs.SingleOrDefault(n => n.Id == id);
                        db.SINHVIENs.DeleteOnSubmit(svDel);
                        db.SubmitChanges();
                        return RedirectToAction("QLNguoithamgia");
                    }
                   
                    return RedirectToAction("QLNguoithamgia");
                default:
                    return View("QLNguoithamgia");
            }
          
        }
        #endregion

        #region NGUOIDUNG
        [HttpGet]
        public ActionResult QLNguoidung()
        {
            GetUserRoles();

            ViewModel vm = new ViewModel();
            vm.CHUCVU = db.CHUCVUs.ToList().OrderBy(n => n.Id);
            vm.NHANVIEN = db.NHANVIENs.ToList().OrderBy(n => n.Id);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QLNguoidung(string submitButton, NHANVIEN nhanvien, FormCollection collection)
        {
            ViewModel vm = new ViewModel();
            vm.CHUCVU = db.CHUCVUs.ToList().OrderBy(n => n.Id);
            vm.NHANVIEN = db.NHANVIENs.ToList().OrderBy(n => n.Id);

            var id = collection["employeeID"];
            var ten = collection["employeeName"];
            var chucvu = collection["positionSelect"];
            var idchucvu = db.CHUCVUs.FirstOrDefault(n => n.Ten == chucvu).Id;
            var email = collection["employeeMail"];
            var sdt = collection["employeePhone"];
            var username = collection["employeeUsername"];
            var password = collection["employeePassword"];

            NHANVIEN nv = db.NHANVIENs.FirstOrDefault(n => n.UserName == username && n.Pwrd == password);

            switch (submitButton)
            {
                case "Add":
                    if (ModelState.IsValid)
                    {
                        if (nv != null)
                        {
                            ViewBag.ThongBao = "Tài khoản đã tồn tại.";
                            return View(vm);
                        }
                        else
                        {
                            nhanvien.Ten = ten;
                            nhanvien.IdChucVu = idchucvu;
                            nhanvien.Mail = email;
                            nhanvien.SDT = sdt;
                            nhanvien.UserName = username;
                            nhanvien.Pwrd = password;

                            db.NHANVIENs.InsertOnSubmit(nhanvien);
                            db.SubmitChanges();
                        }
                    }
                    return RedirectToAction("QLNguoidung");
                case "Edit":
                    if (ModelState.IsValid)
                    {
                        var nvien = db.NHANVIENs.SingleOrDefault(n => n.Id == int.Parse(id));
                        nvien.Ten = ten;
                        nvien.IdChucVu = idchucvu;
                        nvien.Mail = email;
                        nvien.SDT = sdt;
                        nvien.UserName = username;
                        nvien.Pwrd = password;

                        UpdateModel(nvien);
                        db.SubmitChanges();
                        return RedirectToAction("QLNguoidung");
                    }
                    return RedirectToAction("QLNguoidung");
                case "Delete":
                    var nvDel = db.NHANVIENs.SingleOrDefault(n => n.Id == int.Parse(id));
                    db.NHANVIENs.DeleteOnSubmit(nvDel);
                    db.SubmitChanges();
                    return RedirectToAction("QLNguoidung");
                default:
                    return RedirectToAction("QLNguoidung");
            }
        }
        #endregion

        #region CHUCVU
        [HttpGet]
        public ActionResult QLChucvu()
        {
            GetUserRoles();

            var cv = db.CHUCVUs.ToList().OrderBy(n => n.Id);
            return View(cv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QLChucvu(string submitButton, CHUCVU chucvu, FormCollection collection)
        {
            var cv = db.CHUCVUs.ToList().OrderBy(n => n.Id);

            var id = collection["positionID"];
            var ten = collection["positionName"];
            var luong = collection["positionSalary"];
            var quyenhan = collection["positionPower"];

            CHUCVU cvu = db.CHUCVUs.FirstOrDefault(n => n.Ten == ten);

            switch (submitButton)
            {
                case "Add":
                    if (ModelState.IsValid)
                    {
                        if (cvu != null)
                        {
                            ViewBag.ThongBao = "Chức vụ đã tồn tại.";
                            return View(cv);
                        }
                        else
                        {
                            chucvu.Ten = ten;
                            chucvu.Luong = double.Parse(luong);
                            chucvu.QuyenHan = quyenhan;

                            db.CHUCVUs.InsertOnSubmit(chucvu);
                            db.SubmitChanges();
                        }
                    }
                    return RedirectToAction("QLChucvu");
                case "Edit":
                    if (ModelState.IsValid)
                    {
                        var chucv = db.CHUCVUs.SingleOrDefault(n => n.Id == int.Parse(id));
                        chucv.Ten = ten;
                        chucv.Luong = double.Parse(luong);
                        chucv.QuyenHan = quyenhan;

                        UpdateModel(chucv);
                        db.SubmitChanges();
                        return RedirectToAction("QLChucvu");
                    }
                    return RedirectToAction("QLChucvu");
                case "Delete":
                    var cvDel = db.CHUCVUs.SingleOrDefault(n => n.Id == int.Parse(id));
                    db.CHUCVUs.DeleteOnSubmit(cvDel);
                    db.SubmitChanges();
                    return RedirectToAction("QLChucvu");
                default:
                    return RedirectToAction("QLChucvu");
            }
        }
        #endregion

        #region PHANCHIACONGVIEC
        [HttpGet]
        public ActionResult Phanchiacongviec()
        {
            GetUserRoles();

            ViewModel vm = new ViewModel();
            
            vm.CHIENDICH = db.CHIENDICHes.ToList().OrderBy(n => n.Id);
            vm.CONGVIEC = db.CONGVIECs.ToList().OrderBy(n => n.Id);
            vm.SINHVIEN = db.SINHVIENs.ToList().OrderBy(n => n.Id);
            vm.GIANGVIEN = db.GIANGVIENs.ToList().OrderBy(n => n.Id);
            vm.DOI = db.DOIs.ToList().OrderBy(n => n.Id);
            vm.CTCONGVIEC = db.CTCONGVIECs.ToList().OrderBy(n => n.IdDoi);
            
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Phanchiacongviec(string submitButton, FormCollection collection)
        {
            var participaterid = collection["participaterID"];
            var campaignid = collection["campaignID"];
            var teamid = collection["teamID"];
            var jobid = collection["jobID"];

            var pjoin = collection["participaterjoin"];
            var pneed = collection["participaterneed"];
            var pname = collection["participaterName"];
            var pposition = collection["paticipaterPosition"];
            var pphone = collection["participaterPhone"];
            var gender = collection["gender"];
            var birthday = collection["participaterBirthday"];
            var chucvu = collection["paticipaterPosition"];
           
            switch (submitButton)
            {
                case "Add":
               
                    if (ModelState.IsValid)
                    {
                        if (chucvu == "Giáo viên")
                        {
                            var gv = db.GIANGVIENs.SingleOrDefault(n => n.Id == participaterid);
                            gv.IdDoi = teamid;

                            UpdateModel(gv);
                            db.SubmitChanges();

                            var doi = db.DOIs.SingleOrDefault(n => n.Id == teamid);
                            doi.SoLuongThanhVienThamGia += 1;

                            UpdateModel(doi);
                            db.SubmitChanges();
                            return RedirectToAction("Phanchiacongviec");
                        }
                        if (chucvu == "Sinh viên")
                        {
                            var sv = db.SINHVIENs.SingleOrDefault(n => n.Id == participaterid);
                            sv.IdDoi = teamid;

                            UpdateModel(sv);
                            db.SubmitChanges();

                            var doi = db.DOIs.SingleOrDefault(n => n.Id == teamid);
                            doi.SoLuongThanhVienThamGia += 1;

                            UpdateModel(doi);
                            db.SubmitChanges();
                            return RedirectToAction("Phanchiacongviec");
                        }
                    }
                    return RedirectToAction("Phanchiacongviec");

                default:
                    return View("Phanchiacongviec");
            }
        }
        #endregion

        #region TINHCHIPHI
        [HttpGet]
        public ActionResult TinhChiPhi()
        {
            GetUserRoles();

            ViewModel vm = new ViewModel();

            vm.CHIENDICH = db.CHIENDICHes.ToList().OrderBy(n => n.Id);
            vm.CONGVIEC = db.CONGVIECs.ToList().OrderBy(n => n.Id);
            vm.SINHVIEN = db.SINHVIENs.ToList().OrderBy(n => n.Id);
            vm.GIANGVIEN = db.GIANGVIENs.ToList().OrderBy(n => n.Id);
            vm.DOI = db.DOIs.ToList().OrderBy(n => n.Id);
            vm.CTCONGVIEC = db.CTCONGVIECs.ToList().OrderBy(n => n.IdDoi);
            vm.CHIPHIPHATSINH = db.CHIPHIPHATSINHs.ToList().OrderBy(n => n.Id);
            vm.PHIEUTHANHTOAN = db.PHIEUTHANHTOANs.ToList().OrderBy(n => n.Id);


            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TinhChiPhi(string submitButton, FormCollection collection)
        {
            var id = collection["feeID"];
            var idCV = collection["jobID"];
            var idCD = collection["campaignID"];
            var name = collection["feeName"];
            var meaning = collection["feeMeaning"];
            var certificate = collection["feeCertificate"];
            var price = collection["feePrice"];


            switch (submitButton)
            {
                case "Add":
                    if (ModelState.IsValid)
                    {
                        PHIEUTHANHTOAN ptt = db.PHIEUTHANHTOANs.FirstOrDefault(n => n.IdCD == idCD);

                        CHIPHIPHATSINH cpps = new CHIPHIPHATSINH();
                        cpps.IdCV = idCV;
                        cpps.IdPhieuThanhToan = ptt.Id;
                        cpps.Ten = name;
                        cpps.LyDo = meaning;
                        cpps.ChungTu = certificate;
                        cpps.DonGia = double.Parse(price);

                        db.CHIPHIPHATSINHs.InsertOnSubmit(cpps);
                        db.SubmitChanges();

                        ptt.CHiPhiPhatSinh += double.Parse(price);
                        ptt.TongChiPhi = ptt.ChiPhiDuDinh + ptt.CHiPhiPhatSinh;

                        UpdateModel(ptt);
                        db.SubmitChanges();

                    }
                    return RedirectToAction("TinhChiPhi");
                case "Edit":
                    var cppsedit = db.CHIPHIPHATSINHs.SingleOrDefault(n => n.Id == int.Parse(id));

                    if (ModelState.IsValid)
                    {
                        PHIEUTHANHTOAN ptt = db.PHIEUTHANHTOANs.FirstOrDefault(n => n.IdCD == idCD);

                        cppsedit.IdCV = idCV;
                        cppsedit.IdPhieuThanhToan = ptt.Id;
                        cppsedit.Ten = name;
                        cppsedit.LyDo = meaning;
                        cppsedit.ChungTu = certificate;
                        cppsedit.DonGia = double.Parse(price);

                        UpdateModel(cppsedit);
                        db.SubmitChanges();

                        ptt.CHiPhiPhatSinh += double.Parse(price);
                        ptt.TongChiPhi = ptt.ChiPhiDuDinh + ptt.CHiPhiPhatSinh;

                        UpdateModel(ptt);
                        db.SubmitChanges();
                    }
                    return RedirectToAction("TinhChiPhi");
                case "Delete":
                    var cppsdel = db.CHIPHIPHATSINHs.SingleOrDefault(n => n.Id == int.Parse(id));

                    db.CHIPHIPHATSINHs.DeleteOnSubmit(cppsdel);
                    db.SubmitChanges();
                    return RedirectToAction("TinhChiPhi");
                default:
                    return RedirectToAction("TinhChiPhi");
            }
        }

        #endregion

        #region DANHGIACONGVIEC
        [HttpGet]
        public ActionResult Danhgiacongviec()
        {
            GetUserRoles();

            ViewModel vm = new ViewModel();
            vm.CHIENDICH = db.CHIENDICHes.ToList().OrderBy(n => n.Id);
            vm.CONGVIEC = db.CONGVIECs.ToList().OrderBy(n => n.Id);
            vm.CTCONGVIEC = db.CTCONGVIECs.ToList().OrderBy(n => n.IdCV);
            vm.DOI = db.DOIs.ToList().OrderBy(n => n.Id);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Danhgiacongviec(string submitButton, FormCollection collection)
        {
            var idDoi = collection["teamID"];
            var idCV = collection["jobID"];
            var idCD = collection["campaignID"];
            var result = collection["jobResult"];
            var pictures = collection["jobPictures"];
         


            switch (submitButton)
            {
                case "Add":
                    if (ModelState.IsValid)
                    {
                        ViewBag.ThongBao = pictures;
                        CTCONGVIEC ctcv = db.CTCONGVIECs.FirstOrDefault(n => n.IdCV == idCV);
                        ctcv.KetQuaCV = result == "Hoàn thành" ? '1' : '0';
                        ctcv.HinhAnh = pictures;

                        UpdateModel(ctcv);
                        db.SubmitChanges();

                    }
                    return RedirectToAction("Danhgiacongviec");
                case "Edit":
                    CTCONGVIEC ctcvedit = db.CTCONGVIECs.FirstOrDefault(n => n.IdCV == idCV);

                    if (ModelState.IsValid)
                    {
                        ctcvedit.KetQuaCV = result == "Hoàn thành" ? '1' : '0';
                        ctcvedit.HinhAnh = pictures;

                        UpdateModel(ctcvedit);
                        db.SubmitChanges();
                    }
                    return RedirectToAction("Danhgiacongviec");
                case "Delete":
                    CTCONGVIEC ctcvdel = db.CTCONGVIECs.FirstOrDefault(n => n.IdCV == idCV);

                    db.CTCONGVIECs.DeleteOnSubmit(ctcvdel);
                    db.SubmitChanges();
                    return RedirectToAction("Danhgiacongviec");
                default:
                    return RedirectToAction("Danhgiacongviec");
            }
        }
        #endregion

        #region SUATHONGTINCANHAN
        [HttpGet]
        public ActionResult Suathongtincanhan(string name)
        {
            GetUserRoles();

            var nv = db.NHANVIENs.FirstOrDefault(n => n.Ten == name);
            if (nv == null)
            {
                return RedirectToAction("LogIn");
            }
            return View(nv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Suathongtincanhan(string submitButton, NHANVIEN nhanvien, FormCollection collection)
        {
            var id = collection["employeeID"];
            var ten = collection["employeeName"];
            var mail = collection["employeeMail"];
            var sdt = collection["employeeSDT"];
            var username = collection["employeeUsername"];
            var oldpassword = collection["employeeOldPwrd"];
            var newpassword = collection["employeeNewPwrd"];

            switch (submitButton)
            {
                case "Edit":
                    if (ModelState.IsValid)
                    {
                        var nv = db.NHANVIENs.SingleOrDefault(n => n.Id == int.Parse(id));

                        if (nv.Pwrd != oldpassword)
                        {
                            ViewBag.Passworderror = "Nhập sai mật khẩu";
                            return View(nv);
                        }
                        else
                        {
                            nv.Ten = ten;
                            nv.Mail = mail;
                            nv.SDT = sdt;
                            nv.UserName = username;
                            nv.Pwrd = newpassword;

                            UpdateModel(nv);
                            db.SubmitChanges();
                        }
                        return RedirectToAction("Index");
                    }
                    return RedirectToAction("Index");
                default:
                    return RedirectToAction("Index");
            }
        }
        #endregion

        #region DSCHIENDICH
        public ActionResult DSChiendich()
        {
            GetUserRoles();

            var cd = db.CHIENDICHes.ToList().OrderBy(n => n.Id);
            return View(cd);
        }
        #endregion

        #region DSCONGVIEC
        public ActionResult DSCongviec()
        {
            GetUserRoles();

            var cv = db.CONGVIECs.ToList().OrderBy(n => n.Id);
            return View(cv);
        }
        #endregion

        #region DSNGUOITHAMGIA
        public ActionResult DSNguoithamgia()
        {
            GetUserRoles();

            var sv = db.SINHVIENs.ToList().OrderBy(n => n.Id);
            var gv = db.GIANGVIENs.ToList().OrderBy(n => n.Id);
            var tuple = new Tuple<IEnumerable<SINHVIEN>, IEnumerable<GIANGVIEN>>(sv, gv);
            return View(tuple);
        }
        #endregion

        #region DSGIANGVIEN
        public ActionResult DSGiangvien()
        {
            GetUserRoles();

            var gv = db.GIANGVIENs.ToList().OrderBy(n => n.Id);
            return View(gv);
        }
        #endregion

        #region DSSINHVIEN
        public ActionResult DSSinhvien()
        {
            GetUserRoles();

            var sv = db.SINHVIENs.ToList().OrderBy(n => n.Id);
            return View(sv);
        }
        #endregion

        #region DSCHIPHI
        public ActionResult DSChiphi()
        {
            GetUserRoles();

            var cp = db.PHIEUTHANHTOANs.ToList().OrderBy(n => n.Id);
            return View(cp);
        }
        #endregion
    }
}