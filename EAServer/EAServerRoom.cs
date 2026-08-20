using SSX3_Server.EAClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSX3_Server.EAClient.Messages;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Sockets;

namespace SSX3_Server.EAServer
{
    public class EAServerRoom
    {
        public int roomId = -1;
        public string Address = "";
        public string roomType = "Beginner";
        public string roomName = "Null";
        public string roomPassword = "";
        public string roomHost = "Community";
        public List<EAClientManager> Clients = new List<EAClientManager>();

        public bool isGlobal = false;
        int prevListCount = 0;

        public List<PlusMSGMessageOut> plusMSGMessageOuts = new List<PlusMSGMessageOut>();

        public EAServerRoom(int ID, string RoomAddress, string RoomType, string RoomName, string RoomPassword, string RoomHost, bool Global) 
        {
            roomId = ID;
            Address = RoomAddress;
            roomType = RoomType;
            roomName = RoomName;
            roomPassword = RoomPassword;
            roomHost = RoomHost;
            isGlobal = Global;

            ConsoleManager.WriteLine(roomType + " " + roomName + " Room Created by " + roomHost);
        }

        public void AddUser(EAClientManager client)
        {
            client.room = this;

            MoveMessageOut moveMessageOut = new MoveMessageOut();

            moveMessageOut.IDENT = roomId.ToString();
            moveMessageOut.NAME = roomType + "." + roomName;
            moveMessageOut.COUNT = Clients.Count.ToString();

            client.Broadcast(moveMessageOut);

            ConsoleManager.WriteLine(client.LoadedPersona.Name + " Joined Room " + roomType + "." + roomName);

            Clients.Add(client);

            PlusWhoMessageOut plusWhoMessageOut = new PlusWhoMessageOut();

            plusWhoMessageOut.I = Clients.Count.ToString();
            plusWhoMessageOut.N = client.LoadedPersona.Name;
            plusWhoMessageOut.M = client.userData.Name;
            plusWhoMessageOut.A = client.LocalIP;
            plusWhoMessageOut.X = "";
            plusWhoMessageOut.S = client.LoadedPersona.GenerateStat();
            plusWhoMessageOut.R = roomType + "." + roomName;
            plusWhoMessageOut.RI = roomId.ToString();

            client.Broadcast(plusWhoMessageOut);

            BoradcastBackUserList();

            PlusPopMessageOut plusPopMessageOut = new PlusPopMessageOut();

            plusPopMessageOut.Z = roomId + "/" + Clients.Count;

            EAServerManager.Instance.BroadcastMessage(plusPopMessageOut);

            for (int i = 0; i < plusMSGMessageOuts.Count; i++)
            {
                client.Broadcast(plusMSGMessageOuts[i]);
            }



            McommCommands.GenerateMcommMessage(client.LoadedPersona.Name + " Has Joined the Room", this);
        }

        public void RemoveUser(EAClientManager client, bool Quit = false)
        {
            Clients.Remove(client);
            if (!Quit)
            {
                MoveMessageOut moveMessageOut = new MoveMessageOut();

                moveMessageOut.Leaving = true;

                moveMessageOut.IDENT = roomId.ToString();
                moveMessageOut.NAME = roomType + "." + roomName;
                moveMessageOut.COUNT = Clients.Count.ToString();

                client.Broadcast(moveMessageOut);
            }

            ConsoleManager.WriteLine(client.LoadedPersona.Name + " Left Room " + roomType + "." + roomName);

            BoradcastBackUserList();

            PlusPopMessageOut plusPopMessageOut = new PlusPopMessageOut();

            plusPopMessageOut.Z = roomId + "/" + Clients.Count;

            EAServerManager.Instance.BroadcastMessage(plusPopMessageOut);

            if (!isGlobal && Clients.Count == 0)
            {
                //Disconnect Room
                ConsoleManager.WriteLine("Destroying Room " + roomType + "." + roomName);
                EAServerManager.Instance.DestroyRoom(roomId);
            }
            else
            {
                McommCommands.GenerateMcommMessage(client.LoadedPersona.Name + " Has Left the Room", this);
            }
        }

        public PlusRomMessageOut GeneratePlusRoomInfo()
        {
            PlusRomMessageOut _RomMessage = new PlusRomMessageOut();

            _RomMessage.A = Address;
            _RomMessage.I = roomId.ToString();
            _RomMessage.N = roomType + "." + roomName;
            _RomMessage.H = roomHost;
            if (roomPassword != "")
            {
                _RomMessage.F = "P";
            }
            _RomMessage.T = Clients.Count.ToString() + 1;

            return _RomMessage;
        }

        public void PeekBoradcastBackUserList(EAClientManager client)
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                var TempUser = Clients[i].GeneratePlusUser();

