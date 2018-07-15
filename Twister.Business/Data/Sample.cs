using System;

namespace Twister.Business.Data
{
    public class Sample : ICloneable
    {
        private Sample()
        {
        }

        public Sample(float torque, float angle)
        {
            Torque = torque;
            Angle = angle;

            // SampleTime = new DateTimeOffset(DateTime.Now, new TimeSpan(0, 0, 0)).TimeOfDay;
            // need sample time to save to sql server.
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTimeOffset offsetCentralTime = TimeZoneInfo.ConvertTime(new DateTimeOffset(
                DateTime.UtcNow, new TimeSpan(0, 0, 0)), timeZoneInfo);

            SampleTime = offsetCentralTime.TimeOfDay;
        }

	    public Sample(float torque, float angle, double elapsedTime)
	    {
		    Torque = torque;
		    Angle = angle;
		    ElapsedTime = elapsedTime;
	    }

	    public float Torque { get; private set; }

        public float Angle { get; private set; }

        /// <summary>
        ///     The moment in time the sample was taken.
        /// </summary>
        public TimeSpan SampleTime { get; private set; }

		/// <summary>
		/// The elapsed time, from some reference point, that the sample was recorded.
		/// </summary>
	    public double ElapsedTime { get; private set; }

	    public object Clone()
        {
            Sample copy = new Sample
            {
                Angle = Angle,
                SampleTime = SampleTime,
                Torque = Torque
            };

            return copy;
        }


        public override string ToString()
        {
            return string.Format(
                "Torque [{0:n0}] Angle [{1:n3}] Time [{2:o}]",
                Torque,
                Angle,
                SampleTime);
        }
    }
}