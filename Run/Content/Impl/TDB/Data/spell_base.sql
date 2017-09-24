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
# Date/time:                    2009-06-16 20:22:58
# --------------------------------------------------------

/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;*/


#
# Table structure for table 'spell_base'
#

CREATE TABLE `spell_base` (
  `SpellId` int(11) unsigned NOT NULL default '0',
  `Category` int(11) unsigned NOT NULL default '0',
  `DispelType` int(11) unsigned NOT NULL default '0',
  `Mechanic` int(11) unsigned NOT NULL default '0',
  `Attributes` int(11) unsigned NOT NULL default '0',
  `AttributesEx` int(11) unsigned NOT NULL default '0',
  `AttributesExB` int(11) unsigned NOT NULL default '0',
  `AttributesExC` int(11) unsigned NOT NULL default '0',
  `AttributesExD` int(11) unsigned NOT NULL default '0',
  `AttributesExE` int(11) unsigned NOT NULL default '0',
  `AttributesExF` int(11) unsigned NOT NULL default '0',
  `ShapeshiftMask` int(11) unsigned NOT NULL default '0',
  `ExcludeShapeshiftMask` int(11) unsigned NOT NULL default '0',
  `TargetType` int(11) unsigned NOT NULL default '0',
  `TargetCreatureType` int(11) unsigned NOT NULL default '0',
  `RequiresSpellFocus` int(11) unsigned NOT NULL default '0',
  `FacingFlags` int(11) unsigned NOT NULL default '0',
  `CasterAuraState` int(11) unsigned NOT NULL default '0',
  `TargetAuraState` int(11) unsigned NOT NULL default '0',
  `ExcludeCasterAuraState` int(11) unsigned NOT NULL default '0',
  `ExcludeTargetAuraState` int(11) unsigned NOT NULL default '0',
  `CasterAuraSpellId` int(11) unsigned NOT NULL default '0',
  `TargetAuraSpellId` int(11) unsigned NOT NULL default '0',
  `ExcludeCasterSpellId` int(11) unsigned NOT NULL default '0',
  `ExcludeTargetSpellId` int(11) unsigned NOT NULL default '0',
  `CastDelayIndex` int(11) NOT NULL default '0',
  `CooldownTime` int(11) unsigned NOT NULL default '0',
  `CategoryCooldownTime` int(11) unsigned NOT NULL default '0',
  `InterruptFlags` int(11) unsigned NOT NULL default '0',
  `AuraInterruptFlags` int(11) unsigned NOT NULL default '0',
  `ChannelInterruptFlags` int(11) unsigned NOT NULL default '0',
  `ProcTriggerFlags` int(11) unsigned NOT NULL default '0',
  `ProcChance` int(11) unsigned NOT NULL default '0',
  `ProcCharges` int(11) NOT NULL default '0',
  `MaxLevel` int(11) unsigned NOT NULL default '0',
  `BaseLevel` int(11) unsigned NOT NULL default '0',
  `Level` int(11) unsigned NOT NULL default '0',
  `DurationIndex` int(11) NOT NULL default '0',
  `PowerType` int(11) unsigned NOT NULL default '0',
  `PowerCost` int(11) unsigned NOT NULL default '0',
  `PowerCostPerlevel` int(11) unsigned NOT NULL default '0',
  `PowerPerSecond` int(11) unsigned NOT NULL default '0',
  `PowerPerSecondPerLevel` int(11) unsigned NOT NULL default '0',
  `RangeIndex` int(11) unsigned NOT NULL default '0',
  `ProjectileSpeed` float NOT NULL default '0',
  `ModalNextSpell` int(11) unsigned NOT NULL default '0',
  `MaxStackCount` int(11) unsigned NOT NULL default '0',
  `RequiredToolId_1` int(11) unsigned NOT NULL default '0',
  `RequiredToolId_2` int(11) unsigned NOT NULL default '0',
  `ReagentId_1` int(11) unsigned NOT NULL default '0',
  `ReagentId_2` int(11) unsigned NOT NULL default '0',
  `ReagentId_3` int(11) unsigned NOT NULL default '0',
  `ReagentId_4` int(11) unsigned NOT NULL default '0',
  `ReagentId_5` int(11) unsigned NOT NULL default '0',
  `ReagentId_6` int(11) unsigned NOT NULL default '0',
  `ReagentId_7` int(11) unsigned NOT NULL default '0',
  `ReagentId_8` int(11) unsigned NOT NULL default '0',
  `ReagentCount_1` int(11) unsigned NOT NULL default '0',
  `ReagentCount_2` int(11) unsigned NOT NULL default '0',
  `ReagentCount_3` int(11) unsigned NOT NULL default '0',
  `ReagentCount_4` int(11) unsigned NOT NULL default '0',
  `ReagentCount_5` int(11) unsigned NOT NULL default '0',
  `ReagentCount_6` int(11) unsigned NOT NULL default '0',
  `ReagentCount_7` int(11) unsigned NOT NULL default '0',
  `ReagentCount_8` int(11) unsigned NOT NULL default '0',
  `RequiredItemClass` int(11) NOT NULL default '0',
  `RequiredItemSubClassMask` int(11) unsigned NOT NULL default '0',
  `RequiredItemInventorySlotMask` int(11) unsigned NOT NULL default '0',
  `Visual_1` int(11) unsigned NOT NULL default '0',
  `Visual_2` int(11) unsigned NOT NULL default '0',
  `SpellBookIconId` int(11) unsigned NOT NULL default '0',
  `BuffIconId` int(11) unsigned NOT NULL default '0',
  `Priority` int(11) unsigned NOT NULL default '0',
  `Name` varchar(2000) default NULL,
  `RankDescription` varchar(2000) default NULL,
  `BookDescription` varchar(2000) default NULL,
  `BuffDescription` varchar(2000) default NULL,
  `PowerCostPercentage` int(11) unsigned NOT NULL default '0',
  `StartRecoveryTime` int(11) unsigned NOT NULL default '0',
  `StartRecoveryCategory` int(11) unsigned NOT NULL default '0',
  `MaxTargetLevel` int(11) unsigned NOT NULL default '0',
  `FamilyName` int(11) unsigned NOT NULL default '0',
  `SpellClassMask_1` int(11) unsigned NOT NULL default '0',
  `SpellClassMask_2` int(11) unsigned NOT NULL default '0',
  `SpellClassMask_3` int(11) unsigned NOT NULL default '0',
  `MaxTargets` int(11) unsigned NOT NULL default '0',
  `DefenseType` int(11) unsigned NOT NULL default '0',
  `PreventionType` int(11) unsigned NOT NULL default '0',
  `StanceBarOrder` int(11) NOT NULL default '0',
  `DamageMultiplier_1` float NOT NULL default '0' COMMENT 'Chain Effect Amplitude',
  `DamageMultiplier_2` float NOT NULL default '0',
  `DamageMultiplier_3` float NOT NULL default '0',
  `MinFactionId` int(11) unsigned NOT NULL default '0',
  `MinReputation` int(11) unsigned NOT NULL default '0',
  `RequiredAuraVision` int(11) unsigned NOT NULL default '0',
  `RequiredTotemCategoryId_1` int(11) unsigned NOT NULL default '0',
  `RequiredTotemCategoryId_2` int(11) unsigned NOT NULL default '0',
  `RequiredAreaId` int(11) unsigned NOT NULL default '0',
  `SchoolMask` int(11) unsigned NOT NULL default '0',
  `RuneCostId` int(11) unsigned NOT NULL default '0',
  `MissileId` int(11) unsigned NOT NULL default '0',
  PRIMARY KEY  (`SpellId`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;*/
