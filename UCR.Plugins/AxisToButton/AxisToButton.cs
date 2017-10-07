﻿using System;
using System.ComponentModel.Composition;
using System.Xml.Serialization;
using UCR.Core.Models.Binding;
using UCR.Core.Models.Plugin;
using UCR.Core.Utilities;

namespace UCR.Plugins.AxisToButton
{
    [Export(typeof(Plugin))]
    public class AxisToButton : Plugin
    {
        [XmlIgnore]
        public DeviceBinding Input { get; set; }
        [XmlIgnore]
        public DeviceBinding OutputLow { get; set; }
        [XmlIgnore]
        public DeviceBinding OutputHigh { get; set; }
        
        public bool Invert { get; set; }

        private int _deadZoneValue;
        private string _deadZone;
        public string DeadZone
        {
            get { return _deadZone; }
            set
            {
                SetIntValue(ref _deadZoneValue, value);
                ContextChanged();
                _deadZone = value;
            }
        }

        private long direction = 0;

        public override string PluginName()
        {
            return "Axis to buttons";
        }

        public AxisToButton()
        {
            Input = InitializeInputMapping(InputChanged);
            OutputLow = InitializeOutputMapping();
            OutputHigh = InitializeOutputMapping();
            DeadZone = "0";
        }

        private void InputChanged(long value)
        {
            if (Invert) value *= -1;
            value = Math.Sign(ApplyDeadZone(value));
            if (value == direction) return;
            switch (value)
            {
                case 0:
                    WriteOutput(OutputLow, 0);
                    WriteOutput(OutputHigh, 0);
                    break;
                case -1:
                    WriteOutput(OutputLow, 1);
                    break;
                case 1:
                    WriteOutput(OutputHigh, 1);
                    break;
            }
            direction = value;
        }

        private long ApplyDeadZone(long value)
        {
            var gap = (_deadZoneValue / 100.0) * Constants.AxisMaxValue;
            var remainder = Constants.AxisMaxValue - gap;
            var gapPercent = Math.Max(0, Math.Abs(value) - gap) / remainder;
            return (long)(gapPercent * Constants.AxisMaxValue * Math.Sign(value));
        }

        private static void SetIntValue(ref int field, string value)
        {
            int result;
            if (int.TryParse(value, out result))
            {
                field = result;
            }
        }
    }
}
