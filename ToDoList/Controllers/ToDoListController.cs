using Microsoft.AspNetCore.Mvc;

namespace LifePlanner.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class ToDoListController : ControllerBase
    {
        public ToDoListController()
        {
        }

        // GET: api/todo
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(new[] { "Task 1", "Task 2" });
        }
    }
}

