/***************[ AR 3D Modeler - Cursor 3D Module ]***************
 * Application version: 1.03
 * 
 * 
 * This program is one of five modules for a current project.
 * The program was intentionally split into different application
 * for easier debuging, testing, updating, and replacing module
 * whitout having to rebuilding all modules (Actually because i
 * haven't learn about multi-threading that time).
 * 
 * There's currently a problem with reading data while debugging
 * where the program will give an error with file within "Source"
 * folder even if the folder is there. The program need to build
 * then run without the IDE.
 * 
 * EDIT: I forgot that i have created a setting to let the
 *       application load all the files manu manually (using full
 *       path) and automatic (using dynamic path from executable
 *       running path). If you found error with folder loading,
 *       set "StartFromApplicationPath" to "False" and write
 *       the full path of every cursor template.
 *       
 * What is the application supposed to do?
 * 1. First, it compute all templates (for performance reason)
 * 2. Then, it will ask for input to work with
 * 3. The application will use "convex hull" detection with
 *    skin color (i forgot what it called) to remove background
 * 4. The application will use the "skin" (to use as source
 *    image for the SURF class)
 * 5. The SURF class then use the input image to find what
 *    gesture is the user is currently doing
 * 
 * Note:
 * 1. Refer to the "/Informations/References.txt" for
 *    details.
 * 2. Current implementation will be performance and resource
 *    hungry because of many recalculations and redundancies.
 * 3. The source file size will be huge because it contains
 *    not also working codes, but also backup codes and sample
 *    codes.
 * 
 * All the source is freely to use (at least for now) without
 * having to tell me first (well, i also get to write the source 
 * for free too).
 */

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
//using Emgu.CV.XFeatures2D;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.UI;
using System.Windows.Forms;
using Accord;
using Accord.Imaging.Filters;
using System.Drawing.Imaging;
using System.IO.Pipes;
using HandGestureRecognition.SkinDetector;



namespace Kursor3D_Kursor3DModule
{
    class Program
    {
        #region Application informations
        
        #region Config file information
        static Configuration savedInformation   = new Configuration();
        static Configuration locations          = new Configuration();
        #endregion Config file information

        #region Application start and load modes
        static bool     startFromApplicationPath;
        static string   applicationPath;
        static bool     startApplicationHidden;
        #endregion Application start and load modes

        #region Received images
        // Received images (to prevent race condition)
        static Image<Bgr, byte> cursorReceivedImage   = null;
        static Image<Bgr, byte> selectReceivedImage   = null;
        static Image<Bgr, byte> moveReceivedImage     = null;
        static Image<Bgr, byte> scaleReceivedImage    = null;
        static Image<Bgr, byte> rotateReceivedImage   = null;

        static Bitmap receivedImage;
        #endregion Received images

        #region Processed images
        // Processed images
        static Image<Bgr, byte> processedImage = null;
        static string convertedImage = string.Empty;
        #endregion Processed images

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

        #region Gesture informations

        #region Gesture templates location
        static string cursorGestureTypeImagesLocation = string.Empty;
        static string selectGestureTypeImagesLocation = string.Empty;
        static string moveGestureTypeImagesLocation = string.Empty;
        static string scaleGestureTypeImagesLocation = string.Empty;
        static string rotateGestureTypeImagesLocation = string.Empty;
        static string openMenuGestureTypeImagesLocation = string.Empty;
        #endregion Gesture templates location

        #region Gesture found informations
        static bool isHandFound = false;
        static bool isCursorGestureDetected = false;
        static bool isSelectGestureDetected = false;
        static bool isMoveGestureDetected = false;
        static bool isScaleGestureDetected = false;
        static bool isRotateGestureDetected = false;
        static bool isOpenMenuGestureDetected = false;
        #endregion Gesture found informations

        #region Gesture images

        #region Gestures source image
        // Gesture processed image
        static Image<Bgr, byte> loadedCursorGestureImage = null;
        static Image<Bgr, byte> loadedSelectGestureImage = null;
        static Image<Bgr, byte> loadedMoveGestureImage = null;
        static Image<Bgr, byte> loadedScaleGestureImage = null;
        static Image<Bgr, byte> loadedRotateGestureImage = null;
        static Image<Bgr, byte> loadedOpenMenuGestureImage = null;
        #endregion Gestures source image

        #region Gesture processed image
        // Gesture processed image
        static Image<Gray, byte> resultCursorGesture = null;
        static Image<Gray, byte> resultSelectGesture = null;
        static Image<Gray, byte> resultMoveGesture = null;
        static Image<Gray, byte> resultScaleGesture = null;
        static Image<Gray, byte> resultRotateGesture = null;
        static Image<Gray, byte> resultOpenMenuGesture = null;
        #endregion Gesture processed image

        #endregion Gesture images

        #region Other gesture informations
        Bitmap handFinderImageSource = null;
        static string cursorPosition { set; get; }
        static int currentNumber = 0;
        static long gestureScore = 0;

        static double handDepth { set; get; }
        static Bitmap findDepthpreviousFrame = null;

        static bool isGestureImageAvailable { set; get; }
        static string gestureType { set; get; }
        static Bitmap gestureRecognitionSourceImage = null;
        static Bitmap gestureRecognitionPreviousFrame = null;

        static HandGestureRecognition.GestureRecognitionClass handFinder = new HandGestureRecognition.GestureRecognitionClass();
        #endregion Other gesture informations

        #region Gesture templates
        static Image<Bgr, byte>[] cursorTemplate = null;
        static Image<Bgr, byte>[] selectTemplate = null;
        static Image<Bgr, byte>[] moveTemplate = null;
        static Image<Bgr, byte>[] rotateTemplate = null;
        static Image<Bgr, byte>[] scaleTemplate = null;
        static Image<Bgr, byte>[] openMenuTemplate = null;
        #endregion Gesture tmeplates

