using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twister.Business.Tests
{
	public class FatigueTestCondition
	{
		/// <summary>
		/// The unique identifier for the fatigue test condition.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The fatigue test id that this test condition belongs with.
		/// </summary>
		public int FatigueTestId { get; set; }

		/// <summary>
		/// The target torque in the clockwise direction.
		/// </summary>
		public int ClockwiseTorque { get; set; }

		/// <summary>
		/// The target torque in the counterclockwise direction.
		/// </summary>
		public int CounterclockwiseTorque { get; set; }

		/// <summary>
		/// The rotation speed, measured in cycles per second.
		/// </summary>
		public int CyclesPerSecond { get; set; }

		/// <summary>
		/// The number of cycles that need to be completed in order for the
		/// test condition to be satisfied.
		/// </summary>
		public int CyclesRequired { get; set; }

		/// <summary>
		/// The number of cycles that occur before the torque measurement is verified.
		/// </summary>
		public int CalibrationInterval { get; set; }

	}
}
