document.addEventListener('DOMContentLoaded', () => {
  loadCart();
});

function loadCart() {
  const container = document.getElementById("cart-container");
  if (!container) throw "#cart-container not found";
  container.innerHTML = "";

  const userId = container.getAttribute("data-user-id");

  fetch("/api/cart?id=" + userId)
    .then(r => r.json())
    .then(j => {
      let html = "";
      if (j.data == null || j.data.cartProducts == null || j.data.cartProducts.length == 0) {
        html = "Кошик порожній";
        const cartTotalItems = document.getElementById('cart-total-items');
        if (cartTotalItems) {
          cartTotalItems.innerText = 0;
        }
      }
      else {
        let total = 0;
        let totalCnt = 0;
        html = '<div class="row"><div class="col col-8">';
        for (let cartProduct of j.data.cartProducts) {
          html += `<div class="row my-2 cart-product-item" data-cp-id="${cartProduct.id}">
                    <div class="col col-2">
                        <img src="/Home/Download/Shop_${cartProduct.product.image}" alt="Picture"/>
                    </div>
                    <div class="col col-8 cart-product-info">
                        <p>${cartProduct.product.name}</p>
                        <p class="text-muted">${cartProduct.product.description}</p>
                    </div>
                    <div class="col col-2 cart-product-calc">
                        <div class="d-flex justify-content-between align-items-center">
                            <button class="decrement-btn">-</button>
                            <b data-role="cart-product-cnt">${cartProduct.cnt}</b>
                            <button class="increment-btn">+</button>
                        </div>
                        <div class="text-center mx-auto cart-product-sum" >
                            ₴ <span data-role="cart-product-sum"
                                data-price="${cartProduct.product.price}">${(cartProduct.cnt * cartProduct.product.price).toFixed(2)}</span>
                        </div>
                    </div>
                </div>`;
          total += cartProduct.cnt * cartProduct.product.price;
          totalCnt += cartProduct.cnt;
        }
        if (totalCnt > 0) {
          html += `<div class="d-flex align-items-center justify-content-center my-2">
                    <b>Разом: <span data-role="cart-total">${total.toFixed(2)}</span> грн</b>
                </div>`;
        }

        html += '</div></div>';

        const cartTotalItems = document.getElementById('cart-total-items');
        if (cartTotalItems) {
          cartTotalItems.innerText = totalCnt;
        }
      }
      container.innerHTML = html;

      addCartEventListeners();
    });
}

function addCartEventListeners() {
  const decrementButtons = document.querySelectorAll('.decrement-btn');
  const incrementButtons = document.querySelectorAll('.increment-btn');

  decrementButtons.forEach(button => {
    button.addEventListener('click', decrementClick);
  });

  incrementButtons.forEach(button => {
    button.addEventListener('click', incrementClick);
  });
}

function updateTotal() {
  let total = 0;
  let totalCnt = 0;
  for (let s of document.querySelectorAll('[data-role="cart-product-sum"]')) {
    total += Number(s.innerHTML);
  }
  for (let cnt of document.querySelectorAll('[data-role="cart-product-cnt"]')) {
    totalCnt += Number(cnt.innerHTML);
  }
  document.querySelector('[data-role="cart-total"]').innerHTML = total.toFixed(2);

  const cartTotalItems = document.getElementById('cart-total-items');
  if (cartTotalItems) {
    cartTotalItems.innerText = totalCnt;
  }
}

function decrementClick(e) {
  updateCart(e, -1);
}

function incrementClick(e) {
  updateCart(e, 1);
}

function updateCart(e, increment) {
  const parentBlock = e.target.closest("[data-cp-id]");
  const cpId = parentBlock.getAttribute("data-cp-id");
  fetch(`/api/cart?increment=${increment}&cpId=${cpId}`, {
    method: 'PUT'
  }).then(r => r.json()).then(j => {
    if (j.meta.count == 0) {
      loadCart();
      return;
    }
    const cntBlock = parentBlock.querySelector('[data-role="cart-product-cnt"]');
    cntBlock.innerHTML = j.meta.count;
    const sumBlock = parentBlock.querySelector('[data-role="cart-product-sum"]');
    sumBlock.innerHTML = (j.meta.count * sumBlock.getAttribute('data-price')).toFixed(2);
    updateTotal();
  });
}
