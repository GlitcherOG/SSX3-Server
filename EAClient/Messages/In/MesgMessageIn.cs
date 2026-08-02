using SSX3_Server.EAServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SSX3_Server.EAClient.Messages
{
    public class MesgMessageIn : EAMessage
    {
        public override string MessageType { get { return "mesg"; } }

        public string PRIV;
        public string TEXT;
        public string ATTR;

        public override void AssignValues()
        {
            PRIV = GetStringData("PRIV");
            TEXT = GetStringData("TEXT");
            ATTR = GetStringData("ATTR");
        }

        public override void AssignValuesToString()
        {
            //AddStringData("PRIV", PRIV);
            //AddStringData("TEXT", TEXT);
            //AddStringData("ATTR", ATTR);
        }

        public override void ProcessCommand(EAClientManager client, EAServerRoom room = null)
        {
            MesgMessageIn message = new MesgMessageIn();

            if (message.PRIV == client.LoadedPersona.Name)
            {
                message.SubMessage = "self";
                client.Broadcast(message);
                return;
            }

            client.Broadcast(message);

            if (ATTR=="N3")
            {
                if(PRIV=="Mcomm")
                {
                    if (TEXT.Contains("challenge"))
                    {
                        //Abort Chal
                        PlusMSGMessageOut plusMSGMessageOut = new PlusMSGMessageOut();

                        plusMSGMessageOut.N = "Mcomm";
                        plusMSGMessageOut.T = "abortChal";
                        plusMSGMessageOut.F = "P3";

                        client.Broadcast(plusMSGMessageOut);
                    }
                    return;
                }

                if (TEXT.Contains("challenge"))
                {
                    var TempClient = EAServerManager.Instance.GetUserPersona(PRIV);

                    if (TempClient != null)
                    {
                        var TempChallange = new MesgMessageIn.Challange();

                        string[] TempString = TEXT/*.Remove('\"')*/.Split(' ');

                        TempChallange.TrackID = TempString[1];
                        TempChallange.Gamemode1 = TempString[2];
                        TempChallange.Gamemode2 = TempString[3];
                        TempChallange.Ranked = TempString[4];
                        TempChallange.Multipliers = TempString[5];
                        TempChallange.Powerups = TempString[6];
                        TempChallange.AI = TempString[7];
                        TempChallange.PointIcons = TempString[8];
                        TempChallange.GameVersion = TempString[9];
                        TempChallange.U1 = TempString[10];
                        TempChallange.U2 = TempString[11];
                        TempChallange.U3 = TempString[12];
                        TempChallange.U4 = TempString[13];

                        client.challange = TempChallange;

                        PlusMSGMessageOut plusMSGMessageOut = new PlusMSGMessageOut();

                        if (EAServerManager.Instance.config.AllowCrossPlay)
                        {
                            TempChallange.GameVersion = EAClientManager.VersionCodes[TempClient.VERS];
                        }

                        plusMSGMessageOut.N = client.LoadedPersona.Name;
                        plusMSGMessageOut.T = "\"challenge " + TempChallange.TrackID + " " + TempChallange.Gamemode1 + " " + TempChallange.Gamemode2 + " " + TempChallange.Ranked 
                            + " " + TempChallange.Multipliers + " " + TempChallange.Powerups + " " + TempChallange.AI + " " + TempChallange.PointIcons + " " + TempChallange.GameVersion + " " +
                            TempChallange.U1 + " " + TempChallange.U2 + " " + TempChallange.U3 + " " + TempChallange.U4 + "\"";
                        plusMSGMessageOut.F = "P3";

                        TempClient.Broadcast(plusMSGMessageOut);

                        ConsoleManager.WriteLine(client.LoadedPersona.Name + " Challanaged " + PRIV);
                    }
                }
                else
                {
                    var TempClient = EAServerManager.Instance.GetUserPersona(PRIV);

                    if (TempClient != null)
                    {
                        PlusMSGMessageOut plusMSGMessageOut = new PlusMSGMessageOut();

                        plusMSGMessageOut.N = client.LoadedPersona.Name;
                        plusMSGMessageOut.T = TEXT;
                        plusMSGMessageOut.F = "P3";

                        bool RecentlyStartedChal = client.EnteringChalAt.HasValue && (DateTime.Now - client.EnteringChalAt.Value).TotalSeconds < 5;

                        if (!(TEXT == "decline" && RecentlyStartedChal))
                        {
                            TempClient.Broadcast(plusMSGMessageOut);
                        }

                        if (TEXT.Contains("lockchal"))
                        {
                            //client.Broadcast(plusMSGMessageOut); //This is wrong? Why does it work? Check If not needed remove to save data
                            ConsoleManager.WriteLine(client.LoadedPersona.Name + " Accepted Challanage from " + PRIV);
                        }

                        if (TEXT.Contains("abortChal"))
                        {
                            ChalMessageIn.RemoveChallange(client, this);

                            if (client.room != null)
                            {
                                DQUEMessageout dQUEMessageout = new DQUEMessageout();

                                client.Broadcast(dQUEMessageout);
                            }

                            ConsoleManager.WriteLine(client.LoadedPersona.Name + " Aborted Challanage from " + PRIV);
                        }
                    }
                }
            }
            else
            {
                if(client.room!=null)
                {
                    client.room.ProcessMessage(this, client);
                }
            }
        }

        public struct Challange
        {
            public string TrackID;
            public string Gamemode1;
            public string Gamemode2;
            public string Ranked;
            public string Multipliers;
            public string Powerups;
            public string AI;
            public string PointIcons;
            public string GameVersion;
            public string U1;
            public string U2;
            public string U3;
            public string U4;
        }
    }
}
