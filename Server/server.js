"use strict";

var leaderboards = require("./leaderboards");
var http = require("http");

class Server
{
	constructor()
	{
		this.port = 8080;
		this.ip = "localhost";
		
		this.leaderboards = new leaderboards.Leaderboards();
		this.start();
	}

	start()
	{
		this.server = http.createServer((req, res) =>
		{
			this.processRequest(req, res);
		});
		
		this.server.on("clientError", (err, socket) =>
		{
			socket.end("HTTP/1.1 400 Bad Request\r\n\r\n");
		});
		console.log("Server created");
	}
	
	listen()
	{
		this.server.listen(this.port, this.ip);
		console.log("Server listening for connections");
	}

	processRequest(req, res)
	{
		// Process the request from the client
		// We are only supporting POST
		if (req.method === "POST")
		{
			// Post data may be sent in chunks so need to build it up
			var body = "";
			req.on("data", (data) =>
			{
				body += data;
				// Prevent large files from being posted
				if (body.length > 1024)
				{
					// Tell Unity that the data sent was too large
					res.writeHead(413, {"Content-Type": "text/plain"});
					res.end("Error 413");
				}
			});
			req.on("end", () =>
			{
				// Now that we have all data from the client, we process it
				body = decodeURIComponent(body);
				console.log("Received data: " + body);
				var vars = body.split("=");
				if (vars[0] == "d")
				{
					var data = Buffer.from(vars[1], "base64");
					console.log("Data: " + data);
					this.processCommand(data.toString(), res);
				}
			});
		}
		else
		{
			// Tell Unity that the method is not allowed
			res.writeHead(405, {"Content-Type": "text/plain"});
			res.end("Error 405");
		}
	}

	getCommandFromBody(body)
	{
		var command = { method:null, which:-1, scores:[0], userName:""};
		// Split the key / pair values and print them out
		var vars = body.split("&");
		for (var t = 0; t < vars.length; t++)
		{
			var pair = vars[t].split("=");
			var key = decodeURIComponent(pair[0]);
			var val = decodeURIComponent(pair[1]);
			if (key == "w")
				command.which = parseInt(val);
			else if (key == "s")
				command.scores[0] = parseInt(val);
			else if (key == "n")
				command.userName = val;
			else if (key == "m")
				command.method = val;
			else if (key == "a")
				command.scores = val.split(",");
			console.log(key + ":" + val);
		}
		
		return command;
	}
	
	processCommand(body, res)
	{
		var cmd = this.getCommandFromBody(body);
		// Sanity check the input, if valid then update the users score
		if (cmd.method != null && cmd.userName.length > 0 && cmd.userName.length < 17)
		{
			if (cmd.method == "score")
			{
				this.processCommandScore(cmd, res);
			}
			else if (cmd.method == "rank")
			{
				this.processCommandRank(cmd, res);
			}
			else if (cmd.method == "scores")
			{
				this.processCommandScores(cmd, res);
			}
			else if (cmd.method == "ranks")
			{
				this.processCommandRanks(cmd, res);
			}
		}
	}
	
	processCommandScore(cmd, res)
	{
		// Sanity check score and leaderboard index
		if (cmd.scores[0] > 0 && cmd.scores[0] < this.leaderboards.MAX_SCORE && cmd.which > 0 && cmd.which <= this.leaderboards.MAX_BOARDS)
		{
			// Set score
			this.leaderboards.setUserScore(cmd.which, cmd.userName, cmd.scores[0], (err) =>
			{
				if (err) console.log("Error: " + err);
				// Tell Unity that we received the data OK
				res.writeHead(200, {"Content-Type": "text/plain"});
				res.end("OK");
			});
		}
	}
	
	processCommandRank(cmd, res)
	{
		// Sanity check leaderboard index
		if (cmd.which > 0 && cmd.which <= this.leaderboards.MAX_BOARDS)
		{
			// Get rank
			this.leaderboards.getUserRank(cmd.which, cmd.userName, (err, rank) =>
			{
				if (err) console.log("Error: " + err);
				// Send rank back to unity
				res.writeHead(200, {"Content-Type": "text/plain"});
				res.end(rank.toString());
			});
		}
	}

	processCommandScores(cmd, res)
	{
		var len = cmd.scores.length;
		if (len > this.leaderboards.MAX_BOARDS)
		{
			// Too many scores
			return;
		}
		
		// Set scores
		this.leaderboards.setUserScores(cmd.userName, cmd.scores, (err) =>
		{
			if (err) console.log("Error: " + err);
			// Tell Unity that we received the data OK
			res.writeHead(200, {"Content-Type": "text/plain"});
			res.end("OK");
		});
	}
	
	processCommandRanks(cmd, res)
	{
		// Get ranks
		this.leaderboards.getUserRanks(cmd.userName, (err, ranks) =>
		{
			if (err) console.log("Error: " + err);
			// Send ranks collection back to Unity
			res.writeHead(200, {"Content-Type": "text/plain"});
			var rs = "";
			for (var t = 0; t < ranks.length; t++)
			{
				rs += ranks[t].toString();
				if (t < ranks.length - 1)
					rs += ",";
			}
			res.end(rs);
		});
	}
	
}

module.exports.Server = Server;
