## UDP Socket based Chat Client in C#

An improvised chat client, this project is a proof of concept and an exploration platform for me to be able to make a proper UDP-client-server design later.

I wrote this project to get hands on experience with UDP networking, my goal is to better understand how to handle networking traffic at a low level, to understand how it should be designed for performance, maintainability, scaleability and testability.

Additional goals are to make a predictable and to some degree reliable networking solution using UDP building out an acknowledgement system by myself as opposed to using the reliable TCP protocol. 

Eventually base a proper design of what I learned in this project and build an asynchronous UDP based server-client chat application with different channels for reliability (over TCP or UDP with own ack-system) or high performance (unreliable, but low overhead).

### TODO: 
- [ ] Retry connecting a couple of times
- [ ] Make it possible to choose ip and port of the server to connect to through the terminal/console
- [ ] Send ack to server when message is received
- [ ] Write tests
- [ ] Do a complete analysis and decide on staying with this solution or starting a new one.
