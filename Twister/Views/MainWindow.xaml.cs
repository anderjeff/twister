using System;
using System.ComponentModel;
using System.Windows;
using Twister.ViewModels;

namespace Twister.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = MainWindow_VM.Instance;
            _contentControl.Content = MainWindow_VM.Instance.CurrentViewModel;

            MainWindow_VM.Instance.PropertyChanged += MainWindow_PropertyChanged;

            DataContextChanged += MainWindow_DataContextChanged;
            layoutRoot.DataContextChanged += LayoutRoot_DataContextChanged;
        }

        private void LayoutRoot_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            object ctx = layoutRoot.DataContext;
        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            object context = DataContext;
        }

        private void MainWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentViewModel")
            {
                object ctx = layoutRoot.DataContext;
                _contentControl.Content = MainWindow_VM.Instance.CurrentViewModel;
                ctx = layoutRoot.DataContext;
            }
        }

        private void mainWindow_Closed(object sender, EventArgs e)
        {
            MainWindow_VM.Instance.EndTestSession();
        }
    }
}