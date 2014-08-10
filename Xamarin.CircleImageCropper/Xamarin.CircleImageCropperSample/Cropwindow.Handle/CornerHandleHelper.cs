using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Xamarin.CircleImageCropperSample.Cropwindow.Pair;

namespace Xamarin.CircleImageCropperSample.Cropwindow.Handle
{
    public class CornerHandleHelper : HandleHelper
    {

        public CornerHandleHelper(EdgeType horizontalEdge, EdgeType verticalEdge)
            : base(horizontalEdge, verticalEdge)
        {

        }

        // HandleHelper Methods ////////////////////////////////////////////////////

        public override void updateCropWindow(float x,
                              float y,
                              float targetAspectRatio,
                              Rect imageRect,
                              float snapRadius)
        {

            EdgePair activeEdges = getActiveEdges(x, y, targetAspectRatio);
            EdgeType primaryEdge = activeEdges.primary;
            EdgeType secondaryEdge = activeEdges.secondary;

            primaryEdge.adjustCoordinate(x, y, imageRect, snapRadius, targetAspectRatio);
            secondaryEdge.adjustCoordinate(targetAspectRatio);

            if (secondaryEdge.isOutsideMargin(imageRect, snapRadius))
            {
                secondaryEdge.snapToRect(imageRect);
                primaryEdge.adjustCoordinate(targetAspectRatio);
            }
        }
    }
}