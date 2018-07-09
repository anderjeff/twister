using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twister.Business.Hardware
{
	public class SimulatedTorqueCell : ITorqueCell
	{
		public SimulatedTorqueCell()
		{
		}

		public float Torque { get; private set; }
		public void RefreshTorque()
		{
			Torque = 1;
		}
	}
}
