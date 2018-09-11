using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twister.Business.Hardware
{
	public interface ITorqueCell
	{
		float Torque { get; }
		void RefreshTorque();
	}
}
