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

namespace ExerciseExercisesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExercisesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ExercisesController(IConfiguration config)
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
        public async Task<IActionResult> Get(string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = "";
                    string exerciseColumns = "SELECT e.Id, e.name, e.language";
                    string exerciseTable = "FROM Exercise e";

                    if (include == "student")
                    {
                        string studentColumns = @", 
                        s.Id AS 'Student Id', 
                        s.firstName AS 'Student First Name', 
                        s.lastName AS 'Student Last Name',
                        s.slackHandle AS 'Slack Handle'";
                        string studentsTable = @"
                         JOIN StudentExercise se ON e.Id = se.exerciseId 
                        JOIN Student s ON se.StudentId=s.Id;";
                        command = $@"{exerciseColumns} 
                                    {studentColumns} 
                                    {exerciseTable} 
                                    {studentsTable}";

                    }
                    else
                    {
                        command = $"{exerciseColumns} {exerciseTable}";
                    }

                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Exercise> Exercises = new List<Exercise>();

                    while (reader.Read())
                    {

                        Exercise Exercise = new Exercise
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Language = reader.GetString(reader.GetOrdinal("language"))

                        };

                        if(include == "student")
                        {
                            Student currentStudent = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Student Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("Student First Name")),
                                LastName = reader.GetString(reader.GetOrdinal("Student Last Name")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("Slack Handle"))
                            };

                            if (Exercises.Any(e => e.id == Exercise.id))
                            {
                                Exercise thisExercise = Exercises.Where(e => e.id == Exercise.id).FirstOrDefault();
                                thisExercise.assignedStudents.Add(currentStudent);
                            }
                            else
                            {
                                Exercise.assignedStudents.Add(currentStudent);
                                Exercises.Add(Exercise);

                            }

                        } else
                        {
                            Exercises.Add(Exercise);

                        }
                        
                    }
                    reader.Close();


                    return Ok(Exercises);
                }
            }
        }

        [HttpGet("{id}", Name = "GetExercise")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, name, language
                        FROM Exercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Exercise Exercise = null;

                    if (reader.Read())
                    {
                        Exercise = new Exercise
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Language = reader.GetString(reader.GetOrdinal("lastName"))

                        };
                    }
                    reader.Close();

                    return Ok(Exercise);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Exercise Exercise)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Exercise (name, language)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @lang)";
                    cmd.Parameters.Add(new SqlParameter("@name", Exercise.Name));
                    cmd.Parameters.Add(new SqlParameter("@lang", Exercise.Language));
                    int newId = (int)cmd.ExecuteScalar();
                    Exercise.id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, Exercise);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Exercise Exercise)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Exercise
                                            SET name=@n, 
                                            language=@lang, 
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@n", Exercise.Name));
                        cmd.Parameters.Add(new SqlParameter("@lang", Exercise.Language));
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
                if (!ExerciseExists(id))
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
                        cmd.CommandText = @"DELETE FROM Exercise WHERE Id = @id";
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
                if (!ExerciseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ExerciseExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            name, language
                        FROM Exercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}