using SSX3_Server.EAClient;
using SSX3_Server.EAClient.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSX3_Server.EAServer
{
    public static class McommCommands
    {
        public static void ProcessCommandUser(EAClientManager client)
        {

        }

        public static void ProcessCommandRoom(EAClientManager client, EAServerRoom room, string Text)
        {
            Text = Text.TrimStart('!');
            string[] split = Text.Split(' ');

            if (split[0].ToLower()=="global")
            {
                room.isGlobal = !room.isGlobal;

                if (room.isGlobal)
                {
                    GenerateMcommMessage("Room is now set to global", room);
                }
                else
                {
                    GenerateMcommMessage("Room is now removed from global", room);
                }
            }
            if (split[0].ToLower()== "ftrack")
            {
                if(split.Length>1)
                {
                    try
                    {
                        int ID = int.Parse(split[1]);

                        client.ForceTrackID = ID;
                        GenerateMcommMessageUser("Track ID Set", client);
                    }
                    catch
                    {
                        GenerateMcommMessageUser("Invalid ID",client);
                    }
                }
                else
                {
                    GenerateMcommMessageUser("Please add track id", client);
                }
            }

            if (split[0].ToLower() == "fevent")
            {
                if (split.Length > 1)
                {
                    try
                    {
                        int ID = int.Parse(split[1]);

                        client.ForceGamemodeID = ID;

                        GenerateMcommMessageUser("Event ID Set", client);
                    }
                    catch
                    {
                        GenerateMcommMessageUser("Invalid ID", client);
                    }
                }
                else
                {
                    GenerateMcommMessageUser("Please add event id", client);
                }
            }

            if (split[0].ToLower() == "ip")
            {
                if (split.Length > 1)
                {
                    try
                    {
                        client.OverrideIP = split[1];

                        GenerateMcommMessageUser("Override IP Set", client);
                    }
                    catch
                    {
                        GenerateMcommMessageUser("Invalid IP Set", client);
                    }
                }
                else
                {
                    GenerateMcommMessageUser("Please add IP", client);
                }
            }

            if (split[0].ToLower() == "crossregion")
            {
                EAServerManager.Instance.config.AllowCrossPlay = !EAServerManager.Instance.config.AllowCrossPlay;

                if(EAServerManager.Instance.config.AllowCrossPlay)
                {
                    GenerateMcommMessageUser("Cross-Region is now enabled", client);
                }
                else
                {
                    GenerateMcommMessageUser("Cross-Region is now disabled", client);
                }

                EAServerManager.Instance.config.CreateJson(AppContext.BaseDirectory + "\\ServerConfig.cfg");
            }

            if (split[0].ToLower() == "password")
            {
                if (split.Length==2)
                {
                    room.roomPassword = split[1];
                    GenerateMcommMessageUser("Room Password Changed", client);
                    EAServerManager.Instance.BroadcastMessage(room.GeneratePlusRoomInfo());
                }
                else if (split.Length == 1)
                {
                    room.roomPassword = "";
                    GenerateMcommMessageUser("Room Password Removed", client);
                    EAServerManager.Instance.BroadcastMessage(room.GeneratePlusRoomInfo());
                }
                else
                {
                    GenerateMcommMessageUser("Password must not have a space", client);
                }    
            }
        }

        public static void GenerateMcommMessage(string Text, EAServerRoom room)
        {
            PlusMSGMessageOut plusMSGMessageOut = new PlusMSGMessageOut();

            plusMSGMessageOut.N = "Mcomm";
            plusMSGMessageOut.T = Text;
            plusMSGMessageOut.F = "C";

            room.plusMSGMessageOuts.Add(plusMSGMessageOut);

            if (room.plusMSGMessageOuts.Count > 6)
            {
                room.plusMSGMessageOuts.RemoveAt(0);
            }

            room.BroadcastAllUsers(plusMSGMessageOut);
        }

        public static void GenerateMcommMessageUser(string Text, EAClientManager client)
        {
            PlusMSGMessageOut plusMSGMessageOut = new PlusMSGMessageOut();

            plusMSGMessageOut.N = "Mcomm";
            plusMSGMessageOut.T = Text;
            plusMSGMessageOut.F = "C";

            client.Broadcast(plusMSGMessageOut);
        }
    }
}
