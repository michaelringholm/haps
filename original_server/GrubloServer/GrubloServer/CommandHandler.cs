using HapsCommands;
using Newtonsoft.Json;
using System;

namespace GrubloServer
{
    class CommandHandler
    {
        public string Handle(string command)
        {
            try
            {
                if (command.Contains("__id0__"))
                    return HandleCreateUser(command);

                if (command.Contains("__id1__"))
                    return HandleRequestSequence(command);

                if (command.Contains("__id2__"))
                    return HandleClaimWin(command);

                throw new ArgumentException("Unknown command: " + command);
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }
        }

        private string HandleCreateUser(string command)
        {
            CreateUser cu = JsonConvert.DeserializeObject<CreateUser>(command);
            return Guid.NewGuid().ToString();
        }

        private string HandleRequestSequence(string command)
        {
            // Update these when odds change
            const int CurrentDistId = 1;
            const uint CurrentFlags = 0xffffffff; // Flags that are off cannot be won. Client will filter out wins.

            RequestSequence rs = JsonConvert.DeserializeObject<RequestSequence>(command);
            ItemSequence seq = new ItemSequence();
            seq.s = (int)(DateTime.UtcNow.Ticks * 17);
            seq.suc = 10;
            seq.f = CurrentFlags;

            // If client's dist is not up to date we send a new one
            if (rs.distId != CurrentDistId)
            {
                seq.did = CurrentDistId;
                seq.d = Distribution.CurrentDistribution;
            }

            // Client cache: cache latest sequence sent. Just memory for small wins, but persisted for large wins?

            string str = JsonConvert.SerializeObject(seq);
            return str;
        }

        private string HandleClaimWin(string command)
        {
            ClaimWin cw = JsonConvert.DeserializeObject<ClaimWin>(command);
            return "";
        }
    }
}
