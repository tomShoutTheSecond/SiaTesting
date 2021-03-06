using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.DataViewModels;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class FilesView : BaseActivity<FilesViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.FilesView;

        public RecyclerView UploadedFilesRecyclerView { get; set; }

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            UploadedFilesRecyclerView = FindViewById<RecyclerView>(Resource.Id.UploadedFilesList);

            await ViewModel.InitializeTask.Task;

            Log.Trace("MainView OnCreate()");

            ViewModel.SelectFileAsyncFunc = () => AndroidUtil.SelectFile(this);
            ViewModel.SelectImageAsyncFunc = () => AndroidUtil.SelectImage(this);
            ViewModel.FileTapCommand = new MvxCommand<SkyFile>(skyFile => AndroidUtil.OpenFileInBrowser(this, skyFile));
            ViewModel.AfterFileSelected = new MvxCommand(() => AfterFileWasSelected());
            ViewModel.HighlightNewFile = new MvxCommand(() => HighlightNewFile());

            //this sets all progressbars in the app to white
            //I don't think we need this any more as progress bar color should be set in styles.xml
            var progressBar = FindViewById<ProgressBar>(Resource.Id.ProgressBar);
            if (progressBar != null)
                progressBar.IndeterminateDrawable.SetColorFilter(Color.White, PorterDuff.Mode.SrcIn);
        }

        private void HighlightNewFile()
        {
            UploadedFilesRecyclerView.SmoothScrollToPosition(0);
        }

        private void AfterFileWasSelected()
        {
            int? previouslySelectedIndex = ViewModel.GetIndexForPreviouslySelectedFile();

            if (previouslySelectedIndex == null)
            {
                UploadedFilesRecyclerView.GetAdapter().NotifyItemChanged(ViewModel.CurrentlySelectedFileIndex);
            }
            else if (ViewModel.CurrentlySelectedFileIndex == previouslySelectedIndex.Value)
            {
                UploadedFilesRecyclerView.GetAdapter().NotifyItemChanged(ViewModel.CurrentlySelectedFileIndex);
            }
            else
            {
                UploadedFilesRecyclerView.GetAdapter().NotifyItemChanged(previouslySelectedIndex.Value);
                UploadedFilesRecyclerView.GetAdapter().NotifyItemChanged(ViewModel.CurrentlySelectedFileIndex);
            }
        }

        protected override async void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            try
            {
                if (requestCode == AndroidUtil.PickFileRequestCode)
                {
                    if (data == null)
                        return;

                    await HandlePickedFile(data);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private async Task HandlePickedFile(Intent data)
        {
            var stagedFile = await AndroidUtil.HandlePickedFile(this, data);
            ViewModel.StageFile(stagedFile);
        }
    }
}
