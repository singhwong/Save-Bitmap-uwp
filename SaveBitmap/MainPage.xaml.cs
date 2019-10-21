using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SaveBitmap
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        private StorageFile save_file;
        private async void filePicker_bt_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FileOpenPicker file_picker = new FileOpenPicker();
            file_picker.FileTypeFilter.Add(".mp4");
            file_picker.FileTypeFilter.Add(".mp3");
            save_file = await file_picker.PickSingleFileAsync();
            try//防止专辑图片为空
            {
                using (var thumbnail = await save_file.GetThumbnailAsync(ThumbnailMode.MusicView, 200))//获取媒体专辑image
                {
                    var video_cover = new BitmapImage();
                    video_cover.SetSource(thumbnail);
                    image.Source = video_cover;
                }
            }
            catch
            {
            }
        }

        private async void savePicture_bt_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                await SaveImage(save_file, image);
                bool result = await Windows.System.Launcher.LaunchFolderAsync(saved_folder);
            }
            catch (Exception error)
            {
                var message = new MessageDialog(error.ToString());
                await message.ShowAsync();
            }
            
        }

        private StorageFolder saved_folder;
        private async Task<StorageFile> SaveImage(StorageFile file,Image image)
        {
            string desiredName = file.DisplayName + ".jpg";
            saved_folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Saved Picture", CreationCollisionOption.OpenIfExists);
            var saveFile = await saved_folder.CreateFileAsync(desiredName, CreationCollisionOption.GenerateUniqueName);
            try
            {

                RenderTargetBitmap bitmap = new RenderTargetBitmap();
                await bitmap.RenderAsync(image);
                var pixelBuffer = await bitmap.GetPixelsAsync();
                using (var fileStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Ignore,
                         (uint)bitmap.PixelWidth,
                         (uint)bitmap.PixelHeight,
                         DisplayInformation.GetForCurrentView().LogicalDpi,
                         DisplayInformation.GetForCurrentView().LogicalDpi,
                         pixelBuffer.ToArray());
                    await encoder.FlushAsync();
                }
            }
            catch
            {
            }
            return saveFile;
        }
    }
}
