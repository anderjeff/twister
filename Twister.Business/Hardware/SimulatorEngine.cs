using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twister.Business.Tests;

namespace Twister.Business.Hardware
{
	public class SimulatorEngine
	{
		private Stopwatch _sw;

		public SimulatorEngine(Stopwatch sw)
		{
			_sw = sw;
		}

		public FatigueTestCondition CurrentCondition { get; set; }

		public void StartSimulate()
		{
			_sw.Start();
		}

		public void StopSimulate()
		{
			_sw.Stop();
		}

		/// <summary>
		/// Gets the elapsed time that the SimulatorEngine has been running.
		/// </summary>
		/// <returns>The number of milliseconds the simulator has been running.</returns>
		public long ElapsedMilliseconds()
		{
			return _sw.ElapsedMilliseconds;
		}
	}
}
