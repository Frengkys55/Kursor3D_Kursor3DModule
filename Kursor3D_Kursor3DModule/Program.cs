using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IPC;
using ImageProcessor;
using Emgu.CV;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using Emgu.CV.Structure;
using Accord.Imaging;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.UI;
using System.Windows.Forms;
using Accord;
using Accord.Imaging.Filters;
using System.Drawing.Imaging;

namespace Kursor3D_Kursor3DModule
{
    class Program
    {
        #region Application informations

        #region Config file information
        static Configuration savedInformation   = new Configuration();
        static Configuration locations          = new Configuration();
        #endregion Config file information

        #region Gesture templates location
        static string   cursorGestureTypeImagesLocation   = string.Empty;
        static string   selectGestureTypeImagesLocatuon   = string.Empty;
        static string   moveGestureTypeImagesLocation     = string.Empty;
        static string   scaleGestureTypeImagesLocation    = string.Empty;
        static string   rotateGestureTypeImagesLocation   = string.Empty;
        static string   openMenuGestureTypeImagesLocation = string.Empty;
        #endregion Gesture templates location

        #region Application start and load modes
        static bool     startFromApplicationPath;
        static string   applicationPath;
        static bool     startApplicationHidden;
        #endregion Application start and load modes

        #region Gesture found informations
        static bool isHandFound = false;
        static bool isCursorGestureDetected     = false;
        static bool isSelectGestureDetected     = false;
        static bool isMoveGestureDetected       = false;
        static bool isScaleGestureDetected      = false;
        static bool isRotateGestureDetected     = false;
        static bool isOpenMenuGestureDetected   = false;
        #endregion Gesture found informations

        #region Received images
        // Received images (to prevent race condition)
        static Mat cursorReceivedImage   = null;
        static Mat selectReceivedImage   = null;
        static Mat moveReceivedImage     = null;
        static Mat scaleReceivedImage    = null;
        static Mat rotateReceivedImage   = null;

        static Bitmap receivedImage;
        #endregion Received images

        #region Processed images
        // Processed images
        static Bitmap resultImage = null;
        #endregion Processed images

        #region Gesture processed image
        // Gesture processed image
        static Mat resultCursorGesture      = null;
        static Mat resultSelectGesture      = null;
        static Mat resultMoveGesture        = null;
        static Mat resultScaleGesture       = null;
        static Mat resultRotateGesture      = null;
        static Mat resultOpenMenuGesture    = null;
        #endregion Gesture processed image

        #region Connection informations
        static string imageNotifierChannel          = string.Empty;
        static string cursorAndGestureInfoChannel   = string.Empty;
        static string mmfFileName                   = string.Empty;
        #endregion Connectoin informations

        #region Inter-Process Communication Objects

        // For receive information from the Penghubung Module
        static NamedPipesServer imageServer = null;
        // For sending info about cursor position and gesture information
        static NamedPipeClient cursorAndGestureInfo = null;

        #endregion Inter-Process Communication Objects

        #region SURF Information

        // Cursor SURF information
        static double cursorThreshold   = 0.0002;
        static int cursorOctaves        = 5;
        static int cursorInitial        = 2;

        // Select SURF information
        static double selectThreshold   = 0.0002;
        static int selectOctaves        = 5;
        static int selectInitial        = 2;

        // Move SURF information
        static double moveThreshold     = 0.0002;
        static int moveOctaves          = 5;
        static int moveInitial          = 2;

        // Scale SURF information
        static double scaleThreshold    = 0.0002;
        static int scaleOctaves         = 5;
        static int scaleInitial         = 2;

        // Rotate SURF information
        static double rotateThreshold   = 0.0002;
        static int rotateOctaves        = 5;
        static int rotateInitial        = 2;

        // Open Menu SURF information
        static double openMenuThreshold = 0.0002;
        static int openMenuOctaves      = 5;
        static int openMenuInitial      = 2;

        #endregion SURF Information

        #region Data Loader
        
