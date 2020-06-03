using System;
using System.Drawing;
using System.Windows.Forms;
using KeePass.Plugins;
using KeePass.UI;

namespace KeePassFIDO2
{
	public class KeePassFIDO2Ext : Plugin
	{
		public IPluginHost PluginHost;
		private readonly FIDO2KeyProvider keyProvider = new FIDO2KeyProvider();

		public override bool Initialize(IPluginHost host)
		{
			if (host == null)
			{
				return false;
			}

			PluginHost = host;
			PluginHost.KeyProviderPool.Add(keyProvider);
			return true;
		}

		private void OnMenuItemClick(object sender, EventArgs e)
		{
			var form = new FIDO2OptionsForm(this);
			UIUtil.ShowDialogAndDestroy(form);
		}

		public override void Terminate()
		{
			PluginHost.KeyProviderPool.Remove(keyProvider);
		}

		public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
		{
			if (t != PluginMenuType.Main)
			{
				return null;
			}

			var menuItem = new ToolStripMenuItem("KeePassFIDO2");
			menuItem.Click += OnMenuItemClick;

			return menuItem;
		}

		public override Image SmallIcon { get; } // TODO: create and add an icon
		public override string UpdateUrl => "https://raw.githubusercontent.com/MartinKolarik/KeePassFIDO2/master/KeePassPlugin/keepass.version";
	}
}
