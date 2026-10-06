using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TP_Product.Models;
using TP_Product.Models.Repositories;
using TP_Product.ViewModels;

namespace TP_Product.Controllers
{
    public class ProductController : Controller
    {
        readonly IProductRepository ProductRepository;
        readonly ICategorieRepository CategRepository;
        private readonly IWebHostEnvironment hostingEnvironment;

        public ProductController(IProductRepository ProdRepository,
            ICategorieRepository categRepository,
            IWebHostEnvironment hostingEnvironment)
        {
            ProductRepository = ProdRepository;
            CategRepository = categRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        private void LoadCategories() =>
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");

        public ActionResult Index() => View(ProductRepository.GetAll());

        public ActionResult Details(int id) => View(ProductRepository.GetById(id));

        public ActionResult Search(string val)
        {
            var result = ProductRepository.FindByName(val ?? "");
            return View("Index", result);
        }

        // ---------- CREATE ----------
        public ActionResult Create()
        {
            LoadCategories();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel model)
        {
            LoadCategories();
            if (ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadedFile(model.ImagePath);

                Product newProduct = new Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    QteStock = model.QteStock,
                    CategoryId = model.CategoryId,
                    Image = uniqueFileName
                };
                ProductRepository.Add(newProduct);
                return RedirectToAction("Details", new { id = newProduct.ProductId });
            }
            return View(model);
        }

        // ---------- EDIT ----------
        public ActionResult Edit(int id)
        {
            LoadCategories();
            Product product = ProductRepository.GetById(id);
            if (product == null) return NotFound();

            EditViewModel vm = new EditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                QteStock = product.QteStock,
                CategoryId = product.CategoryId,
                ExistingImagePath = product.Image
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditViewModel model)
        {
            LoadCategories();
            if (ModelState.IsValid)
            {
                Product product = ProductRepository.GetById(model.ProductId);
                if (product == null) return NotFound();

                product.Name = model.Name;
                product.Price = model.Price;
                product.QteStock = model.QteStock;
                product.CategoryId = model.CategoryId;

                if (model.ImagePath != null)
                {
                    if (model.ExistingImagePath != null)
                    {
                        string oldPath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingImagePath);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }
                    product.Image = ProcessUploadedFile(model.ImagePath);
                }

                Product updated = ProductRepository.Update(product);
                if (updated != null) return RedirectToAction("Index");
                return NotFound();
            }
            return View(model);
        }

        // ---------- DELETE ----------
        public ActionResult Delete(int id) => View(ProductRepository.GetById(id));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            ProductRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        // ---------- Upload ----------
        [NonAction]
        private string ProcessUploadedFile(IFormFile file)
        {
            if (file == null) return null;
            string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
            return uniqueFileName;
        }
    }
}