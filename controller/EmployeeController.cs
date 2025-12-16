using Microsoft.AspNetCore.Mvc;
using payroll.model;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace payroll.controller
{
    public class EmployeeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context, IHttpClientFactory factory){
            _context = context;
            _httpClient = factory.CreateClient();
        }

        public async Task<IActionResult> Index(){
            var employees = await _context.Employee.ToListAsync();
            return View(employees);
        }
        public async Task<IActionResult> Details(string id){
            var employee = await _context.Employee.FindAsync(id);
            if (employee == null)
            {
                return NotFound("Employee not found");
            }
            return View(employee);
        }

        [HttpGet]
        public IActionResult Create(){
            //create page 
            var model = new Employee
            {
                EmployeeName = "",
                BirthDate = new DateOnly(2000, 1, 1),
                DailyRate = 0,
                WorkSchedule = WorkSchedule.MWF
            };
            return View(model);
        }
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Employee employee){
            // create method
            ModelState.Remove("EmployeeNumber");

            if (!ModelState.IsValid){
                return View(employee);
            }

            try{
                string lastName;
                if (employee.EmployeeName.Contains(',')){
                    lastName = employee.EmployeeName.Split(',')[0].Trim();
                } else {
                    lastName = employee.EmployeeName.Split(' ').Last().Trim();
                }
                //EmployeeNumber Generation
                string namePart = new string(lastName
                                            .Where(char.IsLetter)
                                            .Take(3)
                                            .ToArray())
                                            .ToUpper()
                                            .PadRight(3, '*');

                var randomId = Random.Shared.Next(0, 100000).ToString("D5");
                var birthdateStr = employee.BirthDate.ToString("ddMMMyyyy").ToUpper();

                //Assign the generated ID to the model
                employee.EmployeeNumber = $"{namePart}-{randomId}-{birthdateStr}";

                //Save directly to PostgreSQL
                _context.Employee.Add(employee);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Employee created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex){
                // prevents duplicate
                ModelState.AddModelError("", "Database Save Error: " + (ex.InnerException?.Message ?? ex.Message));
                return View(employee);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id){
            // fetch data using employee ID
            var employee = await _context.Employee.FindAsync(id);
            if (employee != null){
                // delete employee using employee ID
                _context.Employee.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employee deleted successfully!";
            } else {
                TempData["ErrorMessage"] = "Employee not found.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id){
            // edit page
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var employee = await _context.Employee.FindAsync(id);
            if (employee == null)
                return NotFound();

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string EmployeeNumber, string EmployeeName, string BirthDateString, decimal DailyRate, WorkSchedule WorkSchedule){
            ModelState.Remove("EmployeeNumber");
            ModelState.Remove("BirthDate");

            if (!DateOnly.TryParse(BirthDateString, out var birthDate)) {
                ModelState.AddModelError("BirthDate", "Invalid date format");
            }

            if (!ModelState.IsValid){
                return View(new Employee {
                    // get existing data on fields
                    EmployeeNumber = EmployeeNumber,
                    EmployeeName = EmployeeName,
                    BirthDate = birthDate == default ? new DateOnly(2000,1,1) : birthDate,
                    DailyRate = DailyRate,
                    WorkSchedule = WorkSchedule
                });
            }

            var existingEmployee = await _context.Employee.FindAsync(EmployeeNumber);
            if (existingEmployee == null)
                return NotFound("Employee not found.");
            if (!string.IsNullOrWhiteSpace(EmployeeName))
                existingEmployee.EmployeeName = EmployeeName;
            // save updated data into database
            existingEmployee.BirthDate = birthDate;
            existingEmployee.DailyRate = DailyRate;
            existingEmployee.WorkSchedule = WorkSchedule;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}