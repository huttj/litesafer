/**
 * Created by joshua.hutt on 9/12/2015.
 */
var fs  = require('fs');
var app = require('http').createServer(handler)
var io  = require('socket.io')(app);

var log = console.log.bind(console);

var MjpegConsumer = require("mjpeg-consumer");
var MotionStream = require("motion").Stream;

var PORT = 3000;
var mime = {
    'css': 'text/css',
    'html': 'text/html',
    'js': 'application/javascript',
    'woff': 'application/x-font-woff'
};

var consumer = new MjpegConsumer();
var motion = new MotionStream();

io.on('connection', function (socket) {
    console.log('connection!');

    socket.on('photo', function (data) {
        console.log('');
        console.log(data);
    });

});


// request(options).pipe(consumer).pipe(motion).pipe(writer);

function handler(req, res) {
    var url = req.url.split('?')[0];
    if (url == '/') url = '/index.html';

    var exploded = url.split('.');
    var contentType = mime[exploded[exploded.length - 1]] || 'application/octet-stream';

    res.setHeader('Content-Type', contentType);
    console.log('GET', url);

    fs.readFile(
        __dirname + '/' + url,
        function (err, data) {
            if (err) {
                res.writeHead(500);
                return res.end('Error loading ' + url);
            }
            res.writeHead(200);
            res.end(data);
        }
    );
}

app.listen(PORT);

console.log('listening on', PORT);