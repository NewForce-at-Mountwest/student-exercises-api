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
    public class InstructorsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public InstructorsController(IConfiguration config)
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
                        SELECT i.Id AS 'Instructor Id', 
                        i.firstName AS 'Instructor First Name', 
                        i.lastName AS 'Instructor Last Name', 
                        i.slackHandle AS 'Slack Handle', 
                        c.name AS 'Cohort Name', 
                        c.Id AS 'Cohort Id' 
                        FROM Instructor i
                        JOIN Cohort c ON i.cohortId = c.Id";


                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Instructor> Instructors = new List<Instructor>();

                    while (reader.Read())
                    {

                        Instructor Instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Instructor Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("Instructor First Name")),
                            LastName = reader.GetString(reader.GetOrdinal("Instructor Last Name")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("Slack Handle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("Cohort Id")),
                            CurrentCohort = new Cohort()
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Cohort Id")),
                                name = reader.GetString(reader.GetOrdinal("Cohort Name")),
                            },
                        };

                        Instructors.Add(Instructor);
                    }
                    reader.Close();


                    return Ok(Instructors);
                }
            }
        }

        [HttpGet("{id}", Name = "GetInstructor")]
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
                        FROM Instructor
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor Instructor = null;

                    if (reader.Read())
                    {
                        Instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("firstName")),
                            LastName = reader.GetString(reader.GetOrdinal("lastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("slackHandle"))
                        };
                    }
                    reader.Close();

                    return Ok(Instructor);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Instructor Instructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Instructor (firstName, lastName, slackHandle, cohortId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstName, @lastName, @slackHandle, @cohortId)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", Instructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", Instructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", Instructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", Instructor.CohortId));



                    int newId = (int)cmd.ExecuteScalar();
                    Instructor.Id = newId;
                    return CreatedAtRoute("GetInstructor", new { id = newId }, Instructor);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Instructor Instructor)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Instructor
                                            SET firstName=@firstName, 
                                            lastName=@lastName, 
                                            slackHandle=@slackHandle, 
                                            cohortId=@cohortId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstName", Instructor.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", Instructor.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", Instructor.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", Instructor.CohortId));
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
                if (!InstructorExists(id))
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
                        cmd.CommandText = @"DELETE FROM Instructor WHERE Id = @id";
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
                if (!InstructorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool InstructorExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, firstName, lastName, slackHandle, cohortId
                        FROM Instructor
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}