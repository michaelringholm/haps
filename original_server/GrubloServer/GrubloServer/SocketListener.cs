using GrubloServer;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// State object for reading client data asynchronously
public class StateObject
{
    // Client  socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int BufferSize = 1024;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];
    // Received data string.
    public StringBuilder sb = new StringBuilder();
}

public class SocketListener
{
    private static CommandHandler commandHandler = new CommandHandler();
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public static void Log(string msg, params object[] args)
    {
        string formattedMsg = string.Format(msg, args);
        string line = string.Format("{0}> {1}", DateTime.Now.ToString("dd-MM HH:mm:ss"), formattedMsg);
        Console.WriteLine(line);
    }

    public static void StartListening()
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = IPAddress.Any;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 80);

        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(100);

            while (true)
            {
                // Set the event to nonsignaled state.
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.
                Log("Waiting for a connection...");
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                // Wait until a connection is made before continuing.
                allDone.WaitOne();
            }
        }
        catch (Exception e)
        {
            Log(e.ToString());
        }

        Log("\nPress ENTER to continue...");
        Console.Read();
    }

    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.
        allDone.Set();

        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
    }

    public static void ReadCallback(IAsyncResult ar)
    {
        try
        {
            String content = String.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Check for double newline. If it is not there, read more data.
                content = state.sb.ToString();
                if (content.IndexOf(string.Format("{0}{1}", Environment.NewLine, Environment.NewLine)) > -1)
                {
                    Log("Read {0} bytes from socket, Data : {1}", content.Length, content);

                    int endLine = content.IndexOf(" HTTP/1.1");
                    int startLine = content.IndexOf("/");
                    content = content.Substring(startLine + 1, (endLine - startLine - 1));
                    content = System.Uri.UnescapeDataString(content);
                    string result = commandHandler.Handle(content.Trim());

                    string header =
    @"Access-Control-Allow-Origin: *
Content-Type: text/html; charset=utf-8
";

                    result = string.Format("HTTP/1.1 200 OK\n{0}\na:{1}\n\n", header, result);
                    Log("Sending: {0}", result);
                    Send(handler, result);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
        }
        catch(Exception e)
        {
            Log(e.Message);
        }
    }

    private static void Send(Socket handler, String data)
    {
        byte[] byteData = Encoding.ASCII.GetBytes(data);
        handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket handler = (Socket)ar.AsyncState;

            int bytesSent = handler.EndSend(ar);
            Log("Sent {0} bytes to client.", bytesSent);

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        catch (Exception e)
        {
            Log(e.ToString());
        }
    }
}
