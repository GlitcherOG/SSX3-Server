﻿using SSX3_Server.EAClient.Messages;
using SSX3_Server.EAServer;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SSX3_Server.EAClient
{
    //TODO
    //MOVE MESSAGES TO BE PHRASED BETTER
    //FIX PNG MESSAGES
    //FIND OUT MORE MESSAGES

    public class EAClientManager
    {
        public int ID;
        public string SESS;
        public string MASK;

        public string ADDR;
        public string PORT;

        public string SKEY;

        public string NAME;
        public string PASS;
        public string SPAM;
        public string MAIL;
        public string GEND;
        public string BORN;
        public string DEFPER;
        public string ALTS;
        public string MINAGE;

        public List<string> PersonaList;
        public EAUserPersona LoadedPersona = new EAUserPersona();

        public string TOS;
        public string MID;
        public string PID;
        public string HWIDFLAG;
        public string HWMASK;

        public string PROD;
        public string VERS;
        public string LANG;
        public string SLUS;

        public string SINCE;
        public string LAST;

        public Thread LoopThread;
        public TcpClient MainClient = null;
        NetworkStream MainNS = null;

        TcpListener BuddyListener;
        public TcpClient BuddyClient = null;
        NetworkStream BuddyNS = null;

        //10 seconds to start till proper connection establised
        //ping every 1 min if failed ping close connection
        public bool LoggedIn = false;
        int TimeoutSeconds=30;

        DateTime LastSend;
        DateTime LastRecive;
        DateTime LastPing;
        public int Ping = 20;

        public EAClientManager(TcpClient tcpClient, int InID, string SESSin, string MASKin)
        {
            ID = InID;
            SESS = SESSin;
            MASK = MASKin;
            MainClient = tcpClient;
            MainNS = MainClient.GetStream();

            IPEndPoint remoteIpEndPoint = MainClient.Client.RemoteEndPoint as IPEndPoint;
            BuddyListener = new TcpListener(remoteIpEndPoint.Address, 13505);
            BuddyListener.Start();

            LastRecive = DateTime.Now;
            LastSend = DateTime.Now;
            LastPing = DateTime.Now;

            LoopThread = new Thread(MainListen);
            LoopThread.Start();
        }

        public void MainListen()
        {
            while (MainClient.Connected)  //while the client is connected, we look for incoming messages
            {
                try
                {
                    //Read Main Network Stream
                    if (MainClient.Available > 0)
                    {
                        byte[] msg = new byte[65535];     //the messages arrive as byte array
                        MainNS.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client
                        if (msg[0] != 0)
                        {
                            LastRecive = DateTime.Now;
                            LastPing = DateTime.Now;
                            ProcessMessage(msg);
                        }
                    }

                    if (BuddyClient != null)
                    {
                        if (BuddyClient.Available > 0)
                        {
                            byte[] msg = new byte[65535];     //the messages arrive as byte array
                            BuddyNS.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client
                            if (msg[0] != 0)
                            {
                                LastRecive = DateTime.Now;
                                LastPing = DateTime.Now;
                                ProcessMessage(msg);
                            }
                        }
                    }


                    //If Buddy Listener Connection Pending
                    if (BuddyListener != null)
                    {
                        if (BuddyListener.Pending())
                        {
                            BuddyClient = BuddyListener.AcceptTcpClient();
                            BuddyNS = BuddyClient.GetStream();
                            BuddyListener.Stop();
                            BuddyListener = null;
                        }
                    }

                    if ((DateTime.Now - LastPing).TotalSeconds >= 30)
                    {
                        LastPing = DateTime.Now;
                        _PngMessageOut msg2 = new _PngMessageOut();
                        Broadcast(msg2);
                    }

                    if ((DateTime.Now - LastRecive).TotalSeconds >= TimeoutSeconds)
                    {
                        //If no response from server for timeout break
                        break;
                    }
                }
                catch
                {
                    //Unknown Connection Error
                    //Most Likely Game has crashed
                    Console.WriteLine("Connection Crashed, Disconnecting...");
                    SaveEAUserData();
                    SaveEAUserPersona();
                    CloseConnection();
                    EAServerManager.Instance.DestroyClient(ID);
                }
            }

            //Disconnect and Destroy
            Console.WriteLine("Client Disconnecting...");
            CloseConnection();
            EAServerManager.Instance.DestroyClient(ID);
        }

        public void ProcessMessage(byte[] array)
        {
            string InMessageType = EAMessage.MessageCommandType(array);

            if (InMessageType == "addr")
            {
                AddrMessageIn addrMessageIn = new AddrMessageIn();
                addrMessageIn.PraseData(array);

                ADDR = addrMessageIn.ADDR;
                PORT = addrMessageIn.PORT;

            }
            else if (InMessageType == "skey")
            {
                //Generate SKEY BACK
                SkeyMessageInOut msg = new SkeyMessageInOut();
                msg.PraseData(array);
                SKEY = msg.SKEY;

                msg.SKEY = "$37940faf2a8d1381a3b7d0d2f570e6a7";

                Broadcast(msg);
            }
            else if (InMessageType == "sele")
            {
                SeleMessageInOut msg = new SeleMessageInOut();

                msg.PraseData(array);

                msg.ROOMS = EAServerManager.Instance.rooms.Count.ToString();
                msg.USERS = EAServerManager.Instance.clients.Count.ToString();
                msg.RANKS = "0";
                msg.MESGS = "0";
                msg.GAMES = "0";

                Broadcast(msg);
            }
            else if (InMessageType == "auth")
            {
                AuthMessageIn authMessageIn = new AuthMessageIn();
                authMessageIn.PraseData(array);
                //Apply AUTH Data

                //Confirm Auth Data with saves
                var UserData = GetUserData(authMessageIn.NAME);
                if (UserData != null)
                {
                    AuthMessageOut msg2 = new AuthMessageOut();

                    if (((UserData.Name == authMessageIn.NAME /*&& TempData.Pass == msg.stringDatas[1].Value*/) || UserData.Bypass == true) && UserData.Banned == false)
                    {
                        NAME = UserData.Name;
                        PASS = UserData.Pass;
                        SPAM = UserData.Spam;
                        MAIL = UserData.Mail;
                        GEND = UserData.Gend;
                        BORN = UserData.Born;
                        DEFPER = UserData.Defper;
                        ALTS = UserData.Alts;
                        MINAGE = UserData.Minage;
                        LANG = UserData.Lang;
                        PROD = UserData.Prod;
                        VERS = UserData.Vers;
                        SLUS = UserData.GameReg;

                        SINCE = UserData.Since;

                        PersonaList = UserData.PersonaList;

                        LAST = DateTime.Now.ToString("yyyy.MM.dd hh:mm:ss");

                        SaveEAUserData();

                        msg2.TOS = "1";
                        msg2.MAIL = UserData.Mail;
                        msg2.PERSONAS = GetPersonaList();
                        msg2.BORN = UserData.Born;
                        msg2.GEND = UserData.Gend;
                        msg2.FROM = "US";
                        msg2.LANG = "en";
                        msg2.SPAM = UserData.Spam;
                        msg2.SINCE = UserData.Since;

                        TimeoutSeconds = 60;
                        LoggedIn = true;
                        Broadcast(msg2);

                        EAServerManager.Instance.SendRooms(this);
                    }
                    else
                    {
                        msg2.SubMessage = "imst";
                        Broadcast(msg2);
                    }

                }
                else
                {
                    AuthMessageOut msg2 = new AuthMessageOut();
                    msg2.SubMessage = "imst";
                    Broadcast(msg2);
                }
            }
            else if (InMessageType == "acct")
            {
                //acct - Standard Response
                //acctdupl - Duplicate Account
                //acctimst - Invalid Account

                //Set Data Into Client

                AcctMessageIn msg = new AcctMessageIn();
                msg.PraseData(array);

                AcctMessageOut msg2 = new AcctMessageOut();

                //Check if user exists if so send back this
                var Temp = GetUserData(msg.NAME);
                string ClientTime = DateTime.Now.ToString("yyyy.MM.dd hh:mm:ss");
                if (Temp != null)
                {
                    msg2.SubMessage = "dupl";

                    Broadcast(msg2);
                    return;
                }
                else
                {
                    Temp = new EAUserData();
                    Temp.Name = msg.NAME;
                    Temp.Pass = msg.PASS;
                    Temp.Spam = msg.SPAM;
                    Temp.Mail = msg.MAIL;
                    Temp.Gend = msg.GEND;
                    Temp.Born = msg.BORN;
                    Temp.Defper = msg.DEFPER;
                    Temp.Alts = msg.ALTS;
                    Temp.Minage = msg.MINAGE;
                    Temp.Lang = msg.LANG;
                    Temp.Prod = msg.PROD;
                    Temp.Vers = msg.VERS;
                    Temp.GameReg = msg.SLUS;
                    Temp.PersonaList = new List<string>();

                    Temp.Since = ClientTime;
                    Temp.Last = ClientTime;

                    Temp.CreateJson(AppContext.BaseDirectory + "\\Users\\" + msg.NAME.ToLower() + ".json");
                }

                //Create save and send back data

                msg2.TOS = "1";
                msg2.NAME = msg.NAME;
                msg2.AGE = "21";
                msg2.PERSONAS = "";
                msg2.SINCE = ClientTime;
                msg2.LAST = ClientTime;

                Broadcast(msg2);
            }
            else if (InMessageType == "cper")
            {
                CperMessageInOut msg = new CperMessageInOut();
                msg.PraseData(array);

                var TempPersona = GetUserPersona(msg.PERS);
                if (TempPersona != null)
                {
                    msg.SubMessage = "dupl";
                    Broadcast(msg);
                    return;
                }

                //Create Persona

                EAUserPersona NewPersona = new EAUserPersona();

                NewPersona.Owner = NAME;
                NewPersona.Name = msg.PERS;

                string ClientTime = DateTime.Now.ToString("yyyy.MM.dd hh:mm:ss");

                NewPersona.Since = ClientTime;
                NewPersona.Last = ClientTime;

                NewPersona.CreateJson(AppContext.BaseDirectory + "\\Personas\\" + NewPersona.Name.ToLower() + ".json");

                PersonaList.Add(NewPersona.Name);

                SaveEAUserData();

                Broadcast(msg);
            }
            else if (InMessageType == "dper")
            {
                DperMessageInOut msg = new DperMessageInOut();
                msg.PraseData(array);

                //Create Persona
                bool Removed = false;

                for (int i = 0; i < PersonaList.Count; i++)
                {
                    if (msg.PERS == PersonaList[i])
                    {
                        PersonaList.RemoveAt(i);
                        File.Delete(AppContext.BaseDirectory + "\\Personas\\" + msg.stringDatas[0].Value.ToLower() + ".json");
                        Removed = true;
                    }
                }
                SaveEAUserData();

                if (Removed == false)
                {
                    msg.SubMessage = "imst";
                    Broadcast(msg);
                }

                Broadcast(msg);
            }
            else if (InMessageType == "pers")
            {
                PersMessageIn msg = new PersMessageIn();
                msg.PraseData(array);

                LoadedPersona = GetUserPersona(msg.PERS);
                bool CheckFailed = false;
                if (LoadedPersona != null)
                {
                    if (LoadedPersona.Owner != NAME)
                    {
                        CheckFailed = true;
                    }
                }
                else
                {
                    CheckFailed = true;
                }

                PersMessageOut msg2 = new PersMessageOut();

                if (CheckFailed)
                {
                    msg2.SubMessage = "imst";
                    Broadcast(msg2);
                    return;
                }

                LoadedPersona.Last = DateTime.Now.ToString("yyyy.MM.dd hh:mm:ss");

                msg2.A = EAServerManager.Instance.config.ListerIP.ToString();
                msg2.LA = EAServerManager.Instance.config.ListerIP.ToString();
                msg2.LOC = "enUS";
                msg2.MA = EAServerManager.Instance.config.ListerIP.ToString();
                msg2.NAME = NAME;
                msg2.PERS = LoadedPersona.Name;
                msg2.LAST = LAST;
                msg2.PLAST = LoadedPersona.Last;
                msg2.SINCE = SINCE;
                msg2.LKEY = "3fcf27540c92935b0a66fd3b0000283c";

                Broadcast(msg2);
            }
            else if (InMessageType == "onln")
            {
                OnlnMessageIn onlnMessageIn = new OnlnMessageIn();
                onlnMessageIn.PraseData(array);

                Broadcast(onlnMessageIn);
            }
            else if (InMessageType == "news")
            {
                NewsMessageIn msg = new NewsMessageIn();
                msg.PraseData(array);

                NewsMessageOut msg2 = new NewsMessageOut();

                msg2.SubMessage = "new" + msg.NAME;

                msg2.BUDDYSERVERNAME = "ps2ssx04.ea.com";

                msg2.NEWS = EAServerManager.Instance.News;

                Broadcast(msg2);
            }
            else if (InMessageType == "~png")
            {
                _PngMessageIn msg2 = new _PngMessageIn();

                msg2.PraseData(array);

                Ping = int.Parse(msg2.TIME);
            }
            else if (InMessageType == "user")
            {
                UserMessageIn msg = new UserMessageIn();

                msg.PraseData(array);

                OnlnMessageIn msg2 = new OnlnMessageIn();
                msg2.PERS = msg.PERS;
                Broadcast(msg2);

                UserMessageOut userMessageOut = new UserMessageOut();

                userMessageOut.PERS = msg.PERS;
                userMessageOut.MESG = msg.PERS;
                userMessageOut.ADDR = "192.168.0.141";

                Broadcast(userMessageOut);
            }
            else if (InMessageType == "quik")
            {
                QuikMessageIn msg = new QuikMessageIn();

                msg.PraseData(array);

                if (msg.KIND == "DeathRace")
                {
                    //quick match search
                }
                else if (msg.KIND == "*")
                {
                    //stop quick match search
                }

                //Broadcast(msg);
            }
            else if (InMessageType == "move")
            {
                MoveMessageIn msg = new MoveMessageIn();

                msg.PraseData(array);

                Thread.Sleep(3000);

                //PersMessageOut msg2 = new PersMessageOut();

                //msg2.SubMessage = "room";
                //Broadcast(msg2);

                MoveMessageOut moveMessageOut = new MoveMessageOut();

                moveMessageOut.SubMessage = "room";

                moveMessageOut.IDENT = "1";
                moveMessageOut.NAME = msg.NAME;
                moveMessageOut.COUNT = "0";

                Broadcast(moveMessageOut);

                PlusWhoMessageOut plusWhoMessageOut = new PlusWhoMessageOut();

                plusWhoMessageOut.I = ID.ToString();
                plusWhoMessageOut.N = LoadedPersona.Name;
                plusWhoMessageOut.M = NAME;
                plusWhoMessageOut.A = ADDR;
                plusWhoMessageOut.X = "";
                plusWhoMessageOut.S = "1";
                plusWhoMessageOut.R = msg.NAME;
                plusWhoMessageOut.RI = "1";

                Broadcast(plusWhoMessageOut);

                PlusUserMessageOut plusUserMessageOut = new PlusUserMessageOut();

                plusUserMessageOut.I = ID.ToString();
                plusUserMessageOut.N = LoadedPersona.Name;
                plusUserMessageOut.M = NAME;
                plusUserMessageOut.A = ADDR;
                plusUserMessageOut.X = "";
                plusUserMessageOut.G = "0";
                plusUserMessageOut.P = Ping.ToString();

                Broadcast(plusUserMessageOut);

                plusUserMessageOut = new PlusUserMessageOut();

                plusUserMessageOut.I = ID.ToString();
                plusUserMessageOut.N = LoadedPersona.Name;
                plusUserMessageOut.M = NAME;
                plusUserMessageOut.A = ADDR;
                plusUserMessageOut.X = "";
                plusUserMessageOut.G = "0";
                plusUserMessageOut.P = Ping.ToString();

                Broadcast(plusUserMessageOut);

                PlusPopMessageOut plusPopMessageOut = new PlusPopMessageOut();

                plusPopMessageOut.Z = "1" + "/" + "1";

                Broadcast(plusPopMessageOut);

                PlusMSGMessageOut plusMSGMessageOut = new PlusMSGMessageOut();

                Broadcast(plusMSGMessageOut);

                //PlusSesMessageOut plus = new PlusSesMessageOut();

                //Broadcast(plus);
            }
            else if (InMessageType == "chal")
            {
                ChalMessageIn chalMessageIn = new ChalMessageIn();
                chalMessageIn.PraseData(array);
                chalMessageIn.PERS = "test";
                Broadcast(chalMessageIn);
            }
            else if (InMessageType == "room")
            {
                RoomMessageIn msg = new RoomMessageIn();

                msg.PraseData(array);

                Broadcast(msg);

                PlusWhoMessageOut plusWhoMessageOut = new PlusWhoMessageOut();

                plusWhoMessageOut.I = "1";
                plusWhoMessageOut.N = LoadedPersona.Name;
                plusWhoMessageOut.M = NAME;
                plusWhoMessageOut.A = ADDR;
                plusWhoMessageOut.X = "";
                plusWhoMessageOut.R = msg.NAME;
                plusWhoMessageOut.RI = "1";

                Broadcast(plusWhoMessageOut);

                _RomMessage Test = new _RomMessage();

                Test.H = LoadedPersona.Name;

                Broadcast(Test);

                MoveMessageOut moveMessageOut = new MoveMessageOut();

                moveMessageOut.IDENT = "1";
                moveMessageOut.NAME = msg.RoomType + "." + msg.NAME;
                moveMessageOut.COUNT = "1";

                Broadcast(moveMessageOut);
            }
            else if (InMessageType == "peek")
            {
                PeekMessageIn msg = new PeekMessageIn();

                msg.PraseData(array);

                var Room = EAServerManager.Instance.GetRoom(msg.NAME);

                if (Room != null)
                {
                    Room.BoradcastBackUserList(this);
                }
            }
            else if (InMessageType == "snap")
            {
                snapMessageIn msg = new snapMessageIn();

                msg.PraseData(array);

                Broadcast(msg);

                PlusSnapMessageOut plusSnapMessageOut = new PlusSnapMessageOut();

                Broadcast(plusSnapMessageOut);

                plusSnapMessageOut = new PlusSnapMessageOut();

                plusSnapMessageOut.S = "20";
                plusSnapMessageOut.R = "2";
                plusSnapMessageOut.N = "Gamer1";

                Broadcast(plusSnapMessageOut);
            }
            else
            {
                Console.WriteLine("Unknown Message " + InMessageType);
                Console.WriteLine(System.Text.Encoding.UTF8.GetString(array));
            }
        }

        public void ProcessBuddyMessage(byte[] array)
        {

        }

        public void Broadcast(EAMessage msg)
        {
            LastSend = DateTime.Now;
            byte[] bytes = msg.GenerateData();
            MainNS.Write(bytes, 0, bytes.Length);
        }

        public PlusUserMessageOut GeneratePlusUser()
        {
            PlusUserMessageOut plusUserMessageOut = new PlusUserMessageOut();

            plusUserMessageOut.I = "1";
            plusUserMessageOut.N = LoadedPersona.Name;
            plusUserMessageOut.M = NAME;
            plusUserMessageOut.A = EAServerManager.Instance.config.ListerIP;
            plusUserMessageOut.X = "";
            plusUserMessageOut.G = "0";
            plusUserMessageOut.P = Ping.ToString();

            return plusUserMessageOut;
        }

        public EAUserData GetUserData(string Name)
        {
            if(Path.Exists(AppContext.BaseDirectory + "\\Users\\" + Name.ToLower() + ".json"))
            {
                return EAUserData.Load(AppContext.BaseDirectory + "\\Users\\" + Name.ToLower() + ".json");
            }

            return null;
        }

        public EAUserPersona GetUserPersona(string Name)
        {
            if (Path.Exists(AppContext.BaseDirectory + "\\Personas\\" + Name.ToLower() + ".json"))
            {
                return EAUserPersona.Load(AppContext.BaseDirectory + "\\Personas\\" + Name.ToLower() + ".json");
            }

            return null;
        }

        public string GetPersonaList()
        {
            string StringPersonas = "";

            for (int i = 0; i < PersonaList.Count; i++)
            {
                if(i==0)
                {
                    StringPersonas = PersonaList[i];
                }
                else
                {
                    StringPersonas = StringPersonas + "," + PersonaList[i];
                }
            }

            return StringPersonas;
        }

        public void SaveEAUserData()
        {
            if (NAME != "" && NAME != null)
            {
                EAUserData eAMessage = new EAUserData();
                eAMessage.AddUserData(this);
                eAMessage.CreateJson(AppContext.BaseDirectory + "\\Users\\" + NAME.ToLower() + ".json");
            }
        }

        public void SaveEAUserPersona()
        {
            if (LoadedPersona.Name != "")
            {
                LoadedPersona.CreateJson(AppContext.BaseDirectory + "\\Personas\\" + NAME.ToLower() + ".json");
            }
        }

        public void CloseConnection()
        {
            MainNS.Close();
            MainClient.Close();
            if (BuddyListener.Pending())
            {
                BuddyClient.Close();
                BuddyNS.Close();
            }
            else
            {
                BuddyListener.Stop();
            }
        }
    }
}
