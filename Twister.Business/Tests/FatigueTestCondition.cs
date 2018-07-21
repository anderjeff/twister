using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twister.Business.Tests
{
	public class FatigueTestCondition
	{
		private const int MAX_SPEED = 3;
		private const int MIN_SPEED = 1;
		private const int MAX_TORQUE = 20000;
		private const int MIN_TORQUE = -20000;
		private int _clockwiseTorque;
		private int _counterClockwiseTorque;
		private int _cyclesPerSecond;


		/// <summary>
		/// Gets or sets the unique identifier for the fatigue test condition.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the fatigue test id that this test condition belongs with.
		/// </summary>
		public int FatigueTestId { get; set; }

		/// <summary>
		/// Gets or sets the target torque in the clockwise direction.
		/// </summary>
		public int ClockwiseTorque
		{
			get { return _clockwiseTorque; }
			set
			{
				if (value <= MAX_TORQUE)
				{
					_clockwiseTorque = value;
				}
				else
				{
					throw new ArgumentOutOfRangeException($"Clockwise torque cannot exceed {MAX_TORQUE} in-lbs.");
				}

			}
		}

		/// <summary>
		/// Gets or sets the target torque in the counterclockwise direction.
		/// </summary>
		public int CounterclockwiseTorque
		{
			get { return _counterClockwiseTorque; }
			set
			{
				if (value >= MIN_TORQUE)
				{
					_counterClockwiseTorque = value;
				}
				else
				{
					throw new ArgumentOutOfRangeException($"Clockwise torque cannot exceed {MIN_TORQUE} in-lbs.");
				}

			}
		}

		/// <summary>
		/// Gets or sets the rotation speed, measured in cycles per second.
		/// </summary>
		public int CyclesPerSecond
		{
			get { return _cyclesPerSecond; }
			set
			{
				if (value > 0 && value < 4)
				{
					_cyclesPerSecond = value;
				}
				else
				{
					throw new ArgumentOutOfRangeException($"Cycles per second must be between {MIN_SPEED} and {MAX_SPEED}.");
				}
			}
		}

		/// <summary>
		/// Gets or sets the number of cycles required to be completed by this <see cref="FatigueTestCondition"/>
		/// </summary>
		public int CyclesRequired { get; set; }

		/// <summary>
		/// Gets or sets the number of cycles that have been completed .
		/// </summary>
		public int CyclesCompleted { get; set; }

		/// <summary>
		/// Gets or sets the number of cycles that occur before the torque measurement is verified.
		/// </summary>
		/// <remarks>
		/// The test is allowed to run so fast that the torque cell cannot provide
		/// values quick enough.  So the calibration interval provides a time every
		/// x number of cycles where the test slows down temporarily to measure the
		/// current torque and then set the +/- rotational angle to match.
		/// </remarks>
		public int CalibrationInterval { get; set; }

		/// <summary>
		/// Gets the number of cycles remaining before this <see cref="FatigueTestCondition"/> is complete.
		/// </summary>
		public int CyclesRemaining 
		{
			get { return CyclesRequired - CyclesCompleted; }
		}

		/// <summary>
		/// Gets the estimated time remaining before all of the remaining cycles are completed
		/// for this <see cref="FatigueTestCondition"/>.
		/// </summary>
		public TimeSpan TimeRemaining
		{
			get
			{
				return GetTimeRemaining();
			}
		}

		/// <summary>
		/// Gets the height of the waveform representing torque vs. time.
		/// </summary>
		public int Amplitude
		{
			get { return (ClockwiseTorque - CounterclockwiseTorque) / 2; }
		}

		/// <summary>
		/// Gets the midpoint value of torque.
		/// </summary>
		public int VerticalShift
		{
			get { return Amplitude + CounterclockwiseTorque; }
		}

		/// <summary>
		/// Helper method to isolate the logic outside of the getter.
		/// </summary>
		/// <returns>
		/// The <see cref="TimeSpan"/> representing the time remaining until all
		/// of the required cycles are completed for this <see cref="FatigueTestCondition"/>.
		/// </returns>
		private TimeSpan GetTimeRemaining()
		{
			// 0 cycles per second is not allowed, but temporarily it may be possible, so this covers it.
			if (CyclesPerSecond == 0)
			{
				return new TimeSpan(0, 0, 0);
			}

			int cyclesRemaining = CyclesRemaining;
			int secondsRemaining = 0;
			if (CalibrationInterval < 1)
			{
				secondsRemaining = cyclesRemaining / CyclesPerSecond;
				return new TimeSpan(0, 0, secondsRemaining);
			}

			// already checked CalibrationInterval for zero.
			int calCyclesRemaining = cyclesRemaining / CalibrationInterval;
			int cyclesNotCalRemaining = cyclesRemaining - calCyclesRemaining;
			secondsRemaining = calCyclesRemaining + (cyclesNotCalRemaining / CyclesPerSecond);

			var timeRemaining = new TimeSpan(0, 0, secondsRemaining);
			return timeRemaining;
		}
	}
}
