﻿using System;
using System.Threading.Tasks;
using ActionRepeater.Core.Action;
using ActionRepeater.Core.Input;
using ActionRepeater.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ActionRepeater.UI.Services;

public class ContentDialogService
{
    // would have set it in the ctor but it must be set after MainWindow.Content has loaded.
    internal XamlRoot XamlRoot { get; set; } = null!;

    public ContentDialogService()
    {
        System.Diagnostics.Debug.WriteLineIf(XamlRoot is null, "XamlRoot should be set.");
    }

    public async Task ShowErrorDialog(string title, string message)
    {
        await new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Title = $"❌ {title}",
            Content = message,
            CloseButtonText = "Ok",
        }.ShowAsync();
    }

    public async Task ShowMessageDialog(string title, string? message = null)
    {
        await new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Title = title,
            Content = message,
            CloseButtonText = "Ok",
        }.ShowAsync();
    }

    public async Task ShowYesNoMessageDialog(string title, string? message = null, Action? onYesClick = null, Action? onNoClick = null)
    {
        await new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Title = title,
            Content = message,
            PrimaryButtonText = "Yes",
            PrimaryButtonCommand = onYesClick is null ? null : new RelayCommand(onYesClick),
            SecondaryButtonText = "No",
            SecondaryButtonCommand = onNoClick is null ? null : new RelayCommand(onNoClick),
        }.ShowAsync();
    }

    public async Task ShowEditActionDialog(ActionType actionType)
    {
        ContentDialog dialog = new()
        {
            XamlRoot = XamlRoot,
            PrimaryButtonText = "Add",
            SecondaryButtonText = "Cancel",
        };

        EditActionDialogViewModel vm = new(actionType, dialog);

        dialog.Content = new Views.EditActionView(isAddView: true) { ViewModel = vm };
        dialog.PrimaryButtonCommand = vm.AddActionCommand;

        await dialog.ShowAsync();
    }

    public async Task ShowEditActionDialog(ObservableObject editActionViewModel, InputAction actionToEdit)
    {
        if (ActionManager.IsActionTiedToModifiedAction(actionToEdit))
        {
            await ShowErrorDialog("This action is not editable.", (actionToEdit is KeyAction ka && ka.IsAutoRepeat)
                ? "This is an auto repeat action, edit the original key down action if you want to change the key."
                : ActionManager.ActionTiedToModifiedActMsg);

            return;
        }

        ContentDialog dialog = new()
        {
            XamlRoot = XamlRoot,
            PrimaryButtonText = "Update",
            SecondaryButtonText = "Cancel",
        };

        EditActionDialogViewModel vm = new(editActionViewModel, dialog);

        dialog.Content = new Views.EditActionView() { ViewModel = vm };
        dialog.PrimaryButtonCommand = vm.UpdateActionCommand;
        dialog.PrimaryButtonCommandParameter = actionToEdit;

        await dialog.ShowAsync();
    }
}
