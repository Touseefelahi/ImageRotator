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

namespace ImageRotator.Core
{
    public class ImageViewModel : BaseViewModel
    {
        public Image<Bgr, byte> Graphics { get; set; }
        Image<Bgr, byte> GraphicsDefault { get; set; }

        public ICommand CommandOpenImage { get; set; }
        public ICommand CommandFlipImage { get; set; }
        public ICommand CommandSaveImage { get; set; }
        public ICommand CommandMouseUp { get; set; }
        public ICommand CommandShowDefault { get; set; }
        public ICommand CommandMouseMove { get; set; }
        public bool IsVerticallyAligned { get; set; } = true;
        public string PathOfFile { get; set; }
        public Point PointA { get; set; }
        public Point PointB { get; set; }
        public Point MousePositionCurrent { get; set; }

        SemaphoreSlim signalForNextImage = new SemaphoreSlim(0);

        public double Angle { get; set; }
        public bool IsPointA { get; set; } = true;
        public string Info { get; private set; } = "Open and select image files to get started";
        public string ImageCounterDisplay { get; set; }

        int totalImages;
        int currentImageNumber;

        double displayWidth;
        double displayHeight;
        double actualWidth;
        double actualHeight;

        double baseTriangle;
        double perpendicularTriangle;
        double hypotenuseTriangle;
        Bgr background = new Bgr(0,0,0);
        public ImageViewModel()
        {
            CommandOpenImage = new RelayCommand(OpenImage);
            CommandSaveImage = new RelayCommand(SaveImage);
            CommandFlipImage = new RelayCommand(FlipImage);
            CommandShowDefault = new RelayCommand(ShowDefaultImage);
            CommandMouseUp = new RelayParameterizedCommand((parameter) => MouseUp(parameter));
            CommandMouseMove = new RelayParameterizedCommand((parameter) => MouseMove(parameter));
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

        private void ShowDefaultImage()
        {
            Graphics = GraphicsDefault.Clone();
            IsPointA = true;
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
                if (IsPointA) RotateImage();
            }

        }

        private void RotateImage()
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

        string fileName;
        private void SaveImage()
        {
            var fileNameRotated = Path.GetFileNameWithoutExtension(fileName) + "_Rotated.png";
            var path = Path.GetDirectoryName(fileName);
            var fullPath = $"{path}\\{fileNameRotated}";
            Graphics.Save(fullPath);
            Info = $"{fileNameRotated} Saved Successfully";
            signalForNextImage.Release();
            PointA = new Point();
            PointB = new Point();
            Angle = 0;
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
                    GraphicsDefault = new Image<Bgr, byte>(fileName);
                    Graphics = GraphicsDefault.Clone();
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
