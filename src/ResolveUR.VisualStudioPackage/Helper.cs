﻿using System;
using System.Diagnostics;
using System.Globalization;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ResolveURVisualStudioPackage
{
    class Helper: Package
    {

        public event EventHandler ResolveurCanceled;

        private bool _dialogCanceled;
        private IVsThreadedWaitDialog2 _progressDialog = null;
        public IVsThreadedWaitDialog2 ProgressDialog
        {
            get
            {
                return _progressDialog;
            }
            set
            {
                _progressDialog = value;
            }
        }
       
        private OutputWindow _outputWindow;
        public OutputWindow OutputWindow
        {
            get
            {
                return _outputWindow;
            }
            set
            {
                _outputWindow = value;
            }
        }

        public string CurrentProject
        {
            get;
            set;
        }
        public int ItemGroupCount
        {
            get;
            set;
        }

        public int CurrentReferenceCountInItemGroup
        {
            get;
            set;
        }

        public int TotalReferenceCount
        {
            get;
            set;
        }

        IVsUIShell _uiShell;
        public IVsUIShell UiShell
        {
            get
            {
                return _uiShell;
            }
            set
            {
                _uiShell = value;
            }
        }

        public void ShowMessageBox(string title, string message)
        {
            System.Threading.Thread.Sleep(1000);
            var clsid = Guid.Empty;
            int result;
            UiShell.ShowMessageBox(
                    0,
                    ref clsid,
                    title,
                    string.Format(CultureInfo.CurrentCulture,
                    message,
                    this.ToString()),
                    string.Empty,
                    0,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                    OLEMSGICON.OLEMSGICON_INFO,
                    0,
                    out result);
        }

        public void SetMessage(string message)
        {
            if (OutputWindow != null)
            {
                OutputWindow.ActivePane.OutputString(message);
                OutputWindow.ActivePane.OutputString(Environment.NewLine);
            }

            Debug.WriteLine(message);

            ProgressDialog.UpdateProgress
            (
                string.Empty,
                CurrentProject + Environment.NewLine +
                "Resolving Reference Group: " + ItemGroupCount + Environment.NewLine + 
                message, 
                message, 
                ++CurrentReferenceCountInItemGroup, 
                TotalReferenceCount, 
                false, 
                out _dialogCanceled
            );
            if (_dialogCanceled) handleResolveurCancelation(_dialogCanceled);
        }

        public void EndWaitDialog()
        {
            int userCanceled;
            ProgressDialog.EndWaitDialog(out userCanceled);
            if (userCanceled != 0) handleResolveurCancelation(userCanceled != 0);
        }

        private void handleResolveurCancelation(bool userCanceled)
        {
            if (ResolveurCanceled != null) ResolveurCanceled(null, null);
            if (userCanceled) ShowMessageBox(Constants.AppName + " Status", "Canceled");
        }

    }
}