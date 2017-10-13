using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace PygmyMonkey.ColorPalette
{
	public class ColorPaletteDemo2 : MonoBehaviour
	{
		[SerializeField] private InputField m_inputURL;
		[SerializeField] private Text m_textInfo;
		[SerializeField] private GameObject[] m_cubeArray;

		void Awake()
		{
			m_textInfo.text = "";
		}

		public void onButtonDownloadClicked()
		{
			StartCoroutine(downloadPaletteCoroutine());
		}

		private IEnumerator downloadPaletteCoroutine()
		{
			m_textInfo.text = "Downloading palette...";

			yield return null;

			ColorPalette colorPalette = null;
			
			try
			{
				Uri uri = new Uri(m_inputURL.text);
				
				if (uri.Host.EndsWith("colourlovers.com"))
				{
					colorPalette = ColourLoversWebsiteImporter.Import(uri);
				}
				else if (uri.Host.EndsWith("dribbble.com"))
				{
					colorPalette = DribbbleWebsiteImporter.Import(uri);
				}
				else if (uri.Host.EndsWith("colorcombos.com"))
				{
					colorPalette = ColorCombosWebsiteImporter.Import(uri);
				}
				else
				{
					m_textInfo.text = "Sorry we do not support downloading palettes from the website " + uri.Host + " for now.";
				}
			}
			catch (Exception e)
			{
				m_textInfo.text = "Sorry an error occured: " + e.Message;
			}
			
			if (colorPalette != null)
			{
				m_textInfo.text = "Palette downloaded (found " + colorPalette.colorInfoList.Count + " colors).";
				applyColorPaletteToCubes(colorPalette);
			}
		}

		private void applyColorPaletteToCubes(ColorPalette colorPalette)
		{
			if (colorPalette.colorInfoList == null || colorPalette.colorInfoList.Count == 0)
			{
				return;
			}

			for (int i = 0; i < m_cubeArray.Length; i++)
			{
				int colorIndex = i;
				if (colorIndex >= colorPalette.colorInfoList.Count) // If there is less color than cubes, we take the last color
				{
					colorIndex = colorPalette.colorInfoList.Count - 1;
				}

				m_cubeArray[i].GetComponent<Renderer>().material.color = colorPalette.colorInfoList[colorIndex].color;
			}
		}
	}
}