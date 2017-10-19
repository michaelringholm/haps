using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace PygmyMonkey.ColorPalette
{
	public static class ColourLoversWebsiteImporter
	{
		public static ColorPalette Import(Uri uri)
		{
			ColorPalette colorPalette = new ColorPalette();
			colorPalette.name = uri.AbsolutePath;

			if (uri.AbsolutePath.Contains("/pattern/template/"))
			{
				throw new UnityException("Sorry, we do not support getting 'pattern templates' from colourLovers.com.");
			}
			else if (uri.AbsolutePath.Contains("/shape/"))
			{
				throw new UnityException("Sorry, we do not support getting 'shapes' from colourLovers.com.");
			}
			else if (uri.AbsolutePath.Contains("/color/"))
			{
				throw new UnityException("Sorry, we do not support getting 'simple colors' from colourLovers.com.");
			}

			WebClient client = new WebClient();
			using (Stream stream = client.OpenRead(uri))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					string line;
					ColorInfo colorInfo = new ColorInfo();
					
					while ((line = reader.ReadLine()) != null)
					{
						if (line.Contains("feature-detail") && line.Contains("h1"))
						{
							colorPalette.name = line.Substring(line.IndexOf("<h1>") + 4, line.IndexOf("</h1>") - line.IndexOf("<h1>") - 4);
						}
						
						if (line.Contains("<h3 class=\"left mr-10"))
						{
							string textToFind = "<h3 class=\"left mr-10\" style=\"height: 20px;\">";
							string name = line.Substring(line.IndexOf(textToFind) + textToFind.Length);
							name = name.Substring(name.IndexOf(">") + 1);
							name = name.Substring(0, name.IndexOf("</a>"));
							
							colorInfo = new ColorInfo();
							colorInfo.name = name;
						}

						if (line.Contains("right-col big-number-label") && line.Contains("RGB"))
						{
							string rgbColorsString = line.Substring(line.IndexOf("<h4>") + 4, line.IndexOf("</h4>") - line.IndexOf("<h4>") - 4);
							string[] rgbColorArray = rgbColorsString.Split(',');
							
							colorInfo.color = new Color(int.Parse(rgbColorArray[0])/255f, int.Parse(rgbColorArray[1])/255f, int.Parse(rgbColorArray[2])/255f);
							
							colorPalette.colorInfoList.Add(colorInfo);
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