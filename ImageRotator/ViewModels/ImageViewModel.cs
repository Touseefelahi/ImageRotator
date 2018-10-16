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
using System.Collections.ObjectModel;
using ImageRotator.Properties;

namespace ImageRotator.Core
{
    public class ImageViewModel : BaseViewModel
    {
        public Image<Bgr, byte> Graphics { get; set; }
        public Image<Bgr, byte> GraphicsRotated { get; set; }

        public ICommand CommandOpenImage { get; set; }
        public ICommand CommandFlipImage { get; set; }
        public ICommand CommandSaveImage { get; set; }
        public ICommand CommandMouseUp { get; set; }
        public ICommand CommandRightMouseDown { get; set; }
        public ICommand CommandNextImage { get; set; }
        public ICommand CommandShowDefault { get; set; }
        public ICommand CommandMouseMove { get; set; }
        public ICommand CommandResetFirstPoint { get; set; }

        public ICommand CommandGetReticleShift { get; set; }
        public ICommand CommandClearImage { get; set; }
        public ICommand CommandKeyInput { get; set; }

        public string HelpText { get; set; } = "Press S for Save current frame\n" +
            "Press R to rotate the image again\n" +
            "Press N for next image";

        public bool IsVerticallyAligned { get; set; } = true;
        bool ImageRotated = false;
        public Point PointA { get; set; }
        public Point PointB { get; set; }

        public Point ReferencePointLeft { get; set; }
        public Point ReferencePointRight { get; set; }
        public Point MovingReticlePosition { get; set; }
        public Point ZeroReference { get; set; }
        
        public double ZeroReferenceCorrection
        {
            get => Settings.Default.ZeroReferenceAngle;
            set
            {
                Settings.Default.ZeroReferenceAngle = value;
                Settings.Default.Save();
            }
        }
        public double AnglePerPixel { get; set; }
        public Point MousePositionCurrent { get; set; }

        SemaphoreSlim signalForNextImage = new SemaphoreSlim(0);

        public double Angle { get; set; }
        public bool IsPointA { get; set; } = true;
        public int ReferencePointCounter { get; set; } = 0;

        public string Info { get; private set; } = "Open and select image files to get started";
        public string ImageCounterDisplay { get; set; }
        public bool IsBusy { get; private set; }
        public double MovingReticleShiftPixels { get; set; }
        public double MovingReticleShiftAngle { get; set; }

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
        public int ReferenceValue { get; set; } = 4;
        public ObservableCollection<int> ReferencePointList { get; set; }
        public ImageViewModel()
        {
            CommandOpenImage = new RelayCommand(OpenImage);
            CommandSaveImage = new RelayCommand( SaveImageAsync);
            CommandFlipImage = new RelayCommand(FlipImage);
            CommandShowDefault = new RelayCommand(async () => await ShowDefaultImageAsync());
            CommandMouseUp = new RelayParameterizedCommand((parameter) => MouseUp(parameter));
            CommandMouseMove = new RelayParameterizedCommand((parameter) => MouseMove(parameter));
            CommandResetFirstPoint = new RelayCommand(() => IsPointA = true);
            CommandRightMouseDown = new RelayParameterizedCommand((parameter) => ReferencePointSelection(parameter));
            CommandKeyInput = new RelayParameterizedCommand((parameter) => KeyIn(parameter));

            CommandNextImage = new RelayCommand(NextImage);
            CommandClearImage = new RelayCommand(ClearImage);
            CommandGetReticleShift = new RelayCommand(CalculateReticleShift);

            var listOfReferencePoint = new List<int>
            {
                2,4,6,8,10,12,14,16
            };

            ReferencePointList = new ObservableCollection<int>(listOfReferencePoint);
           
        }

        private void KeyIn(object parameter)
        {
            if (parameter is string InKey)
            {
                switch (InKey)
                {
                    case "S":
                        CommandSaveImage.Execute(null);
                        break;
                    case "R":
                        ImageRotated = false;
                        break;
                    case "N":
                        CommandNextImage.Execute(null);
                        break;

                }
            }
        }

