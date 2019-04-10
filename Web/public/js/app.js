(function () {
    // x-gif: http://geelen.github.io/x-gif

    var content = document.getElementById("content");
    var socket = new WebSocket(GetWebSocketUrl("Osu"));

    socket.onopen = (event) => {
        console.log("Socket opened, hopefully.")

        // TODO: Fetch current BPM
    }

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

    // TODO: Handle automatic reconnect trying
    // TODO: Handle 0 bpm (just play gif at its normal speed?)
})();

function ChangeGifSpeed(gifElement, bpm, isPaused) {

    // TODO: calculate framerate of gif using delay time and browser default FPS (https://github.com/rfrench/gify)
    // TODO: Stop animation when song is paused.
    // TODO: fix cutoff issue, possibly caused by too high a frame rate
    // TODO: stabilize sudden bpm changes
    //bpm /= 8;

    if (isPaused == true)
        gifElement.setAttribute("stopped", "");
    else
        gifElement.removeAttribute("stopped");

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