﻿using CVDataLayer;
using CVModels;
using CVModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CV_Hemsida.Controllers
{
    public class ProfileController : Controller
    {
        private CVContext _dbContext;

        public ProfileController(CVContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult ProfilePage()
        {
            List<Person> listAvPersoner = _dbContext.Personer.ToList();
            ViewBag.Meddelande = "Listan med profiler";
            // TODO: Implementera databaslogiken
            // var projekten = // Hämta projektdata från databasen med LINQ

            return View(listAvPersoner); // Temporärt tills databaslogiken är implementerad

        }


        [HttpGet]
        public IActionResult ViewProfile(string userId)
        {
            var profileUser = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

            if (profileUser == null)
            {
                return NotFound(); // Om profilen inte finns, returnera 404
            }

            if (profileUser.Privat && !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account"); // Användaren är inte inloggad, omdirigera till inloggningssidan
            }

            // Om profilen är privat men användaren är inloggad eller om profilen inte är privat, visa profilen
            var profile = profileUser.Person;

            if (profile == null)
            {
                return NotFound(); // Om profilen inte finns, returnera 404
            }

            return View(profile);
        }



        //Hugos sökruta
        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            // Implementera söklogiken för profiler här baserat på searchTerm
            List<Person> sökResultat = _dbContext.Personer
                .Where(p => p.Förnamn.Contains(searchTerm) || p.Efternamn.Contains(searchTerm)) // Exempel: Sök efter profiler med förnamn eller efternamn som innehåller söktermen
                .ToList();

            if (sökResultat.Count == 0)
            {
                ViewBag.ErrorMessage = "Inga matchningar hittades för din sökning.";
            }

            return View("ProfilePage", sökResultat); // Visa sökresultaten på samma vyn som ProfilePage
        }
        //PS: använder Förnamn och Efternamn här istället för Fullname på kodrad 32
        //slut på sökrutan



        [HttpGet]
        public IActionResult ChangeInformation()
        {
            // Retrieve the current user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Use the user ID to retrieve the corresponding Person from the database
            Person userPerson = _dbContext.Personer.FirstOrDefault(p => p.AnvändarID == userId);

            if (userPerson == null)
            {
                // Handle the case where the user's information is not found
                return RedirectToAction("Index", "Home");
            }

            // Map the user's information to the ChangeInformationViewModel
            var viewModel = new ChangeInformationViewModel
            {
                Förnamn = userPerson.Förnamn,
                Efternamn = userPerson.Efternamn,
                Adress = userPerson.Adress
            };

            return View(viewModel);
        }


      


        [HttpPost]
        public IActionResult SaveInfo(ChangeInformationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the current user's ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Use the user ID to retrieve the corresponding Person from the database
                Person userPerson = _dbContext.Personer.FirstOrDefault(p => p.AnvändarID == userId);

                if (userPerson == null)
                {
                    // Handle the case where the user's information is not found
                    return RedirectToAction("ChangeInformation", model);
                }

                // Update the user's information with the values from the form
                userPerson.Förnamn = model.Förnamn;
                userPerson.Efternamn = model.Efternamn;
                userPerson.Adress = model.Adress;

                // Save changes directly to the database
                _dbContext.SaveChanges();

                return RedirectToAction("ChangeInformation", model); // Redirect to the user's profile page
            }



            // If ModelState is not valid, return to the ChangeInformation view with validation errors
            return View("ChangeInformation", model);
        }





       



        [HttpGet]
        public IActionResult PrivatProfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("ChangeInformation");
            }

            var viewModel = new SetPrivatViewModel
            {
                Privat = user.Privat
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult PrivatProfil(SetPrivatViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

                if (user == null)
                {
                    return RedirectToAction("ChangeInformation");
                }

                user.Privat = model.Privat;
                _dbContext.SaveChanges();

                return RedirectToAction("Index"); // Redirect to the desired action after saving the settings
            }

            // If ModelState is not valid, return the view with validation errors
            return View(model);
        }


     





    }
}
