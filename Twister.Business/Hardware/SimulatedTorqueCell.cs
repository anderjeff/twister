using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twister.Business.Tests;

namespace Twister.Business.Hardware
{
	public class SimulatedTorqueCell : ITorqueCell
	{
		private SimulatorEngine _engine;

		public SimulatedTorqueCell(SimulatorEngine engine)
		{
			_engine = engine;
		}

		// I need min torque, max torque, and speed (could hard code a 3)


		public double LastTime { get; private set; }
		public float Torque { get; private set; }

		public void RefreshTorque()
		{
			int maxTorque = _engine.CurrentCondition.ClockwiseTorque;
			int minTorque = _engine.CurrentCondition.CounterclockwiseTorque;

			int amplitude = (maxTorque - minTorque) / 2;
			int verticalShift = amplitude + minTorque;
			int frequency = _engine.CurrentCondition.CyclesPerSecond;
			LastTime = _engine.ElapsedMilliseconds() / (double) 1000;

			// T(t) = Asin(2πft) + vertical shift... It's a sinusoidal curve.
			Torque = (float) (amplitude * Math.Sin(frequency * 2 * Math.PI * LastTime) + verticalShift);
		}
	}
}
