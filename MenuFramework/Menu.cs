using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MenuFramework
{
	class Menu
	{
		MenuScreen activeScreen;

		List<MenuScreen> screens = new List<MenuScreen>();

		public MenuScreen ActiveScreen
		{
			get { return activeScreen; }
		}

		public List<MenuScreen> Screens
		{
			get { return screens; }
		}

		public Menu()
		{

		}

		public void SwitchActiveScreen(string screenName)
		{
			if (FindScreen(screenName) != null) activeScreen = FindScreen(screenName);
		}

		public MenuScreen FindScreen(string screenName)
		{
			MenuScreen result = null;
			IEnumerable<MenuScreen> matches = screens.Where(t => { return t.Name == screenName; });

			if (matches.Count() > 0) result = matches.First();
			else Console.WriteLine(string.Format("No menu screen found with name {0}.", screenName));
			return result;
		}

		public void Update()
		{
			activeScreen?.Update();

			if (activeScreen == null)
			{
				activeScreen = screens.First();
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			activeScreen?.Draw(spriteBatch);
		}
	}
}
