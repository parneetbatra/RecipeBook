using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecipeBook.Models;

namespace RecipeBook.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class RecipeController : Controller
    {
        private readonly DataBaseContext _context;
        private readonly IHostingEnvironment _environment;

        public RecipeController(DataBaseContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _environment = hostingEnvironment;

        }

        //GET: Recipe  /recipe? page = 1
       [Route("recipe/{Page}")]   // recipe/1
       public IActionResult Index(int Page)
        {
            int Total = _context.Recipes.Count();
            int PageSize = 3;

            ViewBag.Total = Convert.ToInt32(Math.Ceiling((double)Total / (double)PageSize));
            if (Page != 1)
            {
                ViewBag.Previous = Page - 1;
            }
            else
            {
                ViewBag.Previous = null;
            }
            if (Page < Total)
            {
                ViewBag.Next = Page + 1;
            }
            else
            {
                ViewBag.Next = null;
            }


            int Skip = PageSize * (Page - 1);

            List<RecipeUserView> q = (List<RecipeUserView>)(from r in _context.Recipes
                                                            join u in _context.Users on r.UserId equals u.Id
                                                            select new RecipeUserView
                                                            {
                                                                RecipeId = r.Id,
                                                                RecipeName = r.Name,
                                                                RecipeImages = _context.RecipeImages.Where(f => f.RecipeId == r.Id).ToList(),
                                                                RecipeDescription = r.Description,
                                                                RecipeTimeToComplete = r.TimeToComplete,
                                                                Image = u.Image,
                                                                Name = u.Name,
                                                                Id = u.Id
                                                            }).ToList();

            return View(q.AsQueryable().Skip(Skip).Take(PageSize).ToList());

        }

        //GET: Recipe/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            RecipeUserView recipe = (RecipeUserView)(from r in _context.Recipes
                                                     join u in _context.Users on r.UserId equals u.Id
                                                     where r.Id == id
                                                     select new RecipeUserView
                                                     {
                                                         RecipeId = r.Id,
                                                         RecipeName = r.Name,
                                                         RecipeImages = _context.RecipeImages.Where(f => f.RecipeId == r.Id).ToList(),
                                                         RecipeDescription = r.Description,
                                                         RecipeTimeToComplete = r.TimeToComplete,
                                                         Image = u.Image,
                                                         Name = u.Name,
                                                         Id = u.Id,
                                                         Steps = _context.Steps.Where(f => f.RecipeId == r.Id).ToList(),
                                                         Ingredients = _context.Ingredients.Where(f => f.RecipeId == r.Id).ToList()
                                                     }).FirstOrDefault();


            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        // GET: Recipe/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Recipe/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecipeUserView recipeView)
        {
            string fileName = string.Empty;
            string path = string.Empty;

            Recipe recipe = new Recipe();
            recipe.Description = recipeView.RecipeDescription;
            recipe.Name = recipeView.RecipeName;
            recipe.TimeToComplete = recipeView.RecipeTimeToComplete;

            recipe.UserId = Convert.ToInt32(User.FindFirst("Id").Value);
            //recipe.Image = fileName;
            if (ModelState.IsValid)
            {
                _context.Add(recipe);
                await _context.SaveChangesAsync();

                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        var extension = Path.GetExtension(files[i].FileName);
                        fileName = Guid.NewGuid().ToString() + extension;

                        path = Path.Combine(_environment.WebRootPath, "RecipeImages") + "/" + fileName;

                        using (FileStream fs = System.IO.File.Create(path))
                        {
                            files[i].CopyTo(fs);
                            fs.Flush();
                        }

                        RecipeImages recipeImages = new RecipeImages();
                        recipeImages.Image = fileName;
                        recipeImages.RecipeId = recipe.Id;
                        _context.Add(recipeImages);
                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction("Index", "Recipe", new { Page = 1 });
            }
            return View(recipe);
        }

        // GET: Recipe/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            return View(recipe);
        }

        // POST: Recipe/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Recipe recipe)
        {
            if (id != recipe.Id)
            {
                return NotFound();
            }

            string fileName = string.Empty;
            string path = string.Empty;

            recipe.UserId = Convert.ToInt32(User.FindFirst("Id").Value);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recipe);
                    await _context.SaveChangesAsync();

                    var files = HttpContext.Request.Form.Files;
                    if (files.Count > 0)
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            var extension = Path.GetExtension(files[i].FileName);
                            fileName = Guid.NewGuid().ToString() + extension;

                            path = Path.Combine(_environment.WebRootPath, "RecipeImages") + "/" + fileName;

                            using (FileStream fs = System.IO.File.Create(path))
                            {
                                files[i].CopyTo(fs);
                                fs.Flush();
                            }

                            RecipeImages recipeImages = new RecipeImages();
                            recipeImages.Image = fileName;
                            recipeImages.RecipeId = recipe.Id;
                            _context.Add(recipeImages);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipeExists(recipe.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(recipe);
        }

        // GET: Recipe/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes.FindAsync(id);
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            List<RecipeImages> recipeImages = _context.RecipeImages.Where(f => f.RecipeId == id).ToList();
            for (int i = 0; i < recipeImages.Count; i++)
            {
                string path = Path.Combine(_environment.WebRootPath, "RecipeImages") + "/" + recipeImages[i].Image;
                System.IO.File.Delete(path);

            }
            return RedirectToAction("Index", "Recipe", new { Page = 1 });
        }

        // POST: Recipe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Recipe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImageConfirmed(int id)
        {
            var recipeImage = await _context.RecipeImages.FindAsync(id);
            _context.RecipeImages.Remove(recipeImage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecipeExists(int id)
        {
            return _context.Recipes.Any(e => e.Id == id);
        }


    }
}
