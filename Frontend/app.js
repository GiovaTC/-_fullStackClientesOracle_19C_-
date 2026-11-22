const API = "https://localhost:7236/api";
let token = null;
let clientesCache = [];

const $ = (id) => document.getElementById(id);
const mensaje = (texto, error = false) => {
    $("mensaje").textContent = texto;
    $("mensaje").style.color = error ? "#b91c1c" : "#15803d";
};

async function request(ruta, opciones = {}) {
    const headers = { "Content-Type": "application/json", ...(opciones.headers || {}) };
    if (token) headers.Authorization = `Bearer ${token}`;

    const respuesta = await fetch(`${API}${ruta}`, { ...opciones, headers });
    if (respuesta.status === 401 && ruta !== "/auth/login") {
        cerrarSesion();
        throw new Error("Sesión vencida. Inicia sesión nuevamente.");
    }
    if (!respuesta.ok) {
        let detalle = `Error HTTP ${respuesta.status}`;
        try {
            const data = await respuesta.json();
            detalle = data.mensaje || data.title || detalle;
        } catch { }
        throw new Error(detalle);
    }
    if (respuesta.status === 204) return null;
    return respuesta.json();
}

$("loginForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    try {
        const data = await request("/auth/login", {
            method: "POST",
            body: JSON.stringify({ usuario: $("usuario").value.trim(), password: $("password").value })
        });
        token = data.token;
        $("loginPanel").classList.add("oculto");
        $("appPanel").classList.remove("oculto");
        $("password").value = "";
        mensaje(`Bienvenido, ${data.usuario}.`);
        await cargarClientes();
    } catch (err) { mensaje(err.message, true); }
});

async function cargarClientes() {
    try {
        clientesCache = await request("/clientes");
        pintarTabla();
        mensaje(`${clientesCache.length} cliente(s) consultado(s).`);
    } catch (err) { mensaje(err.message, true); }
}

function pintarTabla() {
    const tbody = $("clientesBody");
    tbody.replaceChildren();

    for (const c of clientesCache) {
        const tr = document.createElement("tr");
        const valores = [c.idCliente, c.nombres, c.apellidos, c.correo, c.telefono || ""];
        for (const valor of valores) {
            const td = document.createElement("td");
            td.textContent = valor;
            tr.appendChild(td);
        }

        const acciones = document.createElement("td");
        const editar = document.createElement("button");
        editar.textContent = "Editar";
        editar.addEventListener("click", () => editarCliente(c.idCliente));

        const eliminar = document.createElement("button");
        eliminar.textContent = "Eliminar";
        eliminar.className = "btn-eliminar";
        eliminar.addEventListener("click", () => eliminarCliente(c.idCliente));

        acciones.append(editar, eliminar);
        tr.appendChild(acciones);
        tbody.appendChild(tr);
    }
}

$("clienteForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    const id = $("idCliente").value;
    const cliente = {
        nombres: $("nombres").value.trim(),
        apellidos: $("apellidos").value.trim(),
        correo: $("correo").value.trim(),
        telefono: $("telefono").value.trim() || null
    };

    if (!cliente.nombres || !cliente.apellidos || !cliente.correo) {
        mensaje("Completa los campos obligatorios.", true);
        return;
    }

    try {
        await request(id ? `/clientes/${id}` : "/clientes", {
            method: id ? "PUT" : "POST",
            body: JSON.stringify(cliente)
        });
        limpiarFormulario();
        await cargarClientes();
        mensaje(id ? "Cliente actualizado." : "Cliente registrado.");
    } catch (err) { mensaje(err.message, true); }
});

function editarCliente(id) {
    const c = clientesCache.find(x => x.idCliente === id);
    if (!c) return;
    $("idCliente").value = c.idCliente;
    $("nombres").value = c.nombres;
    $("apellidos").value = c.apellidos;
    $("correo").value = c.correo;
    $("telefono").value = c.telefono || "";
    $("formTitle").textContent = `Editar cliente #${id}`;
    $("guardarBtn").textContent = "Actualizar cliente";
    window.scrollTo({ top: 0, behavior: "smooth" });
}

async function eliminarCliente(id) {
    if (!confirm(`¿Eliminar el cliente ${id}?`)) return;
    try {
        await request(`/clientes/${id}`, { method: "DELETE" });
        await cargarClientes();
        mensaje("Cliente eliminado.");
    } catch (err) { mensaje(err.message, true); }
}

function limpiarFormulario() {
    $("clienteForm").reset();
    $("idCliente").value = "";
    $("formTitle").textContent = "Registrar cliente";
    $("guardarBtn").textContent = "Guardar cliente";
}

function cerrarSesion() {
    token = null;
    clientesCache = [];
    $("appPanel").classList.add("oculto");
    $("loginPanel").classList.remove("oculto");
    $("clientesBody").replaceChildren();
    limpiarFormulario();
}

$("cancelarBtn").addEventListener("click", limpiarFormulario);
$("recargarBtn").addEventListener("click", cargarClientes);
$("logoutBtn").addEventListener("click", cerrarSesion);
    