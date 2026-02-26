document.addEventListener('DOMContentLoaded', function () {

    const modalCambiarContrasena = document.getElementById('modalCambiarContrasena');
    const formCambiarContrasena = document.getElementById('formCambiarContrasena');
    const btnCambiarContrasena = document.getElementById('btnCambiarContrasena');
    const nuevaContrasenaInput = document.getElementById('nuevaContrasena');
    const toggleNuevaContrasena = document.getElementById('toggleNuevaContrasena');
    const idUsuarioContrasena = document.getElementById('idUsuarioContrasena');

    // Verificar si el modal existe, si no, salir de la función (no usar return)
    if (!modalCambiarContrasena) {
        return; // Este return está dentro de la función del DOMContentLoaded, es válido
    }

    modalCambiarContrasena.addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;
        idUsuarioContrasena.value = button.getAttribute('data-id');

        const nombre = button.getAttribute('data-nombre');
        modalCambiarContrasena.querySelector('.modal-title')
            .textContent = `Cambiar Contraseña - ${nombre}`;

        formCambiarContrasena.reset();
        nuevaContrasenaInput.type = 'password';
        toggleNuevaContrasena.querySelector('i').className = 'bi bi-eye';
    });

    toggleNuevaContrasena.addEventListener('click', function () {
        const isPassword = nuevaContrasenaInput.type === 'password';
        nuevaContrasenaInput.type = isPassword ? 'text' : 'password';
        this.querySelector('i').className = isPassword ? 'bi bi-eye-slash' : 'bi bi-eye';
    });

    function validarFortalezaContrasena(password) {
        const regex = /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!"#$%&'()*+,\-./:;<=>?@[\\\]^_`{|}~])[A-Za-z\d!"#$%&'()*+,\-./:;<=>?@[\\\]^_`{|}~]{8,}$/;
        return regex.test(password);
    }

    nuevaContrasenaInput.addEventListener('input', function () {
        this.classList.remove('is-valid', 'is-invalid');
        if (!this.value) return;

        this.classList.add(
            validarFortalezaContrasena(this.value) ? 'is-valid' : 'is-invalid'
        );
    });

    btnCambiarContrasena.addEventListener('click', async function () {
        if (!validarFortalezaContrasena(nuevaContrasenaInput.value)) {
            nuevaContrasenaInput.classList.add('is-invalid');
            return;
        }

        if (!formCambiarContrasena.checkValidity()) {
            formCambiarContrasena.reportValidity();
            return;
        }

        const response = await fetch('/Usuario/CambiarContrasena', {
            method: 'POST',
            headers: {
                'RequestVerificationToken':
                    document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: new FormData(formCambiarContrasena)
        });

        const result = await response.json();

        Swal.fire({
            icon: result.success ? 'success' : 'error',
            title: result.success ? 'Éxito' : 'Error',
            text: result.message
        });

        if (result.success) {
            bootstrap.Modal.getInstance(modalCambiarContrasena).hide();
        }
    });

});