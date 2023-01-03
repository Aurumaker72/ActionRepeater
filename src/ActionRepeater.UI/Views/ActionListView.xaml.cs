﻿using System;
using System.Linq;
using ActionRepeater.Core.Extentions;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ActionRepeater.UI.Views;

public sealed partial class ActionListView : UserControl
{
    private readonly ActionListViewModel _vm;

    // TODO: add multi item select (for deleting and maybe moving)
    public ActionListView()
    {
        _vm = App.Current.Services.GetRequiredService<ActionListViewModel>();
        _vm.ScrollToSelectedItem = ScrollToSelectedItem;

        App.Current.Services.GetRequiredService<Recorder>().ActionAdded += (_, _) => ActionList.ScrollIntoView(_vm.FilteredActions[^1]);

        InitializeComponent();

        _vm.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName?.Equals(nameof(_vm.SelectedActionIndex), StringComparison.Ordinal) == true)
        {
            bool isAnyItemSelected = _vm.SelectedActionIndex >= 0;

            _replaceMenuItem.KeyboardAccelerators[0].IsEnabled = isAnyItemSelected;
            _pasteMenuItem.KeyboardAccelerators[0].IsEnabled = !isAnyItemSelected;

            _vm.SelectedRanges = ActionList.SelectedRanges;
        }
    }

    public void ScrollToSelectedItem() => ActionList.ScrollIntoView(ActionList.SelectedItem);

    private void ActionList_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is ActionViewModel actionItem)
        {
            int rightClickedItemIdx = _vm.FilteredActions.RefIndexOfReverse(actionItem);
            if (!ActionList.SelectedRanges.Any(x => rightClickedItemIdx >= x.FirstIndex && rightClickedItemIdx <= x.LastIndex))
            {
                ActionList.SelectedIndex = rightClickedItemIdx;
                SingleItemMenuFlyout.ShowAt(ActionList, e.GetPosition(ActionList));
                return;
            }

            if (AreMultipleItemsSelected())
            {
                MultiItemMenuFlyout.ShowAt(ActionList, e.GetPosition(ActionList));
                return;
            }

            SingleItemMenuFlyout.ShowAt(ActionList, e.GetPosition(ActionList));
            return;
        }

        ActionList.SelectedIndex = -1;
        NoItemMenuFlyout.ShowAt(ActionList, e.GetPosition(ActionList));
    }

    private async void ActionList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is not ActionViewModel actionVM) return;

        if (!ReferenceEquals(ActionList.SelectedItem, actionVM)) ActionList.SelectedItem = actionVM;

        await _vm.EditSelectedAction();
    }

    private void ActionList_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (((FrameworkElement)e.OriginalSource).DataContext is ActionViewModel)
        {
            _vm.SelectedRanges = ActionList.SelectedRanges;
            return;
        }

        ActionList.SelectedIndex = -1;
    }

    private bool AreMultipleItemsSelected() => ActionList.SelectedRanges.Count > 1 || (ActionList.SelectedRanges.Count == 1 && ActionList.SelectedRanges[0].FirstIndex != ActionList.SelectedRanges[0].LastIndex);
}
