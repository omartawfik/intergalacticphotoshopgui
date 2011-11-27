﻿namespace IntergalacticUI
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Forms;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using IntergalacticControls;
    using IntergalacticControls.Classes;
    using IntergalacticCore;
    using IntergalacticCore.Data;
    using IntergalacticCore.Operations.Filters.EdgeDetection;
    using IntergalacticCore.Operations.Filters.Sharpening;
    using IntergalacticCore.Operations.Filters.Smoothing;
    using IntergalacticCore.Operations.JoinedOperations;
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
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            new PopupViewManager(mainGrid);

            Manager.Instance.OnNewTabAdded += this.UpdateImage;
            Manager.Instance.OnTabChanged += this.UpdateImage;
            Manager.Instance.OnOperationFinshed += this.UpdateImage;

            this.AddActionCategory(
                "File",
                "File.png",
                new ActionPair(this.OpenFile, "Open Image"),
                new ActionPair(this.SaveFile, "Save Image"),
                new ActionPair(this.SaveFileP3, "Save Image as P3"));

            this.AddOperationCategory(
                "Resize",
                "Resize.png",
                new NearestNeighbourResizeOperation(),
                new BilinearResizeOperation(),
                new BorderingOperation());

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
                new NotOperation(),
                new BrightnessOperation(),
                new GrayOperation(),
                new ContrastOperation(),
                new GammaAdjustmentOperation(),
                new QuantizationOperation(),
                new BinarizationOperation(),
                new ColorExtractionOperation());

            this.AddOperationCategory(
                "Joined Operations",
                "Joined.png",
                new AddOperation(),
                new HistogramMatchingOperation(),
                new SubtractOperation());

            this.AddOperationCategory(
                "Smoothing",
                "Smoothing.png",
                new GaussianFilter1D(),
                new GaussianFilter2D(),
                new MeanFilter1D(),
                new MeanFilter2D());

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

            this.InitHistogramView();
            this.InitBackground();
            Manager.Instance.AddTab(new WPFBitmap(new BitmapImage(new Uri("C:\\Users\\Public\\Pictures\\Sample Pictures\\Desert.jpg"))), "Default", false);
        }

        /// <summary>
        /// Saves the current image into a user specified path as text PPM.
        /// </summary>
        private void SaveFileP3()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Exporter.SaveP3(dialog.FileName, Manager.Instance.CurrentTab.Image);
            }
        }

        /// <summary>
        /// Saves the current image into a user specified path.
        /// </summary>
        private void SaveFile()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Manager.Instance.CurrentTab.Image.SaveImage(dialog.FileName);
            }
        }

        /// <summary>
        /// Opens a new file and places it in a new tab.
        /// </summary>
        private void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();

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

            PanelButton button = new PanelButton();
            button.Category = category;
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

            PanelButton button = new PanelButton();
            button.Category = category;
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
            button.SubView = view;
            button.IsLockable = true;

            rightStackController.AddButton(button);
        }

        /// <summary>
        /// initializes the background variables.
        /// </summary>
        private void InitBackground()
        {
            double timeMultiplier = 1;
            NormalMapEffect effect = new NormalMapEffect();
            this.backgroundRect.Effect = effect;

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

            effect.BeginAnimation(NormalMapEffect.ValueXProperty, anim1);
            effect.BeginAnimation(NormalMapEffect.ValueYProperty, anim2);
            effect.BeginAnimation(NormalMapEffect.ValueZProperty, anim3);
        }

        /// <summary>
        /// Updates the displayed image.
        /// </summary>
        /// <param name="mng">Global Manager.</param>
        /// <param name="tab">Active Tab.</param>
        private void UpdateImage(Manager mng, Tab tab)
        {
            this.imageView.Source = ((WPFBitmap)tab.Image).GetImageSource();
        }

        /// <summary>
        /// Updates the displayed image.
        /// </summary>
        /// <param name="mng">Global Manager.</param>
        private void UpdateImage(Manager mng)
        {
            this.imageView.Source = ((WPFBitmap)mng.CurrentTab.Image).GetImageSource();
        }
    }
}