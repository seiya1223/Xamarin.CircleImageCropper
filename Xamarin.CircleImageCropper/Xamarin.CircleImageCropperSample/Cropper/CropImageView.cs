using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.CircleImageCropperSample.CropWindow;
using Xamarin.CircleImageCropperSample.Cropwindow.Pair;
using Xamarin.CircleImageCropperSample.Util;
using Edge = Android.Views.Edge;

namespace Xamarin.CircleImageCropperSample.Cropper
{
    public class CropImageView : FrameLayout
    {
        // Private Constants ///////////////////////////////////////////////////////

        private static Rect EMPTY_RECT = new Rect();

        // Member Variables ////////////////////////////////////////////////////////

        // Sets the default image guidelines to show when resizing
        public static int DEFAULT_GUIDELINES = 1;
        public static bool DEFAULT_FIXED_ASPECT_RATIO = true;
        public static int DEFAULT_ASPECT_RATIO_X = 1;
        public static int DEFAULT_ASPECT_RATIO_Y = 1;

        private static int DEFAULT_IMAGE_RESOURCE = 0;

        private static String DEGREES_ROTATED = "DEGREES_ROTATED";

        private ImageView mImageView;
        private CropOverlayView mCropOverlayView;

        private Bitmap mBitmap;
        private int mDegreesRotated = 0;

        private int mLayoutWidth;
        private int mLayoutHeight;

        // Instance variables for customizable attributes
        private int mGuidelines = DEFAULT_GUIDELINES;
        private bool mFixAspectRatio = DEFAULT_FIXED_ASPECT_RATIO;
        private int mAspectRatioX = DEFAULT_ASPECT_RATIO_X;
        private int mAspectRatioY = DEFAULT_ASPECT_RATIO_Y;
        private int mImageResource = DEFAULT_IMAGE_RESOURCE;

        // Constructors ////////////////////////////////////////////////////////////

        public CropImageView(Context context)
            : base(context)
        {

            init(context);
        }

        public CropImageView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            TypedArray ta = context.ObtainStyledAttributes(attrs, Resource.Styleable.CropImageView, 0, 0);

            try
            {
                mGuidelines = ta.GetInteger(Resource.Styleable.CropImageView_guidelines, DEFAULT_GUIDELINES);
                mFixAspectRatio = ta.GetBoolean(Resource.Styleable.CropImageView_fixAspectRatio, DEFAULT_FIXED_ASPECT_RATIO);
                mAspectRatioX = ta.GetInteger(Resource.Styleable.CropImageView_aspectRatioX, DEFAULT_ASPECT_RATIO_X);
                mAspectRatioY = ta.GetInteger(Resource.Styleable.CropImageView_aspectRatioY, DEFAULT_ASPECT_RATIO_Y);
                mImageResource = ta.GetResourceId(Resource.Styleable.CropImageView_imageResource, DEFAULT_IMAGE_RESOURCE);
            }
            finally
            {
                ta.Recycle();
            }

            init(context);
        }

        // View Methods ////////////////////////////////////////////////////////////

        protected override IParcelable OnSaveInstanceState()
        {

            Bundle bundle = new Bundle();

            bundle.PutParcelable("instanceState", base.OnSaveInstanceState());
            bundle.PutInt(DEGREES_ROTATED, mDegreesRotated);

            return bundle;

        }