        #region Gestures background remover objects
        static HandGestureRecognition.GestureRecognitionClass cursorBackgroundRemover = new HandGestureRecognition.GestureRecognitionClass();
        static HandGestureRecognition.GestureRecognitionClass selectBackgroundRemover = new HandGestureRecognition.GestureRecognitionClass();
        static HandGestureRecognition.GestureRecognitionClass moveBackgroundRemover = new HandGestureRecognition.GestureRecognitionClass();
        static HandGestureRecognition.GestureRecognitionClass rotateBackgroundRemover = new HandGestureRecognition.GestureRecognitionClass();
        static HandGestureRecognition.GestureRecognitionClass scaleBackgroundRemover = new HandGestureRecognition.GestureRecognitionClass();
        static HandGestureRecognition.GestureRecognitionClass openMenuBackgroundRemover = new HandGestureRecognition.GestureRecognitionClass();
        #endregion Gestures background remover objects

        #region Temporary gestures score location
        static int highestScore = 0;
        
        static int highestGestureID = 0; // 1 = Cursor
                                         // 2 = Select
                                         // 3 = Move
                                         // 4 = Rotate
                                         // 5 = Scale
                                         // 6 = OpenMenu
        #endregion Temporary gestures score information

        #endregion Gesture informations

        #region Performance informations
        #region Performnance data
        static long kursor3DOverallPerformance          = 0;
        static long gestureRecognitionPerformance       = 0;
        static long handFinderPerformance = 0;
        static long findDepthPerformance = 0;
        static long HUBModuleReceivingPerformance       = 0;
        static long ModelerModuleSendingPerformance     = 0;
        static long imageConversionPerformance          = 0;
        
        #endregion Performance data
        #region Performance watcher
        static Stopwatch kursor3DOverallPerformanceWatcher      = new Stopwatch();
        static Stopwatch gestureRecognitionPerformanceWatcher   = new Stopwatch();
        static Stopwatch HUBModuleReceivingPerformanceWatcher   = new Stopwatch();
        static Stopwatch ModelerModuleSendingPerformanceWatcher = new Stopwatch();
        static Stopwatch imageConversionPerformanceWatcher      = new Stopwatch();
        #endregion Performance watcher
        #endregion Performance informantions

        #region Convex Hull settings
        static Hsv hsvMin;
        static Hsv hsvMax;
        static Ycc YCrCbMin;
        static Ycc YCrCbMax;
        static Dictionary<string, object> convexHullSettings;
        #endregion Convex Hull settings

        #region Window mode
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        #endregion Window mode

        #region Thread informations
        #region Thread image processing informations
        #region Loaded images
        static bool isImageLoaded = false;
        static bool isCursorGestureSourceImageLoaded = false;
        static bool isSelectGestureSourceImageLoaded = false;
        static bool isMoveGestureSourceImageLoaded = false;
        static bool isScaleGestureSourceImageLoaded = false;
        static bool isRotateGestureSourceImageLoaded = false;
        static bool isOpenMenuGestureSourceImageLoaded = false;
        static bool isHandFinderSourceImageLoaded = false;
        #endregion Loaded images
        #region Processed images
        static bool isCursorGestureSourceImageProcessed = false;
        static bool isSelectGestureSourceImageProcessed = true;
        static bool isMoveGestureSourceImageProcessed = true;
        static bool isScaleGestureSourceImageProcessed = true;
        static bool isRotateGestureSourceImageProcessed = true;
        static bool isOpenMenuGestureSourceImageProcessed = true;
        #endregion Processed images
        #endregion Thread image processing informations
        #region Thread data
        static bool isCursorRecognitionThreadStarted = false;
        static bool isSelectRecognitionThreadStarted = false;
        static bool isMoveRecognitionThreadStarted = false;
        static bool isScaleRecognitionThreadStarted = false;
        static bool isRotateRecognitionThreadSrarted = false;
        static bool isOpenMenuRecognitionThreadStarted = false;
        static bool isHandFinderThreadStarted = false;
        #endregion Thread data
        #region Thread objects
        static Thread cursorRecognitionThread;
        static Thread selectRecognitionThread;
        static Thread moveRecognitionThread;
        static Thread scaleRecognitionThread;
        static Thread rotateRecognitionThread;
        static Thread openMenuRecognitionThread;

        static Thread gestureRecognition;

        // Start new HandFinder thread
        static Thread findHand = new Thread(HandFinder);
        
        // Start new FindDepth thread
        static Thread findDepth = new Thread(FindDepth);
        
        // Start new GestureRecognition thread
        
        
        #endregion Thread objects
        #endregion Thread informations

        #region Other settings
        static int totalFrameProcessed = 0;
        static char receivedCode;
        static bool isExitRequested = false;
        static string TempCursorInfo = string.Empty;
        static bool isDebugging = false;
        static bool UseSampleFunction = false;
        static string SampleImageLocation = string.Empty;
        static string SampleImageFile = string.Empty;
        static EmguCVSURFClass recognitionProcessor = new EmguCVSURFClass();
        #endregion Other settings

        #endregion Application informatiWons

        #region Data Loader

