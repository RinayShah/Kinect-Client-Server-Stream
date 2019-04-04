using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ExampleWpf
{
    sealed class CustomTraceListner : TraceListener
    {
        public override void Write(string message)
        {
            throw new NotImplementedException();
        }

        public override void WriteLine(string message)
        {
            throw new NotImplementedException();
        }
    }
    sealed class TraceListnerWPF : TraceListener
    {
        private readonly TextBox _outputTextBox;
        public TraceListnerWPF(TextBox outputTextBox)
        {
            _outputTextBox = outputTextBox;
        }

        public override void Write(string message)
        {
            _outputTextBox.Dispatcher.BeginInvoke((Action)(() => _outputTextBox.Text += message ?? string.Empty));
        }

        public override void WriteLine(string message)
        {
            _outputTextBox.Dispatcher.BeginInvoke((Action)(() => _outputTextBox.Text += (message + Environment.NewLine)));
        }
    }
}
