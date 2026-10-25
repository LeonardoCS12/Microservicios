// Seccion 8.2 del PDF: abre la conexion WebSocket, dispara la mutation crearProducto y verifica
// que el evento producto_creado llegue por el socket. Requiere Node 22+ y los servicios levantados.
//
//   TOKEN=<jwt> node scripts/prueba-websocket.mjs [http://localhost:8080]
//
const gateway = process.argv[2] ?? "http://localhost:8080";
const token = process.env.TOKEN;
if (!token) {
  console.error("Falta la variable de entorno TOKEN (JWT de la practica 6).");
  process.exit(1);
}

const socket = new WebSocket(`${gateway.replace(/^http/, "ws")}/ws/notificaciones?access_token=${token}`);
await new Promise((resolve, reject) => {
  socket.onopen = resolve;
  socket.onerror = () => reject(new Error("No se pudo abrir el WebSocket."));
});

const evento = new Promise((resolve, reject) => {
  const timer = setTimeout(() => reject(new Error("No llego producto_creado a tiempo.")), 5000);
  socket.onmessage = (e) => {
    const mensaje = JSON.parse(e.data);
    if (mensaje.evento === "producto_creado") {
      clearTimeout(timer);
      resolve(mensaje);
    }
  };
});

const respuesta = await fetch(`${gateway}/graphql`, {
  method: "POST",
  headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
  body: JSON.stringify({
    query: `mutation { crearProducto(input: { name: "Teclado", price: 450.0, status: true }) { id name price } }`,
  }),
});
const resultado = await respuesta.json();
if (resultado.errors) {
  console.error("La mutation fallo:", JSON.stringify(resultado.errors));
  process.exit(1);
}

try {
  console.log("OK, evento recibido:", JSON.stringify(await evento));
  socket.close();
} catch (error) {
  console.error(error.message);
  process.exit(1);
}
