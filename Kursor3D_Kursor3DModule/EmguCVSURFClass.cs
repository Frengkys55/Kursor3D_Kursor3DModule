using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using System.IO;
using System.Drawing;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;

namespace Kursor3D_Kursor3DModule
{
    class EmguCVSURFClass
    {
        #region EmguCV SURF class settings


        #region Gestures data

        #region Gesture templates
        #region Source template images
        //--------------------------------------------
        // Save location of image loaded from disk
        //--------------------------------------------
        public Image<Bgr, byte>[] cursorTemplates;
        public Image<Bgr, byte>[] selectTemplates;
        public Image<Bgr, byte>[] moveTemplates;
        public Image<Bgr, byte>[] rotateTemplates;
        public Image<Bgr, byte>[] scaleTemplates;
        public Image<Bgr, byte>[] openmenuTemplates;
        #endregion Source template images
        #region Source processed template images
        //--------------------------------------------
        // Save location of processed image from the top region,
        // the "Source template images" region.
        // Saving location is separated because descritpors
        // calculation was using greyscale color space instead of
        // RGB color space (refer to "References.txt").
        //--------------------------------------------
        public Image<Gray, byte>[] cursorTemplatesGrayscaled;
        public Image<Gray, byte>[] selectTemplatesGrayscaled;
        public Image<Gray, byte>[] moveTemplatesGreyscaled;
        public Image<Gray, byte>[] rotateTemplatesGeryscaled;
        public Image<Gray, byte>[] scaleTemplatesGreyscaled;
        public Image<Gray, byte>[] openMenuTemplatesGreyscaled;
        #endregion Source processed template images
        #endregion Gesture templates

        #region Gesture skins
        public Image<Gray, byte>[] cursorSkins;
        public Image<Gray, byte>[] selectSkins;
        public Image<Gray, byte>[] moveSkins;
        public Image<Gray, byte>[] rotateSkins;
        public Image<Gray, byte>[] scaleSkins;
        public Image<Gray, byte>[] openMenuSkins;
        #endregion Gesture skins

        #region Gesture matrix
        Matrix<float>[] cursorGestureMatrix;
        Matrix<float>[] selectGestureMatrix;
        Matrix<float>[] moveGestureMatrix;
        Matrix<float>[] rotateGestureMatrix;
        Matrix<float>[] scaleGestureMatrix;
        Matrix<float>[] openMenuGestureMatrix;
        #endregion Gesture matrix

        #region Gestures temporary image
        
        #endregion Gestures temporary image

        #region Gesture scores

        #endregion Gesture scores

        #endregion Gestures data
        #region Images data
        Image<Bgr, byte>[] sourceImages;
        Image<Bgr, byte> sourceImage;
        Image<Bgr, byte> processedImage;

        string[] imageFilePaths;
        #endregion Images data

        #region EmguCV data
        private const double surfHessianThresh = 300;
        private const bool surfExtendedFlag = true;
        Matrix<float> imageDescriptor;
        List<Matrix<float>> imageDescriptors;
        SURFDetector detector = new SURFDetector(surfHessianThresh, surfExtendedFlag);
        #endregion EmguCV data

        #region Other settings
        public bool isThreadStarted { private set; get; }
        public bool isImageLoaded { private set; get; }
        public bool isImageProcessed { private set; get; }

        public bool IsSURFSourceImageLoaded { set; get; }
        public bool IsSURFImageProcessed { private set; get; }
        #endregion Other settings

        #region Thread informations
        System.Threading.Thread surfThread;
        #endregion Thread informations

        #endregion EmguCV SURF class settings
        public EmguCVSURFClass(/*Dictionary<string, string> settings*/)
        {
            
        }
        private void LoadSettings()
        {
            
        }

        public void LoadImage(string path)
        {
            #region Folder check
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException("Directory not found.");
            }
            #endregion Folder check

