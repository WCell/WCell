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
# Date/time:                    2009-06-16 20:25:14
# --------------------------------------------------------

/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;*/


#
# Table structure for table 'skill_initial_ability'
#

CREATE TABLE `skill_initial_ability` (
  `BaseSkillId` mediumint(8) unsigned NOT NULL,
  `GainSpellId` mediumint(8) unsigned NOT NULL,
  PRIMARY KEY  (`BaseSkillId`,`GainSpellId`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;



#
# Dumping data for table 'skills_initialability'
#

LOCK TABLES `skill_initial_ability` WRITE;
/*!40000 ALTER TABLE `skill_initial_ability` DISABLE KEYS;*/
INSERT INTO `skill_initial_ability` (`BaseSkillId`, `GainSpellId`) VALUES
	('164','2660'),
	('164','2662'),
	('164','2663'),
	('164','3115'),
	('164','12260'),
	('165','2149'),
	('165','2152'),
	('165','2881'),
	('165','7126'),
	('165','9058'),
	('165','9059'),
	('171','2329'),
	('171','2330'),
	('171','7183'),
	('182','2383'),
	('185','818'),
	('185','2538'),
	('185','2540'),
	('185','8604'),
	('185','37836'),
	('186','2580'),
	('186','2656'),
	('186','2657'),
	('197','2387'),
	('197','2393'),
	('197','2963'),
	('197','3915'),
	('197','12044'),
	('202','3918'),
	('202','3919'),
	('202','3920'),
	('333','7418'),
	('333','7421'),
	('333','7428'),
	('333','13262'),
	('755','25255'),
	('755','25493'),
	('755','26925'),
	('755','32259'),
	('773','45382'),
	('773','48114'),
	('773','48116'),
	('773','51005'),
	('773','52175'),
	('773','52738'),
	('776','53341'),
	('776','53343');
/*!40000 ALTER TABLE `skill_initial_ability` ENABLE KEYS;*/
UNLOCK TABLES;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;*/
