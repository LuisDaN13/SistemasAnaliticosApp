    // Constantes para evitar strings mágicos
    const MODAL_ID = 'firstLoginPasswordModal';
    const PASSWORD_PATTERN = /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!"#$%&'()*+,\-./:;<=>?@[\\\]^_`{|}~])[A-Za-z\d!"#$%&'()*+,\-./:;<=>?@[\\\]^_`{|}~]{8,}$/;

    // Estado de la aplicación
    const state = {
        userId: window.firstLoginConfig?.userId ?? '',
        showModal: window.firstLoginConfig?.showModal === 'true'
    };

    // Inicialización
    initializeModal();
    initializePasswordToggles();
    initializePasswordChangeHandler();

    function initializeModal() {
        if (state.showModal && state.userId) {
            setTimeout(() => {
                const modalElement = document.getElementById(MODAL_ID);
                if (modalElement) {
                    const modal = new bootstrap.Modal(modalElement, {
                        backdrop: 'static',
                        keyboard: false
                    });
                    modal.show();
                }
            }, 1000);
        }
    }

    function initializePasswordToggles() {
        // Usar event delegation para mejor rendimiento
        $(document).on('click', '#toggleFirstLoginPassword, #toggleFirstLoginConfirm', function () {
            const inputId = this.id === 'toggleFirstLoginPassword'
                ? '#firstLoginNewPassword'
                : '#firstLoginConfirmPassword';
            togglePasswordVisibility($(inputId), $(this).find('i'));
        });
    }

    function initializePasswordChangeHandler() {
        $('#btnFirstLoginChangePassword').click(() => handlePasswordChange(state.userId));
    }

    function togglePasswordVisibility(input, icon) {
        const isPassword = input.attr('type') === 'password';
        input.attr('type', isPassword ? 'text' : 'password');
        icon.toggleClass('bi-eye bi-eye-slash');
    }

    function validatePassword(newPass, confirmPass) {
        const errors = [];

        if (!newPass?.trim() || !confirmPass?.trim()) {
            errors.push('Ambos campos son obligatorios.');
        }

        if (newPass !== confirmPass) {
            errors.push('Las contraseñas no coinciden.');
        }

        if (newPass && !PASSWORD_PATTERN.test(newPass)) {
            errors.push('La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.');
        }

        return errors;
    }

    function showError(message) {
        $('#firstLoginPasswordError').text(message).show();
    }

    function clearError() {
        $('#firstLoginPasswordError').text('').hide();
    }

    function setButtonLoading(button, isLoading, originalText = '') {
        button.prop('disabled', isLoading);
        if (isLoading) {
            button.data('original-text', button.html());
            button.html('<span class="spinner-border spinner-border-sm"></span> Procesando...');
        } else {
            button.html(button.data('original-text') || originalText);
        }
    }

    function handlePasswordChange(userId) {
        const newPass = $('#firstLoginNewPassword').val();
        const confirmPass = $('#firstLoginConfirmPassword').val();
        const $btn = $('#btnFirstLoginChangePassword');

        clearError();

        // Validaciones
        const errors = validatePassword(newPass, confirmPass);
        if (errors.length > 0) {
            showError(errors.join(' '));
            return;
        }

        // Deshabilitar botón
        setButtonLoading($btn, true);

        // Obtener token de verificación
        const token = $('input[name="__RequestVerificationToken"]').val();
        if (!token) {
            showError('Error de seguridad. Recargue la página.');
            setButtonLoading($btn, false);
            return;
        }

        // Enviar AJAX
        $.ajax({
            url: window.firstLoginConfig?.cambiarContrasenaUrl ?? '/Usuario/CambiarContrasena2',
            type: 'POST',
            data: {
                Id: userId,
                NuevaContrasena: newPass,
                __RequestVerificationToken: token
            },
            success: handleSuccessResponse,
            error: handleErrorResponse,
            complete: () => setButtonLoading($btn, false)
        });
    }

    function handleSuccessResponse(response) {
        if (response.success) {
            showSuccessMessage(response);
        } else {
            showError(response.message || 'Error al cambiar la contraseña.');
        }
    }

    function handleErrorResponse(xhr) {
        let errorMessage = 'Error de conexión. Intente nuevamente.';

        if (xhr.responseJSON?.message) {
            errorMessage = xhr.responseJSON.message;
        } else if (xhr.status === 400) {
            errorMessage = 'Solicitud inválida.';
        } else if (xhr.status === 401) {
            errorMessage = 'Sesión expirada. Inicie sesión nuevamente.';
        } else if (xhr.status === 500) {
            errorMessage = 'Error del servidor. Intente más tarde.';
        }

        showError(errorMessage);
    }

    function showSuccessMessage(response) {
        const config = {
            icon: 'success',
            title: response.logout ? 'Contraseña Cambiada' : '¡Éxito!',
            text: response.message,
            confirmButtonText: 'OK'
        };

        Swal.fire(config).then(() => {
            if (response.logout) {
                window.location.href = window.firstLoginConfig?.loginUrl ?? '/Usuario/Login';
            } else {
                $(`#${MODAL_ID}`).modal('hide');
                window.location.reload();
            }
        });
    }
