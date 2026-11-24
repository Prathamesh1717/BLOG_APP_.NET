using BlogApp.Data;
using BlogApp.Models;
using BlogApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace BlogApp.Controllers
{

    //only authenticated users can access this action, if not authenticated, the user will be redirected to the login page
    //you cannot access this action if you are not logged in
    //[Authorize]
    public class PostController : Controller
    {
        private readonly string[] allowedExt = { ".jpg", ".jpeg", ".png", ".gif" };

        private readonly AppDbContext context;

        // Add this field
        private readonly IWebHostEnvironment webHostEnvironment;

       
        public PostController(AppDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<PostController> logger)
        {
            this.context = context;
            this.webHostEnvironment = webHostEnvironment;
           
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        public IActionResult Create()
        {
            //ViewData["Category"] = new SelectList(_context.Categori
            //es, "Id", "Name");
            var postViewModel = new PostViewModel
            {
                Categories = new SelectList(context.Categories, "Id", "Name"),
            };
            return View(postViewModel);
        }

        [HttpGet]

        public async Task<IActionResult> Detail(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var post = await context.Posts.Include(p => p.Category).Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10485760)]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create(PostViewModel postViewModel)
        {
            if (ModelState.IsValid)
            {
                var inputFileExtension = Path.GetExtension(postViewModel.FeatureImage.FileName).ToLower();
                bool isAllowed = allowedExt.Contains(inputFileExtension);
                if (!isAllowed)
                {
                    ModelState.AddModelError("Image", "Invalid image format. Allowed formats are .jpg, .jpeg, .png");
                    return View(postViewModel);
                }
                postViewModel.Post.FeatureImagePath = await UploadFileToFolder(postViewModel.FeatureImage);
                context.Posts.Add(postViewModel.Post);
                context.SaveChanges();

                return RedirectToAction("Index");
            }
            //to get categories after submit 
            postViewModel.Categories = new SelectList(context.Categories, "Id", "Name");
            
            return View(postViewModel);
        }

        [HttpPost]
        //only authenticated users can access this action, if not authenticated, the user will be redirected to the login page
        [Authorize]
        public JsonResult AddComment([FromBody] Comment comment)
        {
            comment.CommentDate = DateTime.Now;
            context.Comments.Add(comment);
            context.SaveChanges();
            return Json(new
            {
                username = comment.UserName,
                content = comment.Content,
                commentDate = comment.CommentDate.ToString("MMMM dd, yyyy")
            });
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(int id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var postFormDb = await context.Posts.FirstOrDefaultAsync(x => x.Id == id);

            if(postFormDb == null)
            {
                return NotFound();
            }
            EditViewModel editViewModel = new EditViewModel
            {
                Post = postFormDb,
                Categories = new SelectList(context.Categories, "Id", "Name"),

            };
            return View(editViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(EditViewModel editViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editViewModel);
            }
            var postFormDb = await context.Posts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == editViewModel.Post.Id);

            if(postFormDb == null)
            {
                return NotFound();
            }
            if(editViewModel.FeatureImage != null)
            {
                var inputFileExtension = Path.GetExtension(editViewModel.FeatureImage.FileName).ToLower();
                bool isAllowed = allowedExt.Contains(inputFileExtension);
                if (!isAllowed)
                {
                    ModelState.AddModelError("Image", "Invalid image format. Allowed formats are .jpg, .jpeg, .png");
                    return View(editViewModel);
                }

                var existingFilePath = Path.Combine(webHostEnvironment.WebRootPath, "images", Path.GetFileName(postFormDb.FeatureImagePath));

                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
                editViewModel.Post.FeatureImagePath = await UploadFileToFolder(editViewModel.FeatureImage);
            }
            else
            {
                editViewModel.Post.FeatureImagePath = postFormDb.FeatureImagePath;
            }
            context.Posts.Update(editViewModel.Post);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var postFormDb = await context.Posts.FirstOrDefaultAsync(x => x.Id == id);
            if (postFormDb == null)
            {
                return NotFound();
            }
            return View(postFormDb);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postFormDb = await context.Posts.FirstOrDefaultAsync(x => x.Id == id);

            // Delete the associated feature image file if it exists
            if (string.IsNullOrEmpty(postFormDb.FeatureImagePath))
            {
                var existingFilePath = Path.Combine(webHostEnvironment.WebRootPath, "images", Path.GetFileName(postFormDb.FeatureImagePath));

                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
            }
            context.Posts.Remove(postFormDb);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        
        //[AllowAnonymous] //you can access this action even if you are not logged in
        public IActionResult Index(int? CategoryId)
        {
            var postQuery = context.Posts.Include(p => p.Category).AsQueryable();

            if (CategoryId.HasValue)
            {
                postQuery = postQuery.Where(p => p.CategoryId == CategoryId.Value);
            }

            var posts = postQuery.ToList();
            ViewBag.Categories = context.Categories.ToList();

            return View(posts);
        }

        private async Task<string> UploadFileToFolder(IFormFile file)
        {
            var inputFileExtension = Path.GetExtension(file.FileName);
            var fileName = Guid.NewGuid().ToString() + inputFileExtension;
            var wwwRootPath = webHostEnvironment.WebRootPath;
            var imagesFolderPath = Path.Combine(wwwRootPath, "images");

            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }

            var filePath = Path.Combine(imagesFolderPath, fileName);

            try
            {
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed.
                return "Error Uploading Image: " + ex.Message;
            }

            return "/images/" + fileName;
        }
    }
}