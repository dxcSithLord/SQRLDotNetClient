﻿using Avalonia;
using ReactiveUI;
using SQRLDotNetClient.Views;
using SQRLUtilsLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQRLDotNetClient.ViewModels
{
    public class NewIdentityModel: ViewModelBase
    {
        public SQRL sqrlInstance { get; }

        public MainWindow ParentWindow { get; set; }
        public NewIdentityModel()
        {
            
        }
        public NewIdentityModel(SQRL sqrlInstance )
        {
            this.sqrlInstance = sqrlInstance;
        }
        public string Message => "To generate a new identity you need to save this rescue code shown below and enter a password. You REALLY need to save this rescue code, we will test you later!";

        public string RescueCode => SQRLUtilsLib.SQRL.FormatRescueCodeForDisplay(sqrlInstance.CreateRescueCode());

        public string Password { get; set; } = string.Empty;

        public string PasswordConfirm { get; set; } = string.Empty;

        public string IdentityName { get; set; } = string.Empty;

        private int _ProgressPercentage = 0;
        //public int ProgressPercentage { get => _ProgressPercentage; set => this.RaiseAndSetIfChanged(ref _ProgressPercentage, value); }
        public int ProgressPercentage { get => _ProgressPercentage; set => this.RaiseAndSetIfChanged(ref _ProgressPercentage,value); }

        public int ProgressMax { get; set; } = 100;

        public string GenerationStep { get; set; }

        public async void GenerateNewIdentity()
        {
            if (this.Password.Equals(this.PasswordConfirm))
            {
                AvaloniaLocator.Current.GetService<MainWindow>().Show();
                SQRLIdentity newId = new SQRLIdentity();
                byte[] iuk = this.sqrlInstance.CreateIUK();
                var progress = new Progress<KeyValuePair<int, string>>(percent =>
                {
                    this.ProgressPercentage = (int)percent.Key / 2;
                    this.GenerationStep = percent.Value + percent.Key;
                });
                newId = await this.sqrlInstance.GenerateIdentityBlock1(iuk, this.Password, newId, progress);
                if (newId.Block1 != null)
                {
                    progress = new Progress<KeyValuePair<int, string>>(percent =>
                    {
                        this.ProgressPercentage = 50 + (int)(percent.Key / 2);
                        this.GenerationStep = percent.Value + percent.Key;
                    });
                    newId = await this.sqrlInstance.GenerateIdentityBlock2(iuk, this.RescueCode, newId, progress);
                    if (newId.Block2 != null)
                    {
                        var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Identity Generated", "Identity Generated!");
                        await messageBoxStandardWindow.Show();
                    }
                }
            }
            else
            {
                var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Error, Passwords don't match!", "Error!",MessageBox.Avalonia.Enums.ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                
                await messageBoxStandardWindow.Show();
            }
        }
    }
}