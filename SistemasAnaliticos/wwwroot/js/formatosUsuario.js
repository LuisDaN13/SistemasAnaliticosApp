
//  FORMATO CÉDULA / DIMEX
const identificacionInput = document.getElementById("identificacion");
const tipoIdentificacion = document.getElementById("tipoId");

function formatearIdentificacion(valor) {
    let value = valor.replace(/\D/g, "");

    if (tipoIdentificacion.value === "cedula") {
        if (value.length > 9) value = value.substring(0, 9);

        if (value.length > 5)
    return value.substring(0,1) + "-" +
    value.substring(1,5) + "-" +
    value.substring(5);
        else if (value.length > 1)
    return value.substring(0,1) + "-" + value.substring(1);
    else
    return value;
    }

    if (tipoIdentificacion.value === "dimex") {
        if (value.length > 12) value = value.substring(0, 12);
    return value;
    }

    return value;
}

// Al escribir
if (identificacionInput && tipoIdentificacion) {
    identificacionInput.addEventListener("input", function () {
        this.value = formatearIdentificacion(this.value);
    });

    // Al cambiar tipo
    tipoIdentificacion.addEventListener("change", function () {
        identificacionInput.value = formatearIdentificacion(identificacionInput.value);
    });

    // 🔥 IMPORTANTE: formatear valor inicial (EDIT)
    identificacionInput.value = formatearIdentificacion(identificacionInput.value);
}


//  FORMATO SALARIO
document.querySelectorAll(".salario-cr").forEach(input => {

    function formatearSalario(valor) {
        let value = valor.replace(/\D/g, "");
        if (value === "") return "";
        return value.replace(/\B(?=(\d{3})+(?!\d))/g, " ");
    }

    input.addEventListener("input", function () {
        this.value = formatearSalario(this.value);
    });

    // 🔥 Formatear valor inicial (EDIT)
    input.value = formatearSalario(input.value);
});

//  FORMATO TELÉFONO COSTA RICA

document.querySelectorAll(".celular-cr").forEach(input => {
    function formatearTelefono(valor) {
        let value = valor.replace(/\D/g, "");
        if (value.length > 8) value = value.substring(0, 8);

        if (value.length > 4)
            return value.substring(0, 4) + " " + value.substring(4);
        else
            return value;
    }

    input.addEventListener("input", function () {
        this.value = formatearTelefono(this.value);
    });

    // 🔥 Formatear valor inicial (EDIT)
    input.value = formatearTelefono(input.value);
});
