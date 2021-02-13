using System;
using System.Collections.Generic;
using System.Text;
using MvvmCross.ViewModels;

namespace SkyDrop.Core.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel
    {
        public string Title { get; set; }
    }
}
