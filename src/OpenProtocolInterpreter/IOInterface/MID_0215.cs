﻿using OpenProtocolInterpreter.Converters;
using System.Collections.Generic;
using System.Linq;

namespace OpenProtocolInterpreter.IOInterface
{
    /// <summary>
    /// MID: IO device status reply
    /// Description: 
    ///     This message is sent as an answer to the MID 0214 IO device status request.
    ///     MID 0215 revision 1 should only be used to get the status of IO devices with max 8 relays/digital
    ///     inputs.
    ///     For external I/O devices each list contain up to 8 relays/digital inputs. For the internal device the lists
    ///     contain up to 4 relays/digital inputs and the remaining 4 will be empty.
    ///     MID 0215 revision 2 can be used to get the status of all types of IO devices with up to 48 relays/digital
    ///     inputs.
    /// Message sent by: Controller
    /// Answer: None
    /// </summary>
    internal class MID_0215 : Mid, IIOInterface
    {
        private readonly IValueConverter<int> _intConverter;
        private readonly IValueConverter<IEnumerable<Relay>> _relayListConverter;
        private readonly IValueConverter<IEnumerable<DigitalInput>> _digitalInputListConverter;
        private const int LAST_REVISION = 2;
        public const int MID = 215;

        public int IODeviceId
        {
            get => RevisionsByFields[HeaderData.Revision][(int)DataFields.IO_DEVICE_ID].GetValue(_intConverter.Convert);
            set => RevisionsByFields[HeaderData.Revision][(int)DataFields.IO_DEVICE_ID].SetValue(_intConverter.Convert, value);
        }
        public List<Relay> Relays { get; set; }
        public List<DigitalInput> DigitalInputs { get; set; }
        //rev 2
        public int NumberOfRelays
        {
            get => RevisionsByFields[2][(int)DataFields.NUMBER_OF_RELAYS].GetValue(_intConverter.Convert);
            private set => RevisionsByFields[2][(int)DataFields.NUMBER_OF_RELAYS].SetValue(_intConverter.Convert, value);
        }
        public int NumberOfDigitalInputs
        {
            get => RevisionsByFields[2][(int)DataFields.NUMBER_OF_DIGITAL_INPUTS].GetValue(_intConverter.Convert);
            private set => RevisionsByFields[2][(int)DataFields.NUMBER_OF_DIGITAL_INPUTS].SetValue(_intConverter.Convert, value);
        }

        public MID_0215(int revision = LAST_REVISION) : base(MID, revision)
        {
            Relays = new List<Relay>();
            DigitalInputs = new List<DigitalInput>();
        }

        internal MID_0215(IMid nextTemplate) : this() => NextTemplate = nextTemplate;

        public override string Pack()
        {
            if (HeaderData.Revision > 1)
            {
                NumberOfRelays = Relays.Count;
                NumberOfDigitalInputs = DigitalInputs.Count;

                var relayListField = RevisionsByFields[2][(int)DataFields.RELAY_LIST];
                relayListField.Size = NumberOfRelays * 4;
                relayListField.Value = _relayListConverter.Convert(Relays);
                RevisionsByFields[2][(int)DataFields.NUMBER_OF_DIGITAL_INPUTS].Index = relayListField.Index + relayListField.Size;

                RevisionsByFields[2][(int)DataFields.DIGITAL_INPUT_LIST].Index = RevisionsByFields[2][(int)DataFields.NUMBER_OF_DIGITAL_INPUTS].Index + 2;
                RevisionsByFields[2][(int)DataFields.DIGITAL_INPUT_LIST].Size = NumberOfDigitalInputs * 4;
                RevisionsByFields[2][(int)DataFields.DIGITAL_INPUT_LIST].Value = _digitalInputListConverter.Convert(DigitalInputs);
            }
            else
            {
                HeaderData.Revision = 1;
                RevisionsByFields[1][(int)DataFields.RELAY_LIST].Value = _relayListConverter.Convert(Relays);
                RevisionsByFields[1][(int)DataFields.DIGITAL_INPUT_LIST].Value = _digitalInputListConverter.Convert(DigitalInputs);
            }

            string pkg = BuildHeader();
            int prefixIndex = 1;
            foreach (var field in RevisionsByFields[HeaderData.Revision])
            {
                pkg += prefixIndex.ToString().PadLeft(2, '0') + field.Value;
                prefixIndex++;
            }
            return pkg;
        }

        public override Mid Parse(string package)
        {
            if (IsCorrectType(package))
            {
                ProcessHeader(package);
                var relayListField = RevisionsByFields[2][(int)DataFields.RELAY_LIST];
                var digitalListField = RevisionsByFields[2][(int)DataFields.DIGITAL_INPUT_LIST];

                if (HeaderData.Revision > 1)
                {
                    int numberOfRelays = _intConverter.Convert(GetValue(RevisionsByFields[2][(int)DataFields.NUMBER_OF_RELAYS], package));
                    relayListField.Size = numberOfRelays * 4;

                    RevisionsByFields[2][(int)DataFields.NUMBER_OF_DIGITAL_INPUTS].Index = relayListField.Index + relayListField.Size;

                    digitalListField.Index = digitalListField.Index + 2;
                    digitalListField.Size = package.Length - digitalListField.Index;
                }

                foreach (var field in RevisionsByFields[HeaderData.Revision])
                    field.Value = GetValue(field, package);

                Relays = _relayListConverter.Convert(relayListField.Value).ToList();
                DigitalInputs = _digitalInputListConverter.Convert(digitalListField.Value).ToList();
                return this;
            }

            return NextTemplate.Parse(package);
        }

        protected override Dictionary<int, List<DataField>> RegisterDatafields()
        {
            return new Dictionary<int, List<DataField>>()
            {
                {
                    1, new List<DataField>()
                            {
                                new DataField((int)DataFields.IO_DEVICE_ID, 20, 2, '0', DataField.PaddingOrientations.LEFT_PADDED),
                                new DataField((int)DataFields.RELAY_LIST, 24, 32),
                                new DataField((int)DataFields.DIGITAL_INPUT_LIST, 58, 32)
                            }
                },
                {
                    2, new List<DataField>()
                            {
                                new DataField((int)DataFields.IO_DEVICE_ID, 20, 2, '0', DataField.PaddingOrientations.LEFT_PADDED),
                                new DataField((int)DataFields.NUMBER_OF_RELAYS, 24, 2, '0', DataField.PaddingOrientations.LEFT_PADDED),
                                new DataField((int)DataFields.RELAY_LIST, 28, 0),
                                new DataField((int)DataFields.NUMBER_OF_DIGITAL_INPUTS, 0, 2, '0', DataField.PaddingOrientations.LEFT_PADDED),
                                new DataField((int)DataFields.DIGITAL_INPUT_LIST, 0, 0)
                            }
                }
            };
        }

        public enum DataFields
        {
            IO_DEVICE_ID,
            RELAY_LIST,
            DIGITAL_INPUT_LIST,
            //rev2 
            NUMBER_OF_RELAYS,
            NUMBER_OF_DIGITAL_INPUTS
        }
    }
}