        protected override void OnRestoreInstanceState(IParcelable parcelable)
        {

            if (parcelable is Bundle)
            {

                Bundle bundle = (Bundle)parcelable;

                // Fixes the rotation of the image when orientation changes.
                mDegreesRotated = bundle.GetInt(DEGREES_ROTATED);
                int tempDegrees = mDegreesRotated;
                rotateImage(mDegreesRotated);
                mDegreesRotated = tempDegrees;
                //TODO: THIS SHOULD WORK, FIX
                base.OnRestoreInstanceState(bundle.GetParcelable("instanceState").JavaCast<IParcelable>());

            }
            else
            {
                base.OnRestoreInstanceState(parcelable);
            }
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {

            if (mBitmap != null)
            {
                Rect bitmapRect = ImageViewUtil.getBitmapRectCenterInside(mBitmap, this);
                mCropOverlayView.setBitmapRect(bitmapRect);
            }
            else
            {
                mCropOverlayView.setBitmapRect(EMPTY_RECT);
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {

            int widthMode = (int)MeasureSpec.GetMode(widthMeasureSpec);
            int widthSize = (int)MeasureSpec.GetSize(widthMeasureSpec);
            int heightMode = (int)MeasureSpec.GetMode(heightMeasureSpec);
            int heightSize = (int)MeasureSpec.GetSize(heightMeasureSpec);

            if (mBitmap != null)
            {

                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

                // Bypasses a baffling bug when used within a ScrollView, where
                // heightSize is set to 0.
                if (heightSize == 0)
                    heightSize = mBitmap.Height;

                int desiredWidth;
                int desiredHeight;

                double viewToBitmapWidthRatio = Double.PositiveInfinity;
                double viewToBitmapHeightRatio = Double.PositiveInfinity;

                // Checks if either width or height needs to be fixed
                if (widthSize < mBitmap.Width)
                {
                    viewToBitmapWidthRatio = (double)widthSize / (double)mBitmap.Width;
                }
                if (heightSize < mBitmap.Height)
                {
                    viewToBitmapHeightRatio = (double)heightSize / (double)mBitmap.Height;
                }

                // If either needs to be fixed, choose smallest ratio and calculate
                // from there
                if (viewToBitmapWidthRatio != Double.PositiveInfinity || viewToBitmapHeightRatio != Double.PositiveInfinity)
                {
                    if (viewToBitmapWidthRatio <= viewToBitmapHeightRatio)
                    {
                        desiredWidth = widthSize;
                        desiredHeight = (int)(mBitmap.Height * viewToBitmapWidthRatio);
                    }
                    else
                    {
                        desiredHeight = heightSize;
                        desiredWidth = (int)(mBitmap.Width * viewToBitmapHeightRatio);
                    }
                }

                // Otherwise, the picture is within frame layout bounds. Desired
                // width is
                // simply picture size
                else
                {
                    desiredWidth = mBitmap.Width;
                    desiredHeight = mBitmap.Height;
                }

                int width = getOnMeasureSpec(widthMode, widthSize, desiredWidth);
                int height = getOnMeasureSpec(heightMode, heightSize, desiredHeight);

                mLayoutWidth = width;
                mLayoutHeight = height;

                Rect bitmapRect = ImageViewUtil.getBitmapRectCenterInside(mBitmap.Width,
                                                                               mBitmap.Height,
                                                                               mLayoutWidth,
                                                                               mLayoutHeight);
                mCropOverlayView.setBitmapRect(bitmapRect);

                // MUST CALL THIS
                SetMeasuredDimension(mLayoutWidth, mLayoutHeight);

            }
            else
            {

                mCropOverlayView.setBitmapRect(EMPTY_RECT);
                SetMeasuredDimension(widthSize, heightSize);
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {

            base.OnLayout(changed, l, t, r, b);

            if (mLayoutWidth > 0 && mLayoutHeight > 0)
            {
                // Gets original parameters, and creates the new parameters
                ViewGroup.LayoutParams origparams = this.LayoutParameters;
                origparams.Width = mLayoutWidth;
                origparams.Height = mLayoutHeight;
                this.LayoutParameters = origparams;
            }
        }

        // Public Methods //////////////////////////////////////////////////////////

        /**
         * Returns the integer of the imageResource
         * 
         * @param int the image resource id
         */
        public int getImageResource()
        {
            return mImageResource;
        }

        /**
         * Sets a Bitmap as the content of the CropImageView.
         * 
         * @param bitmap the Bitmap to set
         */
        public void setImageBitmap(Bitmap bitmap)
        {

            mBitmap = bitmap;
            mImageView.SetImageBitmap(mBitmap);

            if (mCropOverlayView != null)
            {
                mCropOverlayView.resetCropOverlayView();
            }
        }

        /**
         * Sets a Bitmap and initializes the image rotation according to the EXIT data.
         * <p>
         * The EXIF can be retrieved by doing the following:
         * <code>ExifInterface exif = new ExifInterface(path);</code>
         * 
         * @param bitmap the original bitmap to set; if null, this
         * @param exif the EXIF information about this bitmap; may be null
         */
        public void setImageBitmap(Bitmap bitmap, ExifInterface exif)
        {

            if (bitmap == null)
            {
                return;
            }

            if (exif == null)
            {
                setImageBitmap(bitmap);
                return;
            }

            Matrix matrix = new Matrix();
            int orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, 1);
            int rotate = -1;
            //TODO CHECK THIS FIX
            switch (orientation)
            {
                case (int)Android.Media.Orientation.Rotate270:
                    rotate = 270;
                    break;
                case (int)Android.Media.Orientation.Rotate180:
                    rotate = 180;
                    break;
                case (int)Android.Media.Orientation.Rotate90:
                    rotate = 90;
                    break;
            }

            if (rotate == -1)
            {
                setImageBitmap(bitmap);
            }
            else
            {
                matrix.PostRotate(rotate);
                Bitmap rotatedBitmap = Bitmap.CreateBitmap(bitmap,
                                                                0,
                                                                0,
                                                                bitmap.Width,
                                                                bitmap.Height,
                                                                matrix,
                                                                true);
                setImageBitmap(rotatedBitmap);
                bitmap.Recycle();
            }
        }

        /**
         * Sets a Drawable as the content of the CropImageView.
         * 
         * @param resId the drawable resource ID to set
         */
        public void setImageResource(int resId)
        {
            if (resId != 0)
            {
                Bitmap bitmap = BitmapFactory.DecodeResource(Resources, resId);
                setImageBitmap(bitmap);
            }
        }

        /**
         * Gets the cropped image based on the current crop window.
         * 
         * @return a new Bitmap representing the cropped image
         */
        public Bitmap getCroppedImage()
        {

            Rect displayedImageRect = ImageViewUtil.getBitmapRectCenterInside(mBitmap, mImageView);

            // Get the scale factor between the actual Bitmap dimensions and the
            // displayed dimensions for width.
            float actualImageWidth = mBitmap.Width;
            float displayedImageWidth = displayedImageRect.Width();
            float scaleFactorWidth = actualImageWidth / displayedImageWidth;

            // Get the scale factor between the actual Bitmap dimensions and the
            // displayed dimensions for height.
            float actualImageHeight = mBitmap.Height;
            float displayedImageHeight = displayedImageRect.Height();
            float scaleFactorHeight = actualImageHeight / displayedImageHeight;

            // Get crop window position relative to the displayed image.
            float cropWindowX = Edge.LEFT.getCoordinate() - displayedImageRect.Left;
            float cropWindowY = Edge.TOP.getCoordinate() - displayedImageRect.Top;
            float cropWindowWidth = Edge.getWidth();
            float cropWindowHeight = Edge.getHeight();

            // Scale the crop window position to the actual size of the Bitmap.
            float actualCropX = cropWindowX * scaleFactorWidth;
            float actualCropY = cropWindowY * scaleFactorHeight;
            float actualCropWidth = cropWindowWidth * scaleFactorWidth;
            float actualCropHeight = cropWindowHeight * scaleFactorHeight;

            // Crop the subset from the original Bitmap.
            Bitmap croppedBitmap = Bitmap.CreateBitmap(mBitmap,
                                                            (int)actualCropX,
                                                            (int)actualCropY,
                                                            (int)actualCropWidth,
                                                            (int)actualCropHeight);

            return croppedBitmap;
        }

        /**
         * Gets the cropped circle image based on the current crop selection.
         * 
         * @return a new Circular Bitmap representing the cropped image
         */
        public Bitmap getCroppedCircleImage()
        {
            Bitmap bitmap = getCroppedImage();

            Bitmap output = Bitmap.CreateBitmap(bitmap.Width,
                    bitmap.Height, Android.Graphics.Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(output);
            //TODO: FIX THIS
            int color = 0xff424242;
            Paint paint = new Paint();
            Rect rect = new Rect(0, 0, bitmap.Width, bitmap.Height);

            paint.AntiAlias = true;
            canvas.DrawARGB(0, 0, 0, 0);
            paint.Color = color;
            // canvas.drawRoundRect(rectF, roundPx, roundPx, paint);
            canvas.DrawCircle(bitmap.Width / 2, bitmap.Height / 2,
                    bitmap.Width / 2, paint);
            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
            canvas.DrawBitmap(bitmap, rect, rect, paint);
            //Bitmap _bmp = Bitmap.createScaledBitmap(output, 60, 60, false);
            //return _bmp;
            return output;
        }

        /**
         * Gets the crop window's position relative to the source Bitmap (not the image
         * displayed in the CropImageView).
         * 
         * @return a RectF instance containing cropped area boundaries of the source Bitmap
         */
        public RectF getActualCropRect()
        {

            Rect displayedImageRect = ImageViewUtil.getBitmapRectCenterInside(mBitmap, mImageView);

            // Get the scale factor between the actual Bitmap dimensions and the
            // displayed dimensions for width.
            float actualImageWidth = mBitmap.Width;
            float displayedImageWidth = displayedImageRect.Width();
            float scaleFactorWidth = actualImageWidth / displayedImageWidth;

            // Get the scale factor between the actual Bitmap dimensions and the
            // displayed dimensions for height.
            float actualImageHeight = mBitmap.Height;
            float displayedImageHeight = displayedImageRect.Height();
            float scaleFactorHeight = actualImageHeight / displayedImageHeight;

            // Get crop window position relative to the displayed image.
            float displayedCropLeft = EdgeType.LEFT.getCoordinate() - displayedImageRect.Left;
            float displayedCropTop = EdgeType.TOP.getCoordinate() - displayedImageRect.Top;
            float displayedCropWidth = EdgeType.getWidth();
            float displayedCropHeight = EdgeType.getHeight();

            // Scale the crop window position to the actual size of the Bitmap.
            float actualCropLeft = displayedCropLeft * scaleFactorWidth;
            float actualCropTop = displayedCropTop * scaleFactorHeight;
            float actualCropRight = actualCropLeft + displayedCropWidth * scaleFactorWidth;
            float actualCropBottom = actualCropTop + displayedCropHeight * scaleFactorHeight;

            // Correct for floating point errors. Crop rect boundaries should not
            // exceed the source Bitmap bounds.
            actualCropLeft = Math.Max(0f, actualCropLeft);
            actualCropTop = Math.Max(0f, actualCropTop);
            actualCropRight = Math.Min(mBitmap.Width, actualCropRight);
            actualCropBottom = Math.Min(mBitmap.Height, actualCropBottom);

            RectF actualCropRect = new RectF(actualCropLeft,
                                                  actualCropTop,
                                                  actualCropRight,
                                                  actualCropBottom);

            return actualCropRect;
        }

        /**
         * Sets whether the aspect ratio is fixed or not; true fixes the aspect ratio, while
         * false allows it to be changed.
         * 
         * @param fixAspectRatio bool that signals whether the aspect ratio should be
         *            maintained.
         */
        public void setFixedAspectRatio(bool fixAspectRatio)
        {
            mCropOverlayView.setFixedAspectRatio(fixAspectRatio);
        }

        /**
         * Sets the guidelines for the CropOverlayView to be either on, off, or to show when
         * resizing the application.
         * 
         * @param guidelines Integer that signals whether the guidelines should be on, off, or
         *            only showing when resizing.
         */
        public void setGuidelines(int guidelines)
        {
            mCropOverlayView.setGuidelines(guidelines);
        }

        /**
         * Sets the both the X and Y values of the aspectRatio.
         * 
         * @param aspectRatioX int that specifies the new X value of the aspect ratio
         * @param aspectRatioX int that specifies the new Y value of the aspect ratio
         */
        public void setAspectRatio(int aspectRatioX, int aspectRatioY)
        {
            mAspectRatioX = aspectRatioX;
            mCropOverlayView.setAspectRatioX(mAspectRatioX);

            mAspectRatioY = aspectRatioY;
            mCropOverlayView.setAspectRatioY(mAspectRatioY);
        }

        /**
         * Rotates image by the specified number of degrees clockwise. Cycles from 0 to 360
         * degrees.
         * 
         * @param degrees Integer specifying the number of degrees to rotate.
         */
        public void rotateImage(int degrees)
        {

            Matrix matrix = new Matrix();
            matrix.PostRotate(degrees);
            mBitmap = Bitmap.CreateBitmap(mBitmap, 0, 0, mBitmap.Width, mBitmap.Height, matrix, true);
            setImageBitmap(mBitmap);

            mDegreesRotated += degrees;
            mDegreesRotated = mDegreesRotated % 360;
        }

        // Private Methods /////////////////////////////////////////////////////////

        private void init(Context context)
        {

            LayoutInflater inflater = LayoutInflater.From(context);
            View v = inflater.Inflate(Resource.Layout.crop_image_view, this, true);

            mImageView = v.FindViewById<ImageView>(Resource.Id.ImageView_image);

            setImageResource(mImageResource);
            mCropOverlayView = v.FindViewById<CropOverlayView>(Resource.Id.CropOverlayView);
            mCropOverlayView.setInitialAttributeValues(mGuidelines, mFixAspectRatio, mAspectRatioX, mAspectRatioY);
        }

        /**
         * Determines the specs for the onMeasure function. Calculates the width or height
         * depending on the mode.
         * 
         * @param measureSpecMode The mode of the measured width or height.
         * @param measureSpecSize The size of the measured width or height.
         * @param desiredSize The desired size of the measured width or height.
         * @return The  size of the width or height.
         */
        private static int getOnMeasureSpec(int measureSpecMode, int measureSpecSize, int desiredSize)
        {

            // Measure Width
            int spec;
            if (measureSpecMode == (int)MeasureSpecMode.Exactly)
            {
                // Must be this size
                spec = measureSpecSize;
            }
            else if (measureSpecMode == (int)MeasureSpecMode.AtMost)
            {
                // Can't be bigger than...; match_parent value
                spec = Math.Min(desiredSize, measureSpecSize);
            }
            else
            {
                // Be whatever you want; wrap_content
                spec = desiredSize;
            }

            return spec;
        }
    }
}