using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twister.Business.Tests
{
	/// <summary>
	/// Represents a calibration that was done during the course of
	/// a fatigue test. Its purpose is to store the clockwise and
	/// counterclockwise angle and torque values, as well as calculate
	/// the torque based off a provided angle, assuming a linear
	/// torque vs. angle curve.
	/// </summary>
	public class FatigueTestCalibration
	{
		public FatigueTestCalibration(float cwAngle, float ccwAngle, int cwTorque, int ccwTorque)
		{
			ClockwiseAngle = cwAngle;
			CounterClockwiseAngle = ccwAngle;
			ClockwiseTorque = cwTorque;
			CounterClockwiseTorque = ccwTorque;
		}

		public float ClockwiseAngle { get; }
		public float CounterClockwiseAngle { get; }
		public int ClockwiseTorque { get; }
		public int CounterClockwiseTorque { get; }

		private float Slope
		{
			get
			{
				float x1 = ClockwiseAngle;
				float x2 = CounterClockwiseAngle;
				int y1 = ClockwiseTorque;
				int y2 = CounterClockwiseTorque;

				float slope = (y2 - y1) / (x2 - x1);
				return slope;
			}
		}

		private int YIntercept
		{
			get
			{
				// use a known point.
				var y = ClockwiseTorque;
				var x = ClockwiseAngle;

				// b = y - mx
				int yIntercept = y - (int) (Slope * x);
				return yIntercept;
			}
		}

		/// <summary>
		/// Gets a torque value for a given angle, based on the last
		/// set of calibration results.
		/// </summary>
		/// <param name="angle">The measured angle.</param>
		/// <returns>
		/// The torque, in in-lbs, that the angle represents.
		/// </returns>
		public int CalculatedTorqueFromAngle(float angle)
		{
			int angleTemp = (int) (Slope * angle) + YIntercept;
			return angleTemp;
		}
	}
}