        static void ConfigurationLoader()
        {
            applicationPath = AppDomain.CurrentDomain.BaseDirectory;
            startFromApplicationPath = savedInformation.StartFromApplicationPath;
            startApplicationHidden = savedInformation.StartApplicationHidden;
            if (startFromApplicationPath)
            {
                cursorGestureTypeImagesLocation = applicationPath + savedInformation.CursorGestureType;
                selectGestureTypeImagesLocation = applicationPath + savedInformation.SelectGestureType;
                moveGestureTypeImagesLocation = applicationPath + savedInformation.MoveGestureType;
                scaleGestureTypeImagesLocation = applicationPath + savedInformation.ScaleGestureType;
                rotateGestureTypeImagesLocation = applicationPath + savedInformation.RotateGestureType;
                openMenuGestureTypeImagesLocation = applicationPath + savedInformation.OpenMenuGestureType;
            }
            else
            {
                cursorGestureTypeImagesLocation = savedInformation.CursorGestureType;
                selectGestureTypeImagesLocation = savedInformation.SelectGestureType;
                moveGestureTypeImagesLocation = savedInformation.MoveGestureType;
                scaleGestureTypeImagesLocation = savedInformation.ScaleGestureType;
                rotateGestureTypeImagesLocation = savedInformation.RotateGestureType;
                openMenuGestureTypeImagesLocation = savedInformation.OpenMenuGestureType;

            }

            cursorThreshold = savedInformation.CursorThreshold;
            cursorOctaves = savedInformation.CursorOctaves;
            cursorInitial = savedInformation.CursorInitial;

            selectThreshold = savedInformation.SelectThreshold;
            selectOctaves = savedInformation.SelectOctaves;
            selectInitial = savedInformation.SelectInitial;

            moveThreshold = savedInformation.MoveThreshold;
            moveOctaves = savedInformation.MoveOctaves;
            moveInitial = savedInformation.MoveInitial;

            scaleThreshold = savedInformation.ScaleThreshold;
            scaleOctaves = savedInformation.ScaleOctaves;
            scaleInitial = savedInformation.ScaleInitial;

            rotateThreshold = savedInformation.RotateThreshold;
            rotateOctaves = savedInformation.RotateOctaves;
            rotateInitial = savedInformation.RotateInitial;

            openMenuThreshold = savedInformation.OpenMenuThreshold;
            openMenuOctaves = savedInformation.OpenMenuOcaves;
            openMenuInitial = savedInformation.OpenMenuInitial;

            isDebugging = savedInformation.Debug;
            UseSampleFunction = savedInformation.UseSampleFunction;
            SampleImageLocation = savedInformation.SampleImageLocation;
            SampleImageFile = savedInformation.SampleImageFile;
        }
        static void ConnectionChannelInfoLoader()
        {
            cursorAndGestureInfoChannel = savedInformation.NamedPipeCursorAndGestureInfo;
            imageNotifierChannel = savedInformation.NamedPipeImageServer;
            mmfFileName = savedInformation.MemoryMappedFFileName;
        }
        static void CursorImagesLoader()
        {
            try
            {
                string[] cursorTemplate = Directory.GetFiles(cursorGestureTypeImagesLocation, "*.png");
                recognitionProcessor.cursorTemplatesGrayscaled = new Image<Gray, byte>[cursorTemplate.Length];
                recognitionProcessor.cursorTemplates = new Image<Bgr, byte>[cursorTemplate.Length];
                for (int i = 0; i < cursorTemplate.Length; i++)
                {
                    recognitionProcessor.cursorTemplatesGrayscaled[i] = new Image<Gray, byte>(cursorTemplate[i]);
                    recognitionProcessor.cursorTemplates[i] = new Image<Bgr, byte>(cursorTemplate[i]);

                }
            }
            catch (Exception err)
            {
                DefaultErrorWriter(err);
            }

        }
        static void SelectImagesLoader()
        {
            string[] selectTemplate = Directory.GetFiles(selectGestureTypeImagesLocation, "*.png");
            recognitionProcessor.selectTemplatesGrayscaled = new Image<Gray, byte>[selectTemplate.Length];
            recognitionProcessor.selectTemplates = new Image<Bgr, byte>[selectTemplate.Length];
            for (int i = 0; i < selectTemplate.Length; i++)
            {
                recognitionProcessor.selectTemplatesGrayscaled[i] = new Image<Gray, byte>(selectTemplate[i]);
                recognitionProcessor.selectTemplates[i] = new Image<Bgr, byte>(selectTemplate[i]);
            }
        }
        static void MoveImagesLoader()
        {
            string[] moveTemplate = Directory.GetFiles(moveGestureTypeImagesLocation, "*.png");
            recognitionProcessor.moveTemplatesGreyscaled = new Image<Gray, byte>[moveTemplate.Length];
            recognitionProcessor.moveTemplates = new Image<Bgr, byte>[moveTemplate.Length];
            for (int i = 0; i < moveTemplate.Length; i++)
            {
                recognitionProcessor.moveTemplatesGreyscaled[i] = new Image<Gray, byte>(moveTemplate[i]);
                recognitionProcessor.moveTemplates[i] = new Image<Bgr, byte>(moveTemplate[i]);
            }
        }
        static void ScaleImagesLoader()
        {
            string[] scaleTemplate = Directory.GetFiles(scaleGestureTypeImagesLocation, "*.png");
            recognitionProcessor.scaleTemplatesGreyscaled = new Image<Gray, byte>[scaleTemplate.Length];
            recognitionProcessor.scaleTemplates = new Image<Bgr, byte>[scaleTemplate.Length];
            for (int i = 0; i < scaleTemplate.Length; i++)
            {
                recognitionProcessor.scaleTemplatesGreyscaled[i] = new Image<Gray, byte>(scaleTemplate[i]);
                recognitionProcessor.scaleTemplates[i] = new Image<Bgr, byte>(scaleTemplate[i]);
            }
        }
        static void RotateImagesLoader()
        {
            string[] rotateTemplate = Directory.GetFiles(rotateGestureTypeImagesLocation, "*.png");
            recognitionProcessor.rotateTemplatesGeryscaled = new Image<Gray, byte>[rotateTemplate.Length];
            recognitionProcessor.rotateTemplates = new Image<Bgr, byte>[rotateTemplate.Length];
            for (int i = 0; i < rotateTemplate.Length; i++)
            {
                recognitionProcessor.rotateTemplatesGeryscaled[i] = new Image<Gray, byte>(rotateTemplate[i]);
                recognitionProcessor.rotateTemplates[i] = new Image<Bgr, byte>(rotateTemplate[i]);
            }
        }
        static void OpenMenuImagesLoader()
        {
            string[] openMenuFiles = Directory.GetFiles(openMenuGestureTypeImagesLocation, "*.png");
            recognitionProcessor.openMenuTemplatesGreyscaled = new Image<Gray, byte>[openMenuFiles.Length];
            recognitionProcessor.openmenuTemplates = new Image<Bgr, byte>[openMenuFiles.Length];
            for (int i = 0; i < openMenuFiles.Length; i++)
            {
                recognitionProcessor.openMenuTemplatesGreyscaled[i] = new Image<Gray, byte>(openMenuFiles[i]);
                recognitionProcessor.openmenuTemplates[i] = new Image<Bgr, byte>(openMenuFiles[i]);
            }
        }
        static void GestureTypeImagesLoader()
        {
            Console.WriteLine("Now loading \"Cursor\" image templates to memory...");
            CursorImagesLoader();
            Console.WriteLine(recognitionProcessor.cursorTemplatesGrayscaled.Length + " \"cursor\" templates loaded to memory...");
            Console.WriteLine("Now loading \"Select\" templates to memory... ");
            SelectImagesLoader();
            Console.WriteLine(recognitionProcessor.selectTemplatesGrayscaled.Length + " \"select\" templates loaded to memory...");
            Console.WriteLine("Now loading \"Move\" templates to memory... ");
            MoveImagesLoader();
            Console.WriteLine(recognitionProcessor.moveTemplatesGreyscaled.Length + " \"move\" templates loaded to memory...");
            Console.WriteLine("Now loading \"Scale\" templates to memory...");
            ScaleImagesLoader();
            Console.WriteLine(recognitionProcessor.scaleTemplatesGreyscaled.Length + " \"scale\" templates loaded to memory...");
            Console.WriteLine("Now loading \"Rotate\" templates to memory...");
            RotateImagesLoader();
            Console.WriteLine(recognitionProcessor.rotateTemplatesGeryscaled.Length + " \"rotate\" templates loaded to memory...");
            OpenMenuImagesLoader();
        }
        static void ConvexHullSettingsLoader()
        {
            hsvMin = new Hsv(savedInformation.MinHue, savedInformation.MinSaturation, savedInformation.MinValue);
            hsvMax = new Hsv(savedInformation.MaxHue, savedInformation.MaxSaturation, savedInformation.MaxValue);
            YCrCbMin = new Ycc(savedInformation.YCrCb_LumaMin, savedInformation.YCrCb_RedMinusLumaMin, savedInformation.YCrCb_BlueMinusLumaMin);
            YCrCbMax = new Ycc(savedInformation.YCrCb_LumaMax, savedInformation.YCrCb_RedMinusLumaMax, savedInformation.YCrCb_BlueMinusLumaMax);
            convexHullSettings = new Dictionary<string, object>();
            convexHullSettings.Add("hsv_min", hsvMin);
            convexHullSettings.Add("hsvMax", hsvMax);
            convexHullSettings.Add("YCrCb_min", YCrCbMin);
            convexHullSettings.Add("YCrCb_max", YCrCbMax);
        }
        #endregion Data Loader

