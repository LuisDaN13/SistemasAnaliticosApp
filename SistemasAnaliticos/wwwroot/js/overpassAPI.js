const API_URL = "https://overpass-api.de/api/interpreter";

// 🔹 Cargar provincias al inicio
async function cargarProvincias() {
const query = `
[out:json];
area["name"="Costa Rica"]["admin_level"="2"];
relation(area)["boundary"="administrative"]["admin_level"="4"];
out tags;`;

try {
    const res = await fetch(API_URL, {method: "POST", body: query });
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
} catch (error) {
    console.error("Error cargando provincias:", error);
}
}

// 🔹 Cargar cantones según provincia seleccionada
async function cargarCantones() {
const provincia = document.getElementById("provincia").value;
if (!provincia) return;

const query = `
[out:json];
area["name"="${provincia}"]["boundary"="administrative"]["admin_level"="4"];
relation(area)["boundary"="administrative"]["admin_level"="6"];
out tags;`;

try {
    const res = await fetch(API_URL, {method: "POST", body: query });
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

select.disabled = false;

// Limpiar distritos
const distritoSelect = document.getElementById("distrito");
distritoSelect.innerHTML = '<option value="" disabled selected>Seleccione primero el cantón</option>';
distritoSelect.disabled = true;

} catch (error) {
    console.error("Error cargando cantones:", error);
}
}

// 🔹 Cargar distritos según cantón seleccionado
async function cargarDistritos() {
const canton = document.getElementById("canton").value;
if (!canton) return;

const query = `
[out:json];
area["name"="${canton}"]["boundary"="administrative"]["admin_level"="6"];
relation(area)["boundary"="administrative"]["admin_level"="8"];
out tags;`;

try {
    const res = await fetch(API_URL, {method: "POST", body: query });
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

select.disabled = false;

} catch (error) {
    console.error("Error cargando distritos:", error);
}
}

// Inicializar
cargarProvincias();