            #region Image files loader
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            List<String> filesFound = new List<String>();
            var searchOption = SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(path, String.Format("*.{0}", filter), searchOption));
            }
            imageFilePaths = filesFound.ToArray();
            #endregion Image files loader

            #region Image loading
            sourceImages = new Image<Bgr, byte>[imageFilePaths.Length];
            for (int i = 0; i < imageFilePaths.Length; i++)
            {
                sourceImages[i] = new Image<Bgr, byte>(imageFilePaths[i]);
            }
            #endregion Image loading
            isImageLoaded = true;
        }
        
        public void StartSURFThread()
        {
            surfThread = new System.Threading.Thread(SURFThread);
            surfThread.Start();

        }
        public void StopSURFThread()
        {
            if (surfThread.IsAlive)
            {
                surfThread.Abort();
            }
        }
        void SURFThread()
        {
            
            isImageProcessed = true; ;
            isImageLoaded = false;
        }

        #region SURF functions
        
        public void ComputeDescriptors()
        {
            #region Matrix initialization
            if (cursorGestureMatrix == null)
            {
                cursorGestureMatrix = new Matrix<float>[cursorTemplatesGrayscaled.Length];
            }
            if (selectGestureMatrix == null)
            {
                selectGestureMatrix = new Matrix<float>[selectTemplatesGrayscaled.Length];
            }
            if (moveGestureMatrix == null)
            {
                moveGestureMatrix = new Matrix<float>[moveTemplatesGreyscaled.Length];
            }
            if (scaleGestureMatrix == null)
            {
                scaleGestureMatrix = new Matrix<float>[scaleTemplatesGreyscaled.Length];
            }
            if (rotateGestureMatrix == null)
            {
                rotateGestureMatrix = new Matrix<float>[rotateTemplatesGeryscaled.Length];
            }
            if (openMenuGestureMatrix == null)
            {
                openMenuGestureMatrix = new Matrix<float>[openMenuTemplatesGreyscaled.Length];
            }
            #endregion Matrix initialization

            #region Gestures descriptors precalculation
            for (int i = 0; i < cursorTemplatesGrayscaled.Length; i++)
            {
                ComputeDescriptor(cursorTemplatesGrayscaled[i], ref cursorGestureMatrix[i]);
            }
            for (int i = 0; i < scaleTemplatesGreyscaled.Length; i++)
            {
                ComputeDescriptor(scaleTemplatesGreyscaled[i], ref scaleGestureMatrix[i]);
            }
            for (int i = 0; i < moveTemplatesGreyscaled.Length; i++)
            {
                ComputeDescriptor(moveTemplatesGreyscaled[i], ref moveGestureMatrix[i]);
            }
            for (int i = 0; i < scaleTemplatesGreyscaled.Length; i++)
            {
                ComputeDescriptor(scaleTemplatesGreyscaled[i], ref scaleGestureMatrix[i]);
            }
            for (int i = 0; i < rotateTemplatesGeryscaled.Length; i++)
            {
                ComputeDescriptor(rotateTemplatesGeryscaled[i], ref rotateGestureMatrix[i]);
            }
            for (int i = 0; i < openMenuTemplatesGreyscaled.Length; i++)
            {
                ComputeDescriptor(openMenuTemplatesGreyscaled[i], ref openMenuGestureMatrix[i]);
            }
            #endregion Gestures descriptors precalculation
        }

        void ComputeDescriptor(Image<Gray, byte> sourceImage, ref Matrix<float> matrix)
        {
            VectorOfKeyPoint keyPoints = detector.DetectKeyPointsRaw(sourceImage, null);
            matrix = detector.ComputeDescriptorsRaw(sourceImage, null, keyPoints);
        }

        void FindCursorMatch(Image<Bgr, byte> sourceImage)
        {
            
        }
        #endregion SURF functions
        #region Sample codes
        #region Sample data
        public string[] dbImages { set; get; }
        public string queryImage { set; get; }
        public List<string> FileNames;
        public List<int> IndexStart;
        public List<int> IndexEnd;
        public List<int> Similarity;
        #endregion Sample data
        List<IndecesMapping> imap;

        public void RunMatch()
        {
            IList<IndecesMapping> result = Match();
            FileNames = new List<string>();
            IndexStart = new List<int>();
            IndexEnd = new List<int>();
            Similarity = new List<int>();

            for (int i = 0; i < result.Count; i++)
            {
                FileNames.Add(result[i].fileName);
                IndexStart.Add(result[i].IndexStart);
                IndexEnd.Add(result[i].IndexEnd);
                Similarity.Add(result[i].Similarity);
            }
        }

        internal class IndecesMapping
        {
            public string fileName { set; get; }
            public int IndexStart { set; get; }
            public int IndexEnd { set; get; }
            public int Similarity { set; get; }
        }
        IList<IndecesMapping> Match()
        //public void Match()
        {

            // Compute descriptors for each image
            var dbDescList = ComputeMultipleDescriptors(dbImages/*, out imap*/);

            // Concatenate all DB images descriptors into single matrix
            Matrix<float> dbDescs = ConcateDescriptors(dbDescList);

            // Compute descriptors for the new query image
            Matrix<float> queryDescriptors = ComputeSingleDescriptors(queryImage);
            FindMatches(dbDescs, queryDescriptors/*, ref imap*/);
            return imap;
        }

        Matrix<float> ComputeSingleDescriptors(string fileName)
        {
            Matrix<float> descs;
            detector = new SURFDetector(surfHessianThresh, surfExtendedFlag);

            using (Image<Gray, byte> img = new Image<Gray, byte>(fileName))
            {
                VectorOfKeyPoint keyPoints = detector.DetectKeyPointsRaw(img, null);
                descs = detector.ComputeDescriptorsRaw(img, null, keyPoints);
            }
            return descs;
        }

        IList<Matrix<float>> ComputeMultipleDescriptors(string[] fileNames/*, out IList<IndecesMapping> imap*/)
        {
            imap = new List<IndecesMapping>();
            IList<Matrix<float>> descs = new List<Matrix<float>>();

            int r = 0;

            for (int i = 0; i < fileNames.Length; i++)
            {
                var desc = ComputeSingleDescriptors(fileNames[i]);
                descs.Add(desc);

                imap.Add(new IndecesMapping()
                {
                    fileName = fileNames[i],
                    IndexStart = r,
                    IndexEnd = r + desc.Rows - 1
                });
                r += desc.Rows;
            }
            return descs;
        }

        void FindMatches(Matrix<float> dbDescriptors, Matrix<float> queryDescriptors/*, ref IList<IndecesMapping> imap*/)
        {
            var indices = new Matrix<int>(queryDescriptors.Rows, 2); // Matrix that will contain indices of the 2-nearest neighbors found
            var dists = new Matrix<float>(queryDescriptors.Rows, 2); // Matrix that will contain distances to the 2-nearest neighbor found

            // Create FLANN index with 4 kd-trees and performan KNN search over it look for 2 nearest neighbours
            var flannIndex = new Index(dbDescriptors, 4);
            flannIndex.KnnSearch(queryDescriptors, indices, dists, 2, 24);

            for (int i = 0; i < indices.Rows; i++)
            {
                // Filter out all inadequate pairs based on distance between pairs
                if (dists.Data[i, 0] < (0.6 * dists.Data[i, 1]))
                {
                    // Find image from the db to which current descriptors range belong and increment similarity value.
                    // In the actual implementation, this sould be done differently as it's not very efficient for large image collections.
                    for (int j = 0; i < imap.Count; i++)
                    {
                        if (imap[j].IndexStart <= i && imap[j].IndexEnd >= i)
                        {
                            imap[j].Similarity++;
                            break;
                        }
                    }
                    foreach (var img in imap)
                    {
                        if (img.IndexStart <= i && img.IndexEnd >= i)
                        {

                            break;
                        }
                    }
                }
            }
        }

        Matrix<float> ConcateDescriptors(IList<Matrix<float>> descriptors)
        {
            int cols = descriptors[0].Cols;
            int rows = descriptors.Sum(a => a.Rows);
            float[,] concatedDescs = new float[rows, cols];

            int offset = 0;
            foreach (var descriptor in descriptors)
            {
                // Append new descriptors
                Buffer.BlockCopy(descriptor.ManagedArray, 0, concatedDescs, offset, sizeof(float) * descriptor.ManagedArray.Length);
                offset += sizeof(float) * descriptor.ManagedArray.Length;
            }
            return new Matrix<float>(concatedDescs);
        }
        #endregion Sample codes

    }
}