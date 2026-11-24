using BlogApp.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.Controllers
{
    public class AuthController : Controller
    {
        //scope services to use in the controller,accesible through http requests
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
        }
        //Register , Login, Logout
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task <IActionResult> Register(RegisterViewModel model)
        {
            //Check if the model is valid
            if (ModelState.IsValid)
            {
                //create identity user object
                var user = new IdentityUser {UserName = model.Email, Email = model.Email};

                //create user
             var result = await userManager.CreateAsync(user, model.Password);

                //check if the user is created successfully
                if (result.Succeeded)
                {
                    //check if the role exists in the database
                    if (!await roleManager.RoleExistsAsync("User")){

                        await roleManager.CreateAsync(new IdentityRole("User"));
                    }
                    //assign the role to the user
                    await userManager.AddToRoleAsync(user, "User");
                    //user is created successfully, redirect to login page
                    await signInManager.SignInAsync(user, true);

                    return RedirectToAction("Index", "Post");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email or Password is incorrect.");
                    return View(model);
                }

                var signIn = await signInManager.PasswordSignInAsync(user, model.Password, false, false);

                if (!signIn.Succeeded)
                {
                    ModelState.AddModelError("", "Email or Password is incorrect.");
                    return View(model);
                }
                return RedirectToAction("Index", "Post");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout(LoginViewModel model)
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Post");
        }

        [HttpGet]

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

