
using System;
using System.IO;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;

namespace BonsaiShop.Controllers
{
    public class ProductsController : Controller
    {
        private BonsaiShopEntities db = new BonsaiShopEntities();

        // GET: Products
        public ActionResult Index(
            string searchString,
            string sortOrder,
            int? page,
            int? categoryId)
        {
            IQueryable<Product> products =
                db.Products.Include("Category");

            if (categoryId.HasValue)
            {
                products = products.Where(p =>
                    p.CategoryID == categoryId.Value);

                var category = db.Categories.Find(categoryId.Value);

                ViewBag.CategoryName = category != null
                    ? category.CategoryName
                    : "Danh mục";
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(searchString));
            }

            ViewBag.NameSortParm =
                sortOrder == "name" ? "name_desc" : "name";

            ViewBag.PriceSortParm =
                sortOrder == "price" ? "price_desc" : "price";

            switch (sortOrder)
            {
                case "name":
                    products = products.OrderBy(p => p.ProductName);
                    break;

                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;

                case "price":
                    products = products.OrderBy(p => p.Price);
                    break;

                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;

                default:
                    products = products.OrderBy(p => p.ProductID);
                    break;
            }

            int pageSize = 5;
            int pageNumber = page ?? 1;

            int totalProducts = products.Count();

            int totalPages = (int)Math.Ceiling(
                (double)totalProducts / pageSize);

            var productList = products
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchString = searchString;
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CategoryId = categoryId;

            return View(productList);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName"
            );

            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product, HttpPostedFileBase ImageFile)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                ModelState.AddModelError(
                    "ProductName",
                    "Vui lòng nhập tên sản phẩm!"
                );
            }

            if (product.Price < 0)
            {
                ModelState.AddModelError(
                    "Price",
                    "Giá sản phẩm không được âm!"
                );
            }

            if (product.Quantity < 0)
            {
                ModelState.AddModelError(
                    "Quantity",
                    "Số lượng không được âm!"
                );
            }

            // Kiểm tra ảnh
            if (ImageFile == null || ImageFile.ContentLength == 0)
            {
                ModelState.AddModelError(
                    "ImageFile",
                    "Vui lòng chọn hình ảnh sản phẩm!"
                );
            }
            else
            {
                string extension = Path.GetExtension(ImageFile.FileName);

                string[] allowedExtensions =
                    { ".jpg", ".jpeg", ".png", ".webp" };

                if (string.IsNullOrEmpty(extension) ||
                    !allowedExtensions.Contains(extension.ToLower()))
                {
                    ModelState.AddModelError(
                        "ImageFile",
                        "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP!"
                    );
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tạo tên ảnh duy nhất
                    string extension =
                        Path.GetExtension(ImageFile.FileName).ToLower();

                    string fileName =
                        Guid.NewGuid().ToString("N") + extension;

                    // Đường dẫn thư mục lưu ảnh
                    string folderPath = Server.MapPath("~/Content/Images/");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // Lưu ảnh vào thư mục Images
                    string fullPath = Path.Combine(folderPath, fileName);
                    ImageFile.SaveAs(fullPath);

                    // Lưu đường dẫn ảnh vào database
                    product.Image = "Images/" + fileName;

                    db.Products.Add(product);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(
                        "",
                        "Có lỗi xảy ra khi lưu sản phẩm hoặc hình ảnh. Vui lòng thử lại!"
                    );
                }
            }

            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName",
                product.CategoryID
            );

            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int id)
        {
            var product = db.Products.Find(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName",
                product.CategoryID
            );

            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Product product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                ModelState.AddModelError(
                    "ProductName",
                    "Vui lòng nhập tên sản phẩm!"
                );
            }

            if (product.Price < 0)
            {
                ModelState.AddModelError(
                    "Price",
                    "Giá sản phẩm không được âm!"
                );
            }

            if (product.Quantity < 0)
            {
                ModelState.AddModelError(
                    "Quantity",
                    "Số lượng không được âm!"
                );
            }

            if (ModelState.IsValid)
            {
                var existingProduct = db.Products.Find(id);

                if (existingProduct == null)
                {
                    return HttpNotFound();
                }

                existingProduct.ProductName = product.ProductName;
                existingProduct.CategoryID = product.CategoryID;
                existingProduct.Price = product.Price;
                existingProduct.Quantity = product.Quantity;
                existingProduct.Image = product.Image;
                existingProduct.Description = product.Description;

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(
                db.Categories,
                "CategoryID",
                "CategoryName",
                product.CategoryID
            );

            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int id)
        {
            var product = db.Products
                .Include("Category")
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = db.Products.Find(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            db.Products.Remove(product);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Products/Details/5
        public ActionResult Details(int id)
        {
            var product = db.Products
                .Include("Category")
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }
    }
}
