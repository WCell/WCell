namespace WCell.Constants.Pets
{
	public enum PetNameInvalidReason : uint
	{
		Ok,
		Invalid = 1,
		NoName = 2,
		TooShort = 3,
		TooLong = 4,
        // There is no number 5
		MixedLanguages = 6,
		Profane = 7,
		Reserved = 8,
        // There is no number 9
        // There is no number 10
		/// <summary>
		/// For example "errrbear"
		/// </summary>
		ThreeConsecutive = 11,
		InvalidSpace = 12,
		ConsecutiveSpaces = 13,
		RussianConsecutiveSilentChars = 14,
		RussianSilentCharAtBeginOrEnd = 15,
		DeclensionDoesntMatchBaseName = 16
	}
}