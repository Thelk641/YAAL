using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using YAAL.Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace YAAL
{
    public abstract class Command : UserControl
    {
        public Interface_Instruction linkedInstruction;
        public CLMakerWindow clMaker;
        protected Border background;
        protected Button _up;
        protected Button _down;
        protected Button _delete;

        protected Dictionary<string, Action> debouncedEvents = new();
        protected Dictionary<TextBox, string> debouncedSettings = new();
        protected Dictionary<Button, TextBox> explorers = new();

        public void SetBackground()
        {
            var theme = Application.Current.ActualThemeVariant;
            if (theme == ThemeVariant.Dark)
            {
                background.Background = new SolidColorBrush(Color.Parse("#454545"));
            }
            else
            {
                background.Background = new SolidColorBrush(Color.Parse("#AAA"));

            }
        }

        public virtual void SetDebouncedEvents()
        {
            _up.Click += MoveUp;
            _down.Click += MoveDown;
            _delete.Click += DeleteComponent;
        }

        public void Trigger(TextBox box)
        {
            if (this.debouncedEvents.ContainsKey(box.Name))
            {
                this.debouncedEvents[box.Name]();
            } else
            {
                this.linkedInstruction.SetSetting(debouncedSettings[box], box.Text);
            }
        }

        protected void _TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox textbox))
            {
                ErrorManager.ThrowError(
                    "Command - Invalid control type",
                    "Something that isn't a TextBox tried to TextBox specific event _TextChanged. Please report this issue."
                    );
                return;
            }
            Debouncer.Debounce(textbox, this, 1f);
        }

        protected async void _FileExplorer(object? sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            explorers[button].Text = await IOManager.PickFile(clMaker);
        }

        protected async void _FolderExplorer(object? sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            explorers[button].Text = await IOManager.PickFolder(clMaker);
        }

        protected void _TextChangedCustom(object? sender, TextChangedEventArgs e)
        {
            if (sender is Control control && !string.IsNullOrEmpty(control.Name))
            {
                if (debouncedEvents.TryGetValue(control.Name, out var action))
                {
                    Debouncer.Debounce(action, 1f);
                }
            }
        }

        protected abstract void TurnEventsOn();

        protected async void TurnEventsBackOn()
        {
            // Turning events on in the standard thread leads to issues
            // like events that are supposed to be off triggering anyway
            // this fixes this
            await Dispatcher.UIThread.InvokeAsync(() => {
                TurnEventsOn();
            }, DispatcherPriority.Background);
        }

        public void MoveUp(object? sender, RoutedEventArgs e)
        {
            var parent = this.Parent as Panel;
            if (parent != null)
            {
                int index = parent.Children.IndexOf(this);
                if (index > 0)
                {
                    parent.Children.RemoveAt(index);
                    parent.Children.Insert(index - 1, this);
                    clMaker.MoveUp(this);
                }
            }
        }

        public void MoveDown(object? sender, RoutedEventArgs e)
        {
            var parent = this.Parent as Panel;
            if (parent != null)
            {
                int index = parent.Children.IndexOf(this);
                if (index < parent.Children.Count - 1)
                {
                    parent.Children.RemoveAt(index);
                    parent.Children.Insert(index + 1, this);
                    clMaker.MoveDown(this);
                }
            }

        }

        public void DeleteComponent(object? sender = null, RoutedEventArgs e = null)
        {
            DeleteComponent(true);
        }

        public void DeleteComponent(bool passInfo)
        {
            var parent = this.Parent as StackPanel;
            parent?.Children.Remove(this);
            if (passInfo)
            {
                clMaker.RemoveCommand(this);
            }
        }

        public Interface_Instruction GetInstruction()
        {
            return linkedInstruction;
        }

        public void SetCustomLauncher(CustomLauncher launcher)
        {
            linkedInstruction.SetCustomLauncher(launcher);
        }

        public virtual void LoadInstruction(Interface_Instruction newInstruction)
        {
            linkedInstruction = newInstruction;
        }

        public void LoadInstruction(Interface_Instruction newInstruction, CustomLauncher launcher)
        {
            LoadInstruction(newInstruction);
            linkedInstruction.SetCustomLauncher(launcher);
        }
    }
}
