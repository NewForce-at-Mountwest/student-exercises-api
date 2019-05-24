// Print all cohorts on page load
getAllCohorts().then(cohorts => {
  printCohortList(cohorts);
});

addAndPrintCohort = () => {
  const newCohort = {
    name: document.querySelector("#cohort-name-input").value
  };
  createCohort(newCohort)
    .then(getAllCohorts)
    .then(cohorts => printCohortList(cohorts));
};

deleteAndPrintCohorts = id => {
  deleteCohort(id)
    .then(getAllCohorts)
    .then(cohorts => printCohortList(cohorts));
};

startEditForm = id => {
  getOneCohort(id).then(cohort => {
    document.querySelector("#cohort-name-input").value = cohort.name;
    document.querySelector("#add").id = `submit-${id}`;
    document.querySelector(`#submit-${id}`).innerHTML = "Submit Edit";
  });
};

submitEditForm = id => {
  const modifiedCohort = {
    name: document.querySelector("#cohort-name-input").value
  };
  editCohort(id, modifiedCohort)
    .then(getAllCohorts)
    .then(cohorts => {
      printCohortList(cohorts)
      // Change the edit submit btn back to an add btn
      document.querySelector(`#submit-${id}`).id = "add";
      document.querySelector("#add").innerHTML = "Add";
    });
};

document.querySelector("body").addEventListener("click", () => {
  const id = event.target.id.split("-")[1];
  if (event.target.id == "add") {
    addAndPrintCohort();
  } else if (event.target.id.includes("edit")) {
    // Start editing
    startEditForm(id);
  } else if (event.target.id.includes("delete")) {
    deleteAndPrintCohorts(id);
  } else if (event.target.id.includes("submit")) {
    // Submit edit form
    submitEditForm(id);
  }
});
