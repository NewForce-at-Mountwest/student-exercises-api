

const printPersonList = arrayOfPeople => {
  let personList = "";
  arrayOfPeople.forEach(
    person =>
      (personList += `<li>${person.firstName} ${person.lastName} || Slack handle: ${
        person.slackHandle
      }</li>`)
  );
  return personList;
};

const getAndPrintCohorts = () => {
  fetch("https://localhost:5001/api/cohorts")
    .then(cohorts => cohorts.json())
    .then(parsedCohorts => {
      parsedCohorts.forEach(cohort => {
        document.querySelector("#output").innerHTML += `<div>
                <h4>${cohort.name}</h4>
                <h5>Students</h5>
                <ul>${printPersonList(cohort.studentList)}</ul>
                <h5>Instructors</h5>
                <ul>${printPersonList(cohort.instructorList)}</ul>
            </div>`;
      });
    });
};

getAndPrintCohorts();
