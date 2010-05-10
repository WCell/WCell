# HeidiSQL Dump 
#
# --------------------------------------------------------
# Host:                         127.0.0.1
# Database:                     wcellrealmtest
# Server version:               5.0.51b-community-nt
# Server OS:                    Win64
# Target compatibility:         Same as source (5.0.51)
# Target max_allowed_packet:    1048576
# HeidiSQL version:             4.0
# Date/time:                    2009-06-16 20:23:11
# --------------------------------------------------------

/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;*/


#
# Table structure for table 'spell_effects'
#

CREATE TABLE `spell_effects` (
  `SpellId` int(11) unsigned NOT NULL default '0',
  `EffectIndex` int(11) unsigned NOT NULL default '0',
  `SpellEffect` int(11) unsigned NOT NULL default '0',
  `DieSides` int(11) unsigned NOT NULL default '0',
  `BaseDice` int(11) unsigned NOT NULL default '0',
  `DicePerLevel` float NOT NULL default '0',
  `RealPointsPerLevel` float NOT NULL default '0',
  `BasePoints` int(11) NOT NULL default '0',
  `Mechanic` int(11) NOT NULL default '0',
  `ImplicitTargetA` int(11) unsigned NOT NULL default '0',
  `ImplicitTargetB` int(11) unsigned NOT NULL default '0',
  `RadiusIndex` int(11) unsigned NOT NULL default '0',
  `ApplyAuraName` int(11) unsigned NOT NULL default '0',
  `Amplitude` int(11) unsigned NOT NULL default '0',
  `ProcValue` float NOT NULL default '0',
  `ChainTarget` int(11) unsigned NOT NULL default '0',
  `ItemType` int(11) unsigned NOT NULL default '0',
  `MiscValue` int(11) unsigned NOT NULL default '0',
  `MiscValueB` int(11) unsigned NOT NULL default '0',
  `TriggerSpellId` int(11) unsigned NOT NULL default '0',
  `PointsPerComboPoint` float NOT NULL default '0',
  `SpellClassMask_1` int(11) unsigned NOT NULL default '0',
  `SpellClassMask_2` int(11) unsigned NOT NULL default '0',
  `SpellClassMask_3` int(11) unsigned NOT NULL default '0',
  PRIMARY KEY  (`SpellId`,`EffectIndex`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;*/
