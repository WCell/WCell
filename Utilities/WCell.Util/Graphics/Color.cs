using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
	[StructLayout(LayoutKind.Explicit, Size = 4)]
	public struct Color
	{
		#region Constant Colors

		public static readonly Color Transparent = new Color() { ARGBValue = 16777215 };
		public static readonly Color AliceBlue = new Color() { ARGBValue = -984833 };
		public static readonly Color AntiqueWhite = new Color() { ARGBValue = -332841 };
		public static readonly Color Aqua = new Color() { ARGBValue = -16711681 };
		public static readonly Color Aquamarine = new Color() { ARGBValue = -8388652 };
		public static readonly Color Azure = new Color() { ARGBValue = -983041 };
		public static readonly Color Beige = new Color() { ARGBValue = -657956 };
		public static readonly Color Bisque = new Color() { ARGBValue = -6972 };
		public static readonly Color Black = new Color() { ARGBValue = -16777216 };
		public static readonly Color BlanchedAlmond = new Color() { ARGBValue = -5171 };
		public static readonly Color Blue = new Color() { ARGBValue = -16776961 };
		public static readonly Color BlueViolet = new Color() { ARGBValue = -7722014 };
		public static readonly Color Brown = new Color() { ARGBValue = -5952982 };
		public static readonly Color BurlyWood = new Color() { ARGBValue = -2180985 };
		public static readonly Color CadetBlue = new Color() { ARGBValue = -10510688 };
		public static readonly Color Chartreuse = new Color() { ARGBValue = -8388864 };
		public static readonly Color Chocolate = new Color() { ARGBValue = -2987746 };
		public static readonly Color Coral = new Color() { ARGBValue = -32944 };
		public static readonly Color CornflowerBlue = new Color() { ARGBValue = -10185235 };
		public static readonly Color Cornsilk = new Color() { ARGBValue = -1828 };
		public static readonly Color Crimson = new Color() { ARGBValue = -2354116 };
		public static readonly Color Cyan = new Color() { ARGBValue = -16711681 };
		public static readonly Color DarkBlue = new Color() { ARGBValue = -16777077 };
		public static readonly Color DarkCyan = new Color() { ARGBValue = -16741493 };
		public static readonly Color DarkGoldenrod = new Color() { ARGBValue = -4684277 };
		public static readonly Color DarkGray = new Color() { ARGBValue = -5658199 };
		public static readonly Color DarkGreen = new Color() { ARGBValue = -16751616 };
		public static readonly Color DarkKhaki = new Color() { ARGBValue = -4343957 };
		public static readonly Color DarkMagenta = new Color() { ARGBValue = -7667573 };
		public static readonly Color DarkOliveGreen = new Color() { ARGBValue = -11179217 };
		public static readonly Color DarkOrange = new Color() { ARGBValue = -29696 };
		public static readonly Color DarkOrchid = new Color() { ARGBValue = -6737204 };
		public static readonly Color DarkRed = new Color() { ARGBValue = -7667712 };
		public static readonly Color DarkSalmon = new Color() { ARGBValue = -1468806 };
		public static readonly Color DarkSeaGreen = new Color() { ARGBValue = -7357301 };
		public static readonly Color DarkSlateBlue = new Color() { ARGBValue = -12042869 };
		public static readonly Color DarkSlateGray = new Color() { ARGBValue = -13676721 };
		public static readonly Color DarkTurquoise = new Color() { ARGBValue = -16724271 };
		public static readonly Color DarkViolet = new Color() { ARGBValue = -7077677 };
		public static readonly Color DeepPink = new Color() { ARGBValue = -60269 };
		public static readonly Color DeepSkyBlue = new Color() { ARGBValue = -16728065 };
		public static readonly Color DimGray = new Color() { ARGBValue = -9868951 };
		public static readonly Color DodgerBlue = new Color() { ARGBValue = -14774017 };
		public static readonly Color Firebrick = new Color() { ARGBValue = -5103070 };
		public static readonly Color FloralWhite = new Color() { ARGBValue = -1296 };
		public static readonly Color ForestGreen = new Color() { ARGBValue = -14513374 };
		public static readonly Color Fuchsia = new Color() { ARGBValue = -65281 };
		public static readonly Color Gainsboro = new Color() { ARGBValue = -2302756 };
		public static readonly Color GhostWhite = new Color() { ARGBValue = -460545 };
		public static readonly Color Gold = new Color() { ARGBValue = -10496 };
		public static readonly Color Goldenrod = new Color() { ARGBValue = -2448096 };
		public static readonly Color Gray = new Color() { ARGBValue = -8355712 };
		public static readonly Color Green = new Color() { ARGBValue = -16744448 };
		public static readonly Color GreenYellow = new Color() { ARGBValue = -5374161 };
		public static readonly Color Honeydew = new Color() { ARGBValue = -983056 };
		public static readonly Color HotPink = new Color() { ARGBValue = -38476 };
		public static readonly Color IndianRed = new Color() { ARGBValue = -3318692 };
		public static readonly Color Indigo = new Color() { ARGBValue = -11861886 };
		public static readonly Color Ivory = new Color() { ARGBValue = -16 };
		public static readonly Color Khaki = new Color() { ARGBValue = -989556 };
		public static readonly Color Lavender = new Color() { ARGBValue = -1644806 };
		public static readonly Color LavenderBlush = new Color() { ARGBValue = -3851 };
		public static readonly Color LawnGreen = new Color() { ARGBValue = -8586240 };
		public static readonly Color LemonChiffon = new Color() { ARGBValue = -1331 };
		public static readonly Color LightBlue = new Color() { ARGBValue = -5383962 };
		public static readonly Color LightCoral = new Color() { ARGBValue = -1015680 };
		public static readonly Color LightCyan = new Color() { ARGBValue = -2031617 };
		public static readonly Color LightGoldenrodYellow = new Color() { ARGBValue = -329006 };
		public static readonly Color LightGreen = new Color() { ARGBValue = -7278960 };
		public static readonly Color LightGray = new Color() { ARGBValue = -2894893 };
		public static readonly Color LightPink = new Color() { ARGBValue = -18751 };
		public static readonly Color LightSalmon = new Color() { ARGBValue = -24454 };
		public static readonly Color LightSeaGreen = new Color() { ARGBValue = -14634326 };
		public static readonly Color LightSkyBlue = new Color() { ARGBValue = -7876870 };
		public static readonly Color LightSlateGray = new Color() { ARGBValue = -8943463 };
		public static readonly Color LightSteelBlue = new Color() { ARGBValue = -5192482 };
		public static readonly Color LightYellow = new Color() { ARGBValue = -32 };
		public static readonly Color Lime = new Color() { ARGBValue = -16711936 };
		public static readonly Color LimeGreen = new Color() { ARGBValue = -13447886 };
		public static readonly Color Linen = new Color() { ARGBValue = -331546 };
		public static readonly Color Magenta = new Color() { ARGBValue = -65281 };
		public static readonly Color Maroon = new Color() { ARGBValue = -8388608 };
		public static readonly Color MediumAquamarine = new Color() { ARGBValue = -10039894 };
		public static readonly Color MediumBlue = new Color() { ARGBValue = -16777011 };
		public static readonly Color MediumOrchid = new Color() { ARGBValue = -4565549 };
		public static readonly Color MediumPurple = new Color() { ARGBValue = -7114533 };
		public static readonly Color MediumSeaGreen = new Color() { ARGBValue = -12799119 };
		public static readonly Color MediumSlateBlue = new Color() { ARGBValue = -8689426 };
		public static readonly Color MediumSpringGreen = new Color() { ARGBValue = -16713062 };
		public static readonly Color MediumTurquoise = new Color() { ARGBValue = -12004916 };
		public static readonly Color MediumVioletRed = new Color() { ARGBValue = -3730043 };
		public static readonly Color MidnightBlue = new Color() { ARGBValue = -15132304 };
		public static readonly Color MintCream = new Color() { ARGBValue = -655366 };
		public static readonly Color MistyRose = new Color() { ARGBValue = -6943 };
		public static readonly Color Moccasin = new Color() { ARGBValue = -6987 };
		public static readonly Color NavajoWhite = new Color() { ARGBValue = -8531 };
		public static readonly Color Navy = new Color() { ARGBValue = -16777088 };
		public static readonly Color OldLace = new Color() { ARGBValue = -133658 };
		public static readonly Color Olive = new Color() { ARGBValue = -8355840 };
		public static readonly Color OliveDrab = new Color() { ARGBValue = -9728477 };
		public static readonly Color Orange = new Color() { ARGBValue = -23296 };
		public static readonly Color OrangeRed = new Color() { ARGBValue = -47872 };
		public static readonly Color Orchid = new Color() { ARGBValue = -2461482 };
		public static readonly Color PaleGoldenrod = new Color() { ARGBValue = -1120086 };
		public static readonly Color PaleGreen = new Color() { ARGBValue = -6751336 };
		public static readonly Color PaleTurquoise = new Color() { ARGBValue = -5247250 };
		public static readonly Color PaleVioletRed = new Color() { ARGBValue = -2396013 };
		public static readonly Color PapayaWhip = new Color() { ARGBValue = -4139 };
		public static readonly Color PeachPuff = new Color() { ARGBValue = -9543 };
		public static readonly Color Peru = new Color() { ARGBValue = -3308225 };
		public static readonly Color Pink = new Color() { ARGBValue = -16181 };
		public static readonly Color Plum = new Color() { ARGBValue = -2252579 };
		public static readonly Color PowderBlue = new Color() { ARGBValue = -5185306 };
		public static readonly Color Purple = new Color() { ARGBValue = -8388480 };
		public static readonly Color Red = new Color() { ARGBValue = -65536 };
		public static readonly Color RosyBrown = new Color() { ARGBValue = -4419697 };
		public static readonly Color RoyalBlue = new Color() { ARGBValue = -12490271 };
		public static readonly Color SaddleBrown = new Color() { ARGBValue = -7650029 };
		public static readonly Color Salmon = new Color() { ARGBValue = -360334 };
		public static readonly Color SandyBrown = new Color() { ARGBValue = -744352 };
		public static readonly Color SeaGreen = new Color() { ARGBValue = -13726889 };
		public static readonly Color SeaShell = new Color() { ARGBValue = -2578 };
		public static readonly Color Sienna = new Color() { ARGBValue = -6270419 };
		public static readonly Color Silver = new Color() { ARGBValue = -4144960 };
		public static readonly Color SkyBlue = new Color() { ARGBValue = -7876885 };
		public static readonly Color SlateBlue = new Color() { ARGBValue = -9807155 };
		public static readonly Color SlateGray = new Color() { ARGBValue = -9404272 };
		public static readonly Color Snow = new Color() { ARGBValue = -1286 };
		public static readonly Color SpringGreen = new Color() { ARGBValue = -16711809 };
		public static readonly Color SteelBlue = new Color() { ARGBValue = -12156236 };
		public static readonly Color Tan = new Color() { ARGBValue = -2968436 };
		public static readonly Color Teal = new Color() { ARGBValue = -16744320 };
		public static readonly Color Thistle = new Color() { ARGBValue = -2572328 };
		public static readonly Color Tomato = new Color() { ARGBValue = -40121 };
		public static readonly Color Turquoise = new Color() { ARGBValue = -12525360 };
		public static readonly Color Violet = new Color() { ARGBValue = -1146130 };
		public static readonly Color Wheat = new Color() { ARGBValue = -663885 };
		public static readonly Color White = new Color() { ARGBValue = -1 };
		public static readonly Color WhiteSmoke = new Color() { ARGBValue = -657931 };
		public static readonly Color Yellow = new Color() { ARGBValue = -256 };
		public static readonly Color YellowGreen = new Color() { ARGBValue = -6632142 };

		#endregion

		[FieldOffset(0)]
		public int ARGBValue;

		[FieldOffset(3)]
		public byte A;

		[FieldOffset(2)]
		public byte R;

		[FieldOffset(2)]
		public byte G;

		[FieldOffset(1)]
		public byte B;
	}
}