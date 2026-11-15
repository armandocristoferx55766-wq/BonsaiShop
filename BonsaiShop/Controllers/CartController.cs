
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BonsaiShop.Models;

namespace BonsaiShop.Controllers
{
    public class CartController : Controller
    {
        // Kiểm tra người dùng đã đăng nhập chưa
        private bool IsLoggedIn()
        {
            return Session["UserID"] != null;
        }

        // GET: Cart
        public ActionResult Index()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            List<CartItem> cart =
                Session["Cart"] as List<CartItem> ?? new List<CartItem>();

            return View(cart);
        }

        // Thêm sản phẩm vào giỏ hàng
        public ActionResult AddToCart(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new BonsaiShopEntities())
            {
                var product = db.Products.Find(id);

                if (product == null)
                    return HttpNotFound();

                if (product.Quantity <= 0)
                {
                    TempData["CartMessage"] = "Sản phẩm đã hết hàng!";
                    return RedirectToAction("Details", "Products", new { id = id });
                }

                List<CartItem> cart =
                    Session["Cart"] as List<CartItem> ?? new List<CartItem>();

                var item = cart.FirstOrDefault(p => p.ProductID == id);

                if (item != null)
                {
                    if (item.Quantity >= product.Quantity)
                    {
                        TempData["CartMessage"] =
                            "Số lượng sản phẩm vượt quá hàng tồn kho!";
                        return RedirectToAction("Index");
                    }

                    item.Quantity++;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        Image = product.Image,
                        Price = product.Price,
                        Quantity = 1
                    });
                }

                Session["Cart"] = cart;
            }

            return RedirectToAction("Index");
        }

        // Cập nhật số lượng
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            List<CartItem> cart =
                Session["Cart"] as List<CartItem> ?? new List<CartItem>();

            var item = cart.FirstOrDefault(p => p.ProductID == id);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    using (var db = new BonsaiShopEntities())
                    {
                        var product = db.Products.Find(id);

                        if (product == null)
                        {
                            cart.Remove(item);
                        }
                        else if (quantity <= product.Quantity)
                        {
                            item.Quantity = quantity;
                        }
                        else
                        {
                            TempData["CartMessage"] =
                                "Số lượng vượt quá hàng tồn kho!";
                        }
                    }
                }
            }

            Session["Cart"] = cart;

            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public ActionResult RemoveFromCart(int id)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            List<CartItem> cart =
                Session["Cart"] as List<CartItem> ?? new List<CartItem>();

            var item = cart.FirstOrDefault(p => p.ProductID == id);

            if (item != null)
                cart.Remove(item);

            Session["Cart"] = cart;

            return RedirectToAction("Index");
        }
    }
}
