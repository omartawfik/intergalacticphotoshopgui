﻿namespace IntergalacticUI
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using IntergalacticControls;
    using IntergalacticControls.Classes;
    using IntergalacticControls.PopupUI.Notifications;
    using IntergalacticCore;
    using IntergalacticCore.Data;
    using IntergalacticCore.Operations.Filters.EdgeDetection;
    using IntergalacticCore.Operations.Filters.Sharpening;
    using IntergalacticCore.Operations.Filters.Smoothing;
    using IntergalacticCore.Operations.HistogramOperations;
    using IntergalacticCore.Operations.JoinedOperations;
    using IntergalacticCore.Operations.Matlab;
    using IntergalacticCore.Operations.Matlab.PassFilters;
    using IntergalacticCore.Operations.Matlab.Retinex;
    using IntergalacticCore.Operations.Morphology;
    using IntergalacticCore.Operations.Noise.Add;
    using IntergalacticCore.Operations.Noise.Remove;
    using IntergalacticCore.Operations.PixelOperations;
    using IntergalacticCore.Operations.ResizeOperations;
    using IntergalacticCore.Operations.Transformations;
    using IntergalacticUI.Classes;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Used to make double clicks
        /// </summary>
        private DateTime doubleClickTimer;

        /// <summary>
        /// The time it took to show the image
        /// </summary>
        private TimeSpan imageShowingTime;

        /// <summary>
        /// Loading notification view in use
        /// </summary>
        private LoadingNotificationView loadingNotificationView;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            new UIManager(mainGrid);

            Manager.Instance.OnNewTabAdded += this.UpdateImage;
            Manager.Instance.OnTabChanged += this.UpdateImage;
            Manager.Instance.OnOperationFinshed += this.UpdateImage;
            Manager.Instance.OnOperationFinshed += this.AddFrequencyDomainOperationTabs;
            Manager.Instance.OnOperationStarted += this.ShowLoadingNotification;
            Manager.Instance.OnOperationFinshed += this.HideLoadingNotification;
            Manager.Instance.OnOperationFinshed += this.ShowTheFinishingNotification;
            Manager.Instance.OnOperationFailed += this.HideLoadingNotification;
            Manager.Instance.OnOperationFailed += this.ShowTheFinishingNotification;

            this.AddActionCategory(
                "File",
                "File.png",
                new ActionPair(this.OpenFile, "Open.."),
                new ActionPair(this.SaveFile, "Save.."));

            this.AddOperationCategory(
                "Resize",
                "Resize.png",
                new NearestNeighbourResizeOperation(),
                new BilinearResizeOperation());

            this.AddOperationCategory(
                "Transformations",
                "Transformations.png",
                new HorizontalFlipOperation(),
                new VerticalFlipOperation(),
                new HorizontalShearOperation(),
                new VerticalShearOperation(),
                new TranslationOperation());

            this.AddOperationCategory(
                "Pixel Operations",
                "Pixel.png",
                new InverseOperation(),
                new BrightnessOperation(),
                new GrayOperation(),
                new ContrastOperation(),
                new GammaAdjustmentOperation(),
                new QuantizationOperation(),
                new BinarizationOperation(),
                new ColorExtractionOperation(),
                new CustomMaskOperation(),
                new PixelationOperation());

            this.AddOperationCategory(
                "Joined Operations",
                "Joined.png",
                new AddOperation(),
                new SubtractOperation());

            this.AddOperationCategory(
                "Smoothing",
                "Smoothing.png",
                new GaussianFilter1D(),
                new GaussianFilter2D(),
                new MeanFilter1D(),
                new MeanFilter2D(),
                new DialationOperation(),
                new ErosionOperation());

            this.AddOperationCategory(
                "Sharpening",
                "Sharpening.png",
                new BackDiagonalLineSharpeningOperation(),
                new FrontDiagonalLineSharpeningOperation(),
                new HorizontalLineSharpeningOperation(),
                new VerticalLineSharpeningOperation(),
                new LaplacianPointSharpeningOperation(),
                new LaplacianSharpeningOperation());

            this.AddOperationCategory(
                "Edge Detection",
                "EdgeDetection.png",
                new BackDiagonalEdgeDetectionOperation(),
                new FrontDiagonalEdgeDetectionOperation(),
                new HorizontalEdgeDetectionOperation(),
                new VerticalEdgeDetectionOperation(),
                new LaplacianPointDetectionOperation(),
                new LaplacianEdgeDetectionOperation());

            ////this.AddOperationCategory(
            ////   "Matlab Operations",
            ////   "Matlab.png",
            ////   new FrequencyDomainOperation(),
            ////   new LocalHistogramEqualizationOperation(),
            ////   new MultiScaleRetinexOperation(),
            ////   new MultiScaleRetinexWithColorRestorationOperation(),
            ////   new MultiScaleRetinexWithColorRestorationAndGainOffsetOperation());

            ////this.AddOperationCategory(
            ////    "Pass Filters",
            ////    "PassFilter.png",
            ////    new IdealLowPassFilter(),
            ////    new IdealHighPassFilter(),
            ////    new IdealBandPassFilter(),
            ////    new IdealBandRejectFilter(),
            ////    new GaussianLowPassFilter(),
            ////    new GaussianHighPassFilter(),
            ////    new ButterworthLowPassFilter(),
            ////    new ButterworthHighPassFilter(),
            ////    new IdealNotchPassFilter(),
            ////    new IdealNotchRejectFilter());

            this.AddOperationCategory(
                "Noise Operations",
                "Noise.png",
                new AddSaltPepperNoiseOperation(),
                new AddUniformNoiseOperation(),
                new AddGaussianNoiseOperation(),
                new AddExponentialNoiseOperation(),
                new AddRayleighNoiseOperation(),
                new GeometricMeanFilter(),
                new MinFilter(),
                new MaxFilter(),
                new MedianFilter(),
                new AdaptiveMedianFilter(),
                new MidPointFilter());

            this.InitHistogramView();
            this.InitZoomView();
            this.InitBackground();

            this.Width = this.splash.Width;
            this.Height = this.splash.Height;
            this.splash.OpenAnImageAction = this.OpenFile;
        }

        /// <summary>
        /// Saves the current image into a user specified path.
        /// </summary>
        private void SaveFile()
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "Bitmap (*.bmp)|*.bmp|Portable Network Graphics (*.png)|*png|JPEG image (*.jpeg)|*.jpg|PPM P3 image (*.ppm)|*.ppm";
            dialog.FilterIndex = 0;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImageFileType saveType = (ImageFileType)dialog.FilterIndex - 1;
                Manager.Instance.CurrentTab.Image.SaveImage(dialog.FileName, saveType);
            }
        }

        /// <summary>
        /// Opens a new file and places it in a new tab.
        /// </summary>
        private void OpenFile()
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string tabName = System.IO.Path.GetFileName(dialog.FileName).Split('.')[0];
                List<string> allNames = Manager.Instance.GetTabNames();

                if (allNames.Contains(tabName))
                {
                    int counter = 2;
                    while (allNames.Contains(tabName + counter.ToString()))
                    {
                        counter++;
                    }

                    tabName += counter.ToString();
                }

                WPFBitmap bitmap = new WPFBitmap();

                if (System.IO.Path.GetFileName(dialog.FileName).Split('.')[1] == "ppm")
                {
                    Importer.LoadPPM(dialog.FileName, bitmap);
                }
                else
                {
                    bitmap = new WPFBitmap(new BitmapImage(new Uri(dialog.FileName)));
                }

                Manager.Instance.AddTab(bitmap, tabName);
            }

            if (this.splash.Visibility == System.Windows.Visibility.Visible)
            {
                DoubleAnimation l = new DoubleAnimation((System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 900) / 2, TimeSpan.FromSeconds(0.5));
                l.AccelerationRatio = 0.3;
                l.DecelerationRatio = 0.3;

                DoubleAnimation t = new DoubleAnimation((System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 600) / 2, TimeSpan.FromSeconds(0.5));
                t.AccelerationRatio = 0.3;
                t.DecelerationRatio = 0.3;
                t.BeginTime = TimeSpan.FromSeconds(0.5);

                DoubleAnimation w = new DoubleAnimation(900, TimeSpan.FromSeconds(0.5));
                w.AccelerationRatio = 0.3;
                w.DecelerationRatio = 0.3;

                DoubleAnimation h = new DoubleAnimation(600, TimeSpan.FromSeconds(0.5));
                h.AccelerationRatio = 0.3;
                h.DecelerationRatio = 0.3;
                h.BeginTime = TimeSpan.FromSeconds(0.5);

                DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
                fadeOut.BeginTime = TimeSpan.FromSeconds(1);

                this.BeginAnimation(Window.WidthProperty, w);
                this.BeginAnimation(Window.HeightProperty, h);
                this.BeginAnimation(Window.LeftProperty, l);
                this.BeginAnimation(Window.TopProperty, t);
                this.splash.BeginAnimation(FrameworkElement.OpacityProperty, fadeOut);
                this.splashBack.BeginAnimation(FrameworkElement.OpacityProperty, fadeOut);

                IntergalacticControls.UIHelpers.HideElementAfterDelay(this.splash, 1.3);
                IntergalacticControls.UIHelpers.HideElementAfterDelay(this.splashBack, 1.3);
            }
        }

        /// <summary>
        /// Adds a new action category.
        /// </summary>
        /// <param name="name">Name of category.</param>
        /// <param name="icon">Icon name of category.</param>
        /// <param name="actionPairs">Actions included in category.</param>
        private void AddActionCategory(string name, string icon, params ActionPair[] actionPairs)
        {
            ActionCategory category = new ActionCategory(name, new BitmapImage(new Uri("pack://application:,,,/Pictures/" + icon)));

            foreach (ActionPair actionPair in actionPairs)
            {
                category.AddAction(actionPair.Action, actionPair.Name);
            }

            SubmenuContainer menu = new SubmenuContainer();
            menu.SetCategory(category);

            PanelButton button = new PanelButton();
            button.SubViews.Add(menu);
            button.Icon = category.Icon;

            leftStackController.AddButton(button);
        }

        /// <summary>
        /// Adds a new operation category.
        /// </summary>
        /// <param name="name">Name of category.</param>
        /// <param name="icon">Icon name of category.</param>
        /// <param name="operations">Operations included in category.</param>
        private void AddOperationCategory(string name, string icon, params BaseOperation[] operations)
        {
            OperationCategory category = new OperationCategory(name, new BitmapImage(new Uri("pack://application:,,,/Pictures/" + icon)));

            foreach (BaseOperation operation in operations)
            {
                category.AddOperation(operation);
            }

            SubmenuContainer menu = new SubmenuContainer();
            menu.SetCategory(category);

            PanelButton button = new PanelButton();
            button.SubViews.Add(menu);
            button.Icon = category.Icon;

            leftStackController.AddButton(button);
        }

        /// <summary>
        /// Initializes the histogram view.
        /// </summary>
        private void InitHistogramView()
        {
            HistogramView view = new HistogramView();

            PanelButton button = new PanelButton();
            button.Icon = new BitmapImage(new Uri("pack://application:,,,/Pictures/Histogram.png"));
            button.SubViews.Add(view);

            OperationCategory category = new OperationCategory("Histogram Operations", null);
            category.AddOperation(new HistogramMatchingOperation());
            category.AddOperation(new HistogramEqualizationOperation());

            SubmenuContainer menu = new SubmenuContainer();
            menu.SetCategory(category);

            button.SubViews.Add(menu);
            button.IsLockable = true;

            rightStackController.AddButton(button);
        }
        
        /// <summary>
        /// Initializes the zoom view.
        /// </summary>
        private void InitZoomView()
        {
            PanelButton button = new PanelButton();
            button.Icon = new BitmapImage(new Uri("pack://application:,,,/Pictures/Zoom.png"));
            button.SubViews.Add(new ZoomView(imageView));

            rightStackController.AddButton(button);
        }

        /// <summary>
        /// initializes the background variables.
        /// </summary>
        private void InitBackground()
        {
            double timeMultiplier = 1;
            NormalMapEffect effect = new NormalMapEffect();
            ////this.backgroundRect.Effect = effect;

            DoubleAnimation anim1 = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(timeMultiplier * 4));
            anim1.AutoReverse = true;
            anim1.RepeatBehavior = RepeatBehavior.Forever;
            anim1.AccelerationRatio = 0.5;
            anim1.DecelerationRatio = 0.5;
            DoubleAnimation anim2 = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(timeMultiplier * 4));
            anim2.BeginTime = TimeSpan.FromSeconds(timeMultiplier * 2);
            anim2.AccelerationRatio = 0.5;
            anim2.DecelerationRatio = 0.5;
            anim2.AutoReverse = true;
            anim2.RepeatBehavior = RepeatBehavior.Forever;
            DoubleAnimation anim3 = new DoubleAnimation(0, 0.3, TimeSpan.FromSeconds(timeMultiplier * 20));
            anim3.AccelerationRatio = 0.5;
            anim3.DecelerationRatio = 0.5;
            anim3.AutoReverse = true;
            anim3.RepeatBehavior = RepeatBehavior.Forever;

            ////effect.BeginAnimation(NormalMapEffect.ValueXProperty, anim1);
            ////effect.BeginAnimation(NormalMapEffect.ValueYProperty, anim2);
            ////effect.BeginAnimation(NormalMapEffect.ValueZProperty, anim3);
        }

        /// <summary>
        /// Updates the displayed image.
        /// </summary>
        /// <param name="mng">Global Manager.</param>
        /// <param name="dontCare">Dontcare object</param>
        private void UpdateImage(Manager mng, object dontCare)
        {
            if (dontCare != null)
            {
                DateTime start = DateTime.Now;
                this.imageView.Source = ((WPFBitmap)mng.CurrentTab.Image).GetImageSource();
                this.imageShowingTime = DateTime.Now - start;
            }
            else
            {
                DoubleAnimation l = new DoubleAnimation((System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - this.splash.Width) / 2, TimeSpan.FromSeconds(0.5));
                l.AccelerationRatio = 0.3;
                l.DecelerationRatio = 0.3;

                DoubleAnimation t = new DoubleAnimation((System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - this.splash.Height) / 2, TimeSpan.FromSeconds(0.5));
                t.AccelerationRatio = 0.3;
                t.DecelerationRatio = 0.3;
                t.BeginTime = TimeSpan.FromSeconds(0.5);

                DoubleAnimation w = new DoubleAnimation(this.splash.Width, TimeSpan.FromSeconds(0.5));
                w.AccelerationRatio = 0.3;
                w.DecelerationRatio = 0.3;

                DoubleAnimation h = new DoubleAnimation(this.splash.Height, TimeSpan.FromSeconds(0.5));
                h.AccelerationRatio = 0.3;
                h.DecelerationRatio = 0.3;
                h.BeginTime = TimeSpan.FromSeconds(0.5);

                DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromSeconds(0.2));

                this.BeginAnimation(Window.WidthProperty, w);
                this.BeginAnimation(Window.HeightProperty, h);
                this.BeginAnimation(Window.LeftProperty, l);
                this.BeginAnimation(Window.TopProperty, t);
                this.splash.BeginAnimation(FrameworkElement.OpacityProperty, fadeIn);
                this.splashBack.BeginAnimation(FrameworkElement.OpacityProperty, fadeIn);

                this.splash.Visibility = System.Windows.Visibility.Visible;
                this.splashBack.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// Linked to the OnOperationStarted event the the Manager to show the loading notification
        /// </summary>
        /// <param name="mng">The manager</param>
        /// <param name="operation">The operation</param>
        private void ShowLoadingNotification(Manager mng, BaseOperation operation)
        {
            this.loadingNotificationView = new LoadingNotificationView();
            this.loadingNotificationView.ShowNotification();
        }

        /// <summary>
        /// Linked to the OnOperationFailed/Finished event the the Manager to hide the loading notification
        /// </summary>
        /// <param name="mng">The manager</param>
        /// <param name="operation">The operation</param>
        private void HideLoadingNotification(Manager mng, BaseOperation operation)
        {
            this.loadingNotificationView.HideNotification(mng, operation);
        }

        /// <summary>
        /// Adds the components of frequency domain to tabs.
        /// </summary>
        /// <param name="mng">Main Manager.</param>
        /// <param name="operate">Frequency Domain Operation.</param>
        private void AddFrequencyDomainOperationTabs(Manager mng, BaseOperation operate)
        {
            if (operate is FrequencyDomainOperation)
            {
                FrequencyDomainOperation op = operate as FrequencyDomainOperation;

                Manager.Instance.AddTab(op.FrequencyDomainImage, "Frequency");
                Manager.Instance.AddTab(op.RedImage, "RED");
                Manager.Instance.AddTab(op.GreenImage, "GREEN");
                Manager.Instance.AddTab(op.BlueImage, "BLUE");
            }
        }

        /// <summary>
        /// To make the window draggable with the top rectangle
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event agruments</param>
        private void TopTabControllerRect_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Maximizes the window
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event arguments</param>
        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != System.Windows.WindowState.Maximized)
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }
        }

        /// <summary>
        /// Minimizes the window
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event arguments</param>
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        /// <summary>
        /// Closes the window
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event arguments</param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// To handle double clicks on the tab bar
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event arguments</param>
        private void TopTabControllerRect_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DateTime now = DateTime.Now;

            if ((now - this.doubleClickTimer) < TimeSpan.FromMilliseconds(300))
            {
                if (this.WindowState != System.Windows.WindowState.Maximized)
                {
                    this.WindowState = System.Windows.WindowState.Maximized;
                }
                else
                {
                    this.WindowState = System.Windows.WindowState.Normal;
                }
            }

            this.doubleClickTimer = now;
        }

        /// <summary>
        /// Shows the finishing notfication.
        /// </summary>
        /// <param name="mng">The Manager</param>
        /// <param name="operation">The last operation</param>
        private void ShowTheFinishingNotification(Manager mng, BaseOperation operation)
        {
            if (mng.CurrentTab.DidOperationComplete)
            {
                SideNotification finishingNotification = new SideNotification();
                finishingNotification.SetTitle("Done in " + (operation.OperatingTime + this.imageShowingTime).ToString() + " Seconds.");
                finishingNotification.DisplayTimeout = 15;
                finishingNotification.AnimationType = NotificationAnimationType.Fade;
                finishingNotification.ShowNotification();
            }
            else
            {
                SideNotification notification = new SideNotification();
                notification.Height = 150;
                notification.SetTitle("Error while performing operation: " + mng.CurrentTab.LastException.Message);
                notification.ShowNotification();
            }
        }

        /// <summary>
        /// Undos the last operation
        /// </summary>
        private void UndoAction()
        {
            Manager.Instance.UndoOperation();
        }

        /// <summary>
        /// Redos the previous operation
        /// </summary>
        private void RedoOperation()
        {
            Manager.Instance.RedoOperation();
        }
    }
}
