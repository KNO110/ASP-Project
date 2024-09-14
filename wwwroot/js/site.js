document.addEventListener('submit', (e) => {
  const form = e.target;

  if (form.id == 'shop-group-form') {
    e.preventDefault();

    // Валидация формы перед отправкой
    const groupName = form.querySelector('[name="group-name"]').value.trim();
    const groupDescription = form.querySelector('[name="group-description"]').value.trim();

    if (!groupName || groupName.length > 100) {
      alert('Назва групи обов\'язкова та має бути не більше за 100 символів.');
      return;
    }
    if (!groupDescription || groupDescription.length > 500) {
      alert('Опис групи обов\'язковий й маж бути не більше за 500 символів.');
      return;
    }

    const formData = new FormData(form);
    fetch("/api/group", {
      method: 'POST',
      body: formData
    }).then(r => r.json()).then(j => {
      if (j.status == 'OK') {
        window.location.reload();
      } else if (j.message) {
        alert(j.message);
      } else {
        alert('Сталася помилка, дані не були відправлені.');
      }
    }).catch(err => {
      console.error('Помилка запиту:', err);
      alert('Помилка на стороні серверу.');
    });
  }

  else if (form.id == 'shop-product-form') {
    e.preventDefault();

    // Валидация формы перед отправкой
    const name = form.querySelector('[name="product-name"]').value.trim();
    const price = form.querySelector('[name="product-price"]').value;

    if (!name || name.length > 100) {
      alert('Назва продукта обов\'язкова та має бути не більше за 100 символів.');
      return;
    }
    if (!price || isNaN(price) || price <= 0) {
      alert('Ціна має бути додатнім числом.');
      return;
    }

    const formData = new FormData(form);
    fetch("/api/product", {
      method: 'POST',
      body: formData
    }).then(r => r.json()).then(j => {
      if (j.status == 'OK') {
        window.location.reload();
      } else if (j.message) {
        alert(j.message);
      } else {
        alert('Сталася помилка, дані не були відправлені');
      }
    }).catch(err => {
      console.error('Помилка запиту:', err);
      alert('Помилка на стороні серверу.');
    });
  }
});


document.addEventListener('DOMContentLoaded', () => {


  const authButton = document.getElementById("auth-button");
  if (authButton) authButton.addEventListener('click', authClick);
  else console.error("auth-button not found");

  ///Recover button

  const authRecoverButton = document.getElementById("auth-recover-button");
  if (authRecoverButton) authRecoverButton.addEventListener('click', authRecoverClick);
  else console.error("auth-recover-button not found");

  const logOutButton = document.getElementById("log-out-button");
  if (logOutButton) logOutButton.addEventListener('click', logOutClick);

  const profileEditButton = document.getElementById("profile-edit");
  if (profileEditButton) profileEditButton.addEventListener('click', profileEditClick);

  const profileDeleteButton = document.getElementById("profile-delete");
  if (profileDeleteButton) profileDeleteButton.addEventListener('click', profileDeleteClick);

  const productFeedbackButton = document.getElementById("product-feedback-button");
  if (productFeedbackButton) productFeedbackButton.addEventListener('click', productFeedbackClick);

  for (const btn of document.querySelectorAll('[data-role="feedback-edit"]')) {
    btn.addEventListener('click', feedbackEditClick);
  }
  for (const btn of document.querySelectorAll('[data-role="feedback-delete"]')) {
    btn.addEventListener('click', feedbackDeleteClick);
  }
  for (const btn of document.querySelectorAll('[data-role="feedback-restore"]')) {
    btn.addEventListener('click', feedbackRestoreClick);
  }
  for (const btn of document.querySelectorAll('[data-role="add-to-cart"]')) {
    btn.addEventListener('click', addToCartClick);
  }
  const profileAvatarEditButton = document.getElementById('profile-avatar-edit');
  if (profileAvatarEditButton) {
    profileAvatarEditButton.addEventListener('click', function () {
      var fileInput = document.createElement('input');
      fileInput.type = 'file';
      fileInput.accept = 'image/*';
      fileInput.style.display = 'none';

      fileInput.addEventListener('change', function () {
        var file = fileInput.files[0];
        if (file) {
          var formData = new FormData();
          formData.append('avatar', file);

          fetch('/Profile/ChangeAvatar', {
            method: 'POST',
            body: formData,
            headers: {
              'X-Requested-With': 'XMLHttpRequest'
            }
          })
            .then(response => response.json())
            .then(data => {
              if (data.success) {
                showSnackbar('Аватар успішно змінено.', 'success');
                window.location.reload();
              } else {
                showSnackbar(data.error, 'error');
              }
            })
            .catch(error => {
              console.error('Error:', error);
              showSnackbar('Сталася помилка при зміні аватара.', 'error');
            });
        }
      });

      document.body.appendChild(fileInput);
      fileInput.click();
      document.body.removeChild(fileInput);
    });
  }
});


