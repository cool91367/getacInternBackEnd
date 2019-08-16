using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;
using WebApplication.ViewModels;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
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
                return BadRequest("All fields need to be fill");
            }

            var usr = new User { UserName = vm.Username };
            var result = await userManager.CreateAsync(usr, vm.Password);

            if (result.Succeeded)
            {
                return Ok("Register succesfully");
            }
            else
            {
                return BadRequest("User already exsits");
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]UserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("All fields need to be fill");
            }

            var userExist = await userManager.FindByNameAsync(vm.Username);

            if (userExist == null)
            {
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
                    return Ok("Login succesfully");
                }
                else
                {
                    return BadRequest("Password is wrong");
                }
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok("Logout succesfully");
        }

    }
}
