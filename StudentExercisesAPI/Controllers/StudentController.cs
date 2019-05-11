using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using StudentExercisesAPI.Models;
using Microsoft.AspNetCore.Http;

namespace StudentExercisesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = @"
                        SELECT s.Id AS 'Student Id', 
                        s.firstName AS 'Student First Name', 
                        s.lastName AS 'Student Last Name', 
                        s.slackHandle AS 'Slack Handle', 
                        e.name AS 'Exercise Name', 
                        e.language AS 'Exercise Language', 
                        e.Id AS 'Exercise Id', 
                        c.name AS 'Cohort Name', 
                        c.Id AS 'Cohort Id' 
                        FROM Student s 
                        JOIN Cohort c ON s.cohortId = c.Id 
                        JOIN StudentExercise se ON s.Id = se.studentId 
                        JOIN Exercise e ON se.exerciseId=e.Id;";


                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Student> Students = new List<Student>();

                    while (reader.Read())
                    {
                        
                        Student currentStudent = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Student Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("Student First Name")),
                            LastName = reader.GetString(reader.GetOrdinal("Student Last Name")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("Slack Handle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("Cohort Id")),
                            CurrentCohort = new Cohort()
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Cohort Id")),
                                name = reader.GetString(reader.GetOrdinal("Cohort Name")),
                            },
                        };

                        Exercise currentExercise = new Exercise
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Exercise Id")),
                            Name = reader.GetString(reader.GetOrdinal("Exercise Name")),
                            Language = reader.GetString(reader.GetOrdinal("Exercise Language"))

                        };

                        // If the student is already on the list, don't add them again!
                        if (Students.Any(s => s.Id ==  currentStudent.Id))
                        {
                            Student thisStudent = Students.Where(s => s.Id == currentStudent.Id).FirstOrDefault();
                            thisStudent.Exercises.Add(currentExercise);

                        } else
                        {
                            currentStudent.Exercises.Add(currentExercise);
                            Students.Add(currentStudent);
                           
                        }
                        
                    }
                    reader.Close();


                    return Ok(Students);
                }
            }
        }

        [HttpGet("{id}", Name = "GetStudent")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, firstName, lastName, slackHandle, cohortId
                        FROM Student
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student Student = null;

                    if (reader.Read())
                    {
                        Student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("firstName")),
                            LastName = reader.GetString(reader.GetOrdinal("lastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("slackHandle"))
                        };
                    }
                    reader.Close();

                    return Ok(Student);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student Student)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Student (firstName, lastName, slackHandle, cohortId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstName, @lastName, @slackHandle, @cohortId)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", Student.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", Student.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", Student.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", Student.CohortId));



                    int newId = (int)cmd.ExecuteScalar();
                    Student.Id = newId;
                    return CreatedAtRoute("GetStudent", new { id = newId }, Student);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Student Student)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Student
                                            SET firstName=@firstName, 
                                            lastName=@lastName, 
                                            slackHandle=@slackHandle, 
                                            cohortId=@cohortId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstName", Student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", Student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", Student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", Student.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Student WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool StudentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, firstName, lastName, slackHandle, cohortId
                        FROM Student
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}