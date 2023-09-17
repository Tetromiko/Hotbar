using System;
using System.Windows.Input;

namespace HelpBar.Hotkeys
{
    public class GlobalHotkey
    {
        public ModifierKeys Modifier { get; set; }
        public Key Key { get; set; }
        public Action Callback { get; set; }
        public bool CanExecute { get; set; }
        public bool Pressed { get; set; }

        public GlobalHotkey(ModifierKeys modifier, Key key, Action callback, bool canExecute = true, bool pressed = false) 
        { 
            Modifier = modifier;
            Key = key;
            Callback = callback;
            CanExecute = canExecute;
            Pressed = pressed;
        }
    }
}
