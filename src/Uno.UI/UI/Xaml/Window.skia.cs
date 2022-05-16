#nullable enable

using System;
using System.Diagnostics;
using Uno.Disposables;
using Uno.Foundation.Logging;
using Uno.UI.Xaml.Core;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Windows.UI.Xaml
{
	public sealed partial class Window
	{
		private static Window? _current;
		private RootVisual? _rootVisual;
		// private ScrollViewer _rootScrollViewer;
		private Border? _rootBorder;
		private UIElement? _content;

		public Window()
		{
			Init();

			Compositor = new Compositor();
		}

		public void Init()
		{
			Dispatcher = CoreDispatcher.Main;
			CoreWindow = CoreWindow.GetOrCreateForCurrentThread();
			CoreWindow.SetInvalidateRender(QueueInvalidateRender);
			InitDragAndDrop();
		}

		internal static Action InvalidateRender = () => { };
		private bool _renderQueued = false;

		internal void QueueInvalidateRender()
		{
			if (!_isMeasuringOrArranging && !_renderQueued)
			{
				_renderQueued = true;

				CoreDispatcher.Main.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					_renderQueued = false;
					InvalidateRender();
				});
			}
		}

		internal void OnNativeSizeChanged(Size size)
		{
			var newBounds = new Rect(0, 0, size.Width, size.Height);

			if (newBounds != Bounds)
			{
				if (this.Log().IsEnabled(Uno.Foundation.Logging.LogLevel.Debug))
				{
					this.Log().Debug($"OnNativeSizeChanged: {size}");
				}

				Bounds = newBounds;
				
				_rootVisual?.XamlRoot.InvalidateMeasure();
				RaiseSizeChanged(new Windows.UI.Core.WindowSizeChangedEventArgs(size));

				ApplicationView.GetForCurrentView().SetVisibleBounds(newBounds);
			}
		}

		public Compositor Compositor { get; }

		partial void InternalActivate()
		{
			// WebAssemblyRuntime.InvokeJS("Uno.UI.WindowManager.current.activate();");
		}

		private void InternalSetContent(UIElement content)
		{
			if (_rootVisual == null)
			{
				_rootBorder = new Border();
				CoreServices.Instance.PutVisualRoot(_rootBorder);
				_rootVisual = CoreServices.Instance.MainRootVisual;

				if (_rootVisual == null)
				{
					throw new InvalidOperationException("The root visual could not be created.");
				}

				UIElement.LoadingRootElement(_rootVisual);

				Compositor.RootVisual = _rootVisual.Visual;

				_rootVisual?.XamlRoot.InvalidateMeasure();

				UIElement.RootElementLoaded(_rootVisual);
			}

			if (_rootBorder != null)
			{
				_rootBorder.Child = _content = content;
			}
		}

		private UIElement InternalGetContent() => _content!;

		private UIElement InternalGetRootElement() => _rootVisual!;

		private static Window InternalGetCurrentWindow()
		{
			if (_current == null)
			{
				_current = new Window();
			}

			return _current;
		}

		internal IDisposable OpenPopup(Controls.Primitives.Popup popup)
		{
			if (this.Log().IsEnabled(Uno.Foundation.Logging.LogLevel.Debug))
			{
				this.Log().Debug($"Creating popup");
			}

			if (PopupRoot == null)
			{
				throw new InvalidOperationException("PopupRoot is not initialized yet.");
			}

			var popupPanel = popup.PopupPanel;
			PopupRoot.Children.Add(popupPanel);

			return Disposable.Create(() =>
			{

				if (this.Log().IsEnabled(Uno.Foundation.Logging.LogLevel.Debug))
				{
					this.Log().Debug($"Closing popup");
				}

				PopupRoot.Children.Remove(popupPanel);
			});
		}
	}
}
