//Darkmode js
let darkmode = localStorage.getItem('darkmode')
const darkmodeToggle = document.getElementById('darkmode-toggle')

const enableDarkmode = () => {
    document.body.classList.add('darkmode')
    localStorage.setItem('darkmode', 'active')
    darkmodeToggle.checked = true
}

const disableDarkmode = () => {
    document.body.classList.remove('darkmode')
    localStorage.setItem('darkmode', null)
    darkmodeToggle.checked = false
}

if (darkmode === "active") enableDarkmode()

darkmodeToggle.addEventListener("click", () => {
    darkmode = localStorage.getItem('darkmode')
    darkmode !== "active" ? enableDarkmode() : disableDarkmode()
})
