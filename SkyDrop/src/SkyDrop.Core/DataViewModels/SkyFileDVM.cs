using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using Realms;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.DataViewModels
{
    public class SkyFileDVM : MvxNotifyPropertyChanged
    {
        public SkyFileDVM(SkyFile skyFile, IMvxCommand tapCommand, IMvxCommand openCommand, IMvxCommand copySkyLinkCommand)
        {
            SkyFile = skyFile;
            TapCommand = tapCommand;
            OpenCommand = openCommand;
            CopySkyLinkCommand = copySkyLinkCommand;
        }

        public SkyFile SkyFile { get; set; }

        public IMvxCommand TapCommand { get; set; }
        public IMvxCommand OpenCommand { get; set; }
        public IMvxCommand CopySkyLinkCommand { get; set; }

        public bool IsSelected { get; set; }
    }
}
