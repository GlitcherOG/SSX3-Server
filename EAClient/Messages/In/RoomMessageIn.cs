﻿using SSX3_Server.EAServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSX3_Server.EAClient.Messages
{
    public class RoomMessageIn : EAMessage
    {
        public override string MessageType { get { return "room"; } }

        public string RoomType;
        public string NAME;
        public string PASS = "";

        public override void AssignValues()
        {
            string[] Temp = stringDatas[0].Value.Split(".");
            RoomType = Temp[0];
            NAME = Temp[1];

            if (stringDatas.Count > 1)
            {
                PASS = stringDatas[1].Value;
            }
        }

        public override void AssignValuesToString()
        {
            AddStringData("NAME", RoomType + "." + NAME);
            //AddStringData("LIDENT", "0");
            //AddStringData("LCOUNT", "0");
            if (PASS != "")
            {
                AddStringData("PASS", PASS);
            }
        }

        public override void ProcessCommand(EAClientManager client, EAServerRoom room = null)
        {
            for (int i = 0; i < 2; i++)
            {
                client.Broadcast(this);
            }
        }
    }
}