        static void ConfigurationLoader()
        {
            applicationPath             = AppDomain.CurrentDomain.BaseDirectory;
            startFromApplicationPath    = savedInformation.StartFromApplicationPath;
            startApplicationHidden      = savedInformation.StartApplicationHidden;
            if (startFromApplicationPath)
            {
                cursorGestureTypeImagesLocation     = applicationPath + savedInformation.CursorGestureType;
                selectGestureTypeImagesLocatuon     = applicationPath + savedInformation.SelectGestureType;
                moveGestureTypeImagesLocation       = applicationPath + savedInformation.MoveGestureType;
                scaleGestureTypeImagesLocation      = applicationPath + savedInformation.ScaleGestureType;
                rotateGestureTypeImagesLocation     = applicationPath + savedInformation.RotateGestureType;
                openMenuGestureTypeImagesLocation   = applicationPath + savedInformation.OpenMenuGestureType;
            }
            else
            {
                cursorGestureTypeImagesLocation     = savedInformation.CursorGestureType;
                selectGestureTypeImagesLocatuon     = savedInformation.SelectGestureType;
                moveGestureTypeImagesLocation       = savedInformation.MoveGestureType;
                scaleGestureTypeImagesLocation      = savedInformation.ScaleGestureType;
                rotateGestureTypeImagesLocation     = savedInformation.RotateGestureType;
                openMenuGestureTypeImagesLocation   = savedInformation.OpenMenuGestureType;
                
            }

            cursorThreshold     = savedInformation.CursorThreshold;
            cursorOctaves       = savedInformation.CursorOctaves;
            cursorInitial       = savedInformation.CursorInitial;

            selectThreshold     = savedInformation.SelectThreshold;
            selectOctaves       = savedInformation.SelectOctaves;
            selectInitial       = savedInformation.SelectInitial;

            moveThreshold       = savedInformation.MoveThreshold;
            moveOctaves         = savedInformation.MoveOctaves;
            moveInitial         = savedInformation.MoveInitial;

            scaleThreshold      = savedInformation.ScaleThreshold;
            scaleOctaves        = savedInformation.ScaleOctaves;
            scaleInitial        = savedInformation.ScaleInitial;

            rotateThreshold     = savedInformation.RotateThreshold;
            rotateOctaves       = savedInformation.RotateOctaves;
            rotateInitial       = savedInformation.RotateInitial;

            openMenuThreshold   = savedInformation.OpenMenuThreshold;
            openMenuOctaves     = savedInformation.OpenMenuOcaves;
            openMenuInitial     = savedInformation.OpenMenuInitial;
        }
        static void ConnectionChannelInfoLoader()
        {
            cursorAndGestureInfoChannel = savedInformation.NamedPipeCursorAndGestureInfo;
            imageNotifierChannel        = savedInformation.NamedPipeImageServer;
            mmfFileName                 = savedInformation.MemoryMappedFFileName;
        }
        static void CursorImagesLoader()
        {
            
            string[] cursorTemplate = Directory.GetFiles(cursorGestureTypeImagesLocation, "*.png");
            Program.cursorTemplate = new Image<Bgra, byte>[cursorTemplate.Length];
            for (int i = 0; i < cursorTemplate.Length; i++)
            {
                Program.cursorTemplate[i] = new Image<Bgra, byte>(new Bitmap(cursorTemplate[i]));
            }
        }
        static void SelectImagesLoader()
        {
            string[] selectTemplate = Directory.GetFiles(selectGestureTypeImagesLocatuon, "*.png");
            Program.selectTemplate = new Image<Bgra, byte>[selectTemplate.Length];
            for (int i = 0; i < selectTemplate.Length; i++)
            {
                Program.selectTemplate[i] = new Image<Bgra, byte>(new Bitmap(selectTemplate[i]));
            }
        }
        static void MoveImagesLoader()
        {
            string[] moveTemplate = Directory.GetFiles(moveGestureTypeImagesLocation, "*.png");
            Program.moveTemplate = new Image<Bgra, byte>[moveTemplate.Length];
            for (int i = 0; i < moveTemplate.Length; i++)
            {
                Program.moveTemplate[i] = new Image<Bgra, byte>(new Bitmap(moveTemplate[i]));
            }
        }
        static void ScaleImagesLoader()
        {
            string[] scaleTemplate = Directory.GetFiles(scaleGestureTypeImagesLocation, "*.png");
            Program.scaleTemplate = new Image<Bgra, byte>[scaleTemplate.Length];
            for (int i = 0; i < scaleTemplate.Length; i++)
            {
                Program.scaleTemplate[i] = new Image<Bgra, byte>(new Bitmap(scaleTemplate[i]));
            }
        }
        static void RotateImagesLoader()
        {
            string[] rotateTemplate = Directory.GetFiles(rotateGestureTypeImagesLocation, "*.png");
            Program.rotateTemplate = new Image<Bgra, byte>[rotateTemplate.Length];
            for (int i = 0; i < rotateTemplate.Length; i++)
            {
                Program.rotateTemplate[i] = new Image<Bgra, byte>(new Bitmap(rotateTemplate[i]));
            }
        }
        static void OpenMenuImagesLoader()
        {
            string[] CursorFile = Directory.GetFiles(openMenuGestureTypeImagesLocation, "*.png");
            for (int i = 0; i < CursorFile.Length; i++)
            {
                cursorTemplate[i] = new Image<Bgra, byte>(new Bitmap(CursorFile[i]));
            }
        }
        static void GestureTypeImagesLoader()
        {
            Console.WriteLine("Now loading \"Cursor\" image templates to memory...");
            CursorImagesLoader();
            Console.WriteLine(cursorTemplate.Length + " \"cursor\" templates loaded to memory...");
            Console.WriteLine("Now loading \"Select\" templates to memory... ");
            SelectImagesLoader();
            Console.WriteLine(selectTemplate.Length + " \"select\" templates loaded to memory...");
            Console.WriteLine("Now loading \"Move\" templates to memory... ");
            MoveImagesLoader();
            Console.WriteLine(moveTemplate.Length + " \"move\" templates loaded to memory...");
            Console.WriteLine("Now loading \"Scale\" templates to memory...");
            ScaleImagesLoader();
            Console.WriteLine(scaleTemplate.Length + " \"scale\" templates loaded to memory...");
            Console.WriteLine("Now loading \"Rotate\" templates to memory...");
            RotateImagesLoader();
            Console.WriteLine(rotateTemplate.Length + " \"rotate\" templates loaded to memory...");
            OpenMenuImagesLoader();
        }
        #endregion Data Loader

