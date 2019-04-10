(function () {
    connect();
})();

function connect() {
    // x-gif: http://geelen.github.io/x-gif

    //var content = document.getElementById("content");
    var socket = new WebSocket(GetWebSocketUrl("Osu"));

    socket.onopen = (event) => {
        console.log("Socket opened, hopefully.")
    }

    socket.onclose = function (e) {
        console.log('Socket is closed. Reconnect will be attempted in 1 second.', e.reason);
        setTimeout(function () {
            connect();
        }, 1000);
    };

    socket.onerror = function (err) {
        console.error('Socket encountered error: ', err.message, 'Closing socket');
        socket.close();
    };

    socket.onmessage = (event) => {
        console.log("Message received from origin:" + event.origin);
        console.log("data:", event.data);

        let gameState = JSON.parse(event.data);

        let bpm = parseFloat(gameState.Properties.CurrentBPM);
        let isPaused = gameState.Properties.SongIsPaused;

        var gifElement = document.getElementById("gif");
        ChangeGifSpeed(gifElement, bpm, isPaused);
        //console.log("gifElement", gifElement);
    };

    // TODO: Handle 0 bpm (just play gif at its normal speed?)
}


function ChangeGifSpeed(gifElement, bpm, isPaused) {

    // TODO: calculate framerate of gif using delay time and browser default FPS (https://github.com/rfrench/gify)
    // TODO: fix cutoff issue, possibly caused by too high a frame rate
    // TODO: stabilize sudden bpm changes
    //bpm /= 8;

    if (isPaused == true) {
        console.log("Paused gif.");
        gifElement.setAttribute("stopped", "");
    }
    else {
        console.log("Resumed gif.");
        gifElement.removeAttribute("stopped");
    }

    gifElement.setAttribute("bpm", bpm.toString());
    console.log("Changed gif speed to " + bpm.toString());
}

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