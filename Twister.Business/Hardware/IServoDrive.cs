using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twister.Business.Hardware
{
	public interface IServoDrive
	{
		float GearboxAngle { get; }
		void RefreshPosition();
		void StoreParameter(ServoDriveEnums.RegisterAddress location, int value);
		void StoreParameter(ServoDriveEnums.RegisterAddress location, float value);
		int RetrieveParameter(ServoDriveEnums.RegisterAddress location);
		float RetrieveClockwiseLimit();
		float RetrieveCounterclockwiseLimit();
	}
}
