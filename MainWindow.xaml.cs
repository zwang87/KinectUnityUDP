using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit.Interaction;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace KinectForUnityTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        //create UDP connection
        UDPConnection udpConnection = new UDPConnection();

        //Kinect sensor
        KinectSensor sensor = null;
        InteractionClient interactionClient;
        InteractionStream stream;

        private UserInfo[] userInfos = null;
        Skeleton[] skeletonList;

        string handText = "";
        string headText = "";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitKinect();
        }

        public void InitKinect()
        {
            foreach (var i in KinectSensor.KinectSensors)
            {
                if (i.Status == KinectStatus.Connected)
                    sensor = i;
                if (sensor != null)
                {
                    sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    sensor.DepthFrameReady += SensorOnDepthFrameReady;
                    sensor.SkeletonStream.Enable();
                    sensor.SkeletonFrameReady += SensorOnSkeletonFrameReady;
                    interactionClient = new InteractionClient();
                    stream = new InteractionStream(sensor, interactionClient);
                    stream.InteractionFrameReady += StreamOnInteractionFrameReady;
                    sensor.Start();
                }
            }
        }

        private void SensorOnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    stream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                }
            }
        }

        private void SensorOnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletonList = new Skeleton[sensor.SkeletonStream.FrameSkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletonList);
                    stream.ProcessSkeleton(skeletonList, sensor.AccelerometerGetCurrentReading(), skeletonFrame.Timestamp);
                }
            }
        }

        int index = 0;
        private void StreamOnInteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
 
            using (InteractionFrame interactionFrame = e.OpenInteractionFrame())
            {
                if (interactionFrame != null)
                {
                    if (userInfos == null)
                    {
                        userInfos = new UserInfo[InteractionFrame.UserInfoArrayLength];
                    }
                    interactionFrame.CopyInteractionDataTo(userInfos);
                }
                else
                    return;

                foreach (UserInfo player in userInfos)
                {
                    foreach (InteractionHandPointer handPointer in player.HandPointers)
                    {
                        int id = player.SkeletonTrackingId;
                        
                        for (int i = 0; i < skeletonList.Length; i++)
                            if(id == skeletonList[i].TrackingId)
                                index = i;

                        //Trace.WriteLine(index);
                        headText = skeletonList[index].Joints[JointType.Head].Position.X + " " + skeletonList[index].Joints[JointType.Head].Position.Y + " " + skeletonList[index].Joints[JointType.Head].Position.Z;
                        string action = null;
                        switch (handPointer.HandEventType)
                        {
                            case InteractionHandEventType.Grip:
                                action = "Hand Gripped";
                                break;
                            case InteractionHandEventType.GripRelease:
                                action = "Hand Released";
                                break;
                        }
                        if (action != null)
                        {
                            string handSide = "unknown";
                            switch (handPointer.HandType)
                            {
                                case InteractionHandType.Left:
                                    handSide = "Left";
                                    break;
                                case InteractionHandType.Right:
                                    handSide = "Right";
                                    break;
                            }

                            handText = handSide + " " + action;
                            //this.label1.Content = labelText;
                            udpConnection.SendData(System.Text.Encoding.Default.GetBytes(handText));
                        
                        
                        }
                        if (handPointer.HandType == InteractionHandType.Right && handPointer.IsTracked)
                        {
                            string pos = "pos: " + handPointer.RawX + " " + handPointer.RawY + " " + handPointer.RawZ + " " + headText;
                            Trace.WriteLine(pos);
                            udpConnection.SendData(pos);
                        }
                    }
        
                    
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sensor != null)
            {
                sensor.Stop();
            }
            udpConnection.CloseConnection();
        }

    }

}