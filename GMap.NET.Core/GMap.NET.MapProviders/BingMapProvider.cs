﻿
namespace GMap.NET.MapProviders
{
   using System;
   using System.Text;
   using GMap.NET.Projections;
   using System.Diagnostics;
   using System.Net;
   using System.IO;
   using System.Text.RegularExpressions;

   public abstract class BingMapProviderBase : GMapProvider
   {
      public string Version = "689";

      /// <summary>
      /// Bing Maps Customer Identification, more info here
      /// http://msdn.microsoft.com/en-us/library/bb924353.aspx
      /// </summary>
      public string ClientToken = null;

      /// <summary>
      /// Converts tile XY coordinates into a QuadKey at a specified level of detail.
      /// </summary>
      /// <param name="tileX">Tile X coordinate.</param>
      /// <param name="tileY">Tile Y coordinate.</param>
      /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
      /// to 23 (highest detail).</param>
      /// <returns>A string containing the QuadKey.</returns>
      internal string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
      {
         StringBuilder quadKey = new StringBuilder();
         for(int i = levelOfDetail; i > 0; i--)
         {
            char digit = '0';
            int mask = 1 << (i - 1);
            if((tileX & mask) != 0)
            {
               digit++;
            }
            if((tileY & mask) != 0)
            {
               digit++;
               digit++;
            }
            quadKey.Append(digit);
         }
         return quadKey.ToString();
      }

      #region GMapProvider Members
      public override Guid Id
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public override string Name
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public override PureProjection Projection
      {
         get
         {
            return MercatorProjection.Instance;
         }
      }

      GMapProvider[] overlays;
      public override GMapProvider[] Overlays
      {
         get
         {
            if(overlays == null)
            {
               overlays = new GMapProvider[] { this };
            }
            return overlays;
         }
      }

      public override PureImage GetTileImage(GPoint pos, int zoom)
      {
         throw new NotImplementedException();
      }
      #endregion

      public bool TryCorrectVersion = true;
      static bool init = false;

      public override void OnInitialized()
      {
         if(!init && TryCorrectVersion)
         {
            string url = @"http://www.bing.com/maps";
            try
            {
               HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
               if(WebProxy != null)
               {
                  request.Proxy = WebProxy;
#if !PocketPC
                  request.PreAuthenticate = true;
#endif
               }

               request.UserAgent = UserAgent;
               request.Timeout = TimeoutMs;
               request.ReadWriteTimeout = TimeoutMs * 6;

               using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
               {
                  using(Stream responseStream = response.GetResponseStream())
                  {
                     using(StreamReader read = new StreamReader(responseStream))
                     {
                        string html = read.ReadToEnd();

                        Regex reg = new Regex("http://ecn.t(\\d*).tiles.virtualearth.net/tiles/r(\\d*)[?*]g=(\\d*)", RegexOptions.IgnoreCase);
                        Match mat = reg.Match(html);
                        if(mat.Success)
                        {
                           GroupCollection gc = mat.Groups;
                           int count = gc.Count;
                           if(count > 2)
                           {
                              GMapProviders.BingMap.Version = gc[3].Value;
                              GMapProviders.BingMapOld.Version = GMapProviders.BingMap.Version;
                              GMapProviders.BingSatelliteMap.Version = GMapProviders.BingMap.Version;
                              GMapProviders.BingHybridMap.Version = GMapProviders.BingMap.Version;

                              Debug.WriteLine("GMapProviders.BingMap.Version: " + GMapProviders.BingMap.Version);
                           }
                        }
                     }
                  }
               }
               init = true; // try it only once
            }
            catch(Exception ex)
            {
               Debug.WriteLine("TryCorrectBingVersions failed: " + ex.ToString());
            }
         }
      }
   }

   /// <summary>
   /// BingMapProvider provider
   /// </summary>
   public class BingMapProvider : BingMapProviderBase
   {
      public static readonly BingMapProvider Instance;

      BingMapProvider()
      {
      }

      static BingMapProvider()
      {
         Instance = new BingMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("D0CEB371-F10A-4E12-A2C1-DF617D6674A8");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "BingMap";
      public override string Name
      {
         get
         {
            return name;
         }
      }

      public override PureImage GetTileImage(GPoint pos, int zoom)
      {
         string url = MakeTileImageUrl(pos, zoom, Language);

         return GetTileImageUsingHttp(url);
      }

      #endregion

      string MakeTileImageUrl(GPoint pos, int zoom, string language)
      {
         string key = TileXYToQuadKey(pos.X, pos.Y, zoom);
         return string.Format(UrlFormat, GetServerNum(pos, 4), key, Version, language, (!string.IsNullOrEmpty(ClientToken) ? "&token=" + ClientToken : string.Empty));
      }

      static readonly string UrlFormat = "http://ecn.t{0}.tiles.virtualearth.net/tiles/r{1}.png?g={2}&mkt={3}{4}&lbl=l1&stl=h&shading=hill&n=z";
   }
}