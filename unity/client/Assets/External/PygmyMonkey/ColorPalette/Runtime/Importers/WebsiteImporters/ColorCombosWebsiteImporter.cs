using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace PygmyMonkey.ColorPalette
{
	public static class ColorCombosWebsiteImporter
	{
		public static ColorPalette Import(Uri uri)
		{
			ColorPalette colorPalette = new ColorPalette();
			colorPalette.name = uri.AbsolutePath;

			WebClient client = new WebClient();
			using (Stream stream = client.OpenRead(uri))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						if (line.Contains("href") && line.Contains("h1"))
						{
							int startIndex = line.IndexOf(">", line.IndexOf("<h1>") + 4) + 1;
							colorPalette.name = line.Substring(startIndex, line.IndexOf("</a>") - startIndex);
						}
						
						if (line.Contains("var global_hex_colors"))
						{
							string hexColorsString = line.Substring(line.IndexOf("'") + 1, line.LastIndexOf("'") - line.IndexOf("'") - 1);
							
							string[] hexColorArray = hexColorsString.Split(',');
							foreach (string hexColor in hexColorArray)
							{
								colorPalette.colorInfoList.Add(new ColorInfo(null, ColorUtils.HexToColor(hexColor)));
							}
						}
					}
				}
			}
			
			if (colorPalette.colorInfoList == null || colorPalette.colorInfoList.Count == 0)
			{
				throw new UnityException("Error getting palette from the website: " + uri.AbsoluteUri + ". Please contact us at tools@pygmymonkey.com :D");
			}
			
			return colorPalette;
		}
	}
}