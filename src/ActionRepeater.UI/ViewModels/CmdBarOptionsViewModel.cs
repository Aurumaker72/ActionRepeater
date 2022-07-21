﻿using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ActionRepeater.Core;

namespace ActionRepeater.UI.ViewModels;

public class CmdBarOptionsViewModel : ObservableObject
{
    public int CursorMovementCBSelectedIdx
    {
        get => (int)Options.Instance.CursorMovementMode;
        set => Options.Instance.CursorMovementMode = (CursorMovementMode)value;
    }

    public bool UseCursorPosOnClicks
    {
        get => Options.Instance.UseCursorPosOnClicks;
        set => Options.Instance.UseCursorPosOnClicks = value;
    }

    public int MaxClickInterval
    {
        get => Options.Instance.MaxClickInterval;
        set => Options.Instance.MaxClickInterval = value;
    }

    public bool SendKeyAutoRepeat
    {
        get => Options.Instance.SendKeyAutoRepeat;
        set => Options.Instance.SendKeyAutoRepeat = value;
    }

    public CmdBarOptionsViewModel()
    {
        Options.Instance.PropertyChanged += Options_PropertyChanged;
    }

    private void Options_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Options.Instance.CursorMovementMode):
                OnPropertyChanged(nameof(CursorMovementCBSelectedIdx));
                break;

            default:
                OnPropertyChanged(e);
                break;
        }
    }
}
