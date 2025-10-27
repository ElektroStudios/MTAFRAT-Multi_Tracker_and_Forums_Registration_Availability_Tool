----------------------------------------------------------------------------------------------------------
🧩 Multi-Tracker and Forums Registration Availability Tool (MTAFRAT)

Author: ElektroStudios
Website: https://github.com/ElektroStudios/MTAFRAT-Multi_Tracker_and_Forums_Registration_Availability_Tool
----------------------------------------------------------------------------------------------------------

MTAFRAT is a tool designed to automatically check 
registration availability across multiple trackers and forums.

It allows you to quickly analyze registration status using a modular, 
plugin-based system that facilitates the integration of 
new sites without modifying the program core.

README FILE
_____________

📋 Requirements

	This program requires .NET Desktop Runtime 8.0 installed on your computer.

	Download the .NET Desktop Runtime 8.0 64-Bit package here:
		► https://dotnet.microsoft.com/en-us/download/dotnet/8.0

📦 Plugin-Based Architecture

	The program works through a plugin system.

	Each plugin is composed of three main elements:

		● JSON file
			Contains the plugin's basic metadata:
			name, description, login or registration URL, etc.

		● PNG, JPG, BMP, or ICO image file
			This is usually a banner or favicon
			that visually represents the plugin.
			A square image is recommended,
			preferably 48x48 pixels.

		● VB file
			Contains the VB.NET source code that implements the 
			plugin's logic and behavior within the program.

📁 Plugins Location

	All plugins are located in the "plugins" folder.
	If you don't find one of them useful and want to remove it from the program,
	you can delete the folder corresponding to the plugin name,
	or move it to another directory outside of its original location.

🔧 Built-in plugins

	Index  Name            Url
	------ ------          ---
	01     3ChangTrai      https://3changtrai.com/signup.php
	02     3D Torrents     http://www.3dtorrents.org/index.php?page=signup
	03     4th Dimension   https://4thd.xyz/register.php
	04     AlphaRatio      https://alpharatio.cc/register.php
	05     Animebytes      https://animebytes.tv/register/apply
	06     AnimeTorrents   https://animetorrents.me/register.php
	07     AvistaZ         https://avistaz.to/auth/register
	08     BeyondHD        https://beyond-hd.me/login
	09     BitPorn         https://bitporn.eu/register
	10     Blutopia        https://blutopia.cc/application
	11     Cinemageddon    https://cinemageddon.net/signup.php
	12     CinemaZ         https://cinemaz.to/auth/register
	13     DarkPeers       https://darkpeers.org/register
	14     Digital Core    https://digitalcore.club/signup/
	15     Elite-HD        https://www.elitehd.li/ucp.php?mode=register
	16     ExoticaZ        https://exoticaz.to/register
	17     Fappaizuri      https://fappaizuri.me/account-signup.php
	18     FearNoPeer      https://fearnopeer.com/register
	19     Femdomcult      https://femdomcult.org/
	20     GreatPosterWall https://greatposterwall.com/register.php
	21     HD Dolby        https://www.hddolby.com/signup.php
	22     HD-Forever      https://hdf.world/register.php
	23     HD-Olimpo       https://hd-olimpo.club/login
	24     HD-Space        https://hd-space.org/index.php?page=signup
	25     HD-Zero         https://www.hdzero.org/register
	26     HDHome          https://hdhome.org/signup.php
	27     HDTime          https://hdtime.org/signup.php
	28     Hebits          https://hebits.net/register.php
	29     iPtorrents      https://www.iptorrents.com/signup.php
	30     Kleverig        https://www.kleverig.eu/register.php
	31     KrazyZone       https://krazyzone.net/account-signup.php
	32     Lat-Team        https://lat-team.com/application
	33     Locadora        https://locadora.cc/register
	34     LST             https://lst.gg/register
	35     More Than TV    https://www.morethantv.me/application
	36     NicePT          https://www.nicept.net/signup.php
	37     Old Toons World https://oldtoons.world/register
	38     OnlyEncodes     https://onlyencodes.cc/application
	39     PassThePopcorn  https://passthepopcorn.me/register.php
	40     PrivateHD       https://privatehd.to/auth/register
	41     Punto Torrent   https://xbt.puntotorrent.com/index.php?page=signup
	42     Rastastugan     https://rastastugan.org/register
	43     ReelFlix        https://reelflix.xyz/register
	44     SceneRush       https://www.scene-rush.com/signup.php
	45     SeedFile        https://seedfile.io/register
	46     SportsCult      https://sportscult.org/index.php?page=signup
	47     Superbits       https://login.superbits.org/signup/
	48     Tekno3D         https://tracker.tekno3d.com/signup.php
	49     TorrentCFF      https://et8.org/signup.php
	50     Torrent Day     https://www.torrentday.me/register.php
	51     Torrenteros     https://torrenteros.org/application
	52     TorrentLand     https://torrentland.li/login
	53     TorrentLeech    https://www.torrentleech.org/
	54     Upscale Vault   https://upscalevault.com/register
	55     xBytesv2        https://xbytesv2.li/register

💡 Tips

	1.  Some websites may keep registration open for several days, or even weeks and months. 
		However —according to collected information and user feedback— other sites only allow registration 
		for very short periods, sometimes less than 24 hours, including Spanish sites such as Torrentland.

		For this reason, if you don’t want to miss the chance to register on time, it is highly recommended 
		to configure automatic execution every hour. This way, MTAFRAT can reliably detect very short 
		registration periods on a website.

	2.  If you enable parallel execution, plugins will run four at a time, meaning that four instances 
		of the chromedriver.exe process (and their respective Chrome.exe child processes) will open 
		simultaneously. This will significantly reduce the total time needed to run all plugins; 
		however, it may also cause unexpected issues in the application. This feature is experimental.

	3.  If you enable automatic plugin execution, it is recommended to select only those plugins 
		that genuinely interest you. Sometimes a website may be down or cause an unexpected error, 
		prolonging the total execution time excessively.

	4.  Every time you run a plugin, MTAFRAT will automatically generate certain files on disk. 
		This is the application cache. If you wish to clear the cache, simply press the "Clear Cache" 
		button in the program options.

	5.  The first time you run a plugin that interacts with a website protected by Cloudflare 
		(e.g., "AvistaZ", "CinemaZ", or "ExoticaZ"), the Cloudflare trial may fail; however, 
		subsequent attempts should not fail. Keep in mind that if you clear the program cache, 
		the next time you run the plugin it will be as if you are running it for the first time.

	6.  If for any reason MTAFRAT logs strange errors related to Chrome while running a plugin, 
		try closing all running processes with the following names: "Chrome.exe" and "ChromeDriver.exe". 
		You can do this easily using the following command in the Windows CMD:
			Taskkill /F /T /IM "Chrome.exe" /IM "ChromeDriver.exe"
		Once the running processes are closed, open MTAFRAT and try running the plugin again.
