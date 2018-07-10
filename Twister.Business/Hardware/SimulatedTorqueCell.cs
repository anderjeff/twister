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

		// I need min torque, max torque, and speed (could hard code a 3)
		
		public float Torque { get; private set; }
		public void RefreshTorque()
		{	
		}
	}
}
