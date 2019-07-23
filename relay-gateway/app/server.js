const https = require('hyco-https');
const util = require('util');

const ns = process.env.RELAY_NAMESPACE;
const path = process.env.RELAY_HYBRID_CONNECTION_NAME
const keyrule = process.env.RELAY_SAS_KEY_NAME;
const key = process.env.RELAY_SAS_KEY_VALUE;

console.log(util.format("Listening URL https://%s/%s", ns, path));
if (ns === "") {
  console.log("Error: Namespace is NULL")
  process.exit(-1)
}

// Docker HACK 
//https://github.com/Azure/azure-relay-node/issues/26 
global.DTRACE_HTTP_SERVER_RESPONSE = function () { }

var uri = https.createRelayListenUri(ns, path);
var server = https.createRelayedServer(
  {
    server: uri,
    token: () => https.createRelayToken(uri, keyrule, key)
  },
  (req, res) => {
    console.log('request accepted: ' + req.method + ' on ' + req.url);
    res.setHeader('Content-Type', 'text/html');
    res.end('<html><head><title>Hey!</title></head><body>Hello Task!</body></html>');
  });

server.listen((err) => {
  if (err) {
    return console.log('something bad happened', err)
  }
  console.log(`server is listening on ${port}`)
});

server.on('error', (err) => {
  console.log('error: ' + err);
});