function authRecoverClick() {
  if (!document.querySelector('[name="auth-user-date"]')) {
    const modalBody = document.querySelector('#authModal .modal-body');

    const dateFieldHtml = `
    <div class="row">
      <div class="input-group mb-3">
        <span class="input-group-text" id="auth-date-addon"><i class="bi bi-calendar"></i></span>
        <input type="date"
               name="auth-user-date"
               class="form-control"
               placeholder="Дата реєстрації"
               aria-label="Дата реєстрації" aria-describedby="auth-date-addon" />
      </div>
    </div>
    `;
    modalBody.insertAdjacentHTML('beforeend', dateFieldHtml);

    const authButton = document.getElementById("auth-button");
    authButton.innerText = "Відновити";
    authButton.setAttribute('data-recover-mode', 'true');

    const authRecoverButton = document.getElementById("auth-recover-button");
    authRecoverButton.style.display = 'none';
  }
}


function addToCartClick(e) {
  const btn = e.target.closest('[data-role="add-to-cart"]');
  const userId = btn.getAttribute("data-user-id");
  const productId = btn.getAttribute("data-product-id");
  if (!userId) {
    alert("Треба увійти до системи");
    return;
  }
  fetch("/api/cart", {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      userId,
      productId,
      cnt: 1
    })
  }).then(r => r.json()).then(console.log);

  console.log(userId, productId);
}

function feedbackRestoreClick(e) {
  const btn = e.target.closest('[data-feedback-id]');
  const feedbackId = btn.getAttribute("data-feedback-id");
  if (confirm("Дійсно бажаєте відновити відгук?")) {
    fetch("/api/feedback?id=" + feedbackId, {
      method: 'RESTORE'
    }).then(r => r.json()).then(j => {
      if (j.data === 'Restored') {
        window.location.reload();
      }
      else {
        alert("Трапилась якась помилка");
      }
    });
  }
}

function feedbackDeleteClick(e) {
  const btn = e.target.closest('[data-feedback-id]');
  const feedbackId = btn.getAttribute("data-feedback-id");
  if (confirm("Дійсно бажаєте видалити відгук?")) {
    fetch("/api/feedback?id=" + feedbackId, {
      method: 'DELETE'
    }).then(r => r.json()).then(j => {
      if (j.data === 'Deleted') {
        window.location.reload();
      }
      else {
        alert("Трапилась якась помилка");
      }
    });
  }
}

function feedbackEditClick(e) {
        ///// feedbackId - беремо з кнопки, що натискається
  const feedbackId = e.target.closest('[data-feedback-id]').getAttribute('data-feedback-id');
      ///// за знайденим feedbackId шукаємо текст коментаря та його оцінку
  let text = document
    .querySelector(`[data-feedback-id="${feedbackId}"][data-role="feedback-text"]`)
    .innerText;
  let rate = document
    .querySelector(`[data-feedback-id="${feedbackId}"][data-role="feedback-rate"]`)
    .getAttribute('data-value');
      //// переносимо дані у блок редагування
  document.getElementById("product-feedback-rate").value = rate;
  document.getElementById("product-feedback").value = text;
  document.getElementById("product-feedback-title").innerHTML =
    'Редагувати відгук: <button onclick="productFeedbackCancelEdit()" class="btn btn-danger"><i class="bi bi-x-lg"></i></button>';
      //// помічаємо "форму" - додаємо до кнопки додатковий атрибут
  document.getElementById("product-feedback-button").setAttribute('data-edit-id', feedbackId);
        //// console.log('Edit click ' + rate + ' ' + text);
}

function productFeedbackCancelEdit() {
  console.log("Edit cancelled");
}

function productFeedbackClick(e) {
  const txtarea = document.getElementById("product-feedback");
  const userId = txtarea.getAttribute("data-user-id");
  const productId = txtarea.getAttribute("data-product-id");
  const rate = document.getElementById("product-feedback-rate").value;
  var text = txtarea.value.trim();
  const editId = e.target.closest('button').getAttribute('data-edit-id');
  if (editId) {
    fetch("/api/feedback", {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        editId,
        text,
        rate
      })
    }).then(r => r.json()).then(j => {
      if (j.data === 'Updated') {
        window.location.reload();
      }
      else {
        alert("Трапилась якась помилка");
      }
    });
  }
  else {
    fetch("/api/feedback", {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        userId,
        productId,
        text,
        rate
      })
    }).then(r => r.json()).then(j => {
      if (j.data === 'Created') {
        window.location.reload();
      }
      else {
        alert("Трапилась якась помилка");
      }
    });
  }


  // console.log(userId, productId, text);
}

