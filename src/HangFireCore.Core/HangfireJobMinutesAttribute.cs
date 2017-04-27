using System;

namespace HangFireCore.Core
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class HangfireMinutesJobAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly int minutes;

        // This is a positional argument
        public HangfireMinutesJobAttribute(int minutes)
        {
            this.minutes = minutes;
        }

        public int Minutes
        {
            get { return minutes; }
        }
    }
}
