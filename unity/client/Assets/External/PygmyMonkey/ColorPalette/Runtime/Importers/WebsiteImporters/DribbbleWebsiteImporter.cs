using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace PygmyMonkey.ColorPalette
{
	public static class DribbbleWebsiteImporter
	{
		public static ColorPalette Import(Uri uri)
		{
			ColorPalette colorPalette = new ColorPalette();
			colorPalette.name = uri.AbsolutePath;

			WWW www = new WWW(uri.AbsoluteUri);
			while (!www.isDone);

			if (!string.IsNullOrEmpty(www.error))
			{
				throw new UnityException("Error getting palette from the website: " + uri.AbsoluteUri + ". Please contact us at tools@pygmymonkey.com\n" + www.error);
			}

			foreach (string line in www.text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (line.Contains("screenshot-title") && line.Contains("h1"))
				{
					colorPalette.name = line.Substring(line.IndexOf(">") + 1, line.IndexOf("<", line.IndexOf(">")) - line.IndexOf(">") - 1);
				}

				if (line.Contains("<a style=\"background-color:"))
				{
					string hexColorString = line.Substring(line.IndexOf("#") + 1, 6);
					colorPalette.colorInfoList.Add(new ColorInfo(null, ColorUtils.HexToColor(hexColorString)));
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