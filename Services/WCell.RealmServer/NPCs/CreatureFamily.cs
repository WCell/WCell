using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.RealmServer.Skills;

namespace WCell.RealmServer.NPCs
{
    public class CreatureFamily
    {
        public CreatureFamilyId Id;

        public string Name;

        /// <summary>
        /// Pets of this Level will have their max Scale
        /// </summary>
        public int MaxScaleLevel;

        public float MinScale, MaxScale;

        /// <summary>
        /// Scale step per level
        /// </summary>
        public float ScaleStep;

        public PetFoodMask PetFoodMask;

        public PetTalentType PetTalentType;

        public SkillLine SkillLine;

        #region StatModifiers

        //public float StaminaModifier
        //{
        //    get { return StaminaModifierByFamily[(int)Id]; }
        //}

        //public float ArmorModifier
        //{
        //    get { return ArmorModifierByFamily[(int)Id]; }
        //}

        //public float DamageModifier
        //{
        //    get { return DamageModifierByFamily[(int)Id]; }
        //}

        //internal int GetStrengthBonus(int level)
        //{
        //    return 0;
        //}

        //internal int GetAgilityBonus(int level)
        //{
        //    return 0;
        //}

        //internal int GetStaminaBonus(int ownerStamina, int level)
        //{
        //    return 0;
        //}

        //internal int GetIntellectBonus(int level)
        //{
        //    return 0;
        //}

        //internal int GetSpiritBonus(int level)
        //{
        //    return 0;
        //}

        //internal int GetPowerGain(int level)
        //{
        //    return 0;
        //}

        //internal int GetHealthGain(int level)
        //{
        //    return 0;
        //}

        //internal int GetArmorBonus(int ownerArmor, int level)
        //{
        //    return 0;
        //}

        //internal int GetDamageBonus(int ownerDamage, int level)
        //{
        //    return 0;
        //}

        //private static readonly float[] StaminaModifierByFamily = {
        //    1.00f, // None
        //    1.05f, // Wolf
        //    1.05f, // Cat
        //    1.05f, // Spider
        //    1.05f, // Bear
        //    1.05f, // Boar
        //    1.05f, // Crocolisk
        //    1.05f, // CarrionBird
        //    1.05f, // Crab
        //    1.05f, // Gorilla
        //    1.05f, // Raptor
        //    1.05f, // Tallstrider
        //    1.00f, // Felhunter
        //    1.00f, // Voidwalker
        //    1.00f, // Succubus
        //    1.00f, // Doomguard
        //    1.05f, // Scorpid
        //    1.05f, // Turtle
        //    1.00f, // Imp
        //    1.05f, // Bat
        //    1.05f, // Hyena
        //    1.05f, // BirdOfPrey
        //    1.05f, // WindSerpent
        //    1.00f, // RemoteControl
        //    1.00f, // Felguard
        //    1.05f, // Dragonhawk
        //    1.05f, // Ravager
        //    1.05f, // WarpStalker
        //    1.05f, // Sporebat
        //    1.05f, // NetherRay
        //    1.05f, // Serpent
        //    1.05f, // Moth
        //    1.05f, // Chimaera
        //    1.05f, // Devilsaur
        //    1.05f, // Ghoul
        //    1.05f, // Silithid
        //    1.05f, // Worm
        //    1.05f, // Rhino
        //    1.05f, // Wasp
        //    1.05f, // CoreHound
        //    1.05f, // SpiritBeast
        //};

        //private static readonly float[] ArmorModifierByFamily = {
        //    1.00f, // None
        //    1.05f, // Wolf
        //    1.05f, // Cat
        //    1.05f, // Spider
        //    1.05f, // Bear
        //    1.05f, // Boar
        //    1.05f, // Crocolisk
        //    1.05f, // CarrionBird
        //    1.05f, // Crab
        //    1.05f, // Gorilla
        //    1.05f, // Raptor
        //    1.05f, // Tallstrider
        //    1.00f, // Felhunter
        //    1.00f, // Voidwalker
        //    1.00f, // Succubus
        //    1.00f, // Doomguard
        //    1.05f, // Scorpid
        //    1.05f, // Turtle
        //    1.00f, // Imp
        //    1.05f, // Bat
        //    1.05f, // Hyena
        //    1.05f, // BirdOfPrey
        //    1.05f, // WindSerpent
        //    1.00f, // RemoteControl
        //    1.00f, // Felguard
        //    1.05f, // Dragonhawk
        //    1.05f, // Ravager
        //    1.05f, // WarpStalker
        //    1.05f, // Sporebat
        //    1.05f, // NetherRay
        //    1.05f, // Serpent
        //    1.05f, // Moth
        //    1.05f, // Chimaera
        //    1.05f, // Devilsaur
        //    1.05f, // Ghoul
        //    1.05f, // Silithid
        //    1.05f, // Worm
        //    1.05f, // Rhino
        //    1.05f, // Wasp
        //    1.05f, // CoreHound
        //    1.05f, // SpiritBeast
        //};

        //private static readonly float[] DamageModifierByFamily = {
        //    1.00f, // None
        //    1.05f, // Wolf
        //    1.05f, // Cat
        //    1.05f, // Spider
        //    1.05f, // Bear
        //    1.05f, // Boar
        //    1.05f, // Crocolisk
        //    1.05f, // CarrionBird
        //    1.05f, // Crab
        //    1.05f, // Gorilla
        //    1.05f, // Raptor
        //    1.05f, // Tallstrider
        //    1.00f, // Felhunter
        //    1.00f, // Voidwalker
        //    1.00f, // Succubus
        //    1.00f, // Doomguard
        //    1.05f, // Scorpid
        //    1.05f, // Turtle
        //    1.00f, // Imp
        //    1.05f, // Bat
        //    1.05f, // Hyena
        //    1.05f, // BirdOfPrey
        //    1.05f, // WindSerpent
        //    1.00f, // RemoteControl
        //    1.00f, // Felguard
        //    1.05f, // Dragonhawk
        //    1.05f, // Ravager
        //    1.05f, // WarpStalker
        //    1.05f, // Sporebat
        //    1.05f, // NetherRay
        //    1.05f, // Serpent
        //    1.05f, // Moth
        //    1.05f, // Chimaera
        //    1.05f, // Devilsaur
        //    1.05f, // Ghoul
        //    1.05f, // Silithid
        //    1.05f, // Worm
        //    1.05f, // Rhino
        //    1.05f, // Wasp
        //    1.05f, // CoreHound
        //    1.05f, // SpiritBeast
        //};

        #endregion StatModifiers

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, (int)Id);
        }
    } // end class
}