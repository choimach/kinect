using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinect.Recognition.States
{
    public delegate void StateChangedEventHandler(object sesender, StateChangedEventArgs args);

    public class StateChangedEventArgs : EventArgs
    {
        public FSMStateId OldState { get; set; }
        public FSMStateId NewState { get; set; }
    }
}
