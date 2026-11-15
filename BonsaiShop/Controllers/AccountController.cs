
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using System.Web.Mvc;

namespace BonsaiShop.Controllers
{
    public class AccountController : Controller
    {
        private BonsaiShopEntities db = new BonsaiShopEntities();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user)
        {
            // Kiểm tra họ tên
            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                ModelState.AddModelError(
                    "FullName",
                    "Vui lòng nhập họ tên!"
                );
            }
            else if (user.FullName.Trim().Length < 2)
            {
                ModelState.AddModelError(
                    "FullName",
                    "Họ tên phải có ít nhất 2 ký tự!"
                );
            }

            // Kiểm tra Email
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ModelState.AddModelError(
                    "Email",
                    "Vui lòng nhập email!"
                );
            }
            else if (!Regex.IsMatch(
                user.Email.Trim(),
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError(
                    "Email",
                    "Email không đúng định dạng!"
                );
            }
            else if (db.Users.Any(u => u.Email == user.Email.Trim()))
            {
                ModelState.AddModelError(
                    "Email",
                    "Email này đã được đăng ký!"
                );
            }

            // Kiểm tra mật khẩu
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                ModelState.AddModelError(
                    "PasswordHash",
                    "Vui lòng nhập mật khẩu!"
                );
            }
            else if (user.PasswordHash.Length < 6)
            {
                ModelState.AddModelError(
                    "PasswordHash",
                    "Mật khẩu phải có ít nhất 6 ký tự!"
                );
            }

            // Kiểm tra số điện thoại
            if (string.IsNullOrWhiteSpace(user.Phone))
            {
                ModelState.AddModelError(
                    "Phone",
                    "Vui lòng nhập số điện thoại!"
                );
            }
            else if (!Regex.IsMatch(user.Phone.Trim(), @"^0\d{9}$"))
            {
                ModelState.AddModelError(
                    "Phone",
                    "Số điện thoại phải có đúng 10 chữ số và bắt đầu bằng số 0!"
                );
            }

            // Kiểm tra địa chỉ
            if (string.IsNullOrWhiteSpace(user.Address))
            {
                ModelState.AddModelError(
                    "Address",
                    "Vui lòng nhập địa chỉ!"
                );
            }

            // Lưu tài khoản nếu dữ liệu hợp lệ
            if (ModelState.IsValid)
            {
                user.FullName = user.FullName.Trim();
                user.Email = user.Email.Trim();
                user.Phone = user.Phone.Trim();
                user.Address = user.Address.Trim();

                // Mã hóa mật khẩu
                user.PasswordHash = Crypto.HashPassword(user.PasswordHash);

                // Phân quyền khách hàng
                user.Role = "Customer";

                db.Users.Add(user);
                db.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(user);
        }


        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            // Kiểm tra thông tin nhập vào
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập email và mật khẩu!");
                return View();
            }

            // Tìm tài khoản theo email
            email = email.Trim();

            var user = db.Users.FirstOrDefault(u => u.Email == email);

            // Kiểm tra tài khoản và mật khẩu
            if (user == null ||
                !Crypto.VerifyHashedPassword(user.PasswordHash, password))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác!");
                return View();
            }

            // Lưu thông tin người dùng vào Session
            Session["UserID"] = user.UserID;
            Session["FullName"] = user.FullName;
            Session["Role"] = user.Role;

            // Đăng nhập thành công
            return RedirectToAction("Index", "Home");

            //db.Users: tìm tài khoản trong bảng Users.
            //Crypto.VerifyHashedPassword: so sánh mật khẩu nhập vào với mật khẩu đã mã hóa trong database.
            //Session["UserID"]: lưu mã người dùng.
            //Session["FullName"]: lưu tên người dùng.
            //Session["Role"]: lưu quyền Admin hoặc Customer.
            //RedirectToAction: chuyển về trang chủ sau khi đăng nhập
        }


        // GET: Account/Logout
        public ActionResult Logout()
        {
            // Xóa toàn bộ thông tin đăng nhập
            Session.Clear();
            Session.Abandon();

            // Quay về trang chủ
            return RedirectToAction("Index", "Home");

            //Session.Clear(): xóa thông tin người dùng đang lưu.
            //Session.Abandon(): kết thúc phiên đăng nhập.
            //RedirectToAction: chuyển người dùng về trang chủ.
        }


    }
}
