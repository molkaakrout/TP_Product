using Microsoft.AspNetCore.Mvc;
using TP_Product.Models;
using TP_Product.Models.Repositories;

namespace TP_Product.Controllers
{
    public class CategoryController : Controller
    {
        readonly ICategorieRepository CategRepository;
        public CategoryController(ICategorieRepository categRepository)
        {
            CategRepository = categRepository;
        }

        public ActionResult Index() => View(CategRepository.GetAll());

        public ActionResult Details(int id) => View(CategRepository.GetById(id));

        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category c)
        {
            if (!ModelState.IsValid) return View(c);
            CategRepository.Add(c);
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Edit(int id) => View(CategRepository.GetById(id));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Category c)
        {
            if (!ModelState.IsValid) return View(c);
            CategRepository.Update(c);
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Delete(int id) => View(CategRepository.GetById(id));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            CategRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}