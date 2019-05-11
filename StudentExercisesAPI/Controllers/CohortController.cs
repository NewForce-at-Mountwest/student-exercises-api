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

namespace CohortCohortsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CohortsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CohortsController(IConfiguration config)
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
                    string command = @"SELECT c.Id AS 'Cohort Id',
                    c.name AS 'Cohort Name',
                    s.Id AS 'Student Id',
                    s.firstName AS 'Student First Name',
                    s.LastName AS 'Student Last Name',
                    s.slackHandle AS 'Student Slack Handle',
                    i.Id AS 'Instructor Id',
                    i.firstName AS 'Instructor First Name',
                    i.LastName AS 'Instructor Last Name',
                    i.slackHandle AS 'Instructor Slack Handle'
                    FROM Cohort c 
                    JOIN Instructor i ON c.Id = i.cohortId
                    JOIN Student s ON c.Id = s.cohortId";


                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Cohort> Cohorts = new List<Cohort>();

                    while (reader.Read())
                    {

                        Cohort currentCohort = new Cohort
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Cohort Id")),
                            name = reader.GetString(reader.GetOrdinal("Cohort Name")),
                        };

                        Student currentStudent = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Student Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("Student First Name")),
                            LastName = reader.GetString(reader.GetOrdinal("Student Last Name")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("Student Slack Handle"))
                        };

                        Instructor currentInstructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Instructor Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("Instructor First Name")),
                            LastName = reader.GetString(reader.GetOrdinal("Instructor Last Name")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("Instructor Slack Handle"))

                        };


                        // If the cohorts list already has the current cohort in it, don't add it again!
                        if(Cohorts.Any(c => c.id == currentCohort.id))
                        {
                            // Find the cohort in the list if it's already there
                            Cohort cohortToReference = Cohorts.Where(c => c.id == currentCohort.id).FirstOrDefault();

                            // Does the student already exist in the student list? If not, add them
                            if (!cohortToReference.StudentList.Any(s => s.Id == currentStudent.Id))
                            {
                                cohortToReference.StudentList.Add(currentStudent);
                            }

                            // Does the instructor already exist in the instructor list? If not, add them.
                            if (!cohortToReference.InstructorList.Any(i => i.Id == currentInstructor.Id))
                            {
                                cohortToReference.InstructorList.Add(currentInstructor);
                            }
                            //

                        } else
                        {
                            // If the cohort isn't already in the cohort list, let's add it
                            currentCohort.StudentList.Add(currentStudent);
                            currentCohort.InstructorList.Add(currentInstructor);
                            Cohorts.Add(currentCohort);


                        }

                        
                    }
                    reader.Close();


                    return Ok(Cohorts);
                }
            }
        }

        [HttpGet("{id}", Name = "GetCohort")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, name
                        FROM Cohort
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort Cohort = null;

                    if (reader.Read())
                    {
                        Cohort = new Cohort
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id")),
                            name = reader.GetString(reader.GetOrdinal("name"))

                        };
                    }
                    reader.Close();

                    return Ok(Cohort);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Cohort Cohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Cohort (name)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @lang)";
                    cmd.Parameters.Add(new SqlParameter("@name", Cohort.name));
                    int newId = (int)cmd.ExecuteScalar();
                    Cohort.id = newId;
                    return CreatedAtRoute("GetCohort", new { id = newId }, Cohort);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Cohort Cohort)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Cohort
                                            SET name=@n 
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@n", Cohort.name));
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
                if (!CohortExists(id))
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
                        cmd.CommandText = @"DELETE FROM Cohort WHERE Id = @id";
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
                if (!CohortExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool CohortExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            name
                        FROM Cohort
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}