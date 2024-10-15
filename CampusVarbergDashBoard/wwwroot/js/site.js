

const header = document.querySelector('header');
const headerHeight = header.offsetHeight;
const hiddenNavbar = document.getElementById('hiddenNavbar')


function onScroll() {
    if (window.pageYOffset > headerHeight) {
        hiddenNavbar.style.display = 'block';
    }
    else {
        hiddenNavbar.style.display = 'none';
    }
}

window.addEventListener('scroll', onScroll);

