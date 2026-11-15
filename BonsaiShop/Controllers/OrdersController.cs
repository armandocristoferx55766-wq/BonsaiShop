
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BonsaiShop.Models;

namespace BonsaiShop.Controllers
{
    public class OrdersController : Controller
    {
        private BonsaiShopEntities db = new BonsaiShopEntities();

        // Kiểm tra đăng nhập
        private bool IsLoggedIn()
        {
            return Session["UserID"] != null;
        }

        // GET: Orders/Checkout
        public ActionResult Checkout()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            List<CartItem> cart =
                Session["Cart"] as List<CartItem> ?? new List<CartItem>();

            if (!cart.Any())
            {
                TempData["CartMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            var user = db.Users.Find(userId);

            if (user == null)
            {
                Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var model = new CheckoutViewModel
            {
                FullName = user.FullName,
                Phone = user.Phone,
                Address = user.Address,
                CartItems = cart,
                GrandTotal = cart.Sum(item => item.Total)
            };

            return View(model);
        }

        // POST: Orders/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            List<CartItem> cart =
                Session["Cart"] as List<CartItem> ?? new List<CartItem>();

            if (!cart.Any())
            {
                TempData["CartMessage"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            var user = db.Users.Find(userId);

            if (user == null)
            {
                Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    decimal total = 0;

                    // Kiểm tra lại hàng tồn kho
                    foreach (var item in cart)
                    {
                        var product = db.Products.Find(item.ProductID);

                        if (product == null ||
                            item.Quantity <= 0 ||
                            product.Quantity < item.Quantity)
                        {
                            transaction.Rollback();

                            TempData["OrderMessage"] =
                                "Sản phẩm không đủ số lượng trong kho!";

                            return RedirectToAction("Index", "Cart");
                        }

                        total += product.Price * item.Quantity;
                    }

                    // Tạo đơn hàng
                    var order = new Order
                    {
                        UserID = userId,
                        OrderDate = DateTime.Now,
                        TotalAmount = total,
                        ShippingAddress = user.Address,
                        Phone = user.Phone,
                        Status = "Chờ xác nhận"
                    };

                    db.Orders.Add(order);
                    db.SaveChanges();

                    // Lưu chi tiết đơn hàng và trừ tồn kho
                    foreach (var item in cart)
                    {
                        var product = db.Products.Find(item.ProductID);

                        var detail = new OrderDetail
                        {
                            OrderID = order.OrderID,
                            ProductID = product.ProductID,
                            Quantity = item.Quantity,
                            UnitPrice = product.Price
                        };

                        db.OrderDetails.Add(detail);

                        product.Quantity -= item.Quantity;
                    }

                    db.SaveChanges();

                    transaction.Commit();

                    // Xóa giỏ hàng sau khi đặt hàng thành công
                    Session["Cart"] = null;

                    return RedirectToAction("Confirmation",
                        new { id = order.OrderID });
                }
                catch
                {
                    transaction.Rollback();

                    TempData["OrderMessage"] =
                        "Đặt hàng thất bại! Vui lòng thử lại.";

                    return RedirectToAction("Checkout");
                }
            }
        }

        // GET: Orders/Confirmation
        public ActionResult Confirmation(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            var order = db.Orders
                .FirstOrDefault(o => o.OrderID == id && o.UserID == userId);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }
    }
}
