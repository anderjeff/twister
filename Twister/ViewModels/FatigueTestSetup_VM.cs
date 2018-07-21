using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twister.Business.Tests;
using Twister.Utilities;

namespace Twister.ViewModels
{
	public class FatigueTestSetup_VM : Base_VM
	{
		private bool _noConditionsDefined;
		private FatigueTest _fatigueTest;
		private bool _canSeeNext;

		public FatigueTestSetup_VM()
		{
			NoConditionsDefined = true;
			CanSeeNext = false;

			TestConditions = new ObservableCollection<FatigueTestCondition>();
			TestConditions.CollectionChanged += TestConditionsOnCollectionChanged;

			AddConditionCommand = new RelayCommand(AddCondition);
			NextCommand = new RelayCommand(GoToNextScreen);
			RemoveConditionCommand = new RelayCommand<FatigueTestCondition>(RemoveCondition);
		}

		private void RemoveCondition(FatigueTestCondition condition)
		{
			TestConditions.Remove(condition);
		}

		private void GoToNextScreen()
		{
			throw new NotImplementedException();
		}

		private void AddCondition()
		{
			TestConditions.Add(new FatigueTestCondition());
		}

		/// <summary>
		/// Indicates if the test setup has defined any test conditions.
		/// </summary>
		public bool NoConditionsDefined
		{
			get { return _noConditionsDefined; }
			set
			{
				_noConditionsDefined = value;
				OnPropertyChanged();
			}
		}

		public bool CanSeeNext
		{
			get { return _canSeeNext; }
			set
			{
				_canSeeNext = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<FatigueTestCondition> TestConditions { get; set; }
		public RelayCommand AddConditionCommand { get; private set; }
		public RelayCommand NextCommand { get; private set; }
		public RelayCommand<FatigueTestCondition> RemoveConditionCommand { get; private set; }

		private void TestConditionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NoConditionsDefined =  TestConditions.Count == 0;
			CanSeeNext = TestConditions.Count > 0;
		}

	}
}