                //if (TempUser.N.Contains("[" + EAClientManager.VersionPrefix[Clients[i].VERS] + "]"))
                //{
                //    TempUser.N = TempUser.N.Replace("[" + EAClientManager.VersionPrefix[Clients[i].VERS] + "] ", "");
                //}

                TempUser.I = (i + 1).ToString();

                client.Broadcast(TempUser);
            }

            for (int i = Clients.Count; i < client.PrevPeekCount; i++)
            {
                var TempUser = new PlusUserMessageOut();

                TempUser.I = (i + 1).ToString();

                TempUser.N = "";

                client.Broadcast(TempUser);
            }

            client.PrevPeekCount = Clients.Count;

            if (Clients.Count==0)
            {
                PlusUserMessageOut plusUserMessageOut = new PlusUserMessageOut();

                plusUserMessageOut.I = "1";
                plusUserMessageOut.N = "Empty";
                plusUserMessageOut.M = "Empty";
                plusUserMessageOut.A = "";
                plusUserMessageOut.X = "";
                plusUserMessageOut.G = "0";
                plusUserMessageOut.P = "0";

                client.Broadcast(plusUserMessageOut);
            }
        }

        public void BoradcastBackUserList()
        {
            for (int i = Clients.Count; i < prevListCount; i++)
            {
                var TempUser = new PlusUserMessageOut();

                TempUser.I = (i+1).ToString();
                TempUser.P = "0";
                TempUser.N = "";

                BroadcastAllUsers(TempUser);
            }

            //for (int i = 0; i < Clients.Count; i++)
            //{
            //    PlusRNKMessageOut plus = new PlusRNKMessageOut();

            //    plus.A = "0";

            //    BroadcastAllUsers(plus);
            //}

            for (int i = 0; i < Clients.Count; i++)
            {
                BroadcastAllUsers(Clients[i].GeneratePlusUser(i+1));
            }
            prevListCount = Clients.Count;
        }

        public void BroadcastAllUsers(EAMessage message)
        {
            for (int i = 0; i < Clients.Count; i++)
            {
                //if(message.MessageType=="+usr")
                //{
                //    var TempUser = (PlusUserMessageOut)message;

                //    if (TempUser.N.Contains("[" + EAClientManager.VersionPrefix[Clients[i].VERS] + "]"))
                //    {
                //        TempUser.N = TempUser.N.Replace("[" + EAClientManager.VersionPrefix[Clients[i].VERS] + "] ", "");
                //    }

                //    message = (EAMessage)TempUser;
                //}
                Clients[i].Broadcast(message);

            }
        }

        public void ProcessMessage(MesgMessageIn mesgMessageIn, EAClientManager clientManager)
        {
            if (mesgMessageIn.TEXT.TrimStart('\"').StartsWith("!"))
            {
                McommCommands.ProcessCommandRoom(clientManager, this, mesgMessageIn.TEXT.TrimEnd('\"').TrimStart('\"'));
            }
            else
            {
                PlusMSGMessageOut plusMSGMessageOut = new PlusMSGMessageOut();

                plusMSGMessageOut.N = clientManager.LoadedPersona.Name;
                plusMSGMessageOut.T = mesgMessageIn.TEXT;
                plusMSGMessageOut.F = "C";

                plusMSGMessageOuts.Add(plusMSGMessageOut);

                if(plusMSGMessageOuts.Count>6)
                {
                    plusMSGMessageOuts.RemoveAt(0);
                }

                BroadcastAllUsers(plusMSGMessageOut);

                ConsoleManager.WriteLine("("+ roomType+"."+roomName +") " + clientManager.LoadedPersona.Name + ": "+ mesgMessageIn.TEXT);
            }
        }

        public RoomInfo GenerateRoomInfo()
        {
            var RoomInfo = new RoomInfo();

            RoomInfo.roomName = roomName;
            RoomInfo.roomType = roomType.Replace("Intermediate", "Voice");

            if (roomPassword!="")
            {
                RoomInfo.password = true;
            }

            RoomInfo.Players = new List<PlayerInfo>();

            for (int i = 0; i < Clients.Count; i++)
            {
                var TempPlayer = new PlayerInfo();

                TempPlayer.playerName = Clients[i].LoadedPersona.Name;
                TempPlayer.playerVersion = EAClientManager.VersionPrefix[Clients[i].VERS];

                RoomInfo.Players.Add(TempPlayer);
            }

            return RoomInfo;
        }

        public struct RoomInfo
        {
            public string roomName;
            public string roomType;
            public bool password;
            public List<PlayerInfo> Players;
        }

        public struct PlayerInfo
        {
            public string playerName;
            public string playerVersion;
        }
    }
}
