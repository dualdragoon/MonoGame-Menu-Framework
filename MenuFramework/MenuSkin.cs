using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Duality;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MenuFramework
{
	class MenuSkin
	{
		string skinName;
		Texture2D atlas;

		Dictionary<string, Texture2D> textures;

		public Dictionary<string, Texture2D> Textures
		{
			get { return textures; }
		}

		public MenuSkin(string skin, ContentManager content, GraphicsDevice graphicsDevice)
		{
			textures = new Dictionary<string, Texture2D>();
			LoadFromFile(skin, content, graphicsDevice);
		}

		public void LoadFromFile(string skin, ContentManager content, GraphicsDevice graphicsDevice)
		{
			XDocument document = XDocument.Load(skin);

			XAttribute nameAtb = document.Elements().First().Attribute("name");

			if (nameAtb != null) skinName = nameAtb.Value;
			else ErrorHandler.RecordError(3, 405, "name attribute missing from root element. All skins must have a name.", "Missing name attribute.");

			XAttribute atlasAtb = document.Elements().First().Attribute("textureAtlas");

			if (atlasAtb != null) atlas = content.Load<Texture2D>(atlasAtb.Value);
			else ErrorHandler.RecordError(3, 404, "textureAtlas attribute missing from root element. All skins must have a texture atlas.", "Missing textureAtlas attribute.");

			IEnumerable<XElement> frames = document.Descendants("Frame");

			foreach (XElement frameNode in frames)
			{
				int x, y, width = 0, height = 0;
				Rectangle? bounds = null;
				string frameName = "";

				XAttribute frameNameAtb = frameNode.Attribute("name");

				if (frameNameAtb != null) frameName = frameNameAtb.Value;
				else ErrorHandler.RecordError(3, 405, "name attribute missing from Frame. All frames must have a name.", "Missing name attribute.");

				XElement boundsNode = frameNode.Element("Bounds");

				if (boundsNode != null)
				{
					x = int.Parse(boundsNode.Element("X").Value);
					y = int.Parse(boundsNode.Element("Y").Value);
					width = int.Parse(boundsNode.Element("Width").Value);
					height = int.Parse(boundsNode.Element("Height").Value);

					bounds = new Rectangle(x, y, width, height);
				}

				Color[] data = new Color[width * height];

				atlas.GetData(0, bounds, data, 0, data.Length);

				Texture2D frame = new Texture2D(graphicsDevice, width, height);
				frame.SetData(data);

				textures.Add(frameName, frame);
			}
		}
	}
}
