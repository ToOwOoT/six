using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lunch.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lunch.Client.Controllers
{
    public class MainController : Controller
    {
        private readonly FoodService _service;
        private readonly OrderService _orderService;

        public MainController(FoodService service, OrderService orderService)
        {
            _service = service;
            _orderService = orderService;
        }

        public IActionResult FoodPage()
        {
            return View();
        }

        public IActionResult GetFoods(string keyword, string type, int pageSize, int pageIndex)
        {
            var foods = _service.QueryFoods(keyword, type, pageSize, pageIndex, out int total);
            return Ok(new { Result = true, List = foods, Total = total });
        }

        public IActionResult ViewImge(int id)
        {
            var food = _service.GetFood(id);
            if (food == null || food.Image.Length == 0) return Content("Error");

            MemoryStream ms = new MemoryStream(food.Image);
            return new FileStreamResult(ms, new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(food.ImageType));
        }

        [Authorize]
        [HttpPost]
        public IActionResult OrderingFood(int totalPrice, [FromBody]List<int> foodIds)
        {
            if (foodIds.Count == 0) return Ok(new { Result = false });

            var userIdStr = User.Claims.SingleOrDefault(s => s.Type == "UserId").Value;
            int.TryParse(userIdStr, out int userId);
            if (userId == 0) return Ok(new { Result = false });

            var food = _orderService.OrderingFood(userId, totalPrice, foodIds);
            return  Ok(new { Result = food });
        }
    }
}