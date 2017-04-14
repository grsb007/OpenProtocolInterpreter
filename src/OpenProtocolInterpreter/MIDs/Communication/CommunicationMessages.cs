﻿namespace OpenProtocolInterpreter.MIDs.Communication
{
    internal class CommunicationMessages : IMessagesTemplate
    {
        private readonly IMID templates;

        public CommunicationMessages()
        {
            this.templates = new MID_0005(new MID_0004(new MID_0001(new MID_0002(null))));
        }

        public MID processPackage(string package)
        {
            return this.templates.processPackage(package);
        }
    }
}