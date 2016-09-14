"use strict";

var redis = require("redis");

class Leaderboards
{
	constructor()
	{
		this.MAX_BOARDS = 10;
		this.MAX_SCORE = 10000000;
		this.boardName = "lbd";		// Leaderboards prefix

		this.client = redis.createClient();
		this.client.on("error", (err) =>
		{
			console.log("Redis error: " + err);
		});
	}
	
	setUserScore(boardId, user, score, cb)
	{
		var name = this.boardName + ":" + boardId;
		this.client.zadd([name, score, user], (err) =>
		{
			if (cb) cb(err);
		});
	}
	
	setUserScores(userName, scores, cb)
	{
		var multi = this.client.multi();
		for (var t = 1; t < this.MAX_BOARDS + 1; t++)
		{
			var name = this.boardName + ":" + t;
			if (scores[t - 1] > 0 && scores[t - 1] < this.MAX_SCORE)
				multi.zadd([name, scores[t - 1], userName]);
		}
		multi.exec((err, replies) =>
		{
			if (cb) cb(err);
		});
	}
	
	getUserRank(boardId, userName, cb)
	{
		var name = this.boardName + ":" + boardId;
		this.client.zrevrank([name, userName], (err, rank) =>
		{
			if (cb)
			{
				if (err) return cb(err, -1);
				cb(err, rank);
			}
		});
	}
	
	getUserRanks(userName, cb)
	{
		var multi = this.client.multi();
		for (var t = 1; t < this.MAX_BOARDS + 1; t++)
		{
			var name = this.boardName + ":" + t;
			multi.zrevrank([name, userName]);
		}
		multi.exec((err, replies) =>
		{
			var ranks = [];
			replies.forEach((reply, index) =>
			{
				if (reply == null)
					ranks[index] = -1;
				else
					ranks[index] = reply;
			});
			if (cb) cb(err, ranks);
		});
	}
	
	getUserScore(boardId, userName, cb)
	{
		var name = this.boardName + ":" + boardId;
		this.client.zscore([name, userName], (err, score) =>
		{
			if (cb)
			{
				if (err) return cb(err, 0);
				cb(err, score);
			}
		});
	}
}


module.exports.Leaderboards = Leaderboards;
