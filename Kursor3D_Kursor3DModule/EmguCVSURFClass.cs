﻿using System;
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

        #region Gesture processing informations
        
        #region Gesture template processing informations
        // This region contains information for telling
        // that templates has been processed before.
        // This used to prevent recalculation and
        // increase overall processing performance

        bool isCursorTemplatesHasProcessed = false;
        bool isSelcectTemplatesHasProcessed = false;
        bool isMoveTemplatesHasProcessed = false;
        bool isScaleTemplatesHasProcessed = false;
        bool isRotateTemplatesHasProcessed = false;
        bool isOpenMenuTemplatesHasProcessed = false;
        #endregion Gesture template processing informations

        #region Gesture descriptor conversion information
        // This region contains information about template
        // conversion.

        bool isCursorTemplatesHasBeenConverted = false;
        bool isSelectTemplatesHasBeenConverted = false;
        bool isMoveTemplatesHasBeenConverted = false;
        bool isScaleTemplatesHasBeenConverted = false;
        bool isRotateTemplatesHasBeenConverted = false;
        bool isOpenMenuTemplatesHasBeenConverted = false;
        #endregion Gesture descriptor conversion information

        #region Gesture descriptors concatenation informations
        bool isCursorTemplatesHasBeenConcatenated = false;
        bool isSelectTemplatesHasBeenConcatenated = false;
        bool isMoveTemplatesHasBeenConcatenated = false;
        bool isScaleTemplatesHasBeenConcatenated = false;
        bool isRotateTemplatesHasBeenConcatenated = false;
        bool isOpenMenuTemplatesHasBeenConcatenated = false;
        #endregion Gesture descriptors concatenation inforamtions

        #region Source loading informations
        // This region contains informations for telling
        // some functions that source gesture has been
        // loaded and ready for processing.

        public bool isCursorSourceImageHasBeenLoaded = false;
        public bool isSelectSourceImageHasBeenLoaded = false;
        public bool isMoveSourceImageHasBeenLoaded = false;
        public bool isScaleSourceImageHasBeenLoaded = false;
        public bool isRotateSourceImageHasBeenLoaded = false;
        public bool isOpenMenuSourceImageHasBeenLoaded = false;
        #endregion Source loading informations

        #region Source processing information
        public bool isCursorSourceImageHasBeenProcessed = false;
        public bool isSelectSourceImageHasBeenProcessed = false;
        public bool isMoveSourceImageHasBeenProcessed = false;
        public bool isScaleSourceImageHasBeenProcessed = false;
        public bool isRotateSourceImageHasBeenProcessed = false;
        public bool isOpenMenuSourceImageHasBeenProcessed = false;
        #endregion Srouce processing information

        #region Other Gesture information
        Image<Gray, byte> tempCursorSourceImage;
        #endregion Other gesture information

        #endregion Gesture processing informations

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
        #region Templates gesture skins
        public Image<Gray, byte>[] cursorSkins;
        public Image<Gray, byte>[] selectSkins;
        public Image<Gray, byte>[] moveSkins;
        public Image<Gray, byte>[] rotateSkins;
        public Image<Gray, byte>[] scaleSkins;
        public Image<Gray, byte>[] openMenuSkins;
        #endregion Template gesture skins
        #region Source gesture skins
        public Image<Gray, byte> cursorSourceSkin;
        public Image<Gray, byte> selectSourceSkin;
        public Image<Gray, byte> moveSourceSkin;
        public Image<Gray, byte> scaleSourceSkin;
        public Image<Gray, byte> rotateSourceSkin;
        public Image<Gray, byte> openMenuSourceSkin;
        #endregion Source gesture skins
        #endregion Gesture skins

        #region Gesture matrix
        #region Templates
        #region Array
        Matrix<float>[] cursorGestureMatrix;
        Matrix<float>[] selectGestureMatrix;
        Matrix<float>[] moveGestureMatrix;
        Matrix<float>[] rotateGestureMatrix;
        Matrix<float>[] scaleGestureMatrix;
        Matrix<float>[] openMenuGestureMatrix;
        #endregion Array
        #region IList
        IList<Matrix<float>> cursorDescriptorsList;
        IList<Matrix<float>> selectDescriptorsList;
        IList<Matrix<float>> moveDescriptorsList;
        IList<Matrix<float>> scaleDescriptorsList;
        IList<Matrix<float>> rotateDescriptorsList;
        IList<Matrix<float>> openMenuDescriptorsList;
        #endregion IList
        #region Concatenated
        Matrix<float> cursorDescriptorsConcatenated;
        Matrix<float> selectDescriptorsConcatenated;
        Matrix<float> moveDescriptorsConcatenated;
        Matrix<float> scaleDescriptorsConcatenated;
        Matrix<float> rotateDescriptorsConcatenated;
        Matrix<float> openMenuDescriptorsConcatenated;
        #endregion Concatentated
        #endregion Templates
        #region Sources
        Matrix<float> cursorSourceMatrix;
        Matrix<float> selectSourceMatrix;
        Matrix<float> moveSourceMatrix;
        Matrix<float> scaleSourceMatrix;
        Matrix<float> rotateSourceMatrix;
        Matrix<float> openMenuSourceMatrix;
        #endregion Sources
        #endregion Gesture matrix

        #region Gesture scores
        public int cursorGestureScore = 0;
        public int selectGestureScore = 0;
        public int moveGestureScore = 0;
        public int scaleGestureScore = 0;
        public int rotateGestureScore = 0;
        public int openMenuGestureScore = 0;
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

        #region SURF Detectors
        SURFDetector cursorDetector = new SURFDetector(surfHessianThresh, surfExtendedFlag);
        SURFDetector selectDetector = new SURFDetector(surfHessianThresh, surfExtendedFlag);
        SURFDetector moveDetector = new SURFDetector(surfHessianThresh, surfExtendedFlag);
        SURFDetector scaleDetector = new SURFDetector(surfHessianThresh, surfExtendedFlag);
        SURFDetector rotateDetector = new SURFDetector(surfHessianThresh, surfExtendedFlag);
        SURFDetector openMenuDetector = new SURFDetector(surfHessianThresh, surfExtendedFlag);
        #endregion SURF Detectors
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
        //public EmguCVSURFClass(/*Dictionary<string, string> settings*/)
        //{
            
        //}
        

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
            //surfThread = new System.Threading.Thread(SURFThread);
            //surfThread.Start();

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
            // How it works
            // 1. Wait for new image
            // 2. Copy image to each gesture function
            // 3. Tell the function that the image has been copied and ready to proceed
            // 4. Wait for all function to complete
            // 5. Read the results
            // 6. Compare the results and make decision
            // 7. Tell the main function that calculation has been completed
            // 8. Repeat the process

            // Wait for new image
            while (true)
            {
                if (IsSURFSourceImageLoaded)
                {
                    break;
                }
            }

            // Copy new image to each gesture function
            

            isImageProcessed = true; ;
            IsSURFSourceImageLoaded = false;
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

            #region Template descriptors information setter
            isCursorTemplatesHasProcessed = true;
            isSelcectTemplatesHasProcessed = true;
            isMoveTemplatesHasProcessed = true;
            isScaleTemplatesHasProcessed = true;
            isRotateTemplatesHasProcessed = true;
            isOpenMenuTemplatesHasProcessed = true;
            #endregion Descriptors information setter

            ConvertDescriptors();
        }

        void ComputeDescriptor(Image<Gray, byte> sourceImage, ref Matrix<float> matrix)
        {
            VectorOfKeyPoint keyPoints = detector.DetectKeyPointsRaw(sourceImage, null);
            matrix = detector.ComputeDescriptorsRaw(sourceImage, null, keyPoints);
        }

        //public void CursorMatch()
        //{
        //    ComputeDescriptor(cursorSourceSkin, ref cursorSourceMatrix);
        //    CursorFindMatches();
        //}
        
        void ConvertDescriptors()
        {
            #region Conversion functions
            if (cursorGestureMatrix != null)
            {
                if (cursorDescriptorsList == null)
                {
                    cursorDescriptorsList = new List<Matrix<float>>();
                }
                for (int i = 0; i < cursorGestureMatrix.Length; i++)
                {
                    cursorDescriptorsList.Add(cursorGestureMatrix[i]);
                }
                isCursorTemplatesHasBeenConverted = true;
            }
            if (selectGestureMatrix != null)
            {
                if (selectDescriptorsList == null)
                {
                    selectDescriptorsList = new List<Matrix<float>>();
                }
                for (int i = 0; i < cursorGestureMatrix.Length; i++)
                {
                    selectDescriptorsList.Add(cursorGestureMatrix[i]);
                }
                isSelectTemplatesHasBeenConverted = true;
            }
            if (moveGestureMatrix != null)
            {
                if (moveDescriptorsList == null)
                {
                    moveDescriptorsList = new List<Matrix<float>>();
                }
                for (int i = 0; i < cursorGestureMatrix.Length; i++)
                {
                    moveDescriptorsList.Add(cursorGestureMatrix[i]);
                }
                isMoveTemplatesHasBeenConverted = true;
            }
            if (scaleGestureMatrix != null)
            {
                if (scaleDescriptorsList == null)
                {
                    scaleDescriptorsList = new List<Matrix<float>>();
                }
                for (int i = 0; i < cursorGestureMatrix.Length; i++)
                {
                    scaleDescriptorsList.Add(cursorGestureMatrix[i]);
                }
                isScaleTemplatesHasBeenConverted = true;
            }
            if (rotateGestureMatrix != null)
            {
                if (rotateDescriptorsList == null)
                {
                    rotateDescriptorsList = new List<Matrix<float>>();
                }
                for (int i = 0; i < cursorGestureMatrix.Length; i++)
                {
                    rotateDescriptorsList.Add(cursorGestureMatrix[i]);
                }
                isRotateTemplatesHasBeenConverted = true;
            }
            if (openMenuGestureMatrix != null)
            {
                if (openMenuDescriptorsList == null)
                {
                    openMenuDescriptorsList = new List<Matrix<float>>();
                }
                for (int i = 0; i < cursorGestureMatrix.Length; i++)
                {
                    openMenuDescriptorsList.Add(cursorGestureMatrix[i]);
                }
                isOpenMenuTemplatesHasBeenConverted = true;
            }
            #endregion Conversion functions
        }

        void ConcatenateDescriptors()
        {
            #region Concatenation functions
            if (cursorDescriptorsList != null)
            {
                cursorDescriptorsConcatenated = ConcateDescriptors(cursorDescriptorsList);
                isCursorTemplatesHasBeenConcatenated = true;
            }
            if (selectDescriptorsList != null)
            {
                selectDescriptorsConcatenated = ConcateDescriptors(selectDescriptorsList);
                isSelectTemplatesHasBeenConcatenated = true;
            }
            if (moveDescriptorsList != null)
            {
                moveDescriptorsConcatenated = ConcateDescriptors(moveDescriptorsList);
                isMoveTemplatesHasBeenConcatenated = true;
            }
            if (scaleDescriptorsList != null)
            {
                scaleDescriptorsConcatenated = ConcateDescriptors(scaleDescriptorsList);
                isScaleTemplatesHasBeenConcatenated = true;
            }
            if (rotateDescriptorsList != null)
            {
                rotateDescriptorsConcatenated = ConcateDescriptors(rotateDescriptorsList);
                isRotateTemplatesHasBeenConcatenated = true;
            }
            if (openMenuDescriptorsList != null)
            {
                openMenuDescriptorsConcatenated = ConcateDescriptors(openMenuDescriptorsList);
                isOpenMenuTemplatesHasBeenConcatenated = true;
            }
            #endregion Concatenation functions
        }

        void FindCursorMatch()
        {
            // Compute received image descriptor and use it as reference for indices
            cursorSourceMatrix = CursorComputeSingleDescriptor(ref cursorSourceSkin);
            var indices = new Matrix<int>(cursorSourceMatrix.Rows, 2); // Matrix that will contain indices of the 2-nearest neighbors found
            var dists = new Matrix<float>(cursorSourceMatrix.Rows, 2); // Matrix that will contain distances to the 2-nearest neighbor found

            // Create FLANN index with 4 kd-trees and performan KNN search over it look for 2 nearest neighbours
            var flannIndex = new Index(cursorDescriptorsConcatenated, 4);
            flannIndex.KnnSearch(cursorSourceMatrix, indices, dists, 2, 24);

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
        
        #region New main codes
        
        #region Cursor recognition
        #region Cursor data
        public string[] cursorDBImages { set; get; }
        public string cursorQueryImage { set; get; }
        public List<string> cursorFileNames;
        public List<int> cursorIndexStart;
        public List<int> cursorIndexEnd;
        public List<int> cursorSimilarity;

        IList<Matrix<float>> cursorDbDescList;
        Matrix<float> cursorDBDescs;


        #endregion Cursor data

        List<CursorIndecesMapping> cursorImap;

        public void CursorMatchGesture()
        {
            IList<CursorIndecesMapping> result = CursorMatch();
            isCursorSourceImageHasBeenProcessed = true;
            //FileNames = new List<string>();
            //IndexStart = new List<int>();
            //IndexEnd = new List<int>();
            Similarity = new List<int>();
            if (result != null)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    //FileNames.Add(result[i].fileName);
                    //IndexStart.Add(result[i].IndexStart);
                    //IndexEnd.Add(result[i].IndexEnd);
                    Similarity.Add(result[i].Similarity);
                    if (result[i].Similarity > cursorGestureScore)
                    {
                        cursorGestureScore = result[i].Similarity;
                    }
                }
            }
            
            
        }

        internal class CursorIndecesMapping
        {
            public string fileName { set; get; }
            public int IndexStart { set; get; }
            public int IndexEnd { set; get; }
            public int Similarity { set; get; }
        }
        IList<CursorIndecesMapping> CursorMatch()
        {
            if (!isCursorTemplatesHasProcessed)
            {
                // Compute descriptors for each image
                cursorDescriptorsList = CursorComputeMultipleDescriptors(cursorDBImages/*, out imap*/);
            }
            if (!isCursorTemplatesHasBeenConcatenated)
            {
                // Concatenate all DB images descriptors into single matrix
                cursorDescriptorsConcatenated = CursorConcateDescriptors(cursorDescriptorsList);
            }

            // Compute descriptors for the new query image
            Matrix<float> queryDescriptors = CursorComputeSingleDescriptor();
            CursorFindMatches(cursorDescriptorsConcatenated, queryDescriptors/*, ref imap*/);
            return cursorImap;
        }

        Matrix<float> CursorComputeSingleDescriptor()
        {
            tempCursorSourceImage = new Image<Gray, byte>(cursorSourceSkin.Bitmap);
            Matrix<float> descs;
            cursorDetector = new SURFDetector(surfHessianThresh, surfExtendedFlag);

            VectorOfKeyPoint keyPoints = detector.DetectKeyPointsRaw(tempCursorSourceImage, null);
            descs = detector.ComputeDescriptorsRaw(tempCursorSourceImage, null, keyPoints);
            //descs = detector.DetectAndCompute(source, null, keyPoints);

            return descs;
        }

        Matrix<float> CursorComputeSingleDescriptor(ref Image<Gray,  byte> source)
        {
            Matrix<float> descs;
            cursorDetector = new SURFDetector(surfHessianThresh, surfExtendedFlag);
            
            VectorOfKeyPoint keyPoints = detector.DetectKeyPointsRaw(source, null);
            descs = detector.ComputeDescriptorsRaw(source, null, keyPoints);
            //descs = detector.DetectAndCompute(source, null, keyPoints);
            
            return descs;
        }


        IList<Matrix<float>> CursorComputeMultipleDescriptors(string[] fileNames/*, out IList<IndecesMapping> imap*/)
        {
            cursorImap = new List<CursorIndecesMapping>();
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

        void CursorFindMatches(Matrix<float> dbDescriptors, Matrix<float> queryDescriptors/*, ref IList<IndecesMapping> imap*/)
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
                    for (int j = 0; i < cursorImap.Count; i++)
                    {
                        if (cursorImap[j].IndexStart <= i && cursorImap[j].IndexEnd >= i)
                        {
                            cursorImap[j].Similarity++;
                            break;
                        }
                    }
                    foreach (var img in cursorImap)
                    {
                        if (img.IndexStart <= i && img.IndexEnd >= i)
                        {

                            break;
                        }
                    }
                }
            }
        }

        Matrix<float> CursorConcateDescriptors(IList<Matrix<float>> descriptors)
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
        #endregion Cursor recognition

        #endregion Sample codes
    }
}