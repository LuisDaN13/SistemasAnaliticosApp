// JSON data from the repository (simplified structure)
const COSTA_RICA_DATA = {
    "San José": {
        "San José": ["Carmen", "Merced", "Hospital", "Catedral", "Zapote", "San Francisco de Dos Ríos", "Uruca", "Mata Redonda", "Pavas", "Hatillo", "San Sebastián"],
        "Escazú": ["Escazú", "San Antonio", "San Rafael"],
        "Desamparados": ["Desamparados", "San Miguel", "San Juan de Dios", "San Rafael Arriba", "San Antonio", "Frailes", "Patarra", "San Cristóbal", "Rosario", "Damas", "San Rafael Abajo", "Gravilias", "Los Guido"],
        "Puriscal": ["Santiago", "Mercedes Sur", "Barbacoas", "Grifo Alto", "San Rafael", "Candelarita", "Desamparaditos", "San Antonio", "Chires"],
        "Tarrazú": ["San Marcos", "San Lorenzo", "San Carlos"],
        "Aserrí": ["Aserrí", "Tarbaca", "Vuelta de Jorco", "San Gabriel", "Legua", "Monterrey", "Salitrillos"],
        "Mora": ["Colón", "Guayabo", "Tabarcia", "Piedras Negras", "Picagres", "Jaris", "Quitirrisí"],
        "Goicoechea": ["Guadalupe", "San Francisco", "Calle Blancos", "Mata de Plátano", "Ipís", "Rancho Redondo", "Purral"],
        "Santa Ana": ["Santa Ana", "Salitral", "Pozos", "Uruca", "Piedades", "Brasil"],
        "Alajuelita": ["Alajuelita", "San Josecito", "San Antonio", "Concepción", "San Felipe"],
        "Vázquez de Coronado": ["San Isidro", "San Rafael", "Dulce Nombre de Jesús", "Patalillo", "Cascajal"],
        "Acosta": ["San Ignacio", "Guaitil", "Palmichal", "Cangrejal", "Sabanillas"],
        "Tibás": ["San Juan", "Cinco Esquinas", "Anselmo Llorente", "León XIII", "Colima"],
        "Moravia": ["San Vicente", "San Jerónimo", "La Trinidad"],
        "Montes de Oca": ["San Pedro", "Sabanilla", "Mercedes", "San Rafael"],
        "Turrubares": ["San Pablo", "San Pedro", "San Juan de Mata", "San Luis", "Carara"],
        "Dota": ["Santa María", "Jardín", "Copey"],
        "Curridabat": ["Curridabat", "Granadilla", "Sánchez", "Tirrases"],
        "Pérez Zeledón": ["San Isidro de El General", "El General", "Daniel Flores", "Rivas", "San Pedro", "Platanares", "Pejibaye", "Cajón", "Barú", "Río Nuevo", "Páramo", "La Amistad"],
        "León Cortés Castro": ["San Pablo", "San Andrés", "Llano Bonito", "San Isidro", "Santa Cruz", "San Antonio"]
    },
    "Alajuela": {
        "Alajuela": ["Alajuela", "San José", "Carrizal", "San Antonio", "Guácima", "San Isidro", "Sabanilla", "San Rafael", "Río Segundo", "Desamparados", "Turrúcares", "Tambor", "Garita", "Sarapiquí"],
        "San Ramón": ["San Ramón", "Santiago", "San Juan", "Piedades Norte", "Piedades Sur", "San Rafael", "San Isidro", "Ángeles", "Alfaro", "Volio", "Concepción", "Zapotal", "Peñas Blancas"],
        "Grecia": ["Grecia", "San Isidro", "San José", "San Roque", "Tacares", "Río Cuarto", "Puente de Piedra", "Bolívar"],
        "San Mateo": ["San Mateo", "Desmonte", "Jesús María", "Labrador"],
        "Atenas": ["Atenas", "Jesús", "Mercedes", "San Isidro", "Concepción", "San José", "Santa Eulalia", "Escobal"],
        "Naranjo": ["Naranjo", "San Miguel", "San José", "Cirrí Sur", "San Jerónimo", "San Juan", "El Rosario", "Palmitos"],
        "Palmares": ["Palmares", "Zaragoza", "Buenos Aires", "Santiago", "Candelaria", "Esquipulas", "La Granja"],
        "Poás": ["San Pedro", "San Juan", "San Rafael", "Carrillos", "Sabana Redonda"],
        "Orotina": ["Orotina", "El Mastate", "Hacienda Vieja", "Coyolar", "La Ceiba"],
        "San Carlos": ["Quesada", "Florencia", "Buenavista", "Aguas Zarcas", "Venecia", "Pital", "La Fortuna", "La Tigra", "La Palmera", "Venado", "Cutris", "Monterrey", "Pocosol", "Cureña"],
        "Zarcero": ["Zarcero", "Laguna", "Tapezco", "Guadalupe", "Palmira", "Zapote", "Brisas"],
        "Sarchí": ["Sarchí Norte", "Sarchí Sur", "Toro Amarillo", "San Pedro", "Rodríguez"],
        "Upala": ["Upala", "Aguas Claras", "San José o Pizote", "Bijagua", "Delicias", "Dos Ríos", "Yolillal", "Canalete"],
        "Los Chiles": ["Los Chiles", "Caño Negro", "El Amparo", "San Jorge"],
        "Guatuso": ["San Rafael", "Buenavista", "Cote", "Katira"],
        "Río Cuarto": ["Río Cuarto"]
    },
    "Cartago": {
        "Cartago": ["Oriental", "Occidental", "Carmen", "San Nicolás", "Aguacaliente o San Francisco", "Guadalupe o Arenilla", "Corralillo", "Tierra Blanca", "Dulce Nombre", "Llano Grande", "Quebradilla"],
        "Paraíso": ["Paraíso", "Santiago", "Orosi", "Cachí", "Llanos de Santa Lucía"],
        "La Unión": ["Tres Ríos", "San Diego", "San Juan", "San Rafael", "Concepción", "Dulce Nombre", "San Ramón", "Río Azul"],
        "Jiménez": ["Juan Viñas", "Tucurrique", "Pejibaye"],
        "Turrialba": ["Turrialba", "La Suiza", "Peralta", "Santa Cruz", "Santa Teresita", "Pavones", "Tuis", "Tayutic", "Santa Rosa", "Tres Equis", "La Isabel", "Chirripó"],
        "Alvarado": ["Pacayas", "Cervantes", "Capellades"],
        "Oreamuno": ["San Rafael", "Cot", "Potrero Cerrado", "Cipreses", "Santa Rosa"],
        "El Guarco": ["El Tejar", "San Isidro", "Tobosi", "Patio de Agua"]
    },
    "Heredia": {
        "Heredia": ["Heredia", "Mercedes", "San Francisco", "Ulloa", "Varablanca"],
        "Barva": ["Barva", "San Pedro", "San Pablo", "San Roque", "Santa Lucía", "San José de la Montaña"],
        "Santo Domingo": ["Santo Domingo", "San Vicente", "San Miguel", "Paracito", "Santo Tomás", "Santa Rosa", "Tures", "Pará"],
        "Santa Bárbara": ["Santa Bárbara", "San Pedro", "San Juan", "Jesús", "Santo Domingo", "Purabá"],
        "San Rafael": ["San Rafael", "San Josecito", "Santiago", "Ángeles", "Concepción"],
        "San Isidro": ["San Isidro", "San José", "Concepción", "San Francisco"],
        "Belén": ["San Antonio", "La Ribera", "La Asunción"],
        "Flores": ["San Joaquín", "Barrantes", "Llorente"],
        "San Pablo": ["San Pablo", "Rincón de Sabanilla"],
        "Sarapiquí": ["Puerto Viejo", "La Virgen", "Las Horquetas", "Llanuras del Gaspar", "Cureña"]
    },
    "Guanacaste": {
        "Liberia": ["Liberia", "Cañas Dulces", "Mayorga", "Nacascolo", "Curubandé"],
        "Nicoya": ["Nicoya", "Mansión", "San Antonio", "Quebrada Honda", "Sámara", "Nosara", "Belén de Nosarita"],
        "Santa Cruz": ["Santa Cruz", "Bolsón", "Veintisiete de Abril", "Tempate", "Cartagena", "Cuajiniquil", "Diriá", "Cabo Velas", "Tamarindo"],
        "Bagaces": ["Bagaces", "Fortuna", "Mogote", "Río Naranjo"],
        "Carrillo": ["Filadelfia", "Palmira", "Sardinal", "Belén"],
        "Cañas": ["Cañas", "Palmira", "San Miguel", "Bebedero", "Porozal"],
        "Abangares": ["Las Juntas", "Sierra", "San Juan", "Colorado"],
        "Tilarán": ["Tilarán", "Quebrada Grande", "Tronadora", "Santa Rosa", "Líbano", "Tierras Morenas", "Arenal"],
        "Nandayure": ["Carmona", "Santa Rita", "Zapotal", "San Pablo", "Porvenir", "Bejuco"],
        "La Cruz": ["La Cruz", "Santa Cecilia", "La Garita", "Santa Elena"],
        "Hojancha": ["Hojancha", "Monte Romo", "Puerto Carrillo", "Huacas"]
    },
    "Puntarenas": {
        "Puntarenas": ["Puntarenas", "Pitahaya", "Chomes", "Lepanto", "Paquera", "Manzanillo", "Guacimal", "Barranca", "Monte Verde", "Cóbano", "Chacarita", "Chira", "Acapulco", "El Roble", "Arancibia"],
        "Esparza": ["Espíritu Santo", "San Juan Grande", "Macacona", "San Rafael", "San Jerónimo", "Caldera"],
        "Buenos Aires": ["Buenos Aires", "Volcán", "Potrero Grande", "Boruca", "Pilas", "Colinas", "Chánguena", "Biolley", "Brunka"],
        "Montes de Oro": ["Miramar", "La Unión", "San Isidro"],
        "Osa": ["Puerto Cortés", "Palmar", "Sierpe", "Bahía Ballena", "Piedras Blancas", "Bahía Drake"],
        "Quepos": ["Quepos", "Savegre", "Naranjito"],
        "Golfito": ["Golfito", "Puerto Jiménez", "Guaycará", "Pavón"],
        "Coto Brus": ["San Vito", "Sabalito", "Aguabuena", "Limoncito", "Pittier", "Gutiérrez Brown"],
        "Parrita": ["Parrita"],
        "Corredores": ["Corredor", "La Cuesta", "Canoas", "Laurel"],
        "Garabito": ["Jacó", "Tárcoles"]
    },
    "Limón": {
        "Limón": ["Limón", "Valle La Estrella", "Río Blanco", "Matama"],
        "Pococí": ["Guápiles", "Jiménez", "Rita", "Roxana", "Cariari", "Colorado", "La Colonia"],
        "Siquirres": ["Siquirres", "Pacuarito", "Florida", "Germania", "El Cairo", "Alegría"],
        "Talamanca": ["Bratsi", "Sixaola", "Cahuita", "Telire"],
        "Talamanca": ["Bratsi", "Sixaola", "Cahuita", "Telire"],
        "Matina": ["Matina", "Batán", "Carrandi"],
        "Guácimo": ["Guácimo", "Mercedes", "Pocora", "Río Jiménez", "Duacarí"]
    }
};


