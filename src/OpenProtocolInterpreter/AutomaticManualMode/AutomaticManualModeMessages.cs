﻿using OpenProtocolInterpreter.Messages;

namespace OpenProtocolInterpreter.AutomaticManualMode
{
    internal class AutomaticManualModeMessages : IMessagesTemplate
    {
        private readonly IMid _templates;

        public AutomaticManualModeMessages()
        {
            _templates = new Mid0400(new Mid0401(new Mid0402(new Mid0403(new Mid0410(new Mid0411(null))))));
        }

        public AutomaticManualModeMessages(System.Collections.Generic.IEnumerable<Mid> selectedMids)
        {
            _templates = MessageTemplateFactory.BuildChainOfMids(selectedMids);
        }

        public Mid ProcessPackage(string package) => _templates.Parse(package);

        public Mid ProcessPackage(byte[] package) => _templates.Parse(package);
    }
}
