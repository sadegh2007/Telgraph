using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Telegram.Api.TL;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Unigram.Controls.Views
{
    public sealed partial class ConfirmStickerSend : ContentDialog
    {

        private object sticker { get; set; }
        private ViewModels.DialogViewModel ViewModel;

        public ConfirmStickerSend(object sticker, ViewModels.DialogViewModel vm)
        {
            this.InitializeComponent();
            this.sticker = sticker;
            this.ViewModel = vm;

            var imgSticker = Converters.DefaultPhotoConverter.ReturnOrEnqueueSticker(sticker as TLDocument, null);

            img.Source = imgSticker;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.SendStickerCommand.Execute(sticker);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }
    }
}
