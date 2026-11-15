
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;

namespace BonsaiShop.Controllers
{
    public class HomeController : Controller
    {
        private BonsaiShopEntities db = new BonsaiShopEntities();

        // Trang chủ
        public ActionResult Index()
        {
            // Lấy danh sách danh mục
            ViewBag.Categories = db.Categories.ToList();

            // Lấy danh sách sản phẩm
            var products = db.Products
                             .Include("Category")
                             .ToList();

            return View(products);
        }

        // Trang giới thiệu
        public ActionResult About()
        {
            ViewBag.Message = "Giới thiệu về Bonsai Shop";
            return View();
        }

        // Trang liên hệ
        public ActionResult Contact()
        {
            ViewBag.Message = "Thông tin liên hệ Bonsai Shop";
            return View();
        }
    }
}
