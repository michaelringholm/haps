using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace PygmyMonkey.ColorPalette
{
	public static class ColrdWebsiteImporter
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
						if (line.Contains("<h1>") && line.Contains("</h1>"))
						{
							colorPalette.name = line.Substring(line.IndexOf("h1>") + 3);
							colorPalette.name = colorPalette.name.Substring(0, colorPalette.name.IndexOf("</h1>"));
						}

						if (line.Contains("span class=\"gpTitle\"") && !line.Contains("a href"))
						{
							string[] resultArray = System.Text.RegularExpressions.Regex.Split(line, "background-color");
							for (int i = 0; i < resultArray.Length; i++)
							{
								if (!resultArray[i].Contains("opacity"))
								{
									continue;
								}

								string rgb = resultArray[i].Substring(resultArray[i].IndexOf("(") + 1);
								rgb = rgb.Substring(0, rgb.IndexOf(")"));
								string opacity = resultArray[i].Substring(resultArray[i].IndexOf("opacity=") + 8);
								opacity = opacity.Substring(0, opacity.LastIndexOf(");"));

								string[] rgbArray = rgb.Split(',');

								colorPalette.colorInfoList.Add(new ColorInfo(null, new Color(int.Parse(rgbArray[0])/255f, int.Parse(rgbArray[1])/255f, int.Parse(rgbArray[2])/255f, int.Parse(opacity)/100f)));
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