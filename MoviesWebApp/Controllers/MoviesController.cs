using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesWebApp.Models;
using MoviesWebApp.ViewModels;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesWebApp.Controllers
{
    public class MoviesController : Controller
    {

        #region Probs
        private readonly AppDBContext _context;
        private long _maxAllowedPosterSize = 1048576;
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".jpeg", ".png" };
        private readonly IToastNotification _toastNotification;
        private readonly IMapper _mapper;

        #endregion

        #region Constructor
        public MoviesController(AppDBContext context, IToastNotification toastNotification, IMapper mapper)
        {
            _context = context;
            _toastNotification = toastNotification; 
            _mapper = mapper;
        }
        #endregion

        #region Actions 

        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.OrderByDescending(m => m.Rate).ToListAsync();
            return View(movies);
        }

        public async Task<IActionResult> Create()
        {
            var newModel = new MoviewFormVM()
            {
                Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync()
            };

            return View(newModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MoviewFormVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();

                return View(model);
            }
            var files = Request.Form.Files;
            if (!files.Any())
            {
                model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Please Select Moview Poster .. ");
                return View(model);
            }
            var poster = files.FirstOrDefault();
            if (!_allowedExtenstions.Contains(Path.GetExtension(poster.FileName.ToLower())))
            {
                model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Only .jpg and .png are allowed .. ");
                return View(model);
            }
            if (poster.Length > _maxAllowedPosterSize)
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB!");
                return View("Create", model);
            }
            if (poster.Length > _maxAllowedPosterSize)
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB!");
                return View("Create", model);
            }

            using var dataStream = new MemoryStream();
            await poster.CopyToAsync(dataStream);

            //var movie = new Movie
            //{
            //    GenreId = model.GenreId,
            //    Poster = dataStream.ToArray(),
            //    Year = model.Year,
            //    Title = model.Title,
            //    Rate = model.Rate,
            //    Storeline = model.Storeline
            //};
            var movie = _mapper.Map<Movie>(model);
            movie.Poster = dataStream.ToArray();

            _context.Movies.Add(movie);

            _context.SaveChanges();
            _toastNotification.AddSuccessToastMessage("Moview Created Successfully ");
            model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            var model = new MoviewFormVM
            {
                Id = movie.Id,
                GenreId = movie.GenreId,
                Year = movie.Year,
                Storeline = movie.Storeline,
                Rate = movie.Rate,
                Poster = movie.Poster,
                Title = movie.Title,
                Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync(),
            };

            return View("Create", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MoviewFormVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();

                return View(model);
            }
            var movie = await _context.Movies.FindAsync(model.Id);

            if (movie == null)
            {
                return NotFound();
            }

            var files = Request.Form.Files;

            if (files.Any())
            {
                var poster = files.FirstOrDefault();

                using var dataStream = new MemoryStream();
                await poster.CopyToAsync(dataStream);
                model.Poster = dataStream.ToArray();
                if (_allowedExtenstions.Contains(Path.GetExtension(poster.FileName.ToLower())))
                {
                    model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Only .jpg and .png are allowed .. ");
                    return View(model);
                }
                if (poster.Length > _maxAllowedPosterSize)
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB!");
                    return View("Create", model);
                }
                if (poster.Length > _maxAllowedPosterSize)
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB!");
                    return View("Create", model);
                }
                movie.Poster = model.Poster;
            }



            model.Title = model.Title;
            movie.Storeline = model.Storeline;
            movie.Year = model.Year;
            movie.Rate = model.Rate;
            _context.SaveChanges();
            _toastNotification.AddSuccessToastMessage("Moview Updates Successfully ");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.Include(m =>m.Genre).SingleOrDefaultAsync(m =>m.Id==id);
            if (movie == null)
                return NotFound();

            return View(movie);
        }

        public async Task<IActionResult> delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();
            _context.Movies.Remove(movie);
            _context.SaveChanges();
            return Ok();
        }
        #endregion

    }
}
