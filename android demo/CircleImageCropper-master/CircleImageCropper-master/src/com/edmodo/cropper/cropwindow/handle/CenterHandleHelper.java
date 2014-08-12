/*
 * Copyright 2013, Edmodo, Inc. 
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this work except in compliance with the License.
 * You may obtain a copy of the License in the LICENSE file, or at:
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" 
 * BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language 
 * governing permissions and limitations under the License. 
 */

package com.edmodo.cropper.cropwindow.handle;

import android.graphics.Rect;

import com.edmodo.cropper.cropwindow.edge.Edge;
import com.edmodo.cropper.cropwindow.edge.EdgeHelper;

/**
 * HandleHelper class to handle the center handle.
 */
class CenterHandleHelper extends HandleHelper 
{
    // Constructor /////////////////////////////////////////////////////////////

    CenterHandleHelper() 
    {
        super(null, null);
    }

    // HandleHelper Methods ////////////////////////////////////////////////////

    @Override
    void updateCropWindow(float x,
                          float y,
                          Rect imageRect,
                          float snapRadius) 
    {

        float left = EdgeHelper.LEFT.coordinate;
        float top = EdgeHelper.TOP.coordinate;
        float right = EdgeHelper.RIGHT.coordinate;
        float bottom = EdgeHelper.BOTTOM.coordinate;

        final float currentCenterX = (left + right) / 2;
        final float currentCenterY = (top + bottom) / 2;

        final float offsetX = x - currentCenterX;
        final float offsetY = y - currentCenterY;

        // Adjust the crop window.
        EdgeHelper.LEFT.offset(offsetX);
        EdgeHelper.TOP.offset(offsetY);
        EdgeHelper.RIGHT.offset(offsetX);
        EdgeHelper.BOTTOM.offset(offsetY);

        // Check if we have gone out of bounds on the sides, and fix.
        if (EdgeHelper.LEFT.isOutsideMargin(imageRect, snapRadius)) 
        {
            final float offset = EdgeHelper.LEFT.snapToRect(imageRect);
            EdgeHelper.RIGHT.offset(offset);
        }
        else 
    	if (EdgeHelper.RIGHT.isOutsideMargin(imageRect, snapRadius)) 
    	{
            final float offset = EdgeHelper.RIGHT.snapToRect(imageRect);
            EdgeHelper.LEFT.offset(offset);
        }

        // Check if we have gone out of bounds on the top or bottom, and fix.
        if (EdgeHelper.TOP.isOutsideMargin(imageRect, snapRadius)) 
        {
            final float offset = EdgeHelper.TOP.snapToRect(imageRect);
            EdgeHelper.BOTTOM.offset(offset);
        } 
        else 
    	if (EdgeHelper.BOTTOM.isOutsideMargin(imageRect, snapRadius)) 
    	{
            final float offset = EdgeHelper.BOTTOM.snapToRect(imageRect);
            EdgeHelper.TOP.offset(offset);
        }
    }

    @Override
    void updateCropWindow(float x,
                          float y,
                          float targetAspectRatio,
                          Rect imageRect,
                          float snapRadius) {

        updateCropWindow(x, y, imageRect, snapRadius);
    }
}