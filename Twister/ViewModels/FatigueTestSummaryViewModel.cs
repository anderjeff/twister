using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twister.Utilities;

namespace Twister.ViewModels
{
	public class FatigueTestSummaryViewModel : Base_VM
	{
		public FatigueTestSummaryViewModel()
		{
			CloseCommand = new RelayCommand(CloseProgram);
		}

		private void CloseProgram()
		{
			System.Windows.Application.Current.Shutdown();
		}

		public RelayCommand CloseCommand { get; private set; }
	}
}
