﻿using ActionRepeater.Action;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActionRepeater.Extentions;

namespace ActionRepeater.Input;

public static class ActionManager
{
    public static MouseMovement? CursorPathStart { get; set; }
    public static List<MouseMovement> CursorPath { get; } = new();
    public static IReadOnlyList<MouseMovement> AbsoluteCursorPath
    {
        get
        {
            if (CursorPathStart is null) return System.Array.Empty<MouseMovement>();

            List<MouseMovement> absPath = new();

            absPath.Add(CursorPathStart);

            var monitors = Win32.SystemInformation.MonitorRects;

            for (int i = 0; i < CursorPath.Count; ++i)
            {
                MouseMovement delta = CursorPath[i];
                MouseMovement lastAbs = absPath[^1];

                Win32.POINT pt = new(lastAbs.MovPoint.x, lastAbs.MovPoint.y);

                var ogMonitor = monitors.First(m => m.ContainsInclusive(pt.x, pt.y));

                pt.x += delta.MovPoint.x;
                if (!monitors.Any(m => m.ContainsInclusive(pt.x, pt.y)))
                {
                    if (delta.MovPoint.x < 0)
                    {
                        pt.x = ogMonitor.Left;
                    }
                    else
                    {
                        pt.x = ogMonitor.Right;
                    }
                }

                pt.y += delta.MovPoint.y;
                if (!monitors.Any(m => m.ContainsInclusive(pt.x, pt.y)))
                {
                    if (delta.MovPoint.y < 0)
                    {
                        pt.y = ogMonitor.Top;
                    }
                    else
                    {
                        pt.y = ogMonitor.Bottom;
                    }
                }

                if (pt == lastAbs.MovPoint)
                {
                    lastAbs.ActualDelay += delta.ActualDelay;
                    continue;
                }

                absPath.Add(new MouseMovement(pt, delta.ActualDelay));
            }

            return absPath;
        }
    }

    public static ObservableCollection<InputAction> Actions { get; } = new();
    public static ObservableCollection<InputAction> ActionsExlKeyRepeat { get; } = new();

    /// <summary>Contains the indecies of the modified actions in ActionsExlKeyRepeat.</summary>
    private static readonly List<int> _modifiedFilteredActionsIdxs = new();

    public static void AddAction(InputAction action)
    {
        if (action is WaitAction waitAction)
        {
            if (Actions.Count > 0 && Actions[^1] is WaitAction lastWaitAction)
            {
                lastWaitAction.Duration += waitAction.Duration;
            }
            else
            {
                Actions.Add(action);
            }

            int lastFilteredActionIdx = ActionsExlKeyRepeat.Count - 1;
            if (ActionsExlKeyRepeat.Count > 0 && ActionsExlKeyRepeat[lastFilteredActionIdx] is WaitAction lastFilteredWaitAction)
            {
                if (_modifiedFilteredActionsIdxs.Contains(lastFilteredActionIdx))
                {
                    lastFilteredWaitAction.Duration += waitAction.Duration;
                }
                else if (!ReferenceEquals(Actions[^1], lastFilteredWaitAction))
                {
                    ActionsExlKeyRepeat[lastFilteredActionIdx] = new WaitAction(lastFilteredWaitAction.Duration + waitAction.Duration);
                    _modifiedFilteredActionsIdxs.Add(lastFilteredActionIdx);
                }
            }
            else
            {
                ActionsExlKeyRepeat.Add(action);
            }

            return;
        }

        Actions.Add(action);

        if (!(action is KeyAction keyAction && keyAction.IsAutoRepeat))
        {
            ActionsExlKeyRepeat.Add(action);
        }
    }

