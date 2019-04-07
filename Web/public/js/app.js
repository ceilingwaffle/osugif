(function () {
    // x-gif: http://geelen.github.io/x-gif

    var content = document.getElementById("content");

    var socket = new WebSocket(GetWebSocketUrl("ws"));

    socket.onopen = (event) => {
        console.log("Socket opened, hopefully.")
    }

    socket.onmessage = (event) => {
        console.log("Message received:")
        console.log("origin", event.origin);
        console.log("data", event.data);
    }

    // TODO: Handle automatic reconnect trying

})();

function GetWebSocketUrl(pathName) {
    var loc = window.location, new_uri;
    if (loc.protocol === "https:") {
        new_uri = "wss:";
    } else {
        new_uri = "ws:";
    }
    new_uri += "//" + loc.host + "/" + pathName;

    return new_uri;
}