        #region Window mode
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        #endregion Window mode

        #region Other settings
        static char receivedCode;
        static bool isExitRequested = false;
        #endregion Other settings

        #endregion Application informations

        static void Main(string[] args)
        {
            #region Applications data configuration loader
            Console.WriteLine("Reading configuration...");
            ConfigurationLoader();
            ConnectionChannelInfoLoader();
            GestureTypeImagesLoader();
            #endregion Applications data configuration loader

            WindowMode();

            //Sample();

            MainOperation();

            //GestureRecognition();
            Console.ReadLine();
        }

        static void MainOperation()
        {
            // How this program works
            /* All process here is done in an infinity conditional loop.
             * 1. Create new named pipe server and wait for client to connect.
             * 2. If there is a client connected, then the image is saved and
             *    ready to be processed.
             * 3. After finished reading the saved image, start HandFinder()
             *    method and GestureRecognition() method together on separate
             *    thread.
             * 4. After finished processing the image, terminate the thread and
             *    send the result to Modeler Module to continue the process.
             * 5. Repeat the process
             */

            // Module's main process
            int totalFrameProcessed = 0;

            // Program's main loop
            do
            {
                if (totalFrameProcessed == 0)
                {
                    Console.WriteLine("Waiting for new connection...");
                }
                else
                {
                    Console.WriteLine("Waiting for next frame...");
                }

                Stopwatch kursor3DOverallPerformance = new Stopwatch();
                kursor3DOverallPerformance.Start();
                totalFrameProcessed++;

                #region Image notification receiver and loader
                try
                {
                    ImageNotifier();

                    //If Exit is requested
                    if (receivedCode == 'x')
                    {
                        isExitRequested = true;
                        break;
                    }

                    // New image just notified
                    if (receivedCode == 'y')
                    {
                        // Loading preparations
                        byte[] file = null;

                        // Load image from Memory-mapped file.
                        MMF mappedFile = new MMF();
                        mappedFile.OpenExisting(mmfFileName);
                        file = Convert.FromBase64String(mappedFile.ReadContent(MMF.DataType.DataString));

                        // Set to bitmap
                        using (var ms = new MemoryStream(file))
                        {
                            receivedImage = new Bitmap(ms);
                        }
                    }
                    
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                }
                
                #endregion Image notification receiver and loader

                if (!isExitRequested)
                {
                    // Checking methods start here

                    Console.WriteLine("Main thread");

                    // Start new HandFinder thread
                    Thread findHand = new Thread(HandFinder);
                    findHand.Name = "Hand Finder Thread";
                    findHand.IsBackground = true;
                    findHand.Start();

                    // Start new FindDepth thread
                    Thread findDepth = new Thread(FindDepth);
                    findDepth.Start();

                    // Start new GestureRecognition thread
                    Thread gestureRecognition = new Thread(GestureRecognition);
                    gestureRecognition.Start();

                    findHand.Join();
                    findDepth.Join();
                    gestureRecognition.Join();
                    kursor3DOverallPerformance.Stop();
                    string TempCursorInfo = cursorPosition + "|" + handDepth.ToString() + "|" + gestureType + "|" + kursor3DOverallPerformance.ElapsedMilliseconds + "|" + handFinderPerformance + "|" + findDepthPerformance + "|" + gestureRecognitionPerformance;

                    // Notify Modeler module that the process has completed.
                    SendResult(cursorAndGestureInfoChannel, TempCursorInfo);
                }
            } while (true);
        }

