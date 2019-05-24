
const printPersonList = arrayOfPeople => {
    let personList = "";
    if(arrayOfPeople.length > 0){
        arrayOfPeople.forEach(
            person =>
              (personList += `<li>${person.firstName} ${person.lastName} || Slack handle: ${
                person.slackHandle
              }</li>`)
          );

    }
    return personList;
  };

  const printCohortList = arrayOfCohorts => {
    document.querySelector("#output").innerHTML = "";
    document.querySelector("#cohort-name-input").value = "";
    arrayOfCohorts.forEach(cohort => {
        document.querySelector("#output").innerHTML += `<div class="cohort-box">
                <h4>${cohort.name}</h4>
                <h5>Students</h5>
                <ul>${printPersonList(cohort.studentList)}</ul>
                <h5>Instructors</h5>
                <ul>${printPersonList(cohort.instructorList)}</ul>
                <button id="edit-${cohort.id}">Edit</button>
                <button id="delete-${cohort.id}">Delete</button>
            </div>`;
      });
  }