        static void Main(string[] args)
        {
            #region Application data configurations loader
            Console.WriteLine("Reading configuration...");
            ConfigurationLoader();
            ConnectionChannelInfoLoader();
            GestureTypeImagesLoader();
            ConvexHullSettingsLoader();
            #endregion Application data configurations loader

            Console.WriteLine("Processing templates...");
            Console.WriteLine("Removing background...");
            PrecomputeGestureTemplates();
            Console.WriteLine("Background removed.");
            Console.WriteLine("Processing template descriptors...");
            recognitionProcessor.ComputeDescriptors();
            Console.WriteLine("Starting threads");
            ThreadStarter();
            Console.WriteLine("Thread started");
            Console.WriteLine("Processing...");

            WindowMode();
            if (UseSampleFunction)
            {
                Sample();
            }
            else
            {
                MainOperation();
            } 

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
             

            // Program's main loop
            do
            {

                #region Overall performance watcher
                kursor3DOverallPerformanceWatcher.Reset();
                kursor3DOverallPerformanceWatcher.Start();
                #endregion Overall performance watcher

                #region Image notification receiver and loader
                try
                {
                    if (totalFrameProcessed == 0)
                    {
                        Console.WriteLine("Waiting for new connection...");
                    }
                    else
                    {
                        Console.WriteLine("Waiting for next frame...");
                    }
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
                        ImageLoader();
                    }
                }
                catch (Exception err)
                {
                    DefaultErrorWriter(err);
                }

                #endregion Image notification receiver and loader

                if (!isExitRequested)
                {
                    // Checking methods start here
                    Console.WriteLine("Main thread");

                    GestureRecognition();

                    //while (true)
                    //{
                    //    if (isHandFound)
                    //    {
                    //        break;
                    //    }
                    //}


                    kursor3DOverallPerformanceWatcher.Stop();
                    kursor3DOverallPerformance = kursor3DOverallPerformanceWatcher.ElapsedMilliseconds;
                    DataConstruction();
                    SendResult(cursorAndGestureInfoChannel, TempCursorInfo); // Notify Modeler module that the process has completed.
                    totalFrameProcessed++; // Total of processed frame
                }
            } while (true);
        }

