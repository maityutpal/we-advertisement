using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_advertisement.Models.Accounts;

namespace web_advertisement.Controllers
{
    public class Accounts : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _userPool;

        public Accounts(SignInManager<CognitoUser> signInManager,UserManager<CognitoUser>userManager,CognitoUserPool userPool)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userPool = userPool;
        }
        public IActionResult Index()
        {
            
            return View();
        }


        public async Task<IActionResult> Signup()
        {
            var model = new SignupModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel signupModel)
        {
            if (ModelState.IsValid)
            {
                var user = _userPool.GetUser(signupModel.Email);
                if (user.Status!=null)
                {
                    ModelState.AddModelError("UserExist", "User already exist");
                    return View(signupModel);
                }
                user.Attributes.Add(CognitoAttribute.Name.AttributeName, signupModel.Email);
               var createdUser= await _userManager.CreateAsync(user,signupModel.Password);
                if (createdUser.Succeeded)
                {
                    RedirectToAction("Confirm");
                }
            }
            return View();
        }

        public async Task<IActionResult> Confirm()
        {
            var model = new ConfirmModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("UserNotExist", "User not exist with the same email address");
                    return View(model);
                }
                
                var result = await ((CognitoUserManager<CognitoUser>)_userManager).ConfirmSignUpAsync(user, model.Code,true);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach(var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                    return View(model);
                }

            }
            
            
            return View(model);
        }

        public async Task<IActionResult> Login()
        {
            var model = new LoginModel();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
               
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RemeberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("UserError", "Email or password mismatch");
                }
            }
            return View(model);
        }

    }
}
