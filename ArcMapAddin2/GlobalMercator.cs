using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecotrust
{
    class GlobalMercator
    {
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

        public Coords MetersToTMSTile(Double mx, Double my, int zoom)
        {
            // Returns tile coords for given mercator coordinates
            Coords tempc = MetersToPixels(mx, my, zoom);
            Coords outc = PixelsToTile(tempc);
            return outc;
        }

        public Coords MetersToXYZTile(Double mx, Double my, int zoom)
        {
            // Returns XYZ (top-origin) tile for given mercator coordinates
            Coords tempc = MetersToPixels(mx, my, zoom);
            Coords outc = PixelsToTile(tempc);
            outc.y = (Math.Pow(2, zoom) - 1) - outc.y; // flip y axis
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
