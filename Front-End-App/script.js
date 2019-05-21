document.querySelector("#add-btn").addEventListener("click", () => {
    const name = document.querySelector("#cohort-name").value;
    console.log(name);
})


const getAndPrintCoffees = () => {
    fetch('https://localhost:5001/api/cohorts')
    .then(cohorts => cohorts.json())
    .then(parsedCohorts => {
        console.log(parsedCohorts);
        // parsedCohorts.forEach(cohorts => {
        //     document.querySelector("#output").innerHTML += `<div>
        //         <h5>${cohorts.Name}</h5>
        //         <p>${cohorts.BeanType}</p>
        //     </div>`
        // })
    })
}

getAndPrintCoffees();
