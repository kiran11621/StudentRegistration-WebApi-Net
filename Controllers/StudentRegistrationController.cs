
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StudentRegistration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbConnection _db;

        public StudentsController(IDbConnection db)
        {
            _db = db;
        }

        // Get all students
        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            try
            {
                var students = await _db.QueryAsync<Student>("SELECT * FROM GetStudents()");
                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching students", error = ex.Message });
            }
        }

        // Get a student by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            try
            {
                var student = await _db.QueryFirstOrDefaultAsync<Student>(
                    "SELECT * FROM GetStudentById(@Id)", new { Id = id });

                return student != null ? Ok(student) : NotFound(new { message = "Student not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching student", error = ex.Message });
            }
        }

        // Create a new student
        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] Student student)
        {
            try
            {
                // Call PostgreSQL function and get the returned student ID
                var id = await _db.ExecuteScalarAsync<int>(
                    "SELECT AddStudent(@Name, @Age, @Email)", student);

                return CreatedAtAction(nameof(GetStudentById), new { id }, new { message = "Student created", id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating student", error = ex.Message });
            }
        }


        // Update a student
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student student)
        {
            try
            {
                var affectedRows = await _db.ExecuteAsync(
                    "SELECT UpdateStudent(@Id, @Name, @Age, @Email)",
                    new { Id = id, student.Name, student.Age, student.Email });

                return affectedRows < 0 ? Ok(new { message = "Student updated" }) : NotFound(new { message = "Student not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating student", error = ex.Message });
            }
        }

        // Delete a student
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                var affectedRows = await _db.ExecuteAsync("SELECT DeleteStudent(@Id)", new { Id = id });

                return affectedRows < 0 ? Ok(new { message = "Student deleted" }) : NotFound(new { message = "Student not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting student", error = ex.Message });
            }
        }
    }
}
public record Student(int? Id, string Name, int Age, string Email);
