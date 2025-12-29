<script>
    document.addEventListener('DOMContentLoaded', function () {
        const modalCambiarContrasena = document.getElementById('modalCambiarContrasena');
    const formCambiarContrasena = document.getElementById('formCambiarContrasena');
    const btnCambiarContrasena = document.getElementById('btnCambiarContrasena');
    const nuevaContrasenaInput = document.getElementById('nuevaContrasena');
    const toggleNuevaContrasena = document.getElementById('toggleNuevaContrasena');
    const idUsuarioContrasena = document.getElementById('idUsuarioContrasena');

    // Configurar modal cuando se abre
    modalCambiarContrasena.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
    const id = button.getAttribute('data-id');
    const nombre = button.getAttribute('data-nombre');

    // Establecer el ID del usuario
    idUsuarioContrasena.value = id;

    // Actualizar título del modal con el nombre del empleado
    const modalTitle = modalCambiarContrasena.querySelector('.modal-title');
    modalTitle.textContent = `Cambiar Contraseña - ${nombre}`;

    // Limpiar y resetear formulario
    formCambiarContrasena.reset();
    nuevaContrasenaInput.classList.remove('is-invalid', 'is-valid');

    // Restaurar icono de visibilidad
    toggleNuevaContrasena.querySelector('i').className = 'bi bi-eye';
    nuevaContrasenaInput.type = 'password';

    // Remover mensajes de validación anteriores
    const feedbackElements = formCambiarContrasena.querySelectorAll('.invalid-feedback, .valid-feedback');
            feedbackElements.forEach(el => el.remove());

    // Habilitar botón
    btnCambiarContrasena.disabled = false;
    btnCambiarContrasena.innerHTML = 'Cambiar Contraseña';
        });

    // Función para mostrar/ocultar contraseña
    toggleNuevaContrasena.addEventListener('click', function() {
            const type = nuevaContrasenaInput.getAttribute('type') === 'password' ? 'text' : 'password';
    nuevaContrasenaInput.setAttribute('type', type);

    // Cambiar icono
    const icon = this.querySelector('i');
    icon.className = type === 'password' ? 'bi bi-eye' : 'bi bi-eye-slash';
        });

    // Validar fortaleza de contraseña en tiempo real
    function validarFortalezaContrasena(password) {
            const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#._-])[A-Za-z\d@$!%*?&#._-]{8,}$/;
    return regex.test(password);
        }

    // Mostrar feedback visual de validación
    function mostrarFeedbackContrasena(esValida, mensaje) {
        // Remover feedback anterior
        nuevaContrasenaInput.classList.remove('is-invalid', 'is-valid');

    const feedbackExistente = nuevaContrasenaInput.nextElementSibling;
    if (feedbackExistente && (feedbackExistente.classList.contains('invalid-feedback') ||
    feedbackExistente.classList.contains('valid-feedback'))) {
        feedbackExistente.remove();
            }

    if (esValida) {
        nuevaContrasenaInput.classList.add('is-valid');

    const validFeedback = document.createElement('div');
    validFeedback.className = 'valid-feedback mt-1';
    validFeedback.textContent = mensaje || '✓ Contraseña válida';
    nuevaContrasenaInput.parentNode.insertBefore(validFeedback, nuevaContrasenaInput.nextSibling);
            } else {
        nuevaContrasenaInput.classList.add('is-invalid');

    const invalidFeedback = document.createElement('div');
    invalidFeedback.className = 'invalid-feedback mt-1';
    invalidFeedback.textContent = mensaje || 'Contraseña no cumple los requisitos';
    nuevaContrasenaInput.parentNode.insertBefore(invalidFeedback, nuevaContrasenaInput.nextSibling);
            }
        }

    // Validar contraseña al escribir
    nuevaContrasenaInput.addEventListener('input', function() {
            const password = this.value;

    if (password.length === 0) {
        nuevaContrasenaInput.classList.remove('is-invalid', 'is-valid');
    return;
            }

    if (validarFortalezaContrasena(password)) {
        mostrarFeedbackContrasena(true, '✓ Contraseña segura');
            } else {
        mostrarFeedbackContrasena(false,
            'La contraseña debe tener: 8+ caracteres, 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial (@$!%*?&#._-)');
            }
        });

    // Enviar formulario
    btnCambiarContrasena.addEventListener('click', async function() {
            // Validar que haya un ID de usuario
            if (!idUsuarioContrasena.value) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No se ha especificado el usuario'
        });
    return;
            }

    // Validar formulario
    if (!formCambiarContrasena.checkValidity()) {
        // Mostrar validación nativa de HTML5
        formCambiarContrasena.reportValidity();
    return;
            }

    const nuevaContrasena = nuevaContrasenaInput.value;

    // Validar fortaleza de contraseña
    if (!validarFortalezaContrasena(nuevaContrasena)) {
        mostrarFeedbackContrasena(false,
            'La contraseña debe tener: 8+ caracteres, 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial (@$!%*?&#._-)');
    nuevaContrasenaInput.focus();
    return;
            }

    // Deshabilitar botón y mostrar loading
    const originalText = this.innerHTML;
    this.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Procesando...';
    this.disabled = true;

    try {
                const formData = new FormData(formCambiarContrasena);

    // Si necesitas agregar más datos al formData
    // formData.append('campoExtra', 'valor');

    const response = await fetch('/Usuario/CambiarContrasena', {
        method: 'POST',
    headers: {
        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value,
    'Content-Type': 'application/x-www-form-urlencoded'
                    },
    body: new URLSearchParams(formData)
                });

    let result;

    try {
        result = await response.json();
                } catch (jsonError) {
                    throw new Error('Respuesta del servidor no válida');
                }

    if (result.success) {
                    const modal = bootstrap.Modal.getInstance(modalCambiarContrasena);
    modal.hide();

    Swal.fire({
        icon: 'success',
    title: '¡Éxito!',
    text: result.message || 'Contraseña cambiada correctamente',
    timer: 3000,
    showConfirmButton: false,
                        willClose: () => {
                            // Opcional: recargar la página o actualizar datos
                            if (result.redirectUrl) {
        window.location.href = result.redirectUrl;
                            }
                        }
                    });
                } else {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: result.message || 'Error al cambiar la contraseña'
        });
                }
            } catch (error) {
        console.error('Error:', error);
    Swal.fire({
        icon: 'error',
    title: 'Error de conexión',
    text: 'No se pudo completar la operación. Intente nuevamente.'
                });
            } finally {
        // Restaurar botón
        this.innerHTML = originalText;
    this.disabled = false;
            }
        });

    // Cerrar modal con Escape key
    document.addEventListener('keydown', function(event) {
            if (event.key === 'Escape' && modalCambiarContrasena.classList.contains('show')) {
                const modalInstance = bootstrap.Modal.getInstance(modalCambiarContrasena);
    if (modalInstance) {
        modalInstance.hide();
                }
            }
        });

    // Limpiar formulario cuando se cierra el modal
    modalCambiarContrasena.addEventListener('hidden.bs.modal', function() {
        formCambiarContrasena.reset();
    nuevaContrasenaInput.classList.remove('is-invalid', 'is-valid');
        });
    });
</script>