        static void WindowMode()
        {
            // Hide/Show console based on configuration
            var handle = GetConsoleWindow();
            if (startApplicationHidden)
                ShowWindow(handle, SW_HIDE);
            else
                ShowWindow(handle, SW_SHOW);
        }
        
        static void ImageNotifier()
        {
            try
            {
                imageServer = null;
                imageServer = new NamedPipesServer();
                imageServer.CreateNewServerPipe(imageNotifierChannel, NamedPipesServer.PipeDirection.DirectionInOut, NamedPipesServer.SendMode.ByteMode);
                imageServer.WaitForConnection();
                receivedCode = (char)imageServer.ReadByte();
                // Close connection
                imageServer.Disconnect();
                imageServer.ClosePipe();
            }
            catch (Exception err)
            {
                throw;
            }
        }

        static void SendResult(string PipeName, string Content)
        {
            try
            {
                cursorAndGestureInfo = new NamedPipeClient(PipeName);
                if (!cursorAndGestureInfo.CheckConnection())
                {
                    cursorAndGestureInfo.ConnectToServer();
                }
                byte[] tempLocation = new byte[Content.Length];
                int i = 0;
                foreach (char character in Content)
                {
                    tempLocation[i] = (byte)character;
                    i++;
                }
                cursorAndGestureInfo.WriteToServer(tempLocation, 0, tempLocation.Length);
                cursorAndGestureInfo.DisconnectToServer();
            }
            catch (Exception err)
            {
                throw;
            }
            
        }
        
        #region Gesture informations
        Bitmap handFinderImageSource = null;
        static string cursorPosition { set; get; }
        static int currentNumber = 0;
        static long handFinderPerformance = 0;
        static long gestureScore = 0;
        static void HandFinder()
        {
            Stopwatch handFinderPerformanceWatcher = new Stopwatch();
            handFinderPerformanceWatcher.Start();
            Console.WriteLine("HandFinder() thread started");
            cursorPosition = currentNumber.ToString() + "|"+ (currentNumber + 3).ToString();
            currentNumber++;

            /// TODO: Implement Template matching


            handFinderPerformanceWatcher.Stop();
            handFinderPerformance = handFinderPerformanceWatcher.ElapsedMilliseconds;
            
        }

        static double handDepth { set; get; }
        static Bitmap findDepthpreviousFrame = null;
        static long findDepthPerformance = 0;
        static void FindDepth()
        {
            Stopwatch findDepthPerformanceWatcher = new Stopwatch();
            findDepthPerformanceWatcher.Start();

            Console.WriteLine("FindDepth() thread started");
            /* How this method works
            *      Method 1: Stereo depth sensing
            *          1. Read bitmap and previous frame
            *          2. If there is no previous frame, then just pass
            *             current frame to previous frame.
            *          3. If there is a previous frame, then check using
            *             stereo method depth sensing.
            *      Method 2 (Under consideration): Monocular depth sensing
            *          1. Check for detected hand rectangle width and height
            *          2. Calculate the difference between the previous frame's rectangle
            *             and the current frame's found rectangle
            * 
            */
            handDepth = 0;
            /// TODO: Implement the simplest depth sensing method
            findDepthPerformanceWatcher.Stop();
            findDepthPerformance = findDepthPerformanceWatcher.ElapsedMilliseconds;
        }