        static void ThreadStarter()
        {
            findHand.Start();
            //gestureRecognition = new Thread(GestureRecognition);
            //gestureRecognition.Start();
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

        static void DataConstruction()
        {
            cursorPosition = "0" + "|" + "3";
            if (isDebugging)
            {
                using (var ms = new MemoryStream())
                {
                    using (Bitmap tempProcessedImage = new Bitmap(recognitionProcessor.cursorSourceSkin.ToBitmap()))
                    {
                        tempProcessedImage.Save(ms, ImageFormat.Png);
                    }
                    convertedImage = Convert.ToBase64String(ms.ToArray());
                }
            }

            TempCursorInfo = cursorPosition + "|" + handDepth.ToString() + "|" + gestureType + "|" + kursor3DOverallPerformance + "|" + handFinderPerformance + "|" + findDepthPerformance + "|" + gestureRecognitionPerformance;
            if (isDebugging)
            {
                TempCursorInfo += "|" + convertedImage;
            }
        }

        #region Memory-maped file image loader
        static void ImageLoader()
        {
            // Loading preparations
            byte[] file = null;

            // Load image from Memory-mapped file.
            MMF mappedFile = new MMF();
            mappedFile.OpenExisting(mmfFileName);
            file = Convert.FromBase64String(mappedFile.ReadContent(MMF.DataType.DataString));

            // Set to bitmap
            receivedImage = new Bitmap(new MemoryStream(file));
            isImageLoaded = true;
        }
        #endregion Memory-mapped file image loader

        static void ImageNotifier()
        {
            try
            {
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(imageNotifierChannel, PipeDirection.In))
                {
                    Console.WriteLine(pipeServer.IsConnected);
                    pipeServer.WaitForConnection();
                    try
                    {
                        using (StreamReader sr = new StreamReader(pipeServer))
                        {
                            string temp;
                            while ((temp = sr.ReadLine()) != null)
                            {
                                receivedCode = Convert.ToChar(temp);
                            }
                        }
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("ERROR: {0}", e.Message);
                    }
                }
                #region Old named pipe server notifier
                //imageServer = null;
                //imageServer = new NamedPipesServer();
                //imageServer.CreateNewServerPipe(imageNotifierChannel, NamedPipesServer.PipeDirection.DirectionInOut, NamedPipesServer.SendMode.ByteMode);
                //imageServer.WaitForConnection();
                //receivedCode = (char)imageServer.ReadByte();
                //// Close connection
                //imageServer.Disconnect();
                //imageServer.ClosePipe();
                #endregion Old named pipe server notifier
            }
            catch (Exception err)
            {
                DefaultErrorWriter(err);
            }
        }

        static void SendResult(string PipeName, string Content)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
            {
                pipeClient.Connect();
                using (StreamWriter sw = new StreamWriter(pipeClient))
                {
                    sw.AutoFlush = true;
                    sw.WriteLine(Content);
                }
            }

