//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Emgu.CV;
//using Emgu.CV.Features2D;
//using Emgu.CV.CvEnum;
//using Emgu.CV.Util;
//using System.Diagnostics;
//using Emgu.CV.XFeatures2D;
//using Emgu.CV.Structure;
//using System.Drawing;

//namespace Kursor3D_Kursor3DModule
//{
//    class SURFFeatureClass
//    {
//        static public bool GestureFound { get; private set; }
//        static private void SURFDetector(Mat modelImage, Mat observedImage, out long matchTime, out VectorOfKeyPoint modelKeyPoints, out VectorOfKeyPoint observedKeyPoints, VectorOfVectorOfDMatch matches, out Mat mask, out Mat homography)
//        {
//            int k = 2;
//            double uniquenessThreshold = 0.8;
//            double hessianThresh = 300;

//            homography = null;

//            modelKeyPoints = new VectorOfKeyPoint();
//            observedKeyPoints = new VectorOfKeyPoint();

//            Stopwatch watch = new Stopwatch();
//            watch.Start();
//            UMat uModelImage = modelImage.GetUMat(AccessType.Read);
//            UMat uObservedImage = modelImage.GetUMat(AccessType.Read);

//            SURF surfCPU = new SURF(hessianThresh);
//            //extract features from the object image
//            UMat modelDescriptors = new UMat();
//            surfCPU.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);

//            watch = Stopwatch.StartNew();

//            // extract features from the observed image
//            UMat observedDescriptors = new UMat();
//            surfCPU.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);
//            BFMatcher matcher = new BFMatcher(DistanceType.L2);
//            matcher.Add(modelDescriptors);

//            matcher.KnnMatch(observedDescriptors, matches, k, null);
//            mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
//            mask.SetTo(new MCvScalar(255));
//            Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

//            int nonZeroCount = CvInvoke.CountNonZero(mask);

//            if (nonZeroCount >= 4)
//            {
//                nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints, matches, mask, 1.5, 20);
//                if (nonZeroCount >= 4)
//                {
//                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints, observedKeyPoints, matches, mask, 2);
//                    GestureFound = true;
//                }
//            }

//            watch.Stop();
//            matchTime = watch.ElapsedMilliseconds;

//        }

//        static public Mat Draw(Mat modelImage, Mat observedImage, out long matchTime)
//        {
//            Mat homography;
//            VectorOfKeyPoint modelKeyPoints;
//            VectorOfKeyPoint observedKeyPoints;
//            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
//            {
//                Mat mask;
//                SURFDetector(modelImage, observedImage, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
//                   out mask, out homography);

//                //Draw the matched keypoints
//                Mat result = new Mat();
//                Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints, matches, result, new MCvScalar(255, 255, 255), new MCvScalar(255, 255, 255), mask);
//                #region draw the projected region on the image

//                if (homography != null)
//                {
//                    //draw a rectangle along the projected model
//                    Rectangle rect = new Rectangle(Point.Empty, modelImage.Size);
//                    PointF[] pts = new PointF[]
//                    {
//                        new PointF(rect.Left, rect.Bottom),
//                        new PointF(rect.Right, rect.Bottom),
//                        new PointF(rect.Right, rect.Top),
//                        new PointF(rect.Left, rect.Top)
//                    };
//                    pts = CvInvoke.PerspectiveTransform(pts, homography);

//                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
//                    using (VectorOfPoint vp = new VectorOfPoint(points))
//                    {
//                        CvInvoke.Polylines(result, vp, true, new MCvScalar(255, 255, 255, 255), 5);
//                    }
//                }

//                #endregion

//                return result;

//            }
//        }
//    }
//}
