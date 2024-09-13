document.addEventListener('DOMContentLoaded', () => {
  
});

function loadCart() {
  fetch("/api/cart").then(r => r.json()).then(j => {
    ///// С ЭТОГО МОМЕНТА ПРОДОЛЖИТЬ
  });
}