            #region Named pipe client notifier
            //try
            //{
            //    cursorAndGestureInfo = new NamedPipeClient(PipeName);
            //    if (!cursorAndGestureInfo.CheckConnection())
            //    {
            //        cursorAndGestureInfo.ConnectToServer();
            //    }
            //    byte[] tempLocation = new byte[Content.Length];
            //    int i = 0;
            //    foreach (char character in Content)
            //    {
            //        tempLocation[i] = (byte)character;
            //        i++;
            //    }
            //    cursorAndGestureInfo.WriteToServer(tempLocation, 0, tempLocation.Length);
            //    cursorAndGestureInfo.DisconnectToServer();
            //    cursorAndGestureInfo = null;
            //}
            //catch (Exception err)
            //{
            //    DefaultErrorWriter(err);
            //}
            #endregion Named pipe client notifier
        }

        #region Gesture functions
        #region Gesture precomputation
        static void PrecomputeGestureTemplates()
        {
            Console.WriteLine("Processing cursor templates...");
            PrecomputeCursorGestureTemplates();
            Console.WriteLine("Processing select templates...");
            PrecomputeSelectGestureTemplates();
            Console.WriteLine("Processing move templates...");
            PrecomputeMoveGestureTemplates();
            Console.WriteLine("Processing rotate templates...");
            PrecomputeRotateGestureTemplates();
            Console.WriteLine("Processing scale templates...");
            PrecomputeScaleGestureTemplates();
            Console.WriteLine("Processing open menu templates...");
            PrecomputeOpenMenuGestureTemplates();

        }
        static void PrecomputeCursorGestureTemplates()
        {
            // How it works
            // 1. Remove background using convex hull detection and skin detection
            // 2. Save the processed skins
            // 3. Compute with SURF
            // 4. Save the descriptors

            #region Background remover function
            recognitionProcessor.cursorSkins = new Image<Gray, byte>[recognitionProcessor.cursorTemplates.Length];
            for (int i = 0; i < recognitionProcessor.cursorTemplates.Length; i++)
            {
                // Set the image
                handFinder.receivedImage = recognitionProcessor.cursorTemplates[i];
                handFinder.isImageReceived = true;
                // Check for handFinder main process
                if (!handFinder.isMainProcessStarted)
                {
                    handFinder.StartMainProcess(handFinder.receivedImage.Width, handFinder.receivedImage.Height);
                }
                
                // Wait for result
                while (true)
                {
                    if (handFinder.isImageProcessed)
                    {
                        break;
                    }
                }
                handFinder.processedSkin.CopyTo(recognitionProcessor.cursorTemplatesGrayscaled[i]);
            }
            #endregion Background remover function

            
        }
        static void PrecomputeSelectGestureTemplates()
        {
            // How it works
            // 1. Remove background using convex hull detection and skin detection
            // 2. Save the processed skins
            // 3. Compute with SURF
            // 4. Save the descriptors

            #region Background remover function
            recognitionProcessor.selectSkins = new Image<Gray, byte>[recognitionProcessor.selectTemplates.Length];
            for (int i = 0; i < recognitionProcessor.selectTemplates.Length; i++)
            {
                // Set the image
                handFinder.receivedImage = recognitionProcessor.selectTemplates[i];
                handFinder.isImageReceived = true;
                // Check for handFinder main process
                if (!handFinder.isMainProcessStarted)
                {
                    handFinder.StartMainProcess(handFinder.receivedImage.Width, handFinder.receivedImage.Height);
                }

                // Wait for result
                while (true)
                {
                    if (handFinder.isImageProcessed)
                    {
                        break;
                    }
                }
                handFinder.processedSkin.CopyTo(recognitionProcessor.selectTemplatesGrayscaled[i]);
            }
            #endregion Background remover function

            // Descriptors computation will be handled by "ComputeDescriptors()"  
            // function at EmguCVSURFClass.
        }
        static void PrecomputeMoveGestureTemplates()
        {
            // How it works
            // 1. Remove background using convex hull detection and skin detection
            // 2. Save the processed skins
            // 3. Compute with SURF
            // 4. Save the descriptors

            #region Background remover function
            recognitionProcessor.moveSkins = new Image<Gray, byte>[recognitionProcessor.moveTemplates.Length];
            for (int i = 0; i < recognitionProcessor.moveTemplates.Length; i++)
            {
                // Set the image
                handFinder.receivedImage = recognitionProcessor.moveTemplates[i];
                handFinder.isImageReceived = true;
                // Check for handFinder main process
                if (!handFinder.isMainProcessStarted)
                {
                    handFinder.StartMainProcess(handFinder.receivedImage.Width, handFinder.receivedImage.Height);
                }

                // Wait for result
                while (true)
                {
                    if (handFinder.isImageProcessed)
                    {
                        break;
                    }
                }
                handFinder.processedSkin.CopyTo(recognitionProcessor.moveTemplatesGreyscaled[i]);
            }
            #endregion Background remover function

            // Descriptors computation will be handled by "ComputeDescriptors()"  
            // function at EmguCVSURFClass.
        }
        static void PrecomputeRotateGestureTemplates()
        {
            // How it works
            // 1. Remove background using convex hull detection and skin detection
            // 2. Save the processed skins
            // 3. Compute with SURF
            // 4. Save the descriptors

            #region Background remover function
            recognitionProcessor.rotateSkins = new Image<Gray, byte>[recognitionProcessor.rotateTemplates.Length];
            for (int i = 0; i < recognitionProcessor.rotateTemplates.Length; i++)
            {
                // Set the image
                handFinder.receivedImage = recognitionProcessor.rotateTemplates[i];
                handFinder.isImageReceived = true;
                // Check for handFinder main process
                if (!handFinder.isMainProcessStarted)
                {
                    handFinder.StartMainProcess(handFinder.receivedImage.Width, handFinder.receivedImage.Height);
                }

                // Wait for result
                while (true)
                {
                    if (handFinder.isImageProcessed)
                    {
                        break;
                    }
                }
                handFinder.processedSkin.CopyTo(recognitionProcessor.rotateTemplates[i]);
            }
            #endregion Background remover function

            // Descriptors computation will be handled by "ComputeDescriptors()"  
            // function at EmguCVSURFClass.
        }
        static void PrecomputeScaleGestureTemplates()
        {
            // How it works
            // 1. Remove background using convex hull detection and skin detection
            // 2. Save the processed skins
            // 3. Compute with SURF
            // 4. Save the descriptors

            #region Background remover function
            recognitionProcessor.scaleSkins = new Image<Gray, byte>[recognitionProcessor.scaleTemplates.Length];
            for (int i = 0; i < recognitionProcessor.scaleTemplates.Length; i++)
            {
                // Set the image
                handFinder.receivedImage = recognitionProcessor.scaleTemplates[i];
                handFinder.isImageReceived = true;
                // Check for handFinder main process
                if (!handFinder.isMainProcessStarted)
                {
                    handFinder.StartMainProcess(handFinder.receivedImage.Width, handFinder.receivedImage.Height);
                }

                // Wait for result
                while (true)
                {
                    if (handFinder.isImageProcessed)
                    {
                        break;
                    }
                }
                handFinder.processedSkin.CopyTo(recognitionProcessor.scaleTemplatesGreyscaled[i]);
            }
            #endregion Background remover function

            // Descriptors computation will be handled by "ComputeDescriptors()"  
            // function at EmguCVSURFClass.
        }
        static void PrecomputeOpenMenuGestureTemplates()
        {
            // How it works
            // 1. Remove background using convex hull detection and skin detection
            // 2. Save the processed skins
            // 3. Compute with SURF
            // 4. Save the descriptors

            #region Background remover function
            recognitionProcessor.openMenuSkins = new Image<Gray, byte>[recognitionProcessor.openmenuTemplates.Length];
            for (int i = 0; i < recognitionProcessor.openmenuTemplates.Length; i++)
            {
                // Set the image
                handFinder.receivedImage = recognitionProcessor.openmenuTemplates[i];
                handFinder.isImageReceived = true;
                // Check for handFinder main process
                if (!handFinder.isMainProcessStarted)
                {
                    handFinder.StartMainProcess(handFinder.receivedImage.Width, handFinder.receivedImage.Height);
                }

                // Wait for result
                while (true)
                {
                    if (handFinder.isImageProcessed)
                    {
                        break;
                    }
                }
                handFinder.processedSkin.CopyTo(recognitionProcessor.openMenuTemplatesGreyscaled[i]);
            }
            #endregion Background remover function

            // Descriptors computation will be handled by "ComputeDescriptors()"  
            // function at EmguCVSURFClass.
        }
        #endregion Gesture precomputation

        static void HandFinder()
        {
            while (true)
            {
                if (isHandFinderSourceImageLoaded)
                {
                    break;
                }
                Thread.Sleep(5);
            }

            Stopwatch handFinderPerformanceWatcher = new Stopwatch();
            handFinderPerformanceWatcher.Start();
            Console.WriteLine("HandFinder() thread started");
            cursorPosition = "0" + "|" + "3";
            currentNumber++;

            #region Convex hull detection

            /* How it works - Convex hull detection
             * 
             */

            handFinder.LoadSettings(convexHullSettings);
            if (!handFinder.isImageReceived)
            {
                handFinder.receivedImage = new Image<Bgr, byte>(receivedImage);
                handFinder.isImageReceived = true;
            }
            if (!handFinder.isMainProcessStarted)
            {
                handFinder.StartMainProcess(receivedImage.Width, receivedImage.Height);
            }
            while (true)
            {
                if (handFinder.isImageProcessed)
                {
                    break;
                }
                Thread.Sleep(5);
            }
            if (isDebugging)
            {
                processedImage = new Image<Bgr, byte>(handFinder.processedSkin.ToBitmap());
            }
            #endregion Convex hull detection
            /// TODO: Implement Template matching

            handFinderPerformanceWatcher.Stop();
            handFinderPerformance = handFinderPerformanceWatcher.ElapsedMilliseconds;

        }
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
        static void GestureRecognition()
        {
            while (true)
            {
                if (isImageLoaded)
                {
                    break;
                }
                Thread.Sleep(5);
            }
            gestureRecognitionPerformanceWatcher.Reset();
            gestureRecognitionPerformanceWatcher.Start();

            Console.WriteLine("GestureRecocnition() thread started");
            /* How this method works
            *  1. Wait for new image
            *  2. Create 5 copies of the image and save it to each function source image
            *  3. Notify gesture processing thread
            *  4. Wait for result
            *  5. Compare the result
            *  6. Decide the gesture
            *  7. Notify main function
            *  8. Repeat the process
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

            // Start recognition threads (if not started)
            if (!isCursorRecognitionThreadStarted)
            {
                cursorRecognitionThread = new Thread(CursorThread);
                cursorRecognitionThread.Start();
                isCursorRecognitionThreadStarted = true;
            }

            // Copy source images for each functions
            loadedCursorGestureImage = new Image<Bgr, byte>(receivedImage);
            loadedSelectGestureImage = new Image<Bgr, byte>(receivedImage);
            loadedMoveGestureImage = new Image<Bgr, byte>(receivedImage);
            loadedScaleGestureImage = new Image<Bgr, byte>(receivedImage);
            loadedRotateGestureImage = new Image<Bgr, byte>(receivedImage);
            loadedOpenMenuGestureImage = new Image<Bgr, byte>(receivedImage);

            // Tell recognition thread
            isCursorGestureSourceImageLoaded = true;
            isSelectGestureSourceImageLoaded = true;
            isMoveGestureSourceImageLoaded = true;
            isScaleGestureSourceImageLoaded = true;
            isRotateGestureSourceImageLoaded = true;
            isOpenMenuGestureSourceImageLoaded = true;
            //gestureType = "Cursor";
            
            // Wait for all threads to complete operation
            while (true)
            {
                if (isCursorGestureSourceImageProcessed == true &&
                    isSelectGestureSourceImageProcessed == true &&
                    isMoveGestureSourceImageProcessed == true &&
                    isScaleGestureSourceImageProcessed == true &&
                    isRotateGestureSourceImageProcessed == true &&
                    isOpenMenuGestureSourceImageProcessed == true)
                {
                    break;
                }
            }
            #region Decision maker
            // How it works
            // 1. System will find the highest score of all processed information
            // 2. System will which gesture that has the highest score
            

            #region Gestures comparison
            // Read and compare gestures information
            if (recognitionProcessor.cursorGestureScore > highestScore)
            {
                highestScore = recognitionProcessor.cursorGestureScore;
                highestGestureID = 1;
            }
            else if (recognitionProcessor.selectGestureScore > highestScore)
            {
                highestScore = recognitionProcessor.selectGestureScore;
                highestGestureID = 2;
            }
            else if (recognitionProcessor.moveGestureScore > highestScore)
            {
                highestScore = recognitionProcessor.moveGestureScore;
                highestGestureID = 3;
            }
            else if (recognitionProcessor.rotateGestureScore > highestScore)
            {
                highestScore = recognitionProcessor.rotateGestureScore;
                highestGestureID = 4;
            }
            else if (recognitionProcessor.scaleGestureScore > highestScore)
            {
                highestScore = recognitionProcessor.scaleGestureScore;
                highestGestureID = 5;
            }
            else if (recognitionProcessor.openMenuGestureScore > highestScore)
            {
                highestScore = recognitionProcessor.openMenuGestureScore;
                highestGestureID = 6;
            }
            else
            {
                highestScore = 0;
                highestScore = 0;
            }
            #endregion Gestures comparison

            // Set the gesture
            switch (highestGestureID)
            {
                case 1:
                    gestureType = "Cursor";
                    break;
                case 2:
                    gestureType = "Select";
                    break;
                case 3:
                    gestureType = "Move";
                    break;
                case 4:
                    gestureType = "Rotate";
                    break;
                case 5:
                    gestureType = "Scale";
                    break;
                case 6:
                    gestureType = "OpenMenu";
                    break;
                default:
                    gestureType = "None";
                    break;
            }
            #endregion Decision maker
            gestureRecognitionPerformance = gestureRecognitionPerformanceWatcher.ElapsedMilliseconds;
        }
        #endregion Gesture functions
        //static SURFFeatureClass SURFGestureRecognition;

        static void CursorThread()
        {
            // How it works
            // 
            // 1. Wait for new image
            // 2. Remove background using skin extractor and convex hull algirothm
            //    using cursorBackgroundRemover object from GetureRecognitionClass class
            //    (the class was for gesture recognition before but it only able to know
            //    finger number and unable to recognize using templates)
            // 3. Save the skin and copy to cursorSourceSkin in recognitionProcessor (what a weird name) object
            //    from EmguCVSURFClass class
            // 4. Call the function
            // 5. Read the result
            // 6. Report to top caller (whatever it called) GestureRecognition to
            //    determine what gesture is the user is currently doing
            // 7. Repeat the process

            while (true)
            {
                if (isCursorGestureSourceImageLoaded)
                {
                    break;
                }
            }

            // Remove background
            //loadedCursorGestureImage.CopyTo(cursorBackgroundRemover.receivedImage);
            cursorBackgroundRemover.receivedImage = new Image<Bgr, byte>(loadedCursorGestureImage.Bitmap);
            cursorBackgroundRemover.isImageReceived = true;
            if (!cursorBackgroundRemover.isMainProcessStarted)
            {
                cursorBackgroundRemover.StartMainProcess(cursorBackgroundRemover.receivedImage.Width, cursorBackgroundRemover.receivedImage.Height);
            }

            // Wait for process to complete
            while (true)
            {
                if (cursorBackgroundRemover.isImageProcessed)
                {
                    break;
                }
                Thread.Sleep(5);
            }

            //cursorBackgroundRemover.processedSkin.CopyTo(resultCursorGesture);
            resultCursorGesture = cursorBackgroundRemover.processedSkin;
            isCursorGestureSourceImageProcessed = true;
            loadedCursorGestureImage.Dispose();
            isCursorGestureSourceImageLoaded = false;

            // Proceed with matching
            if (recognitionProcessor.cursorSourceSkin != null)
            {
                recognitionProcessor.cursorSourceSkin.Dispose();
            }
            recognitionProcessor.cursorSourceSkin = resultCursorGesture;
            if (recognitionProcessor.cursorSourceSkin != null)
            {
                recognitionProcessor.isCursorSourceImageHasBeenLoaded = true;
            }

            // Call the function
            recognitionProcessor.CursorMatchGesture();
            
            #region Old function
            ////SURFGestureRecognition = new SURFFeatureClass();
            //Image<Bgr, byte> modelImage = null;

            ////CvInvoke.cvCvtColor(cursorTemplate[0], modelImage, COLOR_CONVERSION.CV_BGRA2GRAY);

            //long    matchTime;
            //double  threshold = 0.0002;
            //int     octaves = 5;
            //int     initial = 2;

            ////SpeededUpRobustFeaturesDetector cursorDetector = new SpeededUpRobustFeaturesDetector(threshold, octaves, initial);
            //int number = 0;
            ////SURFAccord();
            #endregion Old function
        }

        static void SURFAccord()
        {
            IntPoint[] correlation1;
            IntPoint[] correlation2;
            //resultCursorGesture = cursorReceivedImage;
            SpeededUpRobustFeaturesDetector surf = new SpeededUpRobustFeaturesDetector(cursorThreshold, cursorOctaves, cursorInitial);
            IEnumerable<SpeededUpRobustFeaturePoint> observedPoints = surf.Transform(cursorReceivedImage.Bitmap);
            IEnumerable<SpeededUpRobustFeaturePoint> modelPoints = surf.Transform(cursorTemplate[0].Bitmap);

            KNearestNeighborMatching matcher = new KNearestNeighborMatching(20);
            IntPoint[][] matches = matcher.Match(observedPoints, modelPoints);

            correlation1 = matches[0];
            correlation2 = matches[1];

            PairsMarker pairsMarker = new PairsMarker(correlation1, correlation2);
        }

        

        static void DefaultErrorWriter(Exception e)
        {
            Console.WriteLine("\nAn error found.\n");
            Console.WriteLine(e.Message);
            Console.WriteLine("Line that causing the problem is:");
            Console.WriteLine(e.Source);
            Console.WriteLine("Below is the stack trace of the cause.");
            Console.WriteLine(e.StackTrace);
            Console.WriteLine("\nWish you good luck to solve it");
        }

        #region Sample functions
        static ImageViewer viewer;

        static void Sample()
        {
            #region Sample 01
            //int totalProcessedFrame = 0;
            //viewer = new ImageViewer();
            //viewer.Text = "test 1";
            //int imageNumber = 0;
            //viewer.Image = resultCursorGesture;
            //Capture capture = new Capture(CaptureType.ANY);
            //Application.Idle += new EventHandler(delegate (object sender, EventArgs e)
            //{
            //    cursorReceivedImage = capture.QuerySmallFrame();
            //    //HandGestureRecognitionSampleMainFunction();
            //    //resultCursorGesture = ImgRecognitionEmGu.DrawMatches.Draw(cursorTemplate[imageNumber].Mat, cursorReceivedImage, out gestureRecognitionPerformance, out gestureScore);
            //    viewer.Image = resultCursorGesture;
            //    viewer.Text = gestureScore.ToString();
            //    imageNumber++;
            //    if (imageNumber == cursorTemplate.Length - 1)
            //    {
            //        imageNumber = 0;
            //    }
            //});
            //viewer.ShowDialog();
            #endregion Sample 01
            Sample02();


            Console.ReadLine();

        }
        static void Sample02()
        {
            #region Sample 02
            string filePath = SampleImageLocation;
            Console.WriteLine("Loading images from " + filePath);
            EmguCVSURFClass emguCVSURFClass = new EmguCVSURFClass();
            emguCVSURFClass.LoadImage(filePath);
            emguCVSURFClass.dbImages = Directory.GetFiles(filePath);
            emguCVSURFClass.queryImage = SampleImageFile;
            emguCVSURFClass.RunMatch();

            // Writing results
            for (int i = 0; i < emguCVSURFClass.FileNames.Count; i++)
            {
                Console.WriteLine(i + " ]----------------------------");
                Console.WriteLine("File name = " + emguCVSURFClass.FileNames[i]);
                Console.WriteLine("Start index = " + emguCVSURFClass.IndexStart[i]);
                Console.WriteLine("End index = " + emguCVSURFClass.IndexEnd[i]);
                Console.WriteLine("Similarity = " + emguCVSURFClass.Similarity[i]);
            }

            Console.WriteLine(emguCVSURFClass.isImageLoaded);
            Console.ReadLine();
            #endregion Sample 02
        }
        //static void HandGestureRecognitionSampleMainFunction()
        //{
        //    if (gestureRecognition == null)
        //    {
        //        return;
        //    }
        //    if (!gestureRecognition.isThreadStarted)
        //    {
        //        gestureRecognition.StartMainProcess(cursorReceivedImage.Width, cursorReceivedImage.Height);
        //    }

        //    gestureRecognition.receivedImage = cursorReceivedImage;
        //    gestureRecognition.isImageReceived = true;
        //    while (true)
        //    {
        //        if (gestureRecognition.isImageProcessed)
        //        {
        //            break;
        //        }
        //    }
        //    processedImage = new Image<Bgr, byte>(gestureRecognition.processedImage.Bitmap);
        //    resultCursorGesture = new Image<Bgr, byte>(gestureRecognition.processedImage.Bitmap);
        //    //CvInvoke.cvCvtColor(resultCursorGesture, gestureRecognition.processedSkin, COLOR_CONVERSION.CV_GRAY2BGR);
        //}
        #endregion Sample functions
    }
}