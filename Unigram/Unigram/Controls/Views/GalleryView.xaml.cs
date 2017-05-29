﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Telegram.Api.Helpers;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Template10.Common;
using Unigram.Converters;
using Unigram.ViewModels;
using Unigram.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using LinqToVisualTree;
using Windows.Foundation.Metadata;
using Windows.UI;
using Microsoft.Graphics.Canvas.Effects;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Unigram.Controls.Views
{
    public sealed partial class GalleryView : ContentDialogBase
    {
        public GalleryViewModelBase ViewModel => DataContext as GalleryViewModelBase;

        public BindConvert Convert => BindConvert.Current;

        private MediaPlayerElement _mediaPlayer;
        private MediaPlayerSurface _mediaSurface;

        private FrameworkElement _surface;
        private SpriteVisual _surfaceVisual;

        private Visual _layerVisual;
        private Visual _topBarVisual;
        private Visual _botBarVisual;

        private GalleryView()
        {
            InitializeComponent();

            Layer.Visibility = Visibility.Collapsed;
            TopBar.Visibility = Visibility.Collapsed;
            BotBar.Visibility = Visibility.Collapsed;

            _mediaPlayer = new MediaPlayerElement { Style = Resources["yolo"] as Style };
            _mediaPlayer.AreTransportControlsEnabled = true;
            _mediaPlayer.TransportControls = Transport;
            _mediaPlayer.SetMediaPlayer(new MediaPlayer());

            _layerVisual = ElementCompositionPreview.GetElementVisual(Layer);
            _topBarVisual = ElementCompositionPreview.GetElementVisual(TopBar);
            _botBarVisual = ElementCompositionPreview.GetElementVisual(BotBar);

            _layerVisual.Opacity = 0;
            _topBarVisual.Offset = new Vector3(0, -48, 0);
            _botBarVisual.Offset = new Vector3(0, 48, 0);

            if (ApiInformation.IsMethodPresent("Windows.UI.Xaml.Hosting.ElementCompositionPreview", "SetImplicitShowAnimation"))
            {
                var easing = ConnectedAnimationService.GetForCurrentView().DefaultEasingFunction;
                var duration = ConnectedAnimationService.GetForCurrentView().DefaultDuration;

                var topShowAnimation = Window.Current.Compositor.CreateVector3KeyFrameAnimation();
                topShowAnimation.InsertKeyFrame(0.0f, new Vector3(0, -48, 0), easing);
                topShowAnimation.InsertKeyFrame(1.0f, new Vector3(), easing);
                topShowAnimation.Target = nameof(Visual.Offset);
                topShowAnimation.Duration = duration;

                var botHideAnimation = Window.Current.Compositor.CreateVector3KeyFrameAnimation();
                botHideAnimation.InsertKeyFrame(0.0f, new Vector3(), easing);
                botHideAnimation.InsertKeyFrame(1.0f, new Vector3(0, 48, 0), easing);
                botHideAnimation.Target = nameof(Visual.Offset);
                botHideAnimation.Duration = duration;

                var topHideAnimation = Window.Current.Compositor.CreateVector3KeyFrameAnimation();
                topHideAnimation.InsertKeyFrame(0.0f, new Vector3(), easing);
                topHideAnimation.InsertKeyFrame(1.0f, new Vector3(0, -48, 0), easing);
                topHideAnimation.Target = nameof(Visual.Offset);
                topHideAnimation.Duration = duration;

                var botShowAnimation = Window.Current.Compositor.CreateVector3KeyFrameAnimation();
                botShowAnimation.InsertKeyFrame(0.0f, new Vector3(0, 48, 0), easing);
                botShowAnimation.InsertKeyFrame(1.0f, new Vector3(), easing);
                botShowAnimation.Target = nameof(Visual.Offset);
                botShowAnimation.Duration = duration;

                var layerShowAnimation = Window.Current.Compositor.CreateScalarKeyFrameAnimation();
                layerShowAnimation.InsertKeyFrame(0.0f, 0.0f, easing);
                layerShowAnimation.InsertKeyFrame(1.0f, 1.0f, easing);
                layerShowAnimation.Target = nameof(Visual.Opacity);
                layerShowAnimation.Duration = duration;

                var layerHideAnimation = Window.Current.Compositor.CreateScalarKeyFrameAnimation();
                layerHideAnimation.InsertKeyFrame(0.0f, 1.0f, easing);
                layerHideAnimation.InsertKeyFrame(1.0f, 0.0f, easing);
                layerHideAnimation.Target = nameof(Visual.Opacity);
                layerHideAnimation.Duration = duration;

                ElementCompositionPreview.SetImplicitShowAnimation(TopBar, topShowAnimation);
                ElementCompositionPreview.SetImplicitHideAnimation(TopBar, topHideAnimation);
                ElementCompositionPreview.SetImplicitShowAnimation(BotBar, botShowAnimation);
                ElementCompositionPreview.SetImplicitHideAnimation(BotBar, botHideAnimation);
                ElementCompositionPreview.SetImplicitShowAnimation(Layer, layerShowAnimation);
                ElementCompositionPreview.SetImplicitHideAnimation(Layer, layerHideAnimation);
            }
        }

        private static GalleryView _current;
        public static GalleryView Current
        {
            get
            {
                return new GalleryView();

                if (_current == null)
                    _current = new GalleryView();

                return _current;
            }
        }

        public IAsyncOperation<ContentDialogBaseResult> ShowAsync(GalleryViewModelBase parameter, EventHandler closing)
        {
            EventHandler handler = null;
            handler = new EventHandler((s, args) =>
            {
                DataContext = null;
                Bindings.StopTracking();

                Closing -= handler;
                closing?.Invoke(this, args);
            });

            Closing += handler;
            return ShowAsync(parameter);
        }

        public IAsyncOperation<ContentDialogBaseResult> ShowAsync(GalleryViewModelBase parameter)
        {
            DataContext = parameter;
            Bindings.Update();

            RoutedEventHandler handler = null;
            handler = new RoutedEventHandler(async (s, args) =>
            {
                Loaded -= handler;
                await ViewModel.OnNavigatedToAsync(parameter, NavigationMode.New, null);
            });

            Loaded += handler;
            return ShowAsync();
        }

        protected override void OnBackRequestedOverride(object sender, HandledEventArgs e)
        {
            Dispose();

            if (ViewModel.SelectedItem == ViewModel.FirstItem)
            {
                var container = Flip.ContainerFromItem(ViewModel.SelectedItem);
                var image = container.Descendants<ImageView>().FirstOrDefault();
                if (image == null)
                {
                    return;
                }

                var animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("FullScreenPicture", image as ImageView);
                if (animation != null)
                {
                    Prepare();

                    Layer.Visibility = Visibility.Collapsed;
                    TopBar.Visibility = Visibility.Collapsed;
                    BotBar.Visibility = Visibility.Collapsed;

                    animation.Completed += (s, args) =>
                    {
                        Hide();
                    };
                }
                else
                {
                    Flip.Opacity = 0;
                    Layer.Visibility = Visibility.Collapsed;
                    TopBar.Visibility = Visibility.Collapsed;
                    BotBar.Visibility = Visibility.Collapsed;

                    Hide();
                }
            }
            else
            {
                Flip.Opacity = 0;
                Layer.Visibility = Visibility.Collapsed;
                TopBar.Visibility = Visibility.Collapsed;
                BotBar.Visibility = Visibility.Collapsed;

                Hide();
            }

            e.Handled = true;
        }

        private async void ImageView_ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = sender as FrameworkElement;
            //var container = image.Ancestors<FlipViewItem>().FirstOrDefault();
            //var item = Flip.ItemFromContainer(container);

            //if (image != null)
            //{
            //    image.Opacity = 1;
            //}

            //if (item == ViewModel.SelectedItem)
            {
                //await Task.Delay(1000);

                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("FullScreenPicture");
                if (animation != null)
                {
                    //if (_layerVisual != null && _topBarVisual != null && _botBarVisual != null)
                    //{
                    //    var batch = _layerVisual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

                    //    var easing = ConnectedAnimationService.GetForCurrentView().DefaultEasingFunction;
                    //    var duration = ConnectedAnimationService.GetForCurrentView().DefaultDuration;

                    //    var animOpacity = _layerVisual.Compositor.CreateScalarKeyFrameAnimation();
                    //    animOpacity.InsertKeyFrame(0, 0, easing);
                    //    animOpacity.InsertKeyFrame(1, 1, easing);
                    //    animOpacity.Duration = duration;
                    //    _layerVisual.StartAnimation("Opacity", animOpacity);

                    //    var animTop = _layerVisual.Compositor.CreateVector3KeyFrameAnimation();
                    //    animTop.InsertKeyFrame(1, new Vector3(0, 0, 0), easing);
                    //    animTop.Duration = duration;
                    //    _topBarVisual.StartAnimation("Offset", animTop);

                    //    var animBot = _layerVisual.Compositor.CreateVector3KeyFrameAnimation();
                    //    animBot.InsertKeyFrame(1, new Vector3(0, 0, 0), easing);
                    //    animBot.Duration = duration;
                    //    _botBarVisual.StartAnimation("Offset", animBot);

                    //    batch.End();
                    //}

                    Layer.Visibility = Visibility.Visible;
                    TopBar.Visibility = Visibility.Visible;
                    BotBar.Visibility = Visibility.Visible;

                    //Flip.Opacity = 1;
                    animation.TryStart(image);
                    animation.Completed += (s, args) =>
                    {
                        Flip.Opacity = 1;
                        Surface.Visibility = Visibility.Collapsed;
                    };
                }
            }
        }

        private string ConvertDate(int value)
        {
            var date = Convert.DateTime(value);
            return string.Format("{0} at {1}", date.Date == DateTime.Now.Date ? "Today" : Convert.ShortDate.Format(date), Convert.ShortTime.Format(date));
        }

        private void Download_Click(object sender, TransferCompletedEventArgs e)
        {
            var border = sender as TransferButton;
            var item = border.DataContext as GalleryItem;

            if (item.IsVideo)
            {
                Play(item);
            }
        }

        private void Play(GalleryItem item)
        {
            try
            {
                var container = Flip.ContainerFromItem(item) as ContentControl;
                if (container != null && container.ContentTemplateRoot is Grid parent)
                {
                    //_surface = parent.FindName("Surface") as ImageView;
                    _surface = parent;

                    _mediaPlayer.MediaPlayer.SetSurfaceSize(new Size(_surface.ActualWidth, _surface.ActualHeight));

                    var swapchain = _mediaPlayer.MediaPlayer.GetSurface(_layerVisual.Compositor);
                    var brush = _layerVisual.Compositor.CreateSurfaceBrush(swapchain.CompositionSurface);
                    var size = new Vector2((float)_surface.ActualWidth, (float)_surface.ActualHeight);

                    //var mask = Unigram.Common.ImageLoader.Instance.LoadCircle(240, Colors.White).Brush;
                    //var graphicsEffect = new AlphaMaskEffect
                    //{
                    //    Source = new CompositionEffectSourceParameter("image"),
                    //    AlphaMask = new CompositionEffectSourceParameter("mask")
                    //};

                    //var effectFactory = _layerVisual.Compositor.CreateEffectFactory(graphicsEffect);
                    //var effectBrush = effectFactory.CreateBrush();
                    //effectBrush.SetSourceParameter("image", brush);
                    //effectBrush.SetSourceParameter("mask", mask);

                    _surfaceVisual = _layerVisual.Compositor.CreateSpriteVisual();
                    _surfaceVisual.Size = size;
                    _surfaceVisual.Brush = brush;

                    ElementCompositionPreview.SetElementChildVisual(_surface, _surfaceVisual);

                    _mediaSurface = swapchain;
                    _mediaPlayer.Source = MediaSource.CreateFromUri(item.GetVideoSource());
                    _mediaPlayer.MediaPlayer.IsLoopingEnabled = item.IsLoop;
                    _mediaPlayer.MediaPlayer.Play();
                }
            }
            catch { }
        }

        private void Flip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispose();
        }

        private void Dispose()
        {
            if (_surface != null)
            {
                ElementCompositionPreview.SetElementChildVisual(_surface, null);
                _surface = null;
            }

            if (_surfaceVisual != null)
            {
                _surfaceVisual.Brush = null;
                _surfaceVisual = null;
            }

            if (_mediaPlayer?.Source != null)
            {
                _mediaPlayer.MediaPlayer.Pause();
                _mediaPlayer.Source = null;
            }
        }
    }
}
