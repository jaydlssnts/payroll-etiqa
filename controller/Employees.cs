using payroll.model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace payroll.controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class Employees : ControllerBase
    {
        private readonly AppDbContext _context;

        public Employees(AppDbContext context){
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(){
            var employees = await _context.Employee.ToListAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id){
            // get employee by employee id
            var employee = await _context.Employee.FindAsync(id);
            if (employee == null) return NotFound();
            return Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Employee employee){
            if (employee == null)
                return BadRequest("Employee object is required.");

            string lastName;
            if (employee.EmployeeName.Contains(','))
                lastName = employee.EmployeeName.Split(',')[0].Trim();
            else
                lastName = employee.EmployeeName.Split(' ').Last().Trim();

            // EmployeeNumber Generation
            string namePart = new string(lastName
                                        .Where(char.IsLetter)
                                        .Take(3)
                                        .ToArray())
                                        .ToUpper()
                                        .PadRight(3, '*');

            var randomId = Random.Shared.Next(0, 100000).ToString("D5");
            var birthdateStr = employee.BirthDate.ToString("ddMMMyyyy").ToUpper();

            employee.EmployeeNumber = $"{namePart}-{randomId}-{birthdateStr}";

            // save to database
            _context.Employee.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = employee.EmployeeNumber }, employee);
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Employee updatedEmployee){
            // update employee by id
            var employee = await _context.Employee.FindAsync(id);
            if (employee == null) return NotFound();

            employee.EmployeeName = updatedEmployee.EmployeeName;
            employee.BirthDate = updatedEmployee.BirthDate;
            employee.DailyRate = updatedEmployee.DailyRate;
            employee.WorkSchedule = updatedEmployee.WorkSchedule;

            // update based on api body
            await _context.SaveChangesAsync();
            return Ok(employee);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id){
            // delete employee by id
            var employee = await _context.Employee.FindAsync(id);
            if (employee == null) return NotFound();

            _context.Employee.Remove(employee);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
