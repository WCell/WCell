using WCell.Util.Graphics;

///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 1/30/2010
///

namespace WCell.Constants.World
{
	public static class RegionBoundaries 
	{
		public static BoundingBox[] GetRegionBoundaries()
		{
			var boxes = new BoundingBox[(int)MapId.End];
			boxes[(int)MapId.EasternKingdoms] = new BoundingBox(new Vector3(-16200f, -6066.667f, -2048f), new Vector3(6600f, 16733.33f, 2048f));
			boxes[(int)MapId.Kalimdor] = new BoundingBox(new Vector3(-13000f, -13000f, -2048f), new Vector3(17266.67f, 17266.67f, 2048f));
			boxes[(int)MapId.Testing] = new BoundingBox(new Vector3(-1333.004f, -1331.893f, -1133.004f), new Vector3(1333.004f, 1334.115f, 1133.004f));
			boxes[(int)MapId.ScottTest] = new BoundingBox(new Vector3(-1333.004f, -1331.893f, -1133.004f), new Vector3(1333.004f, 1334.115f, 1133.004f));
			//boxes[(int)MapId.CashTest] = new BoundingBox(new Vector3(-1333.004f, -1331.893f, -1133.004f), new Vector3(1333.004f, 1334.115f, 1133.004f));
			boxes[(int)MapId.AlteracValley] = new BoundingBox(new Vector3(-2333.333f, -2866.667f, -2048f), new Vector3(1800f, 1266.667f, 2048f));
			boxes[(int)MapId.ShadowfangKeep] = new BoundingBox(new Vector3(-1800f, 866.6666f, -2048f), new Vector3(1266.667f, 3933.333f, 2048f));
			boxes[(int)MapId.StormwindStockade] = new BoundingBox(new Vector3(-351.8212f, -235.679f, -197.2988f), new Vector3(350.3298f, 466.472f, 8.136327f));
			boxes[(int)MapId.UnusedStormwindPrison] = new BoundingBox(new Vector3(-395.5174f, -240.8734f, -77.28123f), new Vector3(232.1672f, 386.8112f, 136.2686f));
			boxes[(int)MapId.Deadmines] = new BoundingBox(new Vector3(-2333.333f, -2333.333f, -2048f), new Vector3(1266.667f, 1266.667f, 2048f));
			boxes[(int)MapId.AzsharaCrater] = new BoundingBox(new Vector3(-1800f, -1800f, -2048f), new Vector3(1800f, 1800f, 2048f));
			boxes[(int)MapId.CollinsTest] = new BoundingBox(new Vector3(-201.4167f, -200.3056f, -1.416667f), new Vector3(201.4167f, 202.5278f, 1.416667f));
			boxes[(int)MapId.WailingCaverns] = new BoundingBox(new Vector3(-754.3612f, -394.9138f, -187.5039f), new Vector3(569.7778f, 929.2252f, 483.4794f));
			boxes[(int)MapId.UnusedMonastery] = new BoundingBox(new Vector3(-249.3954f, -202.3566f, -441.4302f), new Vector3(371.7203f, 418.7592f, 8.790004f));
			boxes[(int)MapId.RazorfenKraul] = new BoundingBox(new Vector3(866.6666f, 866.6666f, -2048f), new Vector3(2866.667f, 2866.667f, 2048f));
			boxes[(int)MapId.BlackfathomDeeps] = new BoundingBox(new Vector3(-624.0243f, -280.5253f, 76.81782f), new Vector3(702.3956f, 1045.895f, 887.996f));
			boxes[(int)MapId.Uldaman] = new BoundingBox(new Vector3(-666.3337f, -256.3541f, -184.3063f), new Vector3(269.7638f, 679.7434f, 371.4725f));
			boxes[(int)MapId.Gnomeregan] = new BoundingBox(new Vector3(-956.1609f, -531.8307f, 200.8883f), new Vector3(342.0382f, 766.3684f, 913.9997f));
			boxes[(int)MapId.SunkenTemple] = new BoundingBox(new Vector3(-524.0962f, -389.7575f, 245.1662f), new Vector3(333.6096f, 467.9484f, 708.519f));
			boxes[(int)MapId.RazorfenDowns] = new BoundingBox(new Vector3(-200f, -733.3333f, -2048f), new Vector3(3400f, 2866.667f, 2048f));
			boxes[(int)MapId.EmeraldDream] = new BoundingBox(new Vector3(-5000f, -4466.667f, -2048f), new Vector3(3933.333f, 4466.667f, 2048f));
			boxes[(int)MapId.ScarletMonastery] = new BoundingBox(new Vector3(-733.3333f, -1266.667f, -2048f), new Vector3(2866.667f, 2333.333f, 2048f));
			boxes[(int)MapId.ZulFarrak] = new BoundingBox(new Vector3(-1266.667f, -2333.333f, -2048f), new Vector3(2866.667f, 1800f, 2048f));
			boxes[(int)MapId.BlackrockSpire] = new BoundingBox(new Vector3(-325.9205f, -296.7951f, -334.3978f), new Vector3(791.5855f, 820.7109f, 226.8285f));
			boxes[(int)MapId.BlackrockDepths] = new BoundingBox(new Vector3(-465.2861f, -408.8925f, -1486.677f), new Vector3(1047.676f, 1104.07f, -258.6438f));
			boxes[(int)MapId.OnyxiasLair] = new BoundingBox(new Vector3(-226.112f, -296.4752f, -63.24594f), new Vector3(486.4644f, 416.1012f, 218.8329f));
			boxes[(int)MapId.OpeningOfTheDarkPortal] = new BoundingBox(new Vector3(-5000f, -733.3333f, -2048f), new Vector3(3933.333f, 8200f, 2048f));
			boxes[(int)MapId.Scholomance] = new BoundingBox(new Vector3(-733.3333f, -1266.667f, -2048f), new Vector3(1800f, 1266.667f, 2048f));
			boxes[(int)MapId.ZulGurub] = new BoundingBox(new Vector3(-13533.33f, -3400f, -2048f), new Vector3(-10466.67f, -333.3333f, 2048f));
			boxes[(int)MapId.Stratholme] = new BoundingBox(new Vector3(1400f, -5000f, -2048f), new Vector3(4466.667f, -1933.333f, 2048f));
			boxes[(int)MapId.Maraudon] = new BoundingBox(new Vector3(-493.3598f, -408.9557f, -1168.871f), new Vector3(1011.501f, 1095.905f, 161.8902f));
			boxes[(int)MapId.DeeprunTram] = new BoundingBox(new Vector3(-2765.709f, -335.6957f, -195.2544f), new Vector3(239.4216f, 2669.435f, 215.7682f));
			boxes[(int)MapId.RagefireChasm] = new BoundingBox(new Vector3(-475.3791f, -266.1914f, -102.9557f), new Vector3(308.5006f, 517.6882f, 416.286f));
			boxes[(int)MapId.MoltenCore] = new BoundingBox(new Vector3(71.33231f, -433.5222f, -1333.055f), new Vector3(1454.985f, 950.1309f, -489.8628f));
			boxes[(int)MapId.DireMaul] = new BoundingBox(new Vector3(-1183.447f, -319.9896f, -927.7581f), new Vector3(1071.345f, 1934.802f, 387.7324f));
			boxes[(int)MapId.AlliancePVPBarracks] = new BoundingBox(new Vector3(-238.3467f, -216.2614f, -83.08028f), new Vector3(290.8767f, 312.962f, 29.42056f));
			boxes[(int)MapId.HordePVPBarracks] = new BoundingBox(new Vector3(-315.5043f, -230.2086f, -273.4287f), new Vector3(386.099f, 471.3947f, 82.77789f));
			boxes[(int)MapId.BlackwingLair] = new BoundingBox(new Vector3(-8733.333f, -2333.333f, -2048f), new Vector3(-6200f, 200f, 2048f));
			boxes[(int)MapId.WarsongGulch] = new BoundingBox(new Vector3(333.3333f, 333.3333f, -2048f), new Vector3(2866.667f, 2866.667f, 2048f));
			boxes[(int)MapId.RuinsOfAhnQiraj] = new BoundingBox(new Vector3(-11933.33f, -1266.667f, -2048f), new Vector3(-7266.667f, 3400f, 2048f));
			boxes[(int)MapId.ArathiBasin] = new BoundingBox(new Vector3(-200f, -200f, -2048f), new Vector3(2333.333f, 2333.333f, 2048f));
			boxes[(int)MapId.Outland] = new BoundingBox(new Vector3(-12466.67f, -15666.67f, -2048f), new Vector3(14066.67f, 10866.67f, 2048f));
			boxes[(int)MapId.AhnQirajTemple] = new BoundingBox(new Vector3(-10333.33f, 333.3333f, -2048f), new Vector3(-7266.667f, 3400f, 2048f));
			boxes[(int)MapId.Karazhan] = new BoundingBox(new Vector3(-11933.33f, -2866.667f, -2048f), new Vector3(-9933.333f, -866.6666f, 2048f));
			boxes[(int)MapId.Naxxramas] = new BoundingBox(new Vector3(866.6666f, -6066.667f, -2048f), new Vector3(4466.667f, -2466.667f, 2048f));
			boxes[(int)MapId.TheBattleForMountHyjal] = new BoundingBox(new Vector3(3000f, -4466.667f, -2048f), new Vector3(6600f, -866.6666f, 2048f));
			boxes[(int)MapId.HellfireCitadelTheShatteredHalls] = new BoundingBox(new Vector3(-551.7536f, -249.4081f, -577.4171f), new Vector3(353.2849f, 655.6305f, 91.74846f));
			boxes[(int)MapId.HellfireCitadelTheBloodFurnace] = new BoundingBox(new Vector3(-413.3602f, -341.1069f, -553.1358f), new Vector3(418.9021f, 491.1554f, 45.87687f));
			boxes[(int)MapId.HellfireCitadelRamparts] = new BoundingBox(new Vector3(-3933.333f, -733.3333f, -2048f), new Vector3(1266.667f, 4466.667f, 2048f));
			boxes[(int)MapId.MagtheridonsLair] = new BoundingBox(new Vector3(-317.1322f, -200.5686f, -236.8137f), new Vector3(287.969f, 404.5325f, 117.6778f));
			boxes[(int)MapId.CoilfangTheSteamvault] = new BoundingBox(new Vector3(-230.1924f, -293.3978f, -128.5667f), new Vector3(806.912f, 743.7065f, 395.9422f));
			boxes[(int)MapId.CoilfangTheUnderbog] = new BoundingBox(new Vector3(-385.9693f, -242.8454f, -423.7346f), new Vector3(798.9348f, 942.0587f, 172.8776f));
			boxes[(int)MapId.CoilfangTheSlavePens] = new BoundingBox(new Vector3(-238.9341f, -261.5359f, -155.0816f), new Vector3(1021.124f, 998.5225f, 345.0134f));
			boxes[(int)MapId.CoilfangSerpentshrineCavern] = new BoundingBox(new Vector3(-315.3267f, -341.8629f, -573.8898f), new Vector3(1351.064f, 1324.528f, 386.7139f));
			boxes[(int)MapId.TempestKeep] = new BoundingBox(new Vector3(-695.4613f, -408.7054f, -1020.114f), new Vector3(697.9153f, 984.6711f, 64.25883f));
			boxes[(int)MapId.TempestKeepTheArcatraz] = new BoundingBox(new Vector3(-441.4468f, -1134.244f, -543.0322f), new Vector3(1094.157f, 401.36f, 50.09064f));
			boxes[(int)MapId.TempestKeepTheBotanica] = new BoundingBox(new Vector3(-836.1586f, -501.6032f, -248.0247f), new Vector3(294.0546f, 628.61f, 256.9099f));
			boxes[(int)MapId.TempestKeepTheMechanar] = new BoundingBox(new Vector3(-405.6038f, -224.2593f, -349.9862f), new Vector3(412.9252f, 594.2697f, 100.839f));
			boxes[(int)MapId.AuchindounShadowLabyrinth] = new BoundingBox(new Vector3(-278.2954f, -208.4949f, -80.96366f), new Vector3(750.6594f, 820.4599f, 539.3844f));
			boxes[(int)MapId.AuchindounSethekkHalls] = new BoundingBox(new Vector3(-578.9166f, -203.3256f, -163.654f), new Vector3(250.8178f, 626.4089f, 285.343f));
			boxes[(int)MapId.AuchindounManaTombs] = new BoundingBox(new Vector3(-260.0058f, -209.5614f, -80.07083f), new Vector3(489.4222f, 539.8667f, 428.7859f));
			boxes[(int)MapId.AuchindounAuchenaiCrypts] = new BoundingBox(new Vector3(-251.1365f, -385.2681f, -294.9506f), new Vector3(638.5625f, 504.4309f, 240.0761f));
			boxes[(int)MapId.NagrandArena] = new BoundingBox(new Vector3(3000f, 1933.333f, -2048f), new Vector3(5000f, 3933.333f, 2048f));
			boxes[(int)MapId.TheEscapeFromDurnholde] = new BoundingBox(new Vector3(333.3333f, -733.3333f, -2048f), new Vector3(3933.333f, 2866.667f, 2048f));
			boxes[(int)MapId.BladesEdgeArena] = new BoundingBox(new Vector3(5133.333f, -733.3333f, -2048f), new Vector3(7133.333f, 1266.667f, 2048f));
			boxes[(int)MapId.BlackTemple] = new BoundingBox(new Vector3(-1266.667f, -1266.667f, -2048f), new Vector3(2333.333f, 2333.333f, 2048f));
			boxes[(int)MapId.GruulsLair] = new BoundingBox(new Vector3(-639.6964f, -215.3801f, -298.5232f), new Vector3(221.2419f, 645.5582f, 29.24125f));
			boxes[(int)MapId.EyeOfTheStorm] = new BoundingBox(new Vector3(333.3333f, -200f, -2048f), new Vector3(3933.333f, 3400f, 2048f));
			boxes[(int)MapId.ZulAman] = new BoundingBox(new Vector3(-1266.667f, -200f, -2048f), new Vector3(1800f, 2866.667f, 2048f));
			boxes[(int)MapId.Northrend] = new BoundingBox(new Vector3(-8733.333f, -9800f, -2048f), new Vector3(12466.67f, 11400f, 2048f));
			boxes[(int)MapId.RuinsOfLordaeron] = new BoundingBox(new Vector3(-200f, 333.3333f, -2048f), new Vector3(2866.667f, 3400f, 2048f));
			boxes[(int)MapId.ExteriorTest] = new BoundingBox(new Vector3(-733.3333f, -3933.333f, -2048f), new Vector3(5000f, 1800f, 2048f));
			boxes[(int)MapId.UtgardeKeep] = new BoundingBox(new Vector3(-2333.333f, -2333.333f, -2048f), new Vector3(2333.333f, 2333.333f, 2048f));
			boxes[(int)MapId.UtgardePinnacle] = new BoundingBox(new Vector3(-2333.333f, -2333.333f, -2048f), new Vector3(2333.333f, 2333.333f, 2048f));
			boxes[(int)MapId.TheNexus] = new BoundingBox(new Vector3(-436.6537f, -468.5912f, -798.2632f), new Vector3(751.5106f, 719.5731f, -64.07553f));
			boxes[(int)MapId.TheOculus] = new BoundingBox(new Vector3(-1266.667f, -1266.667f, -2048f), new Vector3(3400f, 3400f, 2048f));
			boxes[(int)MapId.TheSunwell] = new BoundingBox(new Vector3(-200f, -1266.667f, -2048f), new Vector3(3400f, 2333.333f, 2048f));
			boxes[(int)MapId.MagistersTerrace] = new BoundingBox(new Vector3(-2866.667f, -2866.667f, -2048f), new Vector3(2866.667f, 2866.667f, 2048f));
			boxes[(int)MapId.TheCullingOfStratholme] = new BoundingBox(new Vector3(-733.3333f, -733.3333f, -2048f), new Vector3(2866.667f, 2866.667f, 2048f));
			boxes[(int)MapId.CraigTest] = new BoundingBox(new Vector3(16333.33f, 16333.33f, -2048f), new Vector3(-16866.67f, -16866.67f, 2048f));
			boxes[(int)MapId.SunwellFixUnused] = new BoundingBox(new Vector3(-412.8828f, -259.3424f, -345.7601f), new Vector3(490.9996f, 644.5399f, 7.795757f));
			boxes[(int)MapId.HallsOfStone] = new BoundingBox(new Vector3(-200f, -733.3333f, -2048f), new Vector3(2866.667f, 2333.333f, 2048f));
			boxes[(int)MapId.DrakTharonKeep] = new BoundingBox(new Vector3(-1800f, -1800f, -2048f), new Vector3(1266.667f, 1266.667f, 2048f));
			boxes[(int)MapId.AzjolNerub] = new BoundingBox(new Vector3(-733.3333f, -733.3333f, -2048f), new Vector3(1800f, 1800f, 2048f));
			boxes[(int)MapId.HallsOfLightning] = new BoundingBox(new Vector3(-1266.667f, -1800f, -2048f), new Vector3(2866.667f, 2333.333f, 2048f));
			boxes[(int)MapId.Ulduar] = new BoundingBox(new Vector3(-2333.333f, -2333.333f, -2048f), new Vector3(3400f, 3400f, 2048f));
			boxes[(int)MapId.Gundrak] = new BoundingBox(new Vector3(333.3333f, -733.3333f, -2048f), new Vector3(3400f, 2333.333f, 2048f));
			boxes[(int)MapId.DevelopmentLandNonWeightedTextures] = new BoundingBox(new Vector3(-16200f, -14600f, -2048f), new Vector3(-14200f, -12600f, 2048f));
			boxes[(int)MapId.QAAndDVD] = new BoundingBox(new Vector3(1933.333f, -200f, -2048f), new Vector3(3400f, 1266.667f, 2048f));
			boxes[(int)MapId.StrandOfTheAncients] = new BoundingBox(new Vector3(-1266.667f, -2866.667f, -2048f), new Vector3(4466.667f, 2866.667f, 2048f));
			boxes[(int)MapId.VioletHold] = new BoundingBox(new Vector3(333.3333f, -733.3333f, -2048f), new Vector3(3400f, 2333.333f, 2048f));
			boxes[(int)MapId.EbonHold] = new BoundingBox(new Vector3(-200f, -7133.333f, -2048f), new Vector3(3933.333f, -3000f, 2048f));
			boxes[(int)MapId.TheObsidianSanctum] = new BoundingBox(new Vector3(1933.333f, -733.3333f, -2048f), new Vector3(4466.667f, 1800f, 2048f));
			boxes[(int)MapId.TheEyeOfEternity] = new BoundingBox(new Vector3(-733.3333f, -200f, -2048f), new Vector3(2333.333f, 2866.667f, 2048f));
			boxes[(int)MapId.DalaranSewers] = new BoundingBox(new Vector3(333.3333f, -200f, -2048f), new Vector3(2333.333f, 1800f, 2048f));
			boxes[(int)MapId.TheRingOfValor] = new BoundingBox(new Vector3(-200f, -1266.667f, -2048f), new Vector3(1800f, 733.3333f, 2048f));
			boxes[(int)MapId.AhnKahetTheOldKingdom] = new BoundingBox(new Vector3(-1266.667f, -2866.667f, -2048f), new Vector3(2866.667f, 1266.667f, 2048f));
			boxes[(int)MapId.VaultOfArchavon] = new BoundingBox(new Vector3(-1266.667f, -1266.667f, -2048f), new Vector3(733.3333f, 733.3333f, 2048f));
			boxes[(int)MapId.IsleOfConquest] = new BoundingBox(new Vector3(-3400f, -5000f, -2048f), new Vector3(2866.667f, 1266.667f, 2048f));
			boxes[(int)MapId.IcecrownCitadel] = new BoundingBox(new Vector3(-2333.333f, -4466.667f, -2048f), new Vector3(6066.667f, 3933.333f, 2048f));
			boxes[(int)MapId.TheForgeOfSouls] = new BoundingBox(new Vector3(4066.667f, 1400f, -2048f), new Vector3(6066.667f, 3400f, 2048f));
			boxes[(int)MapId.TrialOfTheCrusader] = new BoundingBox(new Vector3(-200f, -733.3333f, -2048f), new Vector3(2866.667f, 2333.333f, 2048f));
			boxes[(int)MapId.TrialOfTheChampion] = new BoundingBox(new Vector3(-200f, -200f, -2048f), new Vector3(2333.333f, 2333.333f, 2048f));
			boxes[(int)MapId.PitOfSaron] = new BoundingBox(new Vector3(-200f, -733.3333f, -2048f), new Vector3(1800f, 1266.667f, 2048f));
			boxes[(int)MapId.HallsOfReflection] = new BoundingBox(new Vector3(3533.333f, -200f, -2048f), new Vector3(6600f, 2866.667f, 2048f));
			return boxes;
		}

	}

}

