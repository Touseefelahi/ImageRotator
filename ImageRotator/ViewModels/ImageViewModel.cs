using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using ImageRotator.Models;
using System.Runtime.InteropServices;

namespace ImageRotator.Core
{
    public class ImageViewModel : BaseViewModel
    {
        public Image<Bgr, byte> Graphics { get; set; }
        
        public ICommand CommandOpenImage { get; set; }
        public ICommand CommandFlipImage { get; set; }
        public ICommand CommandSaveImage { get; set; }
        public ICommand CommandMouseUp { get; set; }
        public ICommand CommandShowDefault { get; set; }
        public ICommand CommandMouseMove { get; set; }
        public ICommand CommandResetFirstPoint { get; set; }
        public bool IsVerticallyAligned { get; set; } = true;
        public string PathOfFile { get; set; }
        public double ToolTipX { get; set; }
        public double ToolTipY { get; set; }

        string toolTipText;
        public string ToolTipText
        {
            get => toolTipText;
            set
            {
                toolTipText = IsPointA ? "Select First Point" : "Select Second Point" + Environment.NewLine + value;
            }
        }
        public Point PointA { get; set; }
        public Point PointB { get; set; }


        public Point MousePositionCurrent { get; set; }

        SemaphoreSlim signalForNextImage = new SemaphoreSlim(0);

        public double Angle { get; set; }
        public bool IsPointA { get; set; } = true;
        public string Info { get; private set; } = "Open and select image files to get started";
        public string ImageCounterDisplay { get; set; }
        public bool IsBusy { get; private set; }

        int totalImages;
        int currentImageNumber;

        double displayWidth;
        double displayHeight;
        double actualWidth;
        double actualHeight;

        double baseTriangle;
        double perpendicularTriangle;
        double hypotenuseTriangle;
        Bgr background = new Bgr(0, 0, 0);

        public ImageViewModel()
        {
            CommandOpenImage = new RelayCommand(OpenImage);
            CommandSaveImage = new RelayCommand(async () => await SaveImageAsync());
            CommandFlipImage = new RelayCommand(FlipImage);
            CommandShowDefault = new RelayCommand(async () => await ShowDefaultImageAsync());
            CommandMouseUp = new RelayParameterizedCommand((parameter) => MouseUp(parameter));
            CommandMouseMove = new RelayParameterizedCommand((parameter) => MouseMove(parameter));
            CommandResetFirstPoint = new RelayCommand(() => IsPointA = true);
        }


        private void FlipImage()
        {
            Graphics = Graphics.Rotate(180, background);
        }

        private void MouseMove(object parameter)
        {
            if (parameter is Image image)
            {
                MousePositionCurrent = Mouse.GetPosition(image);
            }
        }

        private async Task ShowDefaultImageAsync()
        {
            if (!IsBusy)
            {
                IsBusy = true;
                try
                {

                    await Task.Run(() => Graphics.Dispose());
                    Graphics = new Image<Bgr, byte>(fileName);
                    Info = "Default image loaded";
                    IsPointA = true;
                }
                catch (Exception ex)
                {
                    Info = ex.Message;
                }
                await Task.Delay(200);
                IsBusy = false;
            }
        }

        private void MouseUp(object parameter)
        {
            if (parameter is Image image)
            {
                displayWidth = image.ActualWidth;
                displayHeight = image.ActualHeight;
                actualWidth = Graphics.Width;
                actualHeight = Graphics.Height;
                var point = Mouse.GetPosition(image);
                if (IsPointA)
                {
                    PointA = point;
                }
                else PointB = point;

                IsPointA = !IsPointA;
                if (IsPointA) RotateImageAsync();
            }

        }

        private async void RotateImageAsync()
        {
            if (!IsBusy)
            {
                IsBusy = true;
                try
                {
                    if (PointA != null && PointB != null)
                    {
                        baseTriangle = Math.Abs(PointB.X - PointA.X);
                        perpendicularTriangle = Math.Abs(PointB.Y - PointA.Y);
                        hypotenuseTriangle = Math.Sqrt(Math.Pow(baseTriangle, 2) + Math.Pow(perpendicularTriangle, 2));
                        var angleRadian = Math.Atan(perpendicularTriangle / baseTriangle);
                        Angle = angleRadian * 180.0 / Math.PI;

                        var position = Position.Get(PointA, PointB);
                        if (IsVerticallyAligned)
                        {
                            Angle = 90 - Angle;
                            switch (position)
                            {
                                case FirstPointPosition.LeftDown:
                                case FirstPointPosition.RightUp:
                                    Angle = (-1) * Angle;
                                    break;
                            }
                        }
                        else
                        {
                            switch (position)
                            {
                                case FirstPointPosition.LeftUp:
                                case FirstPointPosition.RightDown:
                                    Angle = (-1) * Angle;
                                    break;
                            }
                        }
                        Graphics = Graphics.Rotate(Angle, background);
                    }
                }
                catch (Exception ex)
                {
                    Info = ex.Message;
                }
                await Task.Delay(50);
                IsBusy = false;
            }
        }

        string fileName;
        private async Task SaveImageAsync()
        {
            try
            {
                var fileNameRotated = Path.GetFileNameWithoutExtension(fileName) + "_Rotated.png";
                var path = Path.GetDirectoryName(fileName);
                var fullPath = $"{path}\\{fileNameRotated}";
                Graphics.Save(fullPath);
                Info = $"{fileNameRotated} Saved Successfully";
                await Task.Run(() => Graphics.Dispose());
                signalForNextImage.Release();
                PointA = new Point();
                PointB = new Point();
                Angle = 0;
            }
            catch (Exception ex)
            {
                Info = ex.Message;
            }
        }

        private async void OpenImage()
        {
            OpenFileDialog openFile = new OpenFileDialog
            {
                Filter = "Image files |*.jpg;*.png;*.bmp|All files (*.*)|*.*",
                Multiselect = true
            };

            if (openFile.ShowDialog() == true)
            {
                totalImages = openFile.FileNames.Count();
                currentImageNumber = 1;
                foreach (var file in openFile.FileNames)
                {
                    fileName = file;
                    Graphics = new Image<Bgr, byte>(fileName); ;
                    ImageCounterDisplay = $"Image Number {currentImageNumber}/{totalImages}";
                    await signalForNextImage.WaitAsync();
                    currentImageNumber++;
                }
                Info = "All Images rotated";
                MessageBox.Show("All images rotated");
            }

        }
    }
}
