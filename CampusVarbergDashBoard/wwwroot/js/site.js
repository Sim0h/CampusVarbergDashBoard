

const header = document.querySelector('header');
const headerHeight = header.offsetHeight;
const hiddenNavbar = document.getElementById('hiddenNavbar')


function onScroll() {
    const pageWidth = window.innerWidth
    if (pageWidth > 576) {
        if (window.pageYOffset > headerHeight) {
            hiddenNavbar.style.display = 'block';
        }
        else {
            hiddenNavbar.style.display = 'none';
        }
    }
}

window.addEventListener('scroll', onScroll);

