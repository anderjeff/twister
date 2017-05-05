using System.Windows;
using System.Windows.Controls;
using Twister.ViewModels;

namespace Twister.Views
{
    /// <summary>
    ///     Interaction logic for FullyReversedTestPage.xaml
    /// </summary>
    public partial class FullyReversedTestUserControl : UserControl
    {
        public FullyReversedTestUserControl()
        {
            InitializeComponent();

            DataContextChanged += FullyReversedTestUserControl_DataContextChanged;
        }

        private void FullyReversedTestUserControl_DataContextChanged(object sender,
            DependencyPropertyChangedEventArgs e)
        {
            FullyReversedTorqueTest_VM vm = DataContext as FullyReversedTorqueTest_VM;
            if (vm != null)
                vm.TorqueAngleChart = _chartTestData;
        }
    }
}