function profileDeleteClick(e) {
  if (confirm("Підтверджуєте видалення облікового запису?")) {
    fetch(
      "/api/auth", {
      method: "UNLINK",
    })
      .then(r => r.json())
      .then(j => {
        if (j.status == "OK") {
          alert("Для відновлення введіть дату реєстрації (" + j.registeredDate + ") та свій пароль");
          window.location = "/";
        }
        else {
          alert(j.message);
        }
      });
  }
}


function profileEditClick(e) {
  const btn = e.target;
  const isEditFinish = btn.classList.contains('bi-check2-square');  // <i class="bi bi-check2-square"></i>

  if (isEditFinish) {
    btn.classList.remove('bi-check2-square');
    btn.classList.add('bi-pencil-square');
  }
  else {
    btn.classList.add('bi-check2-square');
    btn.classList.remove('bi-pencil-square');
  }

  let changes = {};
  for (let elem of document.querySelectorAll('[profile-editable]')) {
    if (isEditFinish) {  // завершення редагування
      elem.removeAttribute('contenteditable');
      // треба визначити чи змінювався елемент
      if (elem.initialText != elem.innerText) {  // зміни є
        // дізнаємось назву поля, за яке він відповідає
        const fieldName = elem.getAttribute('profile-editable');
        // console.log(fieldName + ' -> ' + elem.innerText);
        changes[fieldName] = elem.innerText;
      }
    }
    else {  // початок редагування
      // переводимо елементи в режим редагування
      elem.setAttribute('contenteditable', 'true');
      // зберігаємо значення, що було на початок редагування
      elem.initialText = elem.innerText;  // initalText - нове поле, яке
      // JS дозволяє створювати під час роботи (ми самі його придумали)
    }
  }
  if (isEditFinish) {
    if (Object.keys(changes).length > 0) {
      console.log(changes);
      fetch("/api/auth", {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(changes)
      }).then(r => r.json())
        .then(j => {
          if (j.status == "OK") {
            alert(j.message);
          }
          else {
            // а) відновлення початкових даних, оскільки введені
            // призвели до помилки
            // for (let elem of document.querySelectorAll('[profile-editable]')) {
            //     elem.innerText = elem.initialText;
            // }
            // б) не вимикаємо режим редагування, точніше включаємо знову
            for (let elem of document.querySelectorAll('[profile-editable]')) {
              elem.setAttribute('contenteditable', 'true');
            }
            btn.classList.add('bi-check2-square');
            btn.classList.remove('bi-pencil-square');

            alert(j.message);
          }
        });
    }
    // else {
    //     console.log("No changes");
    // }
  }
}

function logOutClick() {
  fetch('/api/auth', {
    method: 'DELETE'
  }).then(r => location.reload());
}

function authClick() {
  const emailInput = document.querySelector('[name="auth-user-email"]');
  if (!emailInput) throw '[name="auth-user-email"] not found';

  const passwordInput = document.querySelector('[name="auth-user-password"]');
  if (!passwordInput) throw '[name="auth-user-password"] not found';

  const errorDiv = document.getElementById("auth-error");
  if (!errorDiv) throw '"auth-error" not found';
  errorDiv.show = err => {
    errorDiv.style.visibility = "visible";
    errorDiv.innerText = err;
  };
  errorDiv.hide = () => {
    errorDiv.style.visibility = "hidden";
    errorDiv.innerText = "";
  };

  const email = emailInput.value.trim();
  const password = passwordInput.value;

  if (email.length === 0) {
    errorDiv.show("Заповніть E-mail");
    return;
  }
  if (password.length === 0) {
    errorDiv.show("Заповніть пароль");
    return;
  }

  const authButton = document.getElementById("auth-button");
  const isRecoverMode = authButton.getAttribute('data-recover-mode') === 'true';

  if (isRecoverMode) {
    // Recovery mode
    const dateInput = document.querySelector('[name="auth-user-date"]');
    if (!dateInput) throw '[name="auth-user-date"] not found';
    const date = dateInput.value.trim();
    if (date.length === 0) {
      errorDiv.show("Заповніть дату реєстрації");
      return;
    }

    errorDiv.hide();
    fetch(`/api/auth`, {
      method: 'LINK',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        email: email,
        password: password,
        date: date
      })
    }).then(r => r.json()).then(j => {
      if (j.code != 200) {
        errorDiv.show("Відмова. Перевірьте введені дані.");
      }
      else {
        window.location.reload();
      }
    });
  }
  else {
    // Regular login mode
    errorDiv.hide();
    fetch(`/api/auth?email=${email}&password=${password}`, {
      method: 'GET'
    }).then(r => r.json()).then(j => {
      if (j.code != 200) {
        errorDiv.show("Відмова. Перевірьте введені дані.");
      }
      else {
        window.location.reload();
      }
    });
  }
}
/*
    fun() { ***** }

    --- await fun() -------
    ---  *****  -------

    --- fun().then(++++) -------
    --- | --------
        | ***** ++++ (C#)

    --- v --------   =============
                  | ***** ++++ (JS)
*/
