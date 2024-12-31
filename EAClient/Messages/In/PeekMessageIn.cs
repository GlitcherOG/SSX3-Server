﻿using SSX3_Server.EAServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSX3_Server.EAClient.Messages
{
    public class PeekMessageIn : EAMessage
    {
        public override string MessageType { get { return "peek"; } }

        public string NAME;

        public override void AssignValues()
        {
            NAME = stringDatas[0].Value;
        }

        public override void AssignValuesToString()
        {
            AddStringData("NAME", NAME);
        }

        public override void ProcessCommand(EAClientManager client, EAServerRoom room = null)
        {
            var Room = EAServerManager.Instance.GetRoom(NAME);

            if (Room != null)
            {
                Room.BoradcastBackUserList(client);
            }
        }
    }
}