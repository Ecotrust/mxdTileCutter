using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecotrust
{
    class GlobalMercator
    {
        /*
         * Code ported to C# from globalmaptiles.py. Derived from 
         *
###############################################################################
# $Id$
#
# Project:	GDAL2Tiles, Google Summer of Code 2007 & 2008
#           Global Map Tiles Classes
# Purpose:	Convert a raster into TMS tiles, create KML SuperOverlay EPSG:4326,
#			generate a simple HTML viewers based on Google Maps and OpenLayers
# Author:	Klokan Petr Pridal, klokan at klokan dot cz
# Web:		http://www.klokan.cz/projects/gdal2tiles/
#
###############################################################################
# Copyright (c) 2008 Klokan Petr Pridal. All rights reserved.
#
# Permission is hereby granted, free of charge, to any person obtaining a
# copy of this software and associated documentation files (the "Software"),
# to deal in the Software without restriction, including without limitation
# the rights to use, copy, modify, merge, publish, distribute, sublicense,
# and/or sell copies of the Software, and to permit persons to whom the
# Software is furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included
# in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
# OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
# THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
# FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
# DEALINGS IN THE SOFTWARE.
###############################################################################
         */

        public struct Coords
        {
            public Double x;
            public Double y;
        }

        public struct Bounds
        {
            public Double minx;
            public Double miny;
            public Double maxx;
            public Double maxy;
        }

        //self.initialResolution = 2 * math.pi * 6378137 / self.tileSize # 156543.03392804062 for tileSize 256 pixels 
        Double initialResolution = 156543.03392804062;
        
        // self.originShift = 2 * math.pi * 6378137 / 2.0 # 20037508.342789244
        Double originShift = 20037508.342789244;

        int tileSize = 256;

        public Double Resolution(int zoom)
        {
            // Resolution (meters/pixel) for given zoom level (measured at Equator)

            return initialResolution / Math.Pow(2, zoom);
        }

        public Coords MetersToPixels(Double mx, Double my, int zoom)
        {
            //Converts EPSG:900913 to pyramid pixel coordinates in given zoom level

            Double res = Resolution(zoom);            
            
            Coords outc;
            outc.x = (mx + originShift) / res;
            outc.y = (my + originShift) / res;

            return outc;
        }

        public Coords PixelsToTile(Coords tempc)
        {
		    // Returns a tile covering region in given pixel coordinates

            Coords outc;
            outc.x = Math.Ceiling(tempc.x / 256.0) - 1;
            outc.y = Math.Ceiling(tempc.y / 256.0) - 1;
            return outc;
        }

        public Coords MetersToTile(Double mx, Double my, int zoom)
        {
            // Returns tile coords for given mercator coordinates
            Coords tempc = MetersToPixels(mx, my, zoom);
            Coords outc = PixelsToTile(tempc);
            return outc;
        }

        public Coords PixelsToMeters(Double px, Double py, int zoom)
        {

            // Converts pixel coordinates in given zoom level of pyramid to EPSG:900913

            Double res = Resolution(zoom);

            Coords outc;
            outc.x = px * res - originShift;
            outc.y = py * res - originShift;
            return outc;
        }

        public Bounds TileBounds(Double tx, Double ty, int zoom)
        {
            // Returns bounds of the given tile in EPSG:900913 coordinates	
	
            Coords mins = PixelsToMeters(tx * tileSize, ty * tileSize, zoom);
            Coords maxs = PixelsToMeters( (tx+1) * tileSize, (ty+1) * tileSize, zoom);
            Bounds bnd;
            bnd.minx = mins.x;
            bnd.maxx = maxs.x;
            bnd.miny = mins.y;
            bnd.maxy = maxs.y;
            return bnd;
        }
    }
}
