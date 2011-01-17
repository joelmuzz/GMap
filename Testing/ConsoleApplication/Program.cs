﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GMap.NET.Projections;
using DotSpatial.Projections;
using System.Diagnostics;
using GMap.NET;
using GMap.NET.Internals;

namespace ConsoleApplication
{
   class Program
   {
      static void Main(string[] args)
      {
         //if(false)
         {
            Core Core = new Core();
            {
               int Width = 1024;
               int Height = 1024;

               Core.OnMapSizeChanged(Width, Height, false);
               Core.currentRegion = new GRect(-50, -50, Core.Width + 50, Core.Height + 50);

               Core.Zoom = 3;
            }
            Core.StartSystem();

            {
               // keep center on same position, after size changed
               Core.GoToCurrentPosition();

               Core.CurrentPosition = new PointLatLng(0, 111);
            }
         }

         #region -- GetDistanceInMeters test --
         if(false)
         {
            int zoom = 11;
            var p1 = new PointLatLng(54.897894682306, 23.9374116651471);
            var p2 = new PointLatLng(54.8969580002102, 23.937305873711);

            var pr = new LKS94Projection();
            //var pr = new MercatorProjection();

            {
               var px1 = pr.FromLatLngToPixel(p1, zoom);
               var px2 = pr.FromLatLngToPixel(p2, zoom);

               var d = pr.GetDistanceInPixels(px1, px2);
               var r = pr.GetGroundResolution(zoom, p1.Lat);
               var dd = r * d;

               var dd2 = pr.GetDistanceInMeters(p1, p2);

               Debug.WriteLine("d1: " + dd + "m");
               Debug.WriteLine("d2: " + dd2 + "m");
            }
         }
         #endregion

         #region -- MapyCZProjection test --
         if(false)
         {
            double x = 25;
            double y = 50;

            //Sets up a array to contain the x and y coordinates
            double[] xy = new double[2];
            xy[0] = x;
            xy[1] = y;

            //An array for the z coordinate
            double[] z = new double[1];
            z[0] = 1;

            Debug.WriteLine("first0: " + xy[0] + "; " + xy[1]);
            Debug.WriteLine("");

            ProjectionInfo pStart = KnownCoordinateSystems.Geographic.World.WGS1984;
            ProjectionInfo pEnd = new ProjectionInfo("+proj=tmerc +lat_0=0 +lon_0=15 +k=0.9996 +x_0=4200000 +y_0=-1300000 +ellps=WGS84 +datum=WGS84 +to_meter=0.03125 +no_defs");
            Reproject.ReprojectPoints(xy, z, pStart, pEnd, 0, 1);

            Debug.WriteLine(" true1: " + (int) xy[0] + "; " + (int) xy[1]);

            var prj = new MapyCZProjection();
            {
               var p2 = prj.WGSToPP(y, x);

               Debug.WriteLine("false1: " + p2[0] + "; " + p2[1]);

               var p3 = prj.PPToWGS(p2[0], p2[1]);

               Reproject.ReprojectPoints(xy, z, pEnd, pStart, 0, 1);

               Debug.WriteLine("");
               Debug.WriteLine(" true2: " + xy[0] + "; " + xy[1]);
               Debug.WriteLine("false2: " + p3[1] + "; " + p3[0]);
            }
            // 134400000],PARAMETER["false_northing",-41600000
         }
         #endregion

         Console.ReadLine();
      }
   }
}
