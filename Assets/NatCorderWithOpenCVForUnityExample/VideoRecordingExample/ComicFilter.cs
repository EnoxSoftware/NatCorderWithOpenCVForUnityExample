using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using System;

namespace NatCorderWithOpenCVForUnityExample
{
    public class ComicFilter
    {
        Mat grayMat;
        Mat lineMat;
        Mat maskMat;
        Mat bgMat;
        Mat grayDstMat;
        byte[] grayPixels;
        byte[] maskPixels;

        public void Process(Mat src, Mat dst)
        {
            if (src == null)
                throw new ArgumentNullException("src == null");

            if (dst == null)
                throw new ArgumentNullException("dst == null");

            if (grayMat != null && (grayMat.width() != src.width() || grayMat.height() != src.height()))
            {
                grayMat.Dispose();
                grayMat = null;
                lineMat.Dispose();
                lineMat = null;
                maskMat.Dispose();
                maskMat = null;
                bgMat.Dispose();
                bgMat = null;
                grayDstMat.Dispose();
                grayDstMat = null;

                grayPixels = null;
                maskPixels = null;
            }
            grayMat = grayMat ?? new Mat(src.height(), src.width(), CvType.CV_8UC1);
            lineMat = lineMat ?? new Mat(src.height(), src.width(), CvType.CV_8UC1);
            maskMat = maskMat ?? new Mat(src.height(), src.width(), CvType.CV_8UC1);
            //create a striped background.
            bgMat = new Mat(src.height(), src.width(), CvType.CV_8UC1, new Scalar(255));
            for (int i = 0; i < bgMat.rows() * 2.5f; i = i + 4)
            {
                Imgproc.line(bgMat, new Point(0, 0 + i), new Point(bgMat.cols(), -bgMat.cols() + i), new Scalar(0), 1);
            }
            grayDstMat = grayDstMat ?? new Mat(src.height(), src.width(), CvType.CV_8UC1);

            grayPixels = grayPixels ?? new byte[grayMat.cols() * grayMat.rows() * grayMat.channels()];
            maskPixels = maskPixels ?? new byte[maskMat.cols() * maskMat.rows() * maskMat.channels()];


            Imgproc.cvtColor(src, grayMat, Imgproc.COLOR_RGBA2GRAY);
            bgMat.copyTo(grayDstMat);
            Imgproc.GaussianBlur(grayMat, lineMat, new Size(3, 3), 0);
            grayMat.get(0, 0, grayPixels);

            for (int i = 0; i < grayPixels.Length; i++)
            {
                maskPixels[i] = 0;
                if (grayPixels[i] < 70)
                {
                    grayPixels[i] = 0;
                    maskPixels[i] = 1;
                }
                else if (70 <= grayPixels[i] && grayPixels[i] < 120)
                {
                    grayPixels[i] = 100;
                }
                else
                {
                    grayPixels[i] = 255;
                    maskPixels[i] = 1;
                }
            }

            grayMat.put(0, 0, grayPixels);
            maskMat.put(0, 0, maskPixels);
            grayMat.copyTo(grayDstMat, maskMat);

            Imgproc.Canny(lineMat, lineMat, 20, 120);
            lineMat.copyTo(maskMat);
            Core.bitwise_not(lineMat, lineMat);
            lineMat.copyTo(grayDstMat, maskMat);

            Imgproc.cvtColor(grayDstMat, dst, Imgproc.COLOR_GRAY2RGBA);
        }

        public void Dispose()
        {
            if (grayMat != null)
            {
                grayMat.Dispose();
                grayMat = null;
            }
            if (lineMat != null)
            {
                lineMat.Dispose();
                lineMat = null;
            }
            if (maskMat != null)
            {
                maskMat.Dispose();
                maskMat = null;
            }
            if (bgMat != null)
            {
                bgMat.Dispose();
                bgMat = null;
            }
            if (grayDstMat != null)
            {
                grayDstMat.Dispose();
                grayDstMat = null;
            }

            grayPixels = null;
            maskPixels = null;
        }
    }
}