(function () {
    // x-gif: http://geelen.github.io/x-gif

    var content = document.getElementById("content");

    var socket = new WebSocket(GetWebSocketUrl("Osu/BPM"));

    socket.onopen = (event) => {
        console.log("Socket opened, hopefully.")
    }

    socket.onmessage = (event) => {
        console.log("Message received:")
        console.log("origin", event.origin);
        console.log("data", event.data);
    }

    // TODO: Handle automatic reconnect trying

    // TODO: Handle 0 bpm (just play gif at its normal speed?)

})();

function GetWebSocketUrl(pathName) {
    var loc = window.location, new_uri;
    if (loc.protocol === "https:") {
        new_uri = "wss:";
    } else {
        new_uri = "ws:";
    }
    //new_uri += "//" + loc.host + "/" + pathName;
    new_uri += "//" + "127.0.0.1:8081" + "/" + pathName;

    return new_uri;
}