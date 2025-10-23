using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SakiBee.Data;
using SakiBee.Models;
using SakiBee.Models.ViewModels;

namespace SakiBee.Controllers
{
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string[] _allowedExtension = {".jpg", ".jpeg", ".png"};
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostController(AppDbContext context, IWebHostEnvironment webHostEnvironment) 
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Index(int? categoryId)
        {
            var postQuery = _context.Posts.Include(p => p.Category).AsQueryable(); 
            
            if(categoryId.HasValue)
            {
                postQuery = postQuery.Where(p => p.CategoryId == categoryId);
            }
            var posts = postQuery.ToList();

            ViewBag.Categories = _context.Categories.ToList();

            return View(posts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var postViewModel = new PostViewModel();
            postViewModel.Categories = _context.Categories.Select(c =>
                new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }
            ).ToList();
            return View(postViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PostViewModel postViewModel)
        {
            if (!ModelState.IsValid)
            {
                postViewModel.Categories = _context.Categories.Select(c =>
                    new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList();

                return View(postViewModel);
            }


            if (postViewModel.FeatureImage == null || postViewModel.FeatureImage.Length == 0)
            {
                ModelState.AddModelError("FeatureImage", "Please upload an image file.");
                postViewModel.Categories = _context.Categories.Select(c =>
                    new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList();
                return View(postViewModel);
            }

            var inputFileExtension = Path.GetExtension(postViewModel.FeatureImage.FileName).ToLower();
            bool isAllowed = _allowedExtension.Contains(inputFileExtension);

            if (!isAllowed)
            {
                ModelState.AddModelError("FeatureImage", "Invalid image format. Allowed formats are .jpg, .jpeg, .png");
                postViewModel.Categories = _context.Categories.Select(c =>
                    new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList();
                return View(postViewModel);
            }

            postViewModel.Post.FeatureImagePath = await UploadFileToFolder(postViewModel.FeatureImage);
            await _context.Posts.AddAsync(postViewModel.Post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditPost(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = await _context.Posts.FirstOrDefaultAsync(p=>p.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            EditViewModel editViewModel = new EditViewModel
            {
                Post = post,
                Categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View(editViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(EditViewModel editViewModel)
        {
            if(!ModelState.IsValid)
            {
                return View(editViewModel);
            }
            var oldPost = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p=> p.Id == editViewModel.Post.Id);
            
            if (oldPost == null)
            {
                return NotFound();
            }

            if(editViewModel.FeatureImage != null) {
                var inputFileExtension = Path.GetExtension(editViewModel.FeatureImage.FileName).ToLower();
                bool isAllowed = _allowedExtension.Contains(inputFileExtension);

                if (!isAllowed)
                {
                    ModelState.AddModelError("FeatureImage", "Invalid image format. Allowed formats are .jpg, .jpeg, .png");
                    editViewModel.Categories = _context.Categories.Select(c =>
                        new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        }).ToList();
                    return View(editViewModel);
                }

                var existingFilePaht = Path.Combine(_webHostEnvironment.WebRootPath, "images", Path.GetFileName(oldPost.FeatureImagePath));

                if (System.IO.File.Exists(existingFilePaht))
                {
                    System.IO.File.Delete(existingFilePaht);
                }

                editViewModel.Post.FeatureImagePath = await UploadFileToFolder(editViewModel.FeatureImage);
            }
            else
            {
                editViewModel.Post.FeatureImagePath = oldPost.FeatureImagePath;
            }

            _context.Posts.Update(editViewModel.Post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var postDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (postDb == null)
            {
                return NotFound();
            }
            return View(postDb);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var postDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if(string.IsNullOrEmpty(postDb.FeatureImagePath))
            {
                var existingFilePaht = Path.Combine(_webHostEnvironment.WebRootPath, "images", Path.GetFileName(postDb.FeatureImagePath));

                if (System.IO.File.Exists(existingFilePaht))
                {
                    System.IO.File.Delete(existingFilePaht);
                }
            }
            _context.Posts.Remove(postDb);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var post = _context.Posts
                .Include(p => p.Category)
                .Include(p => p.Comments)
                .FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }


        private async Task<string> UploadFileToFolder(IFormFile file)
        {
            if(file == null || file.Length == 0)
            {
                return null;
            }
            var inputFileExtension = Path.GetExtension(file.FileName).ToLower();
            var fileName = Guid.NewGuid().ToString() + inputFileExtension;
            var wwwRootPath = _webHostEnvironment.WebRootPath;
            var imageFolderPath = Path.Combine(wwwRootPath, "images");

            if(!Directory.Exists(imageFolderPath))
            {
                Directory.CreateDirectory(imageFolderPath);
            }
            var filePath = Path.Combine(imageFolderPath, fileName);

            try
            {
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                return "Error Uploading Image" + ex.Message;
            }

            return "/images/" + fileName;
        }

        public JsonResult AddComment([FromBody] Comment comment)
        {
            comment.CommentDate = DateTime.Now;
            _context.Comments.Add(comment);
            _context.SaveChanges();
            return Json(new
            {
                username = comment.UserName,
                content = comment.Content,
                commentDate = comment.CommentDate.ToString("MMMM dd, yyyy")
            });
        }
    }
}
