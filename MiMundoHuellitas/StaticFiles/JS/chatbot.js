document.addEventListener("DOMContentLoaded", function () {

    const chatBubble = document.getElementById("chat-bubble");
    const chatWindow = document.getElementById("chat-window");
    const closeChat = document.getElementById("close-chat");

    if (chatBubble) {
        chatBubble.addEventListener("click", function () {
            chatWindow.classList.toggle("hidden");
        });
    }

    if (closeChat) {
        closeChat.addEventListener("click", function () {
            chatWindow.classList.add("hidden");
        });
    }

});

function respuesta(opcion) {

    let mensaje = "";

    switch (opcion) {
        case 1:
            mensaje = "Para agendar debes iniciar sesión y seleccionar la opción de agendar un servicio 🐶";
            break;
        case 2:
            mensaje = "Ofrecemos servicios de grooming, spa canino, corte de pelo y tienda de productos";
            break;
        case 3:
            mensaje = "Nuestro horario es de lunes a viernes de 8:00am a 5:00pm.";
            break;
        case 4:
            mensaje = "Estamos ubicados en Desamparados,San José.";
            break;
    }

    let chatBody = document.getElementById("chat-body");

    if (chatBody) {
        let nuevaRespuesta = document.createElement("div");
        nuevaRespuesta.classList.add("bot-message");
        nuevaRespuesta.innerText = mensaje;

        chatBody.appendChild(nuevaRespuesta);

        // Auto scroll hacia abajo
        chatBody.scrollTop = chatBody.scrollHeight;
    }
}