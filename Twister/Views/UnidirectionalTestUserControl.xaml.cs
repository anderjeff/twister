using System.Windows;
using System.Windows.Controls;
using Twister.ViewModels;

namespace Twister.Views
{
    /// <summary>
    ///     Interaction logic for UnidirectionalToFailureUserControl.xaml
    /// </summary>
    public partial class UnidirectionalTestUserControl : UserControl
    {
        public UnidirectionalTestUserControl()
        {
            InitializeComponent();
            DataContextChanged += UnidirectionalTestUserControl_DataContextChanged;
        }

        private void UnidirectionalTestUserControl_DataContextChanged(object sender,
            DependencyPropertyChangedEventArgs e)
        {
            UnidirectionalTorqueTest_VM vm = DataContext as UnidirectionalTorqueTest_VM;
            if (vm != null)
                vm.TorqueAngleChart = _chartTestData;
        }
    }
}