using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UDPChat {
    class Client {
        #region Private Members
        private                 Socket      clientSocket;
        private static readonly int         serverPort      = 58888;
        private static readonly string      IPstring        = "169.254.80.80";
        private static readonly IPAddress   serverIp        = IPAddress.Parse( IPstring );
        private                 EndPoint    serverEndPoint;
        private                 bool        isRunning       = false;
        private static readonly int         BUFFER_SIZE     = 1024;
        private                 byte[]      buffer          = new byte[ BUFFER_SIZE ];
        private                 string      clientName      = String.Empty;
        private                 string      input           = string.Empty;
        #endregion

        static void Main ( string[] args ) {
            new Client().Init();
        }

        private void Init () {
            while ( string.IsNullOrWhiteSpace( clientName ) ) {
                Console.WriteLine( "Enter 'exit' to close application at any time." );
                Console.Write( "Enter your name: " );
                clientName = Console.ReadLine().Trim();
            }
            //Initial packet telling server and other user this client has connected
            var loginData = new Packet {
                PacketType = PacketType.Login,
                Name = clientName,
                NameLength = clientName.Length,
                Message = null,
            }.GetData();
            //Initialize
            try {
                clientSocket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
                IPEndPoint serverIpEp = new IPEndPoint( serverIp, serverPort );
                serverEndPoint = ( EndPoint )serverIpEp;
                clientSocket.BeginSendTo( loginData, 0, loginData.Length, SocketFlags.None, serverEndPoint, new AsyncCallback( SendData ), null );
                this.buffer = new byte[1024];
                //TODO: wait for ack from server then continue.
                clientSocket.BeginReceiveFrom( buffer, 0, buffer.Length, SocketFlags.None, ref serverEndPoint, new AsyncCallback( this.RecvData ), null );
            } catch ( Exception excpt ) {
                Console.WriteLine( excpt );
            }
            while ( !isRunning ) { //Block execution until connection msg is recv from server
                Console.WriteLine( "Connecting to server..." );
                Thread.Sleep( 1000 );
            }
            while ( isRunning ) {
                try {
                    while ( string.IsNullOrEmpty( input ) ) {
                        Console.Write( "Type a message: " );
                        input = Console.ReadLine();
                    }
                    if ( input == "exit" ) {
                        isRunning = false;
                        CloseAndDispose();
                        break;
                    }
                    var sendData = new Packet {
                        PacketType = PacketType.Message,
                        Name = clientName,
                        NameLength = clientName.Length,
                        Message = input,
                        MessageLength = input.Length
                    }.GetData();
                    clientSocket.BeginSendTo( sendData, 0, sendData.Length, SocketFlags.None, serverEndPoint, new AsyncCallback( SendData ), null );
                    this.buffer = new byte[1024];
                    clientSocket.BeginReceiveFrom( buffer, 0, buffer.Length, SocketFlags.None, ref serverEndPoint, new AsyncCallback( this.SendData ), null );
                } catch ( Exception excpt ) {
                    Console.WriteLine( excpt );
                } finally {
                    input = string.Empty;
                }
            }
            //send logout msg and wait for ack from server
            //CloseAndDispose();
        }

        private void RecvData ( IAsyncResult ar ) {
            try {
                clientSocket.EndReceive( ar );
                Packet recvData = new Packet(this.buffer);
                if ( recvData.Message != null ) {
                    Console.WriteLine( recvData.Message );
                }
                this.buffer = new byte[1024];
                clientSocket.BeginReceiveFrom( this.buffer, 0, this.buffer.Length, SocketFlags.None, ref serverEndPoint, new AsyncCallback( RecvData ), null );
            } catch ( Exception excpt ) {
                Console.WriteLine( excpt );
            } finally {
                isRunning = true;
            }
        }

        private void SendData ( IAsyncResult ar ) {
            try {
                clientSocket.EndSend( ar );
            } catch ( Exception excpt ) {
                Console.WriteLine( excpt );
            }
        }
        private void CloseAndDispose () {
            Console.WriteLine( "Client shutting down in 3 seconds..." );
            //Send disconnection message to server
            var logoutData = new Packet {
                PacketType = PacketType.Logout,
                Name = clientName,
                NameLength = clientName.Length,
                Message = null,
            }.GetData();
            clientSocket.SendTo( logoutData, 0, logoutData.Length, SocketFlags.None, serverEndPoint );
            //TODO: wait for ack or resend disconnection msg

            Thread.Sleep( 3000 );
            if ( clientSocket != null ) {
                clientSocket.Close();
                clientSocket.Dispose();
            }
            Environment.Exit( 0 );
        }
    }
}
