const http = require("http");

const server = http.createServer((req, res) => {
  const { url, method } = req;
  res.setHeader("content-type", "text/text;charset=utf-8");
  res.end(`[${method}] ${url} => Hello, 我是 server1`);
});

server
  .on("error", console.error)
  .on("listening", () => {
    console.log("listening", server.address());
  })
  .listen(5010);
