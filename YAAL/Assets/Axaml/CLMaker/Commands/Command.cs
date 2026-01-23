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
        public CLM clMaker;
        public CLM_Commands? holder;
        public Interface_CommandSetting settings;
        public string type;

        protected Dictionary<string, Action> debouncedEvents = new();
        protected Dictionary<TextBox, string> debouncedSettings = new();
        protected Dictionary<Button, TextBox> explorers = new();
        public virtual void SetDebouncedEvents()
        {
            this.FindControl<Button>("MoveUp").Click += MoveUp;
            this.FindControl<Button>("MoveDown").Click += MoveDown;
            this.FindControl<Button>("X").Click += DeleteComponent;
        }

        public void Trigger(TextBox box)
        {
            if (this.debouncedEvents.ContainsKey(box.Name))
            {
                this.debouncedEvents[box.Name]();
            } else
            {
                settings.SetSetting(debouncedSettings[box], box.Text);
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
            if (explorers[button].Text == "")
            {
                explorers[button].Text = "\"" + await IOManager.PickFile(clMaker) + "\";";
            } else
            {
                explorers[button].Text += "\"" + await IOManager.PickFile(clMaker) + "\";";
            }
            
        }

        protected async void _FolderExplorer(object? sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (explorers[button].Text == "")
            {
                explorers[button].Text = "\"" + await IOManager.PickFolder(clMaker) + "\";";
            }
            else
            {
                explorers[button].Text += "\"" + await IOManager.PickFolder(clMaker) + "\";";
            }
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
            if(holder != null)
            {
                holder.MoveCommandUp(this);
            }
            
        }

        public void MoveDown(object? sender, RoutedEventArgs e)
        {
            if (holder != null)
            {
                holder.MoveCommandDown(this);
            }
        }

        public void DeleteComponent(object? sender = null, RoutedEventArgs e = null)
        {
            if (holder != null)
            {
                holder.RemoveCommand(this);
            }
        }

        public virtual Interface_CommandSetting GetInstruction()
        {
            return settings;
        }

        public virtual void LoadInstruction(Interface_CommandSetting newSettings)
        {
            settings = newSettings;
        }
    }
}
