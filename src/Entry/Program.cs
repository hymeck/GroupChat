﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Entry
{
    class Program
    {
        public const int CommonPort = 9000;
        public const int ChatPort = 9001;
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                if (args[0] == "-r")
                {
                    SendRequest("hymeck", ChatId);
                }
                
                else if (args[0] == "-c")
                {
                    CreateChat(ChatId);
                }
                
                else
                    PrintSettings();
            }
            
            else
                PrintSettings();
        }

        private static void PrintSettings() => Console.WriteLine("-c - create chat\n-r - request to chat");
        
        public static Guid GenerateChatId() => Guid.NewGuid();

        private static readonly Guid ChatId = Guid.Parse("6cf41f02-ad5f-4828-bc09-078b1de9509e");
        
        public static void CreateChat(Guid chatId, int port = CommonPort)
        {
            var chatListener = new UdpClient(port);
            IPEndPoint requestIp = null;
            Console.WriteLine($"=== Chat '{chatId}' is created ===\nWaiting for connections.");
            try
            {
                while (true)
                {
                    var requestData = chatListener.Receive(ref requestIp);
                    var request = requestData.Deserialize<GroupAccessRequest>();
                    
                    // todo: confirm or deny
                    
                    Console.WriteLine($"{request.Username} requested to access group '{chatId}'");
                    
                    var response = new GroupAccessResponse { Result = GroupAccessResult.Allow};
                    
                    var serializedResponse = response.Serialize();
                    chatListener.Send(serializedResponse, serializedResponse.Length, requestIp);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            finally
            {
                chatListener.Close();
            }
        }

        public static void SendRequest(string username, Guid chatId, int port = CommonPort)
        {
            var localhostIp = IPAddress.Loopback;
            var requestListener = new UdpClient();
            try
            {
                var requestData = new GroupAccessRequest(chatId.ToString(), username).Serialize();
                var remoteHost = new IPEndPoint(localhostIp, port);
                requestListener.Send(requestData, requestData.Length, localhostIp.ToString(), port);


                // var receiveThread = new Thread(_ =>
                // {
                    var response = requestListener.Receive(ref remoteHost);
                    var parsedResponse = response.Deserialize<GroupAccessResponse>();
                    Console.WriteLine($"Response: {parsedResponse}");
                // });
                
                // receiveThread.Start();
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            finally
            {
                requestListener.Close();
            }
        }
    }
}