// ----------------------
//  VALORES ACTUALES (EDIT)
// ----------------------
const provinciaActual = document.getElementById("provincia")?.dataset.value;
const cantonActual = document.getElementById("canton")?.dataset.value;
const distritoActual = document.getElementById("distrito")?.dataset.value;

// ----------------------
//  Referencias globales
// ----------------------
const loader = document.getElementById("ubicacion-loader");
const selects = [
    document.getElementById("provincia"),
    document.getElementById("canton"),
    document.getElementById("distrito")
];

// ----------------------
// Mostrar / ocultar loader
// ----------------------
function mostrarLoader(mostrar) {
    loader.style.display = mostrar ? "block" : "none";
    selects.forEach(s => s.disabled = mostrar);
}

// ----------------------
// Cargar Provincias
// ----------------------
async function cargarProvincias() {
    mostrarLoader(true);

    const select = document.getElementById("provincia");
    select.innerHTML = '<option value="" disabled selected>Seleccione provincia</option>';

    const provincias = Object.keys(COSTA_RICA_DATA).sort();

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

    mostrarLoader(false);
}

// ----------------------
// Cargar Cantones
// ----------------------
async function cargarCantones() {
    const provincia = document.getElementById("provincia").value;
    if (!provincia) return;

    mostrarLoader(true);

    const select = document.getElementById("canton");
    select.innerHTML = '<option value="" disabled selected>Seleccione cantón</option>';

    const cantones = Object.keys(COSTA_RICA_DATA[provincia]).sort();

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

    mostrarLoader(false);
}

// ----------------------
// Cargar Distritos
// ----------------------
async function cargarDistritos() {
    const provincia = document.getElementById("provincia").value;
    const canton = document.getElementById("canton").value;
    if (!provincia || !canton) return;

    mostrarLoader(true);

    const select = document.getElementById("distrito");
    select.innerHTML = '<option value="" disabled selected>Seleccione distrito</option>';

    const distritos = COSTA_RICA_DATA[provincia][canton].sort();

    distritos.forEach(d => {
        const opt = document.createElement("option");
        opt.value = d;
        opt.textContent = d;
        select.appendChild(opt);
    });

    if (distritoActual) {
        select.value = distritoActual;
    }

    mostrarLoader(false);
}

// ----------------------
// Inicializar
// ----------------------
cargarProvincias();