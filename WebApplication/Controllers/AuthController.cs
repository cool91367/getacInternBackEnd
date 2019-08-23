using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;
using WebApplication.ViewModels;
using NLog;

namespace WebApplication.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger authLogger = LogManager.GetCurrentClassLogger();
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                authLogger.Info("400| failed| All fields need to be fill");
                return BadRequest("All fields need to be fill");
            }

            var usr = new User { UserName = vm.Username };
            var result = await userManager.CreateAsync(usr, vm.Password);

            if (result.Succeeded)
            {
                authLogger.Info("200| successfully| {vm.Username} register succeessfully", vm.Username);
                return Ok("Register succesfully");
            }
            else
            {
                authLogger.Info("400| failed| User {vm.Username} already exsits", vm.Username);
                return BadRequest("User already exsits");
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]UserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                authLogger.Info("400| failed| All fields need to be fill");
                return BadRequest("All fields need to be fill");
            }

            var userExist = await userManager.FindByNameAsync(vm.Username);

            if (userExist == null)
            {
                authLogger.Info("400| failed| Username {vm.Username} doesn't exist", vm.Username);
                return BadRequest("Username doesn't exist");
            }
            else
            {
                var result = await signInManager.PasswordSignInAsync(
                    userName: vm.Username,
                    password: vm.Password,
                    isPersistent: true, // TODO: Get this from the viewmodel
                    lockoutOnFailure: false
                );

                if (result.Succeeded)
                {
                    authLogger.Info("200| successfully| {vm.Username} logined", vm.Username);
                    return Ok("Login succesfully");
                }
                else
                {
                    authLogger.Info("400| failed| {vm.Username} Password is wrong", vm.Username);
                    return BadRequest("Password is wrong");
                }
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            authLogger.Info("200| successfully| {vm.Username}", User.Identity.Name);
            return Ok("Logout succesfully");
        }

    }
}
