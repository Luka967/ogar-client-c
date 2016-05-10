# ogar-client-c
An agar.io client specially designed for connecting to Ogar.

Since the 5/5/2016 Miniclip update, users cannot connect to private servers anymore. The protocol has probably been changed, but we still don't know what happened.

This respository has a goal to allow (Windows only :( ) users to connect to Ogar servers and play on them.

### Things done
- [x] Connection
- [x] See the environment
- [x] Mass, name and virus rendering
- [x] W, space, game join, spectate (free-roam still doesn't work)
- [ ] Produce EXE as one file
- [ ] Text, FFA & Chart leaderboard (FFA leaderboard is implemented but weird)

## Using
From the /binaries folder, download the version you want. **Download the EXE file AND the DLL file! Without the DLL file, not even a window won't show!**

From there, you'll automatically connect to one of my test servers `ws://ogar-luka967.c9users.io:8080`. If you wish to connect to some other, go under the `Connect` tab on the top menu and choose `Connect...`.

### Helping development
This app uses NuGet package `websocket-sharp`, which allows connection to WebSocket servers.

**Formatting isn't necessary**, I'll fix it after the pull request was approved.
