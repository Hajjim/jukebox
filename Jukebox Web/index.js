// Load the TCP Library
net = require('net');
let express = require('express');

let app = express();
let bodyParser = require('body-parser');
app.use(bodyParser.json()); // support json encoded bodies
app.set('view engine', 'twig');
app.use(express.static('scripts'));
app.use('/static', express.static('scripts'));
let client = new net.Socket();

let musics = [];

client.on('data', function(data) {
	//console.log('Received: ' + data);
	musics = String(data).split(";");
	musics.pop();
	console.log(musics);
});
client.connect(8052, '127.0.0.1', function() {
	console.log('Connected');
});

client.on('close', function() {
	console.log('Connection closed');
});

app.get('/', (req, res) => {
	res.render('index', { musics: musics});
});

app.post('/unity', (req, res) => {
	console.log("message received");
	client.write('connected');
});

app.post('/music', (req, res) => {
	console.log(req.body);
	console.log("message received");
	client.write(req.body.title);
});
// start server
app.listen(process.env.PORT || 8081);