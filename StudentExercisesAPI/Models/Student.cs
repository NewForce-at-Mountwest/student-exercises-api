/*You must define a type for representing a student in code. A student can only be in one cohort at a time. A student can be working on many exercises at a time.

Properties
First name
Last name
Slack handle
The student's cohort
The collection of exercises that the student is currently working on */

using System;
using System.Collections.Generic;

namespace StudentExercisesAPI.Models{
    public class Student : Person{

 
   


        public List<Exercise> Exercises {get; set;} = new List<Exercise>();

    }
}