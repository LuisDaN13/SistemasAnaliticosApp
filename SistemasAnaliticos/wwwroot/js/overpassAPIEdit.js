const API_URL = "https://overpass-api.de/api/interpreter";

const provinciaActual = document.getElementById("provincia")?.dataset.value;
const cantonActual = document.getElementById("canton")?.dataset.value;
const distritoActual = document.getElementById("distrito")?.dataset.value;

// 🔹 Referencias globales
const loader = document.getElementById("ubicacion-loader");
const selects = [
    document.getElementById("provincia"),
    document.getElementById("canton"),
    document.getElementById("distrito")
];

// 🔹 Mostrar / ocultar animación global
function mostrarLoader(mostrar) {
    loader.style.display = mostrar ? "block" : "none";
    selects.forEach(s => s.disabled = mostrar);
}

// 🔹 Cargar provincias
async function cargarProvincias() {
    mostrarLoader(true);
    const query = `
        [out:json];
        area["name"="Costa Rica"]["admin_level"="2"];
        relation(area)["boundary"="administrative"]["admin_level"="4"];
        out tags;`;

    try {
        const res = await fetch(API_URL, { method: "POST", body: query });
        const data = await res.json();

        const provincias = data.elements.map(e => e.tags.name).sort();
        const select = document.getElementById("provincia");
        select.innerHTML = '<option value="" disabled selected>Seleccione provincia</option>';
        provincias.forEach(p => {
            const opt = document.createElement("option");
            opt.value = p;
            opt.textContent = p;
            select.appendChild(opt);
        });

        if (provinciaActual) {
            select.value = provinciaActual;
            await cargarCantones();
        }

    } catch (error) {
        console.error("Error cargando provincias:", error);
    } finally {
        mostrarLoader(false);
    }
}

// 🔹 Cargar cantones
async function cargarCantones() {
    const provincia = document.getElementById("provincia").value;
    if (!provincia) return;

    mostrarLoader(true);

    const query = `
        [out:json];
        area["name"="${provincia}"]["boundary"="administrative"]["admin_level"="4"];
        relation(area)["boundary"="administrative"]["admin_level"="6"];
        out tags;`;

    try {
        const res = await fetch(API_URL, { method: "POST", body: query });
        const data = await res.json();

        const cantones = data.elements.map(e => e.tags.name).sort();
        const select = document.getElementById("canton");
        select.innerHTML = '<option value="" disabled selected>Seleccione cantón</option>';
        cantones.forEach(c => {
            const opt = document.createElement("option");
            opt.value = c;
            opt.textContent = c;
            select.appendChild(opt);
        });

        if (cantonActual) {
            select.value = cantonActual;
            await cargarDistritos();
        }

    } catch (error) {
        console.error("Error cargando cantones:", error);
    } finally {
        mostrarLoader(false);
    }
}

// 🔹 Cargar distritos
async function cargarDistritos() {
    const canton = document.getElementById("canton").value;
    if (!canton) return;

    mostrarLoader(true);

    const query = `
        [out:json];
        area["name"="${canton}"]["boundary"="administrative"]["admin_level"="6"];
        relation(area)["boundary"="administrative"]["admin_level"="8"];
        out tags;`;

    try {
        const res = await fetch(API_URL, { method: "POST", body: query });
        const data = await res.json();

        const distritos = data.elements.map(e => e.tags.name).sort();
        const select = document.getElementById("distrito");
        select.innerHTML = '<option value="" disabled selected>Seleccione distrito</option>';
        distritos.forEach(d => {
            const opt = document.createElement("option");
            opt.value = d;
            opt.textContent = d;
            select.appendChild(opt);
        });

        if (distritoActual) {
            select.value = distritoActual;
        }

    } catch (error) {
        console.error("Error cargando distritos:", error);
    } finally {
        mostrarLoader(false);
    }
}

// 🔹 Inicializar
cargarProvincias();
