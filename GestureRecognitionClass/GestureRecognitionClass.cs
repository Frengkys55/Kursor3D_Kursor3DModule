using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using System.Drawing;
using System.Threading;

namespace GestureRecognitionClass
{
    public class GestureRecognitionClass
    {
        #region Gesture Class Informations
        MemStorage storage = new MemStorage();
        IColorSkinDetector skinDetector;

        Image<Bgr, Byte> currentFrame;
        Image<Bgr, Byte> currentFrameCopy;

        Capture grabber; // Might need to remove later
        AdaptiveSkinDetector detector;

        int frameWidth;
        int frameHeight;

        Hsv hsv_min;
        Hsv hsv_max;
        Ycc YCrCb_min;
        Ycc YCrCb_max;

        Seq<Point> hull;
        Seq<Point> filterredHull;
        Seq<MCvConvexityDefect> defects;
        MCvConvexityDefect[] defectArray;
        Rectangle handRect;
        MCvBox2D box;
        Ellipse ellipse;


        #region Other informations
        public bool isImageReceived { set; get; }
        public bool isImageProcessed { private set; get; }

        public Image<Bgr, byte> receivedImage { set; get; }
        public Image<Bgr, byte> processedImage { private set; get; }

        Thread mainProcess;
        #endregion Other informations

        #endregion Gesture Class Informations

        public GestureRecognitionClass()
        {
            frameWidth = grabber.Width;
            frameHeight = grabber.Height;
            detector = new AdaptiveSkinDetector(1, AdaptiveSkinDetector.MorphingMethod.NONE);
            hsv_min = new Hsv(0, 45, 0);
            hsv_max = new Hsv(20, 255, 255);
            YCrCb_min = new Ycc(0, 131, 80);
            YCrCb_max = new Ycc(255, 185, 135);
            box = new MCvBox2D();
            ellipse = new Ellipse();
            
            mainProcess = new Thread(MainProcess);
            mainProcess.Start();
        }

        public void MainProcess()
        {
            while (true)
            {
                if (isImageReceived)
                {
                    break;
                }
                Thread.Sleep(5);
            }

            currentFrame = receivedImage;

            if (currentFrame != null)
            {
                currentFrameCopy = currentFrame.Copy();

                skinDetector = new YCrCbSkinDetector();
                Image<Gray, Byte> skin = skinDetector.DetectSkin(currentFrameCopy, YCrCb_min, YCrCb_max);

                ExtractContourAndHull(skin);
                DrawAndComputeFingersNum();
                processedImage = currentFrame;
                isImageProcessed = true;
            }
            isImageReceived = false;
        }
        void ExtractContourAndHull(Image<Gray, Byte> skin)
        {
            Contour<Point> contours = skin.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST);
            Contour<Point> biggestContour = null;

            Double result1 = 0;
            Double result2 = 0;

            while (contours != null)
            {
                result1 = contours.Area;
                if (result1 > result2)
                {
                    result2 = result1;
                    biggestContour = contours;
                }
                contours = contours.HNext;
            }
            if (biggestContour != null)
            {
                Contour<Point> currentContour = biggestContour.ApproxPoly(biggestContour.Perimeter * 0.0025, storage);
                currentFrame.Draw(currentContour, new Bgr(Color.LimeGreen), 2);
                biggestContour = currentContour;

                hull = biggestContour.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);
                box = biggestContour.GetMinAreaRect();
                PointF[] points = box.GetVertices();

                Point[] ps = new Point[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    ps[i] = new Point((int)points[i].X, (int)points[i].Y);
                }
                currentFrame.DrawPolyline(hull.ToArray(), true, new Bgr(200, 125, 75), 2);
                currentFrame.Draw(new CircleF(new PointF(box.center.X, box.center.Y), 3), new Bgr(200, 125, 75), 2);

                PointF center;
                float radius;

                filterredHull = new Seq<Point>(storage);
                for (int i = 0; i < hull.Total; i++)
                {
                    if (Math.Sqrt(Math.Pow(hull[i].X - hull[i + 1].X, 2) + Math.Pow(hull[i].Y - hull[i + 1].Y, 2)) > box.size.Width / 10)
                    {
                        filterredHull.Push(hull[i]);
                    }
                }

                defects = biggestContour.GetConvexityDefacts(storage, Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);

                defectArray = defects.ToArray();
            }
        }

        private void DrawAndComputeFingersNum()
        {
            int fingerNum = 0;

            #region hull drawing
            //for (int i = 0; i < filteredHull.Total; i++)
            //{
            //    PointF hullPoint = new PointF((float)filteredHull[i].X,
            //                                  (float)filteredHull[i].Y);
            //    CircleF hullCircle = new CircleF(hullPoint, 4);
            //    currentFrame.Draw(hullCircle, new Bgr(Color.Aquamarine), 2);
            //}
            #endregion

            #region defects drawing
            for (int i = 0; i < defects.Total; i++)
            {
                PointF startPoint = new PointF((float)defectArray[i].StartPoint.X,
                                                (float)defectArray[i].StartPoint.Y);

                PointF depthPoint = new PointF((float)defectArray[i].DepthPoint.X,
                                                (float)defectArray[i].DepthPoint.Y);

                PointF endPoint = new PointF((float)defectArray[i].EndPoint.X,
                                                (float)defectArray[i].EndPoint.Y);

                LineSegment2D startDepthLine = new LineSegment2D(defectArray[i].StartPoint, defectArray[i].DepthPoint);

                LineSegment2D depthEndLine = new LineSegment2D(defectArray[i].DepthPoint, defectArray[i].EndPoint);

                CircleF startCircle = new CircleF(startPoint, 5f);

                CircleF depthCircle = new CircleF(depthPoint, 5f);

                CircleF endCircle = new CircleF(endPoint, 5f);

                //Custom heuristic based on some experiment, double check it before use
                if ((startCircle.Center.Y < box.center.Y || depthCircle.Center.Y < box.center.Y) && (startCircle.Center.Y < depthCircle.Center.Y) && (Math.Sqrt(Math.Pow(startCircle.Center.X - depthCircle.Center.X, 2) + Math.Pow(startCircle.Center.Y - depthCircle.Center.Y, 2)) > box.size.Height / 6.5))
                {
                    fingerNum++;
                    currentFrame.Draw(startDepthLine, new Bgr(Color.Green), 2);
                    //currentFrame.Draw(depthEndLine, new Bgr(Color.Magenta), 2);
                }


                currentFrame.Draw(startCircle, new Bgr(Color.Red), 2);
                currentFrame.Draw(depthCircle, new Bgr(Color.Yellow), 5);
                //currentFrame.Draw(endCircle, new Bgr(Color.DarkBlue), 4);
            }
            #endregion

            MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_DUPLEX, 5d, 5d);
            currentFrame.Draw(fingerNum.ToString(), ref font, new Point(50, 150), new Bgr(Color.White));
            System.Threading.Thread.Sleep(300);
        }

    }
}
