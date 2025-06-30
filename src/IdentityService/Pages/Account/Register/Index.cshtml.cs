using System;
using System.Linq;
using System.Security.Claims;
using Duende.IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IdentityService.Pages.Account.Register
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class Index : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<Index> _logger;

        public Index(UserManager<ApplicationUser> userManager, ILogger<Index> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; }

        [BindProperty]
        public bool RegisterSuccess { get; set; }

        public IActionResult OnGet(string returnUrl)
        {
            Input = new RegisterViewModel
            {
                ReturnUrl = returnUrl
            };
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            _logger.LogInformation("Registration attempt for user: {Username}", Input?.Username);

            if (Input.Button != "register") return Redirect("~/");

            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is valid, creating user: {Username}", Input.Username);

                var user = new ApplicationUser
                {
                    UserName = Input.Username,
                    Email = Input.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Username} created successfully", Input.Username);

                    await _userManager.AddClaimsAsync(user, new Claim[]
                    {
                        new Claim(JwtClaimTypes.Name, Input.FullName)
                    });

                    RegisterSuccess = true;
                }
                else
                {
                    _logger.LogWarning("User creation failed for {Username}. Errors: {Errors}",
                        Input.Username, string.Join(", ", result.Errors.Select(e => e.Description)));

                    // Add errors to ModelState to display them
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            else
            {
                _logger.LogWarning("ModelState is invalid for user: {Username}. Errors: {Errors}",
                    Input?.Username, string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            return Page();
        }
    }
}