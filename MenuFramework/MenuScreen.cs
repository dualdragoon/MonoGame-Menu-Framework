using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Duality;
using Duality.Interaction;
using MonoGame.Extended;

namespace MenuFramework
{
	class MenuScreen
	{
		ContentManager content;
		GraphicsDeviceManager graphics;
		MenuSkin skin;
		MouseState mouse;
		string name;
		Texture2D background;

		List<Button> buttons;
		List<int> textureDepths;
		List<Texture2D> textures;
		List<Vector2> textureLocations, textureSizes;

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public List<Button> Buttons
		{
			get { return buttons; }
		}

		public List<Vector2> TextureLocations
		{
			get { return textureLocations; }
		}

		public MenuScreen(ContentManager contentManager, GraphicsDeviceManager graphicsDeviceManager, string screenName, MenuSkin menuSkin)
		{
			content = contentManager;
			graphics = graphicsDeviceManager;
			skin = menuSkin;
			LoadFromFile(screenName);
		}

		public void LoadFromFile(string screenName)
		{
			buttons = new List<Button>();
			textureDepths = new List<int>();
			textures = new List<Texture2D>();
			textureLocations = new List<Vector2>();
			textureSizes = new List<Vector2>();

			background = null;

			XDocument document = XDocument.Load(string.Format("{0}", screenName));

			XAttribute nameAtb = document.Elements().First().Attribute("name");

			if (nameAtb != null) name = nameAtb.Value;
			else ErrorHandler.RecordError(3, 405, "name attribute missing from root element. All maps must have a name.", "Missing name attribute.");

			XElement backgroundNode = document.Elements().First().Element("Background");

			if (backgroundNode != null)
			{
				background = content.Load<Texture2D>(backgroundNode.Value);
			}

			IEnumerable<XElement> textureNodes = document.Descendants("Texture");

			foreach (XElement node in textureNodes)
			{
				XAttribute textureNameAtb = node.Attribute("name");

				if (textureNameAtb != null)
				{
					try
					{
						textures.Add(skin.Textures[textureNameAtb.Value]);
					}
					catch (KeyNotFoundException e)
					{
						textures.Add(content.Load<Texture2D>(textureNameAtb.Value));
					}
				}
				else ErrorHandler.RecordError(3, 404, "name attribute missing from Texture. All textures must have a name.", "Missing name attribute");

				XAttribute textureDepthAtb = node.Attribute("depth");

				if (textureDepthAtb != null)
				{
					try
					{
						textureDepths.Add(int.Parse(textureDepthAtb.Value));
					}
					catch
					{
						textureDepths.Add(0);
					}
				}
				else
				{
					textureDepths.Add(0);
				}

				XElement positionNode = node.Element("Position");

				if (positionNode != null)
				{
					textureLocations.Add(new Vector2(float.Parse(positionNode.Element("X").Value), float.Parse(positionNode.Element("Y").Value)));
				}
				else ErrorHandler.RecordError(3, 404, "Position node missing from Texture. All textures must have a position.", "Missing Position node");

				XElement sizeNode = node.Element("Size");

				if (sizeNode != null)
				{
					textureSizes.Add(new Vector2(float.Parse(sizeNode.Element("X").Value), float.Parse(sizeNode.Element("Y").Value)));
				}
				else textureSizes.Add(Vector2.Zero);
			}

			IEnumerable<XElement> buttonNodes = document.Descendants("Button");

			foreach (XElement node in buttonNodes)
			{
				ButtonType type = ButtonType.Ellipse;
				Color hoverColor = Color.White;
				Texture2D normal = null, hovered = null;
				Vector2 position = Vector2.Zero;
				float width = 0, height = 0, diameter = 0;
				string buttonName = "";

				XAttribute nameAttribute = node.Attribute("name");

				if (nameAttribute != null)
				{
					if (FindButton(nameAttribute.Value) == null)
					{
						buttonName = nameAttribute.Value;
					}
					else ErrorHandler.RecordError(3, 405, "Button name not unique within menu screen. Button names within the same file must not conflict.", "Conflicting button name");
				}
				else ErrorHandler.RecordError(3, 404, "name attribute missing from Button. All buttons must have a unique name within menu screen.", "Missing name attribute");

				XElement typeNode = node.Element("ButtonType");

				if (typeNode != null) type = (ButtonType)int.Parse(typeNode.Value);
				else ErrorHandler.RecordError(3, 404, "ButtonType node missing from Button. All buttons must have a corresponding type.", "Missing ButtonType node.");

				XElement colorNode = node.Element("HoverColor");

				if (colorNode != null)
				{
					var property = typeof(Color).GetProperty(colorNode.Value);
					if (property != null)
					{
						hoverColor = (Color)property.GetValue(null);
					}
				}

				XElement normalTextureNode = node.Element("NormalTexture");

				if (normalTextureNode != null)
				{
					try
					{
						normal = skin.Textures[normalTextureNode.Value];
					}
					catch (KeyNotFoundException e)
					{
						normal = content.Load<Texture2D>(normalTextureNode.Value);
					}
				}
				else ErrorHandler.RecordError(3, 404, "NormalTexture node missing from Button. All buttons must have at least a normal texture.", "Missing NormalTexture node.");

				XElement hoveredTextureNode = node.Element("HoveredTexture");

				if (hoveredTextureNode != null)
				{
					try
					{
						hovered = skin.Textures[hoveredTextureNode.Value];
					}
					catch (KeyNotFoundException e)
					{
						hovered = content.Load<Texture2D>(hoveredTextureNode.Value);
					}
				}
				else hovered = null;

				XElement positionNode = node.Element("Position");

				if (positionNode != null)
				{
					position = new Vector2(float.Parse(positionNode.Element("X").Value), float.Parse(positionNode.Element("Y").Value));
				}
				else ErrorHandler.RecordError(3, 404, "Position node missing from Button. All buttons must have a position.", "Missing Position node.");

				if (type == ButtonType.Rectangle)
				{
					XElement sizeNode = node.Element("Size");

					if (sizeNode != null)
					{
						width = float.Parse(sizeNode.Element("Width").Value);
						height = float.Parse(sizeNode.Element("Height").Value);
					}
					else ErrorHandler.RecordError(3, 404, "Size node missing from Rectangle Button. All rectangular buttons must have a size.", "Missing Size node.");
				}
				else if (type == ButtonType.Circle)
				{
					XElement diameterNode = node.Element("Diameter");

					if (diameterNode != null) diameter = float.Parse(diameterNode.Value);
					else ErrorHandler.RecordError(3, 404, "Diameter node missing from Circle Button. All circular buttons must have a diameter.", "Missing Diameter node.");
				}

				if (type == ButtonType.Rectangle) buttons.Add(new Button(position, width, height, buttons.Count, Mouse.GetState(), normal, (hovered != null) ? hovered : normal, true));
				else if (type == ButtonType.Circle) buttons.Add(new Button(position, diameter, buttons.Count, Mouse.GetState(), normal, (hovered != null) ? hovered : normal, true));
				else if (type == ButtonType.Ellipse) buttons.Add(new Button(position, buttons.Count, Mouse.GetState(), normal, (hovered != null) ? hovered : normal, true));

				buttons.Last().Name = buttonName;

				if (hoverColor != Color.White)
				{
					buttons.Last().HoverColor = hoverColor;
					buttons.Last().Entered += Testing;
				}
			}
		}

