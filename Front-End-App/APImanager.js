const createCohort = cohortObject => {
  return fetch("https://localhost:5001/api/cohorts", {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(cohortObject)
  });
};

const deleteCohort = cohortId => {
  return fetch(`https://localhost:5001/api/cohorts/${cohortId}`, {
    method: "DELETE"
  });
};
const getAllCohorts = () => {
  return fetch("https://localhost:5001/api/cohorts").then(cohorts =>
    cohorts.json()
  );
};

const getOneCohort = cohortId =>
  fetch(`https://localhost:5001/api/cohorts/${cohortId}`).then(singleCohort =>
    singleCohort.json()
  );



const editCohort = (idParam, cohortObject) => {
  return fetch(`https://localhost:5001/api/cohorts/${idParam}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(cohortObject)
  });
};
