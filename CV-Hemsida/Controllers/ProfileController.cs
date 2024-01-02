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
        public IActionResult SetUserPrivate(bool isPrivate)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("ChangeInformation");
            }

            user.Privat = isPrivate;
            _dbContext.SaveChanges();

            return RedirectToAction("ChangeInformation");
        }

        [HttpPost]
        public IActionResult SavePrivateValue(ChangeInformationViewModel model, bool isPrivate)
        {
            if (ModelState.IsValid)
            {
                // Dina övriga logiker för att spara andra uppgifter från formuläret
                // ...

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

                if (user == null)
                {
                    return RedirectToAction("ChangeInformation");
                }

                user.Privat = isPrivate;
                _dbContext.SaveChanges();

                return RedirectToAction("ChangeInformation");
            }

            // Om ModelState inte är giltigt, returnera vyn med felmeddelanden
            return View(model);
        }



//        [HttpPost]
//public IActionResult SaveInfo(ChangeInformationViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                // Retrieve the current user's ID
//                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

//                // Use the user ID to retrieve the corresponding Person from the database
//                Person userPerson = _dbContext.Personer.FirstOrDefault(p => p.AnvändarID == userId);

//                if (userPerson == null)
//                {
//                    // Handle the case where the user's information is not found
//                    return RedirectToAction("ChangeInformation", model);
//                }

//                // Update the user's information with the values from the form
//                userPerson.Förnamn = model.Förnamn;
//                userPerson.Efternamn = model.Efternamn;
//                userPerson.Adress = model.Adress;

//                // Save changes directly to the database
//                _dbContext.SaveChanges();

//                return RedirectToAction("ChangeInformation", model); // Redirect to the user's profile page
//            }



//            // If ModelState is not valid, return to the ChangeInformation view with validation errors
//            return View("ChangeInformation", model);
//        }






        [HttpPost]
        public IActionResult SaveInformationAndPrivateValue(ChangeInformationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

                if (user == null)
                {
                    return RedirectToAction("ChangeInformation", model);
                }

                // Hämta användarens Person-information för de saknade värdena
                var userPerson = _dbContext.Personer.FirstOrDefault(p => p.AnvändarID == userId);

                if (userPerson == null)
                {
                    return RedirectToAction("ChangeInformation", model);
                }

                // Använd Person-informationen för förnamn, efternamn och adress om de saknas i modellen
                model.Förnamn ??= userPerson.Förnamn;
                model.Efternamn ??= userPerson.Efternamn;
                model.Adress ??= userPerson.Adress;

                // Sätt det booleska värdet för privat
                user.Privat = model.ÄrProfilPrivat;

                _dbContext.SaveChanges();

                return RedirectToAction("ChangeInformation", model);
            }

            // Om ModelState inte är giltigt, returnera vyn med felmeddelanden
            return View("ChangeInformation", model);
        }














        public IActionResult VisaAnvändaresProfil(string id)
        {
            // Här antar vi att `id` är användarens unika identifierare (som användarens e-post eller användarnamn)
            var user = _dbContext.Users
                        .Include(u => u.Cv)
                        .FirstOrDefault(u => u.Email == id);

            if (user == null || user.Cv == null)
            {
                // Om användaren inte hittas, visa en lämplig sida
                return RedirectToAction("ResourceNotFound");
            }
            else
            {
                var cvViewModel = new AnvändareCVViewModel
                {
                    // Fyll i all information från `user` och `user.Cv` här
                    Namn = user.UserName,
                    Kompetenser = user.Cv.Kompetenser,
                    Utbildningar = user.Cv.Utbildningar,
                    TidigareErfarenhet = user.Cv.TidigareErfarenhet,
                    ProfilbildPath = user.Cv.ProfilbildPath,
                    // Lägg till andra relevanta egenskaper här
                };

                // Skicka denna information till vyn som ska visa användarens profilsida
                return View("VisaAnvändaresProfil", cvViewModel); // Se till att du har en vy som heter "VisaAnvändaresProfil"
            }
        }


    }
}
