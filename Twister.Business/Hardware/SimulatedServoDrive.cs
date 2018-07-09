using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twister.Business.Hardware
{
	public class SimulatedServoDrive : IServoDrive
	{
		public float GearboxAngle { get; private set; }
		public void RefreshPosition()
		{
			GearboxAngle = 2;
		}

		public void StoreParameter(ServoDriveEnums.RegisterAddress location, int value)
		{
			throw new NotImplementedException();
		}

		public void StoreParameter(ServoDriveEnums.RegisterAddress location, float value)
		{
			throw new NotImplementedException();
		}

		public int RetrieveParameter(ServoDriveEnums.RegisterAddress location)
		{
			throw new NotImplementedException();
		}
	}
}
