/*
Exercise
You must define a type for representing an exercise in code. An exercise can be assigned to many students.

Name of exercise
Language of exercise (JavaScript, Python, CSharp, etc.) */

using System;
using System.Collections.Generic;

namespace StudentExercisesAPI.Models{
    public class Exercise{
        public int id {get; set;}

        public string Name {get; set;}

        public string Language {get; set;}

        public List<Student> assignedStudents { get; set; }  = new List<Student>();
    }
}