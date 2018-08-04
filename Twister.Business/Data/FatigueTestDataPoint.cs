using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twister.Business.Data
{
	public class FatigueTestDataPoint
	{
		public FatigueTestDataPoint(int cycleNumber)
		{
			SampleTime = DateTime.Now;
			CycleNumber = cycleNumber;
		}

		/// <summary>
		/// Gets or Sets the maximum angle for the forward (clockwise) cycle.
		/// </summary>
		public float MaxAngle { get; set; }
		/// <summary>
		/// Gets or Sets the maximum torque for the forward (clockwise) cycle.
		/// </summary>
		public int MaxTorque { get; set; }
		/// <summary>
		/// Gets or Sets the maximum angle for the reverse (counterclockwise) cycle.
		/// </summary>
		public float MinAngle { get; set; }
		/// <summary>
		/// Gets or Sets the maximum torque for the reverse (counterclockwise) cycle. 
		/// </summary>
		public int MinTorque { get; set; }
		/// <summary>
		/// Gets the cycle number in the overall test.
		/// </summary>
		public int CycleNumber { get; private set; }
		/// <summary>
		/// Gets the time that the sample was taken.
		/// </summary>
		public DateTime SampleTime { get; private set; }
	}
}
