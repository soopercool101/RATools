﻿using System;
using Jamiras.DataModels;
using Jamiras.ViewModels;

namespace RATools.ViewModels
{
    public class ModifiableTextFieldViewModel : ViewModelBase
    {
        public static readonly ModelProperty TextProperty = ModelProperty.Register(typeof(ModifiableTextFieldViewModel), "Text", typeof(string), String.Empty);
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            internal set { SetValue(TextProperty, value); }
        }

        public static readonly ModelProperty LocalTextProperty = ModelProperty.Register(typeof(ModifiableTextFieldViewModel), "Text", typeof(string), String.Empty);
        public string LocalText
        {
            get { return (string)GetValue(LocalTextProperty); }
            internal set { SetValue(LocalTextProperty, value); }
        }

        public static readonly ModelProperty PublishedTextProperty = ModelProperty.Register(typeof(ModifiableTextFieldViewModel), "Text", typeof(string), String.Empty);
        public string PublishedText
        {
            get { return (string)GetValue(PublishedTextProperty); }
            internal set { SetValue(PublishedTextProperty, value); }
        }

        public static readonly ModelProperty StateProperty = ModelProperty.RegisterDependant(typeof(ModifiableTextFieldViewModel), "State", typeof(ModifiedState), 
            new [] { TextProperty, LocalTextProperty, PublishedTextProperty }, GetState);

        public ModifiedState State
        {
            get { return (ModifiedState)GetValue(StateProperty); }
        }

        private static object GetState(ModelBase model)
        {
            var vm = (ModifiableTextFieldViewModel)model;
            if (!String.IsNullOrEmpty(vm.LocalText))
            {
                if (vm._isNotGenerated)
                    return ModifiedState.NotGenerated;

                if (vm._isNewLocal)
                    return ModifiedState.NewLocal;

                if (vm.LocalText != vm.Text)
                    return ModifiedState.FromLocal;
            }

            if (!String.IsNullOrEmpty(vm.PublishedText) && vm.PublishedText != vm.Text)
                return ModifiedState.FromPublished;

            return ModifiedState.None;
        }

        public bool IsModifiedFromLocal
        {
            get { return (LocalText != Text && !String.IsNullOrEmpty(LocalText)); }
        }

        public bool IsModifiedFromPublished
        {
            get { return (PublishedText != Text && !String.IsNullOrEmpty(PublishedText)); }
        }

        public bool IsNotGenerated
        {
            get { return _isNotGenerated; }
            set
            {
                if (_isNotGenerated != value)
                {
                    _isNotGenerated = value;
                    UpdateLocalText();
                }
            }
        }
        private bool _isNotGenerated;

        public bool IsNewLocal
        {
            get { return _isNewLocal; }
            set
            {
                if (_isNewLocal != value)
                {
                    _isNewLocal = value;
                    UpdateLocalText();
                }
            }
        }
        private bool _isNewLocal;

        private void UpdateLocalText()
        {
            if (_isNotGenerated)
                LocalText = NotGeneratedText;
            else if (_isNewLocal)
                LocalText = NewText;
            else
                LocalText = (string)LocalTextProperty.DefaultValue;
        }

        public static string NotGeneratedText = "Not generated";
        public static string NewText = "New";
    }

    public enum ModifiedState
    {
        None = 0,
        FromLocal = 1,
        FromPublished = 2,
        NotGenerated = 3,
        NewLocal = 4,
    }
}
