Unity leaderboards
------------------
Unity leaderboards using node.js backend. This code can be used to create a client server leaderboard system
that enables users to post scores to multiple leaderboards. The node.js server side supports 4 different methods:

- score - Submits the supplied score to the specified leaderboard for the specified user
- rank - Returns the specified users rank in the specified leaderboard
- scores - Submits the supplied scores to the leaderboards in the order in which they are supplied for the specified user
- ranks - Returns all of the specified users ranks across all leaderboards

The server side is split across 3 files:
- server.js - The web server which interacts with the Uniy app
- leaderboard.js - The leaderboard code which communicates with Redis
- main.js - A main file to create the server abd set it off running

To use, copy the files from the server folder to your server then run main.js then attach the Leaderboards script to a
game object in Unity and call the supplied methods for interacting with the leaderboards.

All code contained within this repo is provided AS IS and withought any warranty whatsoever, feel free to use it as you please in your own projects.