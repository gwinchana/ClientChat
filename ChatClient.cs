using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Data.SQLite;

class ChatClient
{
    public static string username;
    static void Main(string[] args)
    {
        Console.SetWindowSize(40, 30);
        Console.Write("Enter your username: ");
        username = Console.ReadLine();

        TcpClient client = new TcpClient("127.0.0.1", 8888); // Set the IP Address and port for the client connection
        NetworkStream stream = client.GetStream();

        byte[] usernameBytes = Encoding.ASCII.GetBytes(username);
        stream.Write(usernameBytes, 0, usernameBytes.Length);

        Thread receiveThread = new Thread(() => ReceiveMessages(client));
        receiveThread.Start();

        RetrieveMessages(); // Retrieve old messages in the server if there are available.

        // Allows users to input their message to be broadcasted to the server.
        while (true)
        {
            string message = Console.ReadLine();
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            stream.Write(messageBytes, 0, messageBytes.Length);

        }
    }

    static void RetrieveMessages()
    {
        // Sets the connection to the specified database
        string connectionString = @"Data Source=D:\Documents\VS_Projects\ChatApp\ChatServer\bin\Debug\net5.0\ChatDatabase.sqlite;Version=3;";

        // SQL statement to retrieve the data from the db
        string retrieveQuery = "SELECT Username, Message, Timestamp from Messages";

        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            // Open the specified database connection
            connection.Open();

            using (SQLiteCommand command = new SQLiteCommand(retrieveQuery, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    // Loop through the results to get saved messages
                    while (reader.Read())
                    {
                        string name = reader.GetString(0); // Get the value of the column for Name
                        string savedMsg = reader.GetString(1);     // Get the value of the column for the Sent Message
                        string timestamp = reader.GetString(2); // Get the timestamp of old messages

                        Console.WriteLine($"{name}: {savedMsg} ({timestamp})");
                    }
                }
            }
        }
    }

    static void ReceiveMessages(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        while (true)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0) break;

            string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            Console.WriteLine(message);
        }
    }
}
