using System;

namespace HangFireCore.Core
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class HangfireJobMinutesAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly int minutes;

        // This is a positional argument
        public HangfireJobMinutesAttribute(int minutes)
        {
            this.minutes = minutes;
        }

        public int Minutes
        {
            get { return minutes; }
        }
    }
}
