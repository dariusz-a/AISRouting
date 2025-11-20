using System;

namespace AISRouting.Core.Models
{
    public class TimeInterval
    {
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }

        public TimeSpan Duration => Stop - Start;
        public bool IsValid => Stop > Start;
    }
}