		public void Testing(object sender, EventArgs e)
		{
			Console.WriteLine("Entered");
		}

		public Button FindButton(string buttonName)
		{
			Button result = null;
			IEnumerable<Button> matches = buttons.Where(t => { return t.Name == buttonName; });
			if (matches.Count() > 0) result = matches.First();
			return result;
		}

		public void Update()
		{
			mouse = Mouse.GetState();

			foreach (Button i in buttons)
			{
				i.Update(mouse);
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			if (background != null) spriteBatch.Draw(background, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);

			for (int i = 0; i < textures.Count; i++)
			{
				if (textureDepths[i] == 0)
				{
					spriteBatch.Draw(textures[i], new Rectangle(textureLocations[i].ToPoint(), (textureSizes[i] != Vector2.Zero) ? textureSizes[i].ToPoint() : textures[i].Bounds.Size), Color.White);
				}
			}

			foreach (Button i in buttons)
			{
				if (i.Type == ButtonType.Rectangle)
				{
					spriteBatch.Draw(i.Texture, i.Collision.ToRectangle(), null, i.DisplayColor, 0, Vector2.Zero, i.SpriteFlip, 0);
				}
				else
				{
					spriteBatch.Draw(i.Texture, i.Position, null, i.DisplayColor, 0, Vector2.Zero, 1, i.SpriteFlip, 0);
				}
			}

			for (int i = 0; i < textures.Count; i++)
			{
				if (textureDepths[i] == 1)
				{
					spriteBatch.Draw(textures[i], new Rectangle(textureLocations[i].ToPoint(), (textureSizes[i] != Vector2.Zero) ? textureSizes[i].ToPoint() : textures[i].Bounds.Size), Color.White);
				}
			}
		}
	}
}