    /// <summary>Fills ActionsEclKeyRepeat from the actinos in Actions.</summary>
    public static void FillFilteredActionList()
    {
        ActionsExlKeyRepeat.Clear();
        _modifiedFilteredActionsIdxs.Clear();

        for (int i = 0; i < Actions.Count; ++i)
        {
            InputAction action = Actions[i];

            if (action is WaitAction waitAction)
            {
                int lastFilteredActionIdx = ActionsExlKeyRepeat.Count - 1;
                if (ActionsExlKeyRepeat.Count > 0 && ActionsExlKeyRepeat[lastFilteredActionIdx] is WaitAction lastFilteredWaitAction)
                {
                    if (_modifiedFilteredActionsIdxs.Contains(lastFilteredActionIdx))
                    {
                        lastFilteredWaitAction.Duration += waitAction.Duration;
                    }
                    else if (!ReferenceEquals(Actions[i - 1], lastFilteredWaitAction))
                    {
                        ActionsExlKeyRepeat[lastFilteredActionIdx] = new WaitAction(lastFilteredWaitAction.Duration + waitAction.Duration);
                        _modifiedFilteredActionsIdxs.Add(lastFilteredActionIdx);
                    }
                }
                else
                {
                    ActionsExlKeyRepeat.Add(action);
                }

                continue;
            }

            if (!(action is KeyAction keyAction && keyAction.IsAutoRepeat))
            {
                ActionsExlKeyRepeat.Add(action);
            }
        }
    }

    public static bool HasActionBeenModified(InputAction action)
    {
        foreach (int idx in _modifiedFilteredActionsIdxs)
        {
            if (ActionsExlKeyRepeat[idx] == action)
            {
                return true;
            }
        }

        return false;
    }

    public static bool TryRemoveAction(InputAction action)
    {
        if (HasActionBeenModified(action)) return false;

        Actions.Remove(action);
        ActionsExlKeyRepeat.Remove(action);
        return true;
    }

    /// <param name="isFilteredList">true if the action to replace is in the filtered list (<see cref="ActionsExlKeyRepeat"/>)</param>
    /// <param name="index">The index of the action to replace in the list.</param>
    public static void ReplaceAction(bool isFilteredList, int index, InputAction newAction)
    {
        InputAction actionToReplace;
        int replacedActionIdx;

        if (isFilteredList)
        {
            actionToReplace = ActionsExlKeyRepeat[index];

            ActionsExlKeyRepeat[index] = newAction;

            replacedActionIdx = Actions.IndexOf(actionToReplace);
            if (replacedActionIdx == -1) throw new System.NotImplementedException("selected item not available in Actions.");
            Actions[replacedActionIdx] = newAction;

            return;
        }

        actionToReplace = Actions[index];

        Actions[index] = newAction;

        replacedActionIdx = ActionsExlKeyRepeat.IndexOf(actionToReplace);
        if (replacedActionIdx == -1) throw new System.NotImplementedException("selected item not available in ActionsExlKeyRepeat.");
        ActionsExlKeyRepeat[replacedActionIdx] = newAction;
    }

    public static void Clear()
    {
        CursorPathStart = null;
        CursorPath.Clear();
        Actions.Clear();
        ActionsExlKeyRepeat.Clear();
        _modifiedFilteredActionsIdxs.Clear();
    }

    public static void PlayActions()
    {
        if (Actions.Count == 0 && CursorPathStart is null)
        {
            App.MainWindow.UpdatePlayingStatus(null, false);
            return;
        }

        if (Player.IsPlaying)
        {
            Player.Cancel();
            return;
        }

        var actionsToPlay = Options.Instance.SendKeyAutoRepeat ? Actions : ActionsExlKeyRepeat;

        switch (Options.Instance.CursorMovementMode)
        {
            case CursorMovementMode.Absolute:
                Player.PlayActions(actionsToPlay, AbsoluteCursorPath, false);
                break;

            case CursorMovementMode.Relative:
                Player.PlayActions(actionsToPlay, CursorPath, true);
                break;

            default:
                Player.PlayActions(actionsToPlay, null, false);
                break;
        }
    }
}
