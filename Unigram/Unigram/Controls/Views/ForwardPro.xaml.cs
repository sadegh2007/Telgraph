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
    public sealed partial class ForwardPro : ContentDialog
    {

        TLMessage msg = null;
        private ViewModels.DialogViewModel ViewModel;

        public ForwardPro(TLMessageBase msg_, ViewModels.DialogViewModel ViewModel)
        {
            this.InitializeComponent();
            this.msg = msg_ as TLMessage;
            this.ViewModel = ViewModel;

            msg.wn = true;

            if (msg.Media.GetType() == typeof(TLMessageMediaEmpty))
                text.Text = msg.Message;
            else if (msg.Media.GetType() == typeof(TLMessageMediaPhoto))
                text.Text = ((TLMessageMediaPhoto)msg.Media).Caption;
            else if (msg.Media.GetType() == typeof(TLMessageMediaDocument) )
                text.Text = ((TLMessageMediaDocument)msg.Media).Caption;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (msg.Media.GetType() == typeof(TLMessageMediaEmpty))
                msg.Message = text.Text;
            else if (msg.Media.GetType() == typeof(TLMessageMediaPhoto))
                ((TLMessageMediaPhoto)msg.Media).Caption = text.Text;
            else if (msg.Media.GetType() == typeof(TLMessageMediaDocument))
                ((TLMessageMediaDocument)msg.Media).Caption = text.Text;

            ViewModel.MessageForwardWithoutNameCommand.Execute(msg);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
