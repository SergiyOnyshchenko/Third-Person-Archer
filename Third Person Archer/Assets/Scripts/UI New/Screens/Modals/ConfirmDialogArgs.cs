#nullable enable
using System;

namespace UI.Screens
{
    public struct ConfirmDialogArgs
    {
        public string Title;
        public string Message;
        public string ConfirmText;
        public string CancelText;
        public Action? OnConfirm;
        public Action? OnCancel;
    }
}
