using April_17_Homework_Image_Upload_w_Password.Data;
using April_17_Homework_Image_Upload_w_Password.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;


namespace April_17_Homework_Image_Upload_w_Password.Controllers
{
    public class HomeController : Controller
    {
        private string _connectionString = @"Data Source=.\sqlexpress;Initial Catalog=Images; Integrated Security=true;";

        private IWebHostEnvironment _webHostEnvironment;
        public HomeController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(IFormFile imageFile, Image image)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);
            using var fs = new FileStream(filePath, FileMode.CreateNew);
            imageFile.CopyTo(fs);

            image.FileName = fileName;
            var db = new DatabaseManager(_connectionString);
            db.AddImage(image);
            
            return View(image);
        }

        [HttpPost]
        public IActionResult ViewImage(int id, string password)
        {
            var db = new DatabaseManager(_connectionString);
            var image = db.GetImage(id);
            if(image == null)
            {
                return RedirectToAction("Index");
            }
            if(password != image.Password)
            {
                TempData["message"] = "Invalid Password";
            }
            else
            {
                var permittedIds = HttpContext.Session.Get<List<int>>("permittedids");
                if (permittedIds == null)
                {
                    permittedIds = new List<int>();
                }
                permittedIds.Add(id);
                HttpContext.Session.Set("permittedids", permittedIds);
            }
            return Redirect($"/home/viewimage?id={id}");
        }

        public IActionResult ViewImage(int id)
        {
            var vm = new ViewImageViewModel();
            if (TempData["message"] != null)
            {
                vm.Message = (string)TempData["message"];
            }
            if(!PermissionToView(id))
            {
                vm.Show = false;
                vm.Image = new Image { Id = id };
            }
            else
            {
                vm.Show = true;
                var db = new DatabaseManager(_connectionString);
                db.IncrementViewCount(id);
                var image = db.GetImage(id);
                vm.Image = image;
            }
            return View(vm);
        }


        private bool PermissionToView(int id)
        {
            var permittedIds = HttpContext.Session.Get<List<int>>("permittedids");
            if(permittedIds == null)
            {
                return false;
            }
            return permittedIds.Contains(id);
        }
    }



    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            string value = session.GetString(key);

            return value == null ? default(T) :
                JsonSerializer.Deserialize<T>(value);
        }
    }
}