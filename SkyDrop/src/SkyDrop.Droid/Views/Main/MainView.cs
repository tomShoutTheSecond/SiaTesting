﻿using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Commands;
using SkyDrop.Core.DataModels;
using SkyDrop.Core.ViewModels.Main;
using SkyDrop.Droid.Helper;
using Xamarin.Essentials;

namespace SkyDrop.Droid.Views.Main
{
    [Activity(Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class MainView : BaseActivity<MainViewModel>
    {
        protected override int ActivityLayoutId => Resource.Layout.MainView;

        private const int pickFileRequestCode = 100;

        protected override async void OnCreate(Bundle bundle)
        {
            System.Diagnostics.Debug.WriteLine("MainView OnCreate()");

            base.OnCreate(bundle);

            await ViewModel.InitializeTask.Task;

            ViewModel.SelectFileAsyncFunc = SelectFile;
            ViewModel.SelectImageAsyncFunc = SelectImage;
            ViewModel.FileTapCommand = new MvxCommand<SkyFile>(OpenFile);
        }
        
        private async Task SelectImage()
        {
            if (!await CheckPermissions())
                return;

            var intent = new Intent(Intent.ActionGetContent);
            intent.SetType("image/*");
            StartActivityForResult(intent, pickFileRequestCode);
        }
        
        private async Task SelectFile()
        {
            if (!await CheckPermissions())
                return;

            var intent = new Intent(Intent.ActionGetContent);
            intent.SetType("file/*");
            intent.AddCategory(Intent.CategoryOpenable);

            // special intent for Samsung file manager
            Intent sIntent = new Intent("com.sec.android.app.myfiles.PICK_DATA");
            sIntent.AddCategory(Intent.CategoryDefault);

            Intent chooserIntent;
            if (PackageManager.ResolveActivity(sIntent, 0) != null)
            {
                // it is device with Samsung file manager
                chooserIntent = Intent.CreateChooser(sIntent, "Open file");
                chooserIntent.PutExtra(Intent.ExtraInitialIntents, new Intent[] { intent });
            }
            else
            {
                chooserIntent = Intent.CreateChooser(intent, "Open file");
            }

            try
            {
                StartActivityForResult(chooserIntent, pickFileRequestCode);
            }
            catch (Android.Content.ActivityNotFoundException ex)
            {
                Toast.MakeText(this, "No suitable File Manager was found", ToastLength.Short).Show();
            }
        }

        private async Task<bool> CheckPermissions()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.StorageRead>();
                }

                return status == PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("" + ex);

                return false;
            }
        }

        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(requestCode == pickFileRequestCode)
            {
                if (data == null)
                    return;

                //handle the selected file
                var uri = data.Data;
                string mimeType = ContentResolver.GetType(uri);
                System.Diagnostics.Debug.WriteLine("mime type: ", mimeType);

                string extension = System.IO.Path.GetExtension(uri.Path);
                System.Diagnostics.Debug.WriteLine("extension: ", extension);

                System.Diagnostics.Debug.WriteLine("path: ", uri.Path);

                var filename = AndroidUtil.GetFileName(this, uri);

                Toast.MakeText(this, uri.Path, ToastLength.Long).Show();

                var fileBytes = await ReadFile(uri);
                await ViewModel.UploadFile(filename, fileBytes);
            }
        }

        public async Task<byte[]> ReadFile(Android.Net.Uri uri)
        {
            var inputStream = ContentResolver.OpenInputStream(uri);

            byte[] bytes = new byte[inputStream.Length];
            try
            {
                var buffer = new Java.IO.BufferedInputStream(inputStream);
                await buffer.ReadAsync(bytes);//.Read(bytes, 0, bytes.Length);
                buffer.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("File upload failed: " + e);
            }

            return bytes;
        }

        private void OpenFile(SkyFile file)
        {
            Toast.MakeText(this, $"Opening file {file.Filename}", ToastLength.Long).Show();

            var browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(file.Skylink));
            StartActivity(browserIntent);
        }
    }
}
