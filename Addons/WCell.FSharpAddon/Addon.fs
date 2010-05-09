namespace WCell.Addons

open System;
open System.Globalization;
open WCell.Core.Addons;


type public FSharpAddon = 
	class
		// F# has no default ctor
		new() = { }
		interface IWCellAddon
		with
			member public x.get_Name() = "First FSharp Addon"
			member public x.get_Author() = "The WCell Team"
			member public x.get_Website() = "http://WCell.org"
			member public x.GetLocalizedName (culture) = "First FSharp Addon"
			member public x.Init() = 
				Console.WriteLine("FSharp test!");
		end
	end;;