        private void CalculateReticleShift()
        {
            try
            {
                MovingReticleShiftPixels = MovingReticlePosition.X - ZeroReference.X;
                MovingReticleShiftAngle = (MovingReticleShiftPixels / AnglePerPixel) - ZeroReferenceCorrection;
            }
            catch (Exception ex)
            {
                Info = $"{ex.Message}";
            }
        }

        private void ClearImage()
        {
            try
            {
                Graphics = GraphicsRotated.Clone();
            }
            catch (Exception)
            {

            }
        }

        private void NextImage()
        {
            signalForNextImage.Release();
        }

        private void ReferencePointSelection(object parameter)
        {
            if (parameter is Image image)
            {
                displayWidth = image.ActualWidth;
                displayHeight = image.ActualHeight;
                actualWidth = Graphics.Width;
                actualHeight = Graphics.Height;
                var point = Mouse.GetPosition(image);
                try
                {
                    MovingReticlePosition = point;
                    CalculateReticleShift();
                }
                catch (Exception)
                {

                }
            }
        }

        private void GenerateZeroReference()
        {
            var width = ReferencePointRight.X - ReferencePointLeft.X;
            AnglePerPixel = width / (ReferenceValue * 2);
            System.Drawing.Point pointUp = new System.Drawing.Point((int)(ReferencePointLeft.X + width / 2), (int)ReferencePointLeft.Y - 50);
            System.Drawing.Point pointDown = new System.Drawing.Point((int)(ReferencePointLeft.X + width / 2), (int)ReferencePointLeft.Y + 50);
            ZeroReference = new Point(ReferencePointLeft.X + width / 2, ReferencePointLeft.Y);
            CvInvoke.Line(Graphics, pointUp, pointDown, new MCvScalar(50, 250, 50), 2);
            OnPropertyChanged(nameof(Graphics));
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

                if (!ImageRotated)
                {
                    if (ReferencePointCounter == 3)
                    {
                        ReferencePointCounter = 4;
                        PointA = point;
                    }
                    else if (ReferencePointCounter == 4)
                    {
                        ReferencePointCounter = 3;
                        PointB = point;
                    }
                    else
                    {
                        ReferencePointCounter = 4;
                        PointA = point;
                    }

                    IsPointA = !IsPointA;
                    if (IsPointA) RotateImageAsync();
                }
                else
                {
                    switch (ReferencePointCounter)
                    {
                        default:
                            ReferencePointLeft = point;
                            ReferencePointCounter = 1;
                            break;
                        case 1:
                            ReferencePointRight = point;
                            GenerateZeroReference();
                            ReferencePointCounter = 2;
                            break;
                        case 2:
                            MovingReticlePosition = point;
                            ReferencePointCounter = 0;
                            CalculateReticleShift();
                            break;
                    }
                }
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
                        GraphicsRotated = Graphics.Clone();
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
        private void SaveImageAsync()
        {
            try
            {
                var fileNameRotated = Path.GetFileNameWithoutExtension(fileName) + "_Rotated.png";
                var path = Path.GetDirectoryName(fileName);
                var fullPath = $"{path}\\{fileNameRotated}";
                Graphics.Save(fullPath);
                ReferencePointCounter = 0;
                Info = $"{fileNameRotated} Saved Successfully";             
                ImageRotated = true;
              
            }
            catch (Exception ex)
            {
                Info = ex.Message;
            }
        }

        private void ResetDisplay()
        {
            ImageRotated = false;
            ReferencePointCounter = 3;
            IsPointA = true;
            ReferencePointLeft = new Point();
            ReferencePointRight = new Point();
            MovingReticlePosition = new Point();
            AnglePerPixel = 0;
            MovingReticleShiftAngle = 0;
            MovingReticleShiftPixels = 0;
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
                    ResetDisplay();
                    fileName = file;
                    Graphics = new Image<Bgr, byte>(fileName);
                    ImageCounterDisplay = $"Title: {Path.GetFileName(file)}  Total Images: {currentImageNumber}/{totalImages}";

                    await Task.Run(async () =>
                     {
                         await Task.Delay(1000);
                         ReferencePointCounter = 3;
                         IsPointA = true;
                     });

                    await signalForNextImage.WaitAsync();
                    currentImageNumber++;
                }
                Info = "All Images rotated";
                MessageBox.Show("All images rotated");
            }

        }
    }
}