        static bool isGestureImageAvailable { set; get; }
        static string gestureType { set; get; }
        static Bitmap gestureRecognitionSourceImage = null;
        static Bitmap gestureRecognitionPreviousFrame = null;

        static Image<Bgra, byte>[] cursorTemplate = null;
        static Image<Bgra, byte>[] selectTemplate = null;
        static Image<Bgra, byte>[] moveTemplate = null;
        static Image<Bgra, byte>[] rotateTemplate = null;
        static Image<Bgra, byte>[] scaleTemplate = null;
        static Image<Bgra, byte>[] openMenuTemplate = null;
        static long gestureRecognitionPerformance = 0;

        static Stopwatch gestureRecognitionPerformanceWatcher = new Stopwatch();
        #endregion Gesture informations
        static void GestureRecognition()
        {
            
            gestureRecognitionPerformanceWatcher.Reset();
            gestureRecognitionPerformanceWatcher.Start();

            Console.WriteLine("GestureRecocnition() thread started");
            /* How this method works
            *  1. The same as HandFinder() method
            * 
            */

            /* Gesture types
             * 1. Cursor
             * 2. Select
             * 3. Move
             * 4. Rotate
             * 5. Scale
             * 6. Open menu
             * 7. Close (Under consideration)
             * 8. Submenu select (Planing)
             */
            Thread cursorRecognitionThread = new Thread(CursorThread);
            cursorRecognitionThread.Start();
            gestureType = "Cursor";
            
            gestureRecognitionPerformance = gestureRecognitionPerformanceWatcher.ElapsedMilliseconds;


        }
        
        static SURFFeatureClass SURFGestureRecognition;

        static void CursorThread()
        {
            
            SURFGestureRecognition = new SURFFeatureClass();
            Mat modelImage = new Mat();

            CvInvoke.CvtColor(cursorTemplate[0].Mat, modelImage, ColorConversion.Bgra2Gray);
            
            long    matchTime;
            double  threshold = 0.0002;
            int     octaves = 5;
            int     initial = 2;

            //SpeededUpRobustFeaturesDetector cursorDetector = new SpeededUpRobustFeaturesDetector(threshold, octaves, initial);
            int number = 0;
            //SURFAccord();
            
        }

        static void SURFAccord()
        {
            IntPoint[] correlation1;
            IntPoint[] correlation2;
            resultCursorGesture = cursorReceivedImage;
            SpeededUpRobustFeaturesDetector surf = new SpeededUpRobustFeaturesDetector(cursorThreshold, cursorOctaves, cursorInitial);
            IEnumerable<SpeededUpRobustFeaturePoint> observedPoints = surf.Transform(cursorReceivedImage.Bitmap);
            IEnumerable<SpeededUpRobustFeaturePoint> modelPoints = surf.Transform(cursorTemplate[0].Bitmap);

            KNearestNeighborMatching matcher = new KNearestNeighborMatching(20);
            IntPoint[][] matches = matcher.Match(observedPoints, modelPoints);

            correlation1 = matches[0];
            correlation2 = matches[1];

            PairsMarker pairsMarker = new PairsMarker(correlation1, correlation2);
        }

        static ImageViewer viewer;
        
        static void Sample()
        {
            int totalProcessedFrame = 0;
            viewer = new ImageViewer();
            viewer.Text = "test 1";
            
            viewer.Image = resultCursorGesture;
            VideoCapture capture = new VideoCapture(CaptureType.Any);
            Application.Idle += new EventHandler(delegate (object sender, EventArgs e)
            {
                cursorReceivedImage = capture.QuerySmallFrame();
                
                resultCursorGesture = ImgRecognitionEmGu.DrawMatches.Draw(cursorTemplate[0].Mat, cursorReceivedImage, out gestureRecognitionPerformance, out gestureScore);
                viewer.Image = resultCursorGesture;
                viewer.Text = gestureScore.ToString();
            });
            viewer.ShowDialog();
        }
    }
}
