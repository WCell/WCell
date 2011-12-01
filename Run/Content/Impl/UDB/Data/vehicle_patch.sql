/*
author: zergtmn, developed by rsa
HUGE THANKS TO THEM!!!
*/
ALTER TABLE creature_template
  ADD COLUMN `spell5` mediumint(8) unsigned NOT NULL default '0' AFTER `spell4`,
  ADD COLUMN `spell6` mediumint(8) unsigned NOT NULL default '0' AFTER `spell5`,
  ADD COLUMN `spell7` mediumint(8) unsigned NOT NULL default '0' AFTER `spell6`,
  ADD COLUMN `spell8` mediumint(8) unsigned NOT NULL default '0' AFTER `spell7`,
  ADD COLUMN `VehicleId` mediumint(8) unsigned NOT NULL default '0' AFTER `PetSpellDataId`,
  ADD COLUMN `PowerType` tinyint(3) unsigned NOT NULL default '0' AFTER `MaxHealth`;

DROP TABLE IF EXISTS `vehicle_accessory`;
CREATE TABLE `vehicle_accessory` (
  `entry` mediumint(8) unsigned NOT NULL DEFAULT '0',
  `accessory_entry` mediumint(8) unsigned NOT NULL DEFAULT '0',
  `seat_id` tinyint(1) NOT NULL DEFAULT '0',
  `minion` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `description` text NOT NULL,
  PRIMARY KEY (`entry`,`seat_id`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=FIXED COMMENT='Vehicle Accessory System';

INSERT INTO `vehicle_accessory`(`entry`,`accessory_entry`,`seat_id`,`minion`,`description`) values
(28312,28319,7,1,'Wintergrasp Siege Engine'),
(32627,32629,7,1,'Wintergrasp Siege Engine'),
(32930,32933,0,1,'Kologarn'),
(32930,32934,1,1,'Kologarn'),
(33109,33167,1,1,'Salvaged Demolisher'),
(33109,33620,2,1,'Salvaged Demolisher'),
(33060,33067,7,1,'Salvaged Siege Engine'),
(29931,29982,0,0,'Drakkari Rider on Drakkari Rhino'),
(33113,33114,2,1,'Flame Leviathan'),
(33113,33114,3,1,'Flame Leviathan'),
(33113,33139,7,1,'Flame Leviathan'),
(33114,33142,1,1,'Overload Control Device'),
(33114,33143,2,1,'Leviathan Defense Turret'),
(36678,38309,0,1,'Professor Putricide - trigger'),
(33214,33218,1,1,'Mechanolift 304-A'),
(35637,34705,0,0,'Marshal Jacob Alerius\' Mount'),
(35633,34702,0,0,'Ambrose Boltspark\'s Mount'),
(35768,34701,0,0,'Colosos\' Mount'),
(34658,34657,0,0,'Jaelyne Evensong\'s Mount'),
(35636,34703,0,0,'Lana Stouthammer\'s Mount'),
(35638,35572,0,0,'Mokra the Skullcrusher\'s Mount'),
(35635,35569,0,0,'Eressea Dawnsinger\'s Mount'),
(35640,35571,0,0,'Runok Wildmane\'s Mount'),
(35641,35570,0,0,'Zul\'tore\'s Mount'),
(35634,35617,0,0,'Deathstalker Visceri\'s Mount'),
(27241,27268,0,0,'Risen Gryphon'),
(27661,27662,0,0,'Wintergarde Gryphon'),
(29698,29699,0,0,'Drakuru Raptor'),
(33778,33780,0,1,'Tournament Hippogryph'),
(33687,33695,0,1,'Chillmaw'),
(33687,33695,1,1,'Chillmaw'),
(33687,33695,2,1,'Chillmaw'),
(29625,29694,0,0,'Hyldsmeet Proto-Drake'),
(30330,30332,0,0,'Jotunheim Proto-Drake'),
(32189,32190,0,0,'Skybreaker Recon Fighter'),
(36678,38308,1,1,'Professor Putricide - trigger'),
(32640,32642,1,0,'Traveler Mammoth (H) - Vendor'),
(32640,32641,2,0,'Traveler Mammoth (H) - Vendor & Repairer'),
(32633,32638,1,0,'Traveler Mammoth (A) - Vendor'),
(32633,32639,2,0,'Traveler Mammoth (A) - Vendor & Repairer'),
(33669,33666,0,0,'Demolisher Engineer Blastwrench'),
(29555,29556,0,0,'Goblin Sapper'),
(28018,28006,0,1,'Thiassi the Light Bringer'),
(28054,28053,0,0,'Lucky Wilhelm - Apple'),
(28614,28616,0,1,'Scarlet Gryphon Rider'),
(36891,31260,0,0,'Ymirjar Skycaller on Drake'),
(36476,36477,0,0,'Krick on Ick'),
(29433,29440,0,0,'Goblin Sapper in K3');

-- Known vehicles from zergtmn
#UPDATE `creature_template` SET `VehicleId` = 0;
--
UPDATE `creature_template` SET `VehicleId` = 23 WHERE `entry` = 23693;
UPDATE `creature_template` SET `VehicleId` = 108 WHERE `entry` = 24083;
UPDATE `creature_template` SET `VehicleId` = 8 WHERE `entry` = 24418;
UPDATE `creature_template` SET `VehicleId` = 16 WHERE `entry` = 24705;
UPDATE `creature_template` SET `VehicleId` = 17 WHERE `entry` = 24750;
UPDATE `creature_template` SET `VehicleId` = 26 WHERE `entry` = 25334;
UPDATE `creature_template` SET `VehicleId` = 29 WHERE `entry` = 25596;
UPDATE `creature_template` SET `VehicleId` = 72 WHERE `entry` = 25743;
UPDATE `creature_template` SET `VehicleId` = 27 WHERE `entry` = 25762;
UPDATE `creature_template` SET `VehicleId` = 30 WHERE `entry` = 25968;
UPDATE `creature_template` SET `VehicleId` = 62 WHERE `entry` = 26472;
UPDATE `creature_template` SET `VehicleId` = 36 WHERE `entry` = 26523;
UPDATE `creature_template` SET `VehicleId` = 33 WHERE `entry` = 26525;
UPDATE `creature_template` SET `VehicleId` = 34 WHERE `entry` = 26572;
UPDATE `creature_template` SET `VehicleId` = 53 WHERE `entry` = 26590;
UPDATE `creature_template` SET `VehicleId` = 37 WHERE `entry` = 26777;
UPDATE `creature_template` SET `VehicleId` = 38 WHERE `entry` = 26778;
UPDATE `creature_template` SET `VehicleId` = 40 WHERE `entry` = 26893;
UPDATE `creature_template` SET `VehicleId` = 53 WHERE `entry` = 27131;
UPDATE `creature_template` SET `VehicleId` = 43 WHERE `entry` = 27213;
UPDATE `creature_template` SET `VehicleId` = 48 WHERE `entry` = 27241;
UPDATE `creature_template` SET `VehicleId` = 44 WHERE `entry` = 27258;
UPDATE `creature_template` SET `VehicleId` = 50 WHERE `entry` = 27261;
UPDATE `creature_template` SET `VehicleId` = 46 WHERE `entry` = 27270;
UPDATE `creature_template` SET `VehicleId` = 50 WHERE `entry` = 27292;
UPDATE `creature_template` SET `VehicleId` = 49 WHERE `entry` = 27354;
UPDATE `creature_template` SET `VehicleId` = 55 WHERE `entry` = 27496;
UPDATE `creature_template` SET `VehicleId` = 56 WHERE `entry` = 27587;
UPDATE `creature_template` SET `VehicleId` = 57 WHERE `entry` = 27593;
UPDATE `creature_template` SET `VehicleId` = 59 WHERE `entry` = 27626;
UPDATE `creature_template` SET `VehicleId` = 60 WHERE `entry` = 27629;
UPDATE `creature_template` SET `VehicleId` = 61 WHERE `entry` = 27661;
UPDATE `creature_template` SET `VehicleId` = 154 WHERE `entry` = 27671;
UPDATE `creature_template` SET `VehicleId` = 68 WHERE `entry` = 27714;
UPDATE `creature_template` SET `VehicleId` = 70 WHERE `entry` = 27755;
UPDATE `creature_template` SET `VehicleId` = 256 WHERE `entry` = 27761;
UPDATE `creature_template` SET `VehicleId` = 68 WHERE `entry` = 27839;
UPDATE `creature_template` SET `VehicleId` = 79 WHERE `entry` = 27881;
UPDATE `creature_template` SET `VehicleId` = 160 WHERE `entry` = 27894;
UPDATE `creature_template` SET `VehicleId` = 89 WHERE `entry` = 27924;
UPDATE `creature_template` SET `VehicleId` = 97 WHERE `entry` = 27992;
UPDATE `creature_template` SET `VehicleId` = 97 WHERE `entry` = 27993;
UPDATE `creature_template` SET `VehicleId` = 99 WHERE `entry` = 27996;
UPDATE `creature_template` SET `VehicleId` = 105 WHERE `entry` = 28009;
UPDATE `creature_template` SET `VehicleId` = 100 WHERE `entry` = 28018;
UPDATE `creature_template` SET `VehicleId` = 102 WHERE `entry` = 28054;
UPDATE `creature_template` SET `VehicleId` = 106 WHERE `entry` = 28094;
UPDATE `creature_template` SET `VehicleId` = 110 WHERE `entry` = 28192;
UPDATE `creature_template` SET `VehicleId` = 117 WHERE `entry` = 28312;
UPDATE `creature_template` SET `VehicleId` = 116 WHERE `entry` = 28319;
UPDATE `creature_template` SET `VehicleId` = 244 WHERE `entry` = 28366;
UPDATE `creature_template` SET `VehicleId` = 200 WHERE `entry` = 28605;
UPDATE `creature_template` SET `VehicleId` = 123 WHERE `entry` = 28606;
UPDATE `creature_template` SET `VehicleId` = 200 WHERE `entry` = 28607;
UPDATE `creature_template` SET `VehicleId` = 124 WHERE `entry` = 28614;
UPDATE `creature_template` SET `VehicleId` = 156 WHERE `entry` = 28670;
UPDATE `creature_template` SET `VehicleId` = 158 WHERE `entry` = 28781;
UPDATE `creature_template` SET `VehicleId` = 145 WHERE `entry` = 28851;
UPDATE `creature_template` SET `VehicleId` = 68 WHERE `entry` = 28887;
UPDATE `creature_template` SET `VehicleId` = 153 WHERE `entry` = 29043;
UPDATE `creature_template` SET `VehicleId` = 25 WHERE `entry` = 29144;
UPDATE `creature_template` SET `VehicleId` = 166 WHERE `entry` = 29414;
UPDATE `creature_template` SET `VehicleId` = 168 WHERE `entry` = 29433;
UPDATE `creature_template` SET `VehicleId` = 190 WHERE `entry` = 29679;
UPDATE `creature_template` SET `VehicleId` = 192 WHERE `entry` = 29691;
UPDATE `creature_template` SET `VehicleId` = 193 WHERE `entry` = 29698;
UPDATE `creature_template` SET `VehicleId` = 207 WHERE `entry` = 29753;
UPDATE `creature_template` SET `VehicleId` = 202 WHERE `entry` = 29857;
UPDATE `creature_template` SET `VehicleId` = 208 WHERE `entry` = 29918;
UPDATE `creature_template` SET `VehicleId` = 318 WHERE `entry` = 29929;
UPDATE `creature_template` SET `VehicleId` = 196 WHERE `entry` = 30013;
UPDATE `creature_template` SET `VehicleId` = 213 WHERE `entry` = 30066;
UPDATE `creature_template` SET `VehicleId` = 222 WHERE `entry` = 30174;
UPDATE `creature_template` SET `VehicleId` = 225 WHERE `entry` = 30204;
UPDATE `creature_template` SET `VehicleId` = 234 WHERE `entry` = 30228;
UPDATE `creature_template` SET `VehicleId` = 233 WHERE `entry` = 30275;
UPDATE `creature_template` SET `VehicleId` = 177 WHERE `entry` = 30320;
UPDATE `creature_template` SET `VehicleId` = 228 WHERE `entry` = 30330;
UPDATE `creature_template` SET `VehicleId` = 229 WHERE `entry` = 30337;
UPDATE `creature_template` SET `VehicleId` = 245 WHERE `entry` = 30342;
UPDATE `creature_template` SET `VehicleId` = 230 WHERE `entry` = 30343;
UPDATE `creature_template` SET `VehicleId` = 236 WHERE `entry` = 30403;
UPDATE `creature_template` SET `VehicleId` = 242 WHERE `entry` = 30470;
UPDATE `creature_template` SET `VehicleId` = 247 WHERE `entry` = 30564;
UPDATE `creature_template` SET `VehicleId` = 248 WHERE `entry` = 30585;
UPDATE `creature_template` SET `VehicleId` = 250 WHERE `entry` = 30645;
UPDATE `creature_template` SET `VehicleId` = 262 WHERE `entry` = 31125;
UPDATE `creature_template` SET `VehicleId` = 270 WHERE `entry` = 31137;
UPDATE `creature_template` SET `VehicleId` = 263 WHERE `entry` = 31139;
UPDATE `creature_template` SET `VehicleId` = 265 WHERE `entry` = 31224;
UPDATE `creature_template` SET `VehicleId` = 267 WHERE `entry` = 31262;
UPDATE `creature_template` SET `VehicleId` = 279 WHERE `entry` = 31583;
UPDATE `creature_template` SET `VehicleId` = 280 WHERE `entry` = 31641;
UPDATE `creature_template` SET `VehicleId` = 109 WHERE `entry` = 31689;
UPDATE `creature_template` SET `VehicleId` = 284 WHERE `entry` = 31702;
UPDATE `creature_template` SET `VehicleId` = 174 WHERE `entry` = 31722;
UPDATE `creature_template` SET `VehicleId` = 312 WHERE `entry` = 31857;
UPDATE `creature_template` SET `VehicleId` = 312 WHERE `entry` = 31858;
UPDATE `creature_template` SET `VehicleId` = 315 WHERE `entry` = 31861;
UPDATE `creature_template` SET `VehicleId` = 315 WHERE `entry` = 31862;
UPDATE `creature_template` SET `VehicleId` = 290 WHERE `entry` = 31881;
UPDATE `creature_template` SET `VehicleId` = 291 WHERE `entry` = 31884;
UPDATE `creature_template` SET `VehicleId` = 294 WHERE `entry` = 32189;
UPDATE `creature_template` SET `VehicleId` = 312 WHERE `entry` = 32212;
UPDATE `creature_template` SET `VehicleId` = 312 WHERE `entry` = 32213;
UPDATE `creature_template` SET `VehicleId` = 298 WHERE `entry` = 32225;
UPDATE `creature_template` SET `VehicleId` = 318 WHERE `entry` = 32286;
UPDATE `creature_template` SET `VehicleId` = 113 WHERE `entry` = 32323;
UPDATE `creature_template` SET `VehicleId` = 304 WHERE `entry` = 32490;
UPDATE `creature_template` SET `VehicleId` = 165 WHERE `entry` = 32535;
UPDATE `creature_template` SET `VehicleId` = 324 WHERE `entry` = 32627;
UPDATE `creature_template` SET `VehicleId` = 116 WHERE `entry` = 32629;
UPDATE `creature_template` SET `VehicleId` = 312 WHERE `entry` = 32633;
UPDATE `creature_template` SET `VehicleId` = 313 WHERE `entry` = 32640;
UPDATE `creature_template` SET `VehicleId` = 160 WHERE `entry` = 32795;
UPDATE `creature_template` SET `VehicleId` = 158 WHERE `entry` = 32796;
UPDATE `creature_template` SET `VehicleId` = 328 WHERE `entry` = 32930;
UPDATE `creature_template` SET `VehicleId` = 380 WHERE `entry` = 32934;
UPDATE `creature_template` SET `VehicleId` = 336 WHERE `entry` = 33060;
UPDATE `creature_template` SET `VehicleId` = 335 WHERE `entry` = 33062;
UPDATE `creature_template` SET `VehicleId` = 337 WHERE `entry` = 33067;
UPDATE `creature_template` SET `VehicleId` = 338 WHERE `entry` = 33109;
UPDATE `creature_template` SET `VehicleId` = 387 WHERE `entry` = 33113;
UPDATE `creature_template` SET `VehicleId` = 341 WHERE `entry` = 33114;
UPDATE `creature_template` SET `VehicleId` = 342 WHERE `entry` = 33118;
UPDATE `creature_template` SET `VehicleId` = 345 WHERE `entry` = 33167;
UPDATE `creature_template` SET `VehicleId` = 348 WHERE `entry` = 33214;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE `entry` = 33217;
UPDATE `creature_template` SET `VehicleId` = 353 WHERE `entry` = 33293;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE `entry` = 33319;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE `entry` = 33321;
UPDATE `creature_template` SET `VehicleId` = 368 WHERE `entry` = 33513;
UPDATE `creature_template` SET `VehicleId` = 372 WHERE `entry` = 33669;
UPDATE `creature_template` SET `VehicleId` = 375 WHERE `entry` = 33687;
UPDATE `creature_template` SET `VehicleId` = 108 WHERE `entry` = 33778;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE `entry` = 33844;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE `entry` = 33845;
UPDATE `creature_template` SET `VehicleId` = 390 WHERE `entry` = 34120;
UPDATE `creature_template` SET `VehicleId` = 397 WHERE `entry` = 34161;
UPDATE `creature_template` SET `VehicleId` = 430 WHERE `entry` = 34658;
UPDATE `creature_template` SET `VehicleId` = 477 WHERE `entry` = 34703;
UPDATE `creature_template` SET `VehicleId` = 509 WHERE `entry` = 34775;
UPDATE `creature_template` SET `VehicleId` = 438 WHERE `entry` = 34793;
UPDATE `creature_template` SET `VehicleId` = 442 WHERE `entry` = 34796;
UPDATE `creature_template` SET `VehicleId` = 446 WHERE `entry` = 34826;
UPDATE `creature_template` SET `VehicleId` = 452 WHERE `entry` = 34929;
UPDATE `creature_template` SET `VehicleId` = 453 WHERE `entry` = 34935;
UPDATE `creature_template` SET `VehicleId` = 510 WHERE `entry` = 34944;
UPDATE `creature_template` SET `VehicleId` = 447 WHERE `entry` = 35273;
UPDATE `creature_template` SET `VehicleId` = 107 WHERE `entry` = 35373;
UPDATE `creature_template` SET `VehicleId` = 487 WHERE `entry` = 35474;
UPDATE `creature_template` SET `VehicleId` = 107 WHERE `entry` = 35491;
UPDATE `creature_template` SET `VehicleId` = 477 WHERE `entry` = 35572;
UPDATE `creature_template` SET `VehicleId` = 478 WHERE `entry` = 35633;
UPDATE `creature_template` SET `VehicleId` = 479 WHERE `entry` = 35634;
UPDATE `creature_template` SET `VehicleId` = 481 WHERE `entry` = 35636;
UPDATE `creature_template` SET `VehicleId` = 482 WHERE `entry` = 35637;
UPDATE `creature_template` SET `VehicleId` = 483 WHERE `entry` = 35638;
UPDATE `creature_template` SET `VehicleId` = 484 WHERE `entry` = 35640;
UPDATE `creature_template` SET `VehicleId` = 529 WHERE `entry` = 35644;
UPDATE `creature_template` SET `VehicleId` = 489 WHERE `entry` = 35768;
UPDATE `creature_template` SET `VehicleId` = 655 WHERE `entry` = 35819;
UPDATE `creature_template` SET `VehicleId` = 436 WHERE `entry` = 36356;
UPDATE `creature_template` SET `VehicleId` = 522 WHERE `entry` = 36476;
UPDATE `creature_template` SET `VehicleId` = 529 WHERE `entry` = 36559;
UPDATE `creature_template` SET `VehicleId` = 535 WHERE `entry` = 36661;
UPDATE `creature_template` SET `VehicleId` = 551 WHERE `entry` = 36794;
UPDATE `creature_template` SET `VehicleId` = 554 WHERE `entry` = 36838;
UPDATE `creature_template` SET `VehicleId` = 560 WHERE `entry` = 36891;
UPDATE `creature_template` SET `VehicleId` = 562 WHERE `entry` = 36896;
UPDATE `creature_template` SET `VehicleId` = 622 WHERE `entry` = 37120;
UPDATE `creature_template` SET `VehicleId` = 611 WHERE `entry` = 37968;
UPDATE `creature_template` SET `VehicleId` = 636 WHERE `entry` = 38500;

-- Mechano-hog, Mekgineer's Chopper
UPDATE `creature_template` SET `VehicleId` = 318, IconName = 'vehichleCursor' WHERE `entry` IN (29929, 32286);
-- Traveler's Tundra Mammoth
-- Grand Ice Mammoth
-- Grand Black War Mammoth
-- Grand Caravan Mammoth
UPDATE `creature_template` SET `VehicleId` = 312, IconName = 'vehichleCursor' WHERE `entry` IN (32633, 32640, 31857, 31858, 31861, 31862, 32212, 32213);
-- X-53 Touring Rocket
UPDATE `creature_template` SET `VehicleId` = 774, IconName = 'vehichleCursor' WHERE `entry` = 40725;

# Sniffed by zergtmn
UPDATE `creature_template` SET `VehicleId` = 328 WHERE `entry` = 32930;
UPDATE `creature_template` SET `VehicleId` = 380 WHERE `entry` = 32934;
UPDATE `creature_template` SET `VehicleId` = 336 WHERE `entry` = 33060;
UPDATE `creature_template` SET `VehicleId` = 335 WHERE `entry` = 33062;
UPDATE `creature_template` SET `VehicleId` = 337 WHERE `entry` = 33067;
UPDATE `creature_template` SET `VehicleId` = 347 WHERE `entry` = 33108;
UPDATE `creature_template` SET `VehicleId` = 338 WHERE `entry` = 33109;
UPDATE `creature_template` SET `VehicleId` = 387 WHERE `entry` = 33113;
UPDATE `creature_template` SET `VehicleId` = 341 WHERE `entry` = 33114;
UPDATE `creature_template` SET `VehicleId` = 342 WHERE `entry` = 33118;
UPDATE `creature_template` SET `VehicleId` = 345 WHERE `entry` = 33167;
UPDATE `creature_template` SET `VehicleId` = 348 WHERE `entry` = 33214;
UPDATE `creature_template` SET `VehicleId` = 381 WHERE `entry` = 33288;
UPDATE `creature_template` SET `VehicleId` = 353 WHERE `entry` = 33293;
UPDATE `creature_template` SET `VehicleId` = 356 WHERE `entry` = 33364;
UPDATE `creature_template` SET `VehicleId` = 357 WHERE `entry` = 33366;
UPDATE `creature_template` SET `VehicleId` = 358 WHERE `entry` = 33369;
UPDATE `creature_template` SET `VehicleId` = 371 WHERE `entry` = 33651;
UPDATE `creature_template` SET `VehicleId` = 372 WHERE `entry` = 33669;
UPDATE `creature_template` SET `VehicleId` = 108 WHERE `entry` = 33778;
UPDATE `creature_template` SET `VehicleId` = 385 WHERE `entry` = 33983;
UPDATE `creature_template` SET `VehicleId` = 390 WHERE `entry` = 34120;
UPDATE `creature_template` SET `VehicleId` = 392 WHERE `entry` = 34146;
UPDATE `creature_template` SET `VehicleId` = 395 WHERE `entry` = 34150;
UPDATE `creature_template` SET `VehicleId` = 396 WHERE `entry` = 34151;
UPDATE `creature_template` SET `VehicleId` = 397 WHERE `entry` = 34161;
UPDATE `creature_template` SET `VehicleId` = 399 WHERE `entry` = 34183;

UPDATE `creature_template` SET `VehicleId` = 143 WHERE `entry` = 28864; -- Scourge Gryphon
UPDATE `creature_template` SET `VehicleId` = 123 WHERE `entry` = 28605; -- Havenshire Stallion
UPDATE `creature_template` SET `VehicleId` = 135 WHERE `entry` = 28782; -- Acherus Deathcharger
-- UPDATE `creature_template` SET `VehicleId` = 138 WHERE `entry` = 28817; -- Mine Car
UPDATE `creature_template` SET `VehicleId` = 87 WHERE `entry` = 28817; -- Mine Car (safety)
UPDATE `creature_template` SET `VehicleId` = 139 WHERE `entry` = 28833; -- Scarlet Cannon

UPDATE `creature_template` SET `VehicleId` = 370 WHERE `entry` = 33432; -- Leviathan Mk II
UPDATE `creature_template` SET `VehicleId` = 373 WHERE `entry` = 33670; -- Aerial Command Unit

UPDATE `creature_template` SET `VehicleId` = 736 WHERE `entry` = 40305; -- Spirit of the Tiger

#
UPDATE `creature_template` SET `VehicleId` = 220 WHERE `entry` = 30161;
UPDATE `creature_template` SET `VehicleId` = 224 WHERE `entry` = 30234;
UPDATE `creature_template` SET `VehicleId` = 223 WHERE `entry` = 30248;

-- fom Burned
UPDATE creature_template SET IconName="vehichleCursor" WHERE entry IN
(29144,32633,24418,25334,25743,26191,26523,26813,27061,27258,27354,27409,27496,27587,27593,27626,
27661,27692,27714,27755,27756,27838,27839,27850,27883,27905,27996,28851,29563,29598,29602,29708,
29857,29903,30021,30066,30108,30123,30124,30134,30228,30234,30248,30272,30403,30500,31070,31407,
31408,31409,31717,31736,31770,31840,31856,31858,31884,32152,32158,32227,32286,32370,32640,33782);

UPDATE creature_template SET IconName="Gunner" WHERE entry IN
(28319,28366,28833,30236,32629,33067,33167,33080,33139,33264,34111);

-- From Timmit
-- bone spike
UPDATE `creature_template` SET `VehicleId` = 647 WHERE `entry` IN (38711,38970,38971,38972);
UPDATE `creature_template` SET `VehicleId` = 533 WHERE `entry` IN (36619,38233,38459,38460);

-- Putricide
UPDATE `creature_template` SET `VehicleId` = 587 WHERE `entry` IN (36678,38431,38585,38586);
UPDATE `creature_template` SET `VehicleId` = 591 WHERE `entry` IN (37672,38605,38786,38787);

# full fix
UPDATE `creature_template` SET `IconName` = 'vehichleCursor' WHERE `VehicleId` > 0 AND `IconName` IS NULL;

# spellclicks
-- from zergtmn
DELETE FROM `npc_spellclick_spells` WHERE `npc_entry` IN (33109, 33062, 33060);
INSERT INTO `npc_spellclick_spells` VALUES
(33109, 62309, 0, 0, 0, 1),  -- Demolisher
(33062, 65030, 0, 0, 0, 1),  -- Chopper
(33060, 65031, 0, 0, 0, 1);  -- Siege engine

-- vehicle spells

-- from rsa
-- chopper
UPDATE `creature_template` SET `IconName` = 'vehichleCursor', `PowerType` = 3,
`spell1` = 62974, `spell2` = 62286, `spell3` = 62299, `spell4` = 64660, `AIName` = 'NullAI'
WHERE `entry` IN (33062);
-- Siege engine
UPDATE `creature_template` SET `IconName` = 'vehichleCursor',  `PowerType` = 3,
`spell1` = 62345, `spell2` = 62522, `spell3` = 62346, `AIName` = 'NullAI'
WHERE `entry` IN (33060);
-- demolisher
UPDATE `creature_template` SET `IconName` = 'vehichleCursor', `PowerType` = 3,
`spell1` = 62306, `spell2` = 62490, `spell3` = 62308, `spell4` =  62324, `AIName` = 'NullAI'
WHERE `entry` IN (33109);

-- from traponinet

-- Salvaged Siege Turret
UPDATE `creature_template` SET `PowerType`=3,spell1=62358,spell2=62359,spell3=64677,spell4=0,spell5=0,spell6=0 WHERE `entry`=33067;

-- Salvaged Demolisher Mechanic Seat
UPDATE `creature_template` SET `PowerType`=3,spell1=62634,spell2=64979,spell3=62479,spell4=62471,spell5=0,spell6=62428 WHERE `entry`=33167;

-- Earthen Stoneshaper
UPDATE `creature_template` SET `unit_flags`=33587968 WHERE `entry`=33620;

-- from Lordron
-- Spectral tiger
UPDATE `creature_template` SET `VehicleId` = 354 WHERE `entry` = 33357;
-- Shalewing
UPDATE `creature_template` SET `VehicleId` = 146 WHERE `entry` = 28875;
-- Drakkari Skullcrusher
UPDATE `creature_template` SET `VehicleId` = 774 WHERE `entry` = 28844;
-- ICC
UPDATE `creature_template` SET `vehicleId` = 531 WHERE `entry` IN (36598);
UPDATE `creature_template` SET `vehicleId` = 532 WHERE `entry` IN (36609,39120,39121,39122);

-- from YTDB/TC 570
UPDATE `creature_template` SET `VehicleId` = 51  WHERE  `entry` = 27409;
UPDATE `creature_template` SET `VehicleId` = 107 WHERE  `entry` = 28135;
UPDATE `creature_template` SET `VehicleId` = 206 WHERE  `entry` = 28379;
UPDATE `creature_template` SET `VehicleId` = 121 WHERE  `entry` = 28468;
UPDATE `creature_template` SET `VehicleId` = 492 WHERE  `entry` = 25765;
UPDATE `creature_template` SET `VehicleId` = 25  WHERE  `entry` = 27516;
UPDATE `creature_template` SET `VehicleId` = 156 WHERE  `entry` = 26788;
UPDATE `creature_template` SET `VehicleId` = 129 WHERE  `entry` = 28710;
UPDATE `creature_template` SET `VehicleId` = 25  WHERE  `entry` = 28446;
UPDATE `creature_template` SET `VehicleId` = 22  WHERE  `entry` = 24825;
UPDATE `creature_template` SET `VehicleId` = 22  WHERE  `entry` = 24821;
UPDATE `creature_template` SET `VehicleId` = 22  WHERE  `entry` = 24823;
UPDATE `creature_template` SET `VehicleId` = 22  WHERE  `entry` = 24806;
UPDATE `creature_template` SET `VehicleId` = 200 WHERE  `entry` = 26191;
UPDATE `creature_template` SET `VehicleId` = 113 WHERE  `entry` = 28246;
UPDATE `creature_template` SET `VehicleId` = 156 WHERE  `entry` = 27850;
UPDATE `creature_template` SET `VehicleId` = 156 WHERE  `entry` = 27838;
UPDATE `creature_template` SET `VehicleId` = 30  WHERE  `entry` = 25881;
UPDATE `creature_template` SET `VehicleId` = 156 WHERE  `entry` = 26807;
UPDATE `creature_template` SET `VehicleId` = 34  WHERE  `entry` = 26585;
UPDATE `creature_template` SET `VehicleId` = 39  WHERE  `entry` = 26813;
UPDATE `creature_template` SET `VehicleId` = 127 WHERE  `entry` = 28669;
UPDATE `creature_template` SET `VehicleId` = 203 WHERE  `entry` = 29863;
UPDATE `creature_template` SET `VehicleId` = 200 WHERE  `entry` = 27883;
UPDATE `creature_template` SET `VehicleId` = 111 WHERE  `entry` = 28222;
UPDATE `creature_template` SET `VehicleId` = 115 WHERE  `entry` = 28308;
UPDATE `creature_template` SET `VehicleId` = 191 WHERE  `entry` = 29306;
UPDATE `creature_template` SET `VehicleId` = 176 WHERE  `entry` = 29351;
UPDATE `creature_template` SET `VehicleId` = 177 WHERE  `entry` = 29358;
UPDATE `creature_template` SET `VehicleId` = 165 WHERE  `entry` = 29403;
UPDATE `creature_template` SET `VehicleId` = 169 WHERE  `entry` = 29460;
UPDATE `creature_template` SET `VehicleId` = 173 WHERE  `entry` = 29500;
UPDATE `creature_template` SET `VehicleId` = 175 WHERE  `entry` = 29555;
UPDATE `creature_template` SET `VehicleId` = 179 WHERE  `entry` = 29579;
UPDATE `creature_template` SET `VehicleId` = 181 WHERE  `entry` = 29602;
UPDATE `creature_template` SET `VehicleId` = 183 WHERE  `entry` = 29625;
UPDATE `creature_template` SET `VehicleId` = 186 WHERE  `entry` = 29677;
UPDATE `creature_template` SET `VehicleId` = 198 WHERE  `entry` = 29708;
UPDATE `creature_template` SET `VehicleId` = 194 WHERE  `entry` = 29709;
UPDATE `creature_template` SET `VehicleId` = 243 WHERE  `entry` = 29736;
UPDATE `creature_template` SET `VehicleId` = 197 WHERE  `entry` = 29754;
UPDATE `creature_template` SET `VehicleId` = 201 WHERE  `entry` = 29838;
UPDATE `creature_template` SET `VehicleId` = 205 WHERE  `entry` = 29884;
UPDATE `creature_template` SET `VehicleId` = 209 WHERE  `entry` = 29931;
UPDATE `creature_template` SET `VehicleId` = 214 WHERE  `entry` = 30090;
UPDATE `creature_template` SET `VehicleId` = 217 WHERE  `entry` = 30124;
UPDATE `creature_template` SET `VehicleId` = 219 WHERE  `entry` = 30134;
UPDATE `creature_template` SET `VehicleId` = 221 WHERE  `entry` = 30165;
UPDATE `creature_template` SET `VehicleId` = 227 WHERE  `entry` = 30301;
UPDATE `creature_template` SET `VehicleId` = 25  WHERE  `entry` = 30378;
UPDATE `creature_template` SET `VehicleId` = 316 WHERE  `entry` = 30468;
UPDATE `creature_template` SET `VehicleId` = 252 WHERE  `entry` = 30719;
UPDATE `creature_template` SET `VehicleId` = 259 WHERE  `entry` = 31110;
UPDATE `creature_template` SET `VehicleId` = 265 WHERE  `entry` = 31163;
UPDATE `creature_template` SET `VehicleId` = 265 WHERE  `entry` = 31220;
UPDATE `creature_template` SET `VehicleId` = 265 WHERE  `entry` = 31221;
UPDATE `creature_template` SET `VehicleId` = 269 WHERE  `entry` = 31268;
UPDATE `creature_template` SET `VehicleId` = 268 WHERE  `entry` = 31269;
UPDATE `creature_template` SET `VehicleId` = 273 WHERE  `entry` = 31406;
UPDATE `creature_template` SET `VehicleId` = 277 WHERE  `entry` = 31407;
UPDATE `creature_template` SET `VehicleId` = 274 WHERE  `entry` = 31408;
UPDATE `creature_template` SET `VehicleId` = 278 WHERE  `entry` = 31409;
UPDATE `creature_template` SET `VehicleId` = 282 WHERE  `entry` = 31784;
UPDATE `creature_template` SET `VehicleId` = 282 WHERE  `entry` = 31785;
UPDATE `creature_template` SET `VehicleId` = 736 WHERE  `entry` = 31788;
UPDATE `creature_template` SET `VehicleId` = 512 WHERE  `entry` = 31830;
UPDATE `creature_template` SET `VehicleId` = 287 WHERE  `entry` = 31838;
UPDATE `creature_template` SET `VehicleId` = 291 WHERE  `entry` = 32227;
UPDATE `creature_template` SET `VehicleId` = 300 WHERE  `entry` = 32326;
UPDATE `creature_template` SET `VehicleId` = 301 WHERE  `entry` = 32344;
UPDATE `creature_template` SET `VehicleId` = 302 WHERE  `entry` = 32348;
UPDATE `creature_template` SET `VehicleId` = 273 WHERE  `entry` = 32512;
UPDATE `creature_template` SET `VehicleId` = 369 WHERE  `entry` = 32531;
UPDATE `creature_template` SET `VehicleId` = 40  WHERE  `entry` = 30775;
UPDATE `creature_template` SET `VehicleId` = 201 WHERE  `entry` = 30935;
UPDATE `creature_template` SET `VehicleId` = 209 WHERE  `entry` = 30936;
UPDATE `creature_template` SET `VehicleId` = 191 WHERE  `entry` = 31368;
UPDATE `creature_template` SET `VehicleId` = 108 WHERE  `entry` = 31669;
UPDATE `creature_template` SET `VehicleId` = 380 WHERE  `entry` = 32933;
UPDATE `creature_template` SET `VehicleId` = 342 WHERE  `entry` = 33190;
UPDATE `creature_template` SET `VehicleId` = 0 WHERE  `entry` = 33297;
UPDATE `creature_template` SET `VehicleId` = 0 WHERE  `entry` = 33298;
UPDATE `creature_template` SET `VehicleId` = 0 WHERE  `entry` = 33300;
UPDATE `creature_template` SET `VehicleId` = 0 WHERE  `entry` = 33301;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33316;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33317;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33318;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33320;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33322;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33323;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33324;
UPDATE `creature_template` SET `VehicleId` = 0 WHERE  `entry` = 33408;
UPDATE `creature_template` SET `VehicleId` = 0 WHERE  `entry` = 33409;
UPDATE `creature_template` SET `VehicleId` = 0 WHERE  `entry` = 33414;
UPDATE `creature_template` SET `VehicleId` = 0 WHERE  `entry` = 33416;
UPDATE `creature_template` SET `VehicleId` = 0 WHERE  `entry` = 33418;
UPDATE `creature_template` SET `VehicleId` = 368 WHERE  `entry` = 33519;
UPDATE `creature_template` SET `VehicleId` = 369 WHERE  `entry` = 33531;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33790;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33791;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33792;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33793;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33794;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33795;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33796;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33798;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33799;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33800;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33842;
UPDATE `creature_template` SET `VehicleId` = 349 WHERE  `entry` = 33843;
UPDATE `creature_template` SET `VehicleId` = 353 WHERE  `entry` = 33885;
UPDATE `creature_template` SET `VehicleId` = 328 WHERE  `entry` = 33909;
UPDATE `creature_template` SET `VehicleId` = 380 WHERE  `entry` = 33910;
UPDATE `creature_template` SET `VehicleId` = 380 WHERE  `entry` = 33911;
UPDATE `creature_template` SET `VehicleId` = 381 WHERE  `entry` = 33955;
UPDATE `creature_template` SET `VehicleId` = 385 WHERE  `entry` = 33984;
UPDATE `creature_template` SET `VehicleId` = 387 WHERE  `entry` = 34003;
UPDATE `creature_template` SET `VehicleId` = 335 WHERE  `entry` = 34045;
UPDATE `creature_template` SET `VehicleId` = 370 WHERE  `entry` = 34106;
UPDATE `creature_template` SET `VehicleId` = 371 WHERE  `entry` = 34108;
UPDATE `creature_template` SET `VehicleId` = 373 WHERE  `entry` = 34109;
UPDATE `creature_template` SET `VehicleId` = 0 WHERE  `entry` = 34125;
UPDATE `creature_template` SET `VehicleId` = 397 WHERE  `entry` = 34162;
UPDATE `creature_template` SET `VehicleId` = 399 WHERE  `entry` = 34214;
UPDATE `creature_template` SET `VehicleId` = 0 WHERE  `entry` = 36557;
UPDATE `creature_template` SET `VehicleId` = 485 WHERE  `entry` = 35641;
UPDATE `creature_template` SET `VehicleId` = 480 WHERE  `entry` = 35635;
UPDATE `creature_template` SET `VehicleId` = 106 WHERE  `entry` = 34802;
UPDATE `creature_template` SET `VehicleId` = 0 WHERE  `entry` = 36558;
UPDATE `creature_template` SET `VehicleId` = 477 WHERE  `entry` = 36087;
UPDATE `creature_template` SET `VehicleId` = 477 WHERE  `entry` = 36089;
UPDATE `creature_template` SET `VehicleId` = 442 WHERE  `entry` = 35438;
UPDATE `creature_template` SET `VehicleId` = 442 WHERE  `entry` = 35439;
UPDATE `creature_template` SET `VehicleId` = 442 WHERE  `entry` = 35440;
UPDATE `creature_template` SET `VehicleId` = 446 WHERE  `entry` = 35270;
UPDATE `creature_template` SET `VehicleId` = 446 WHERE  `entry` = 35271;
UPDATE `creature_template` SET `VehicleId` = 446 WHERE  `entry` = 35272;
UPDATE `creature_template` SET `VehicleId` = 555 WHERE  `entry` = 36839;
UPDATE `creature_template` SET `VehicleId` = 599 WHERE  `entry` = 37187;
UPDATE `creature_template` SET `VehicleId` = 648 WHERE  `entry` = 38712;
UPDATE `creature_template` SET `VehicleId` = 562 WHERE  `entry` = 37636;
UPDATE `creature_template` SET `VehicleId` = 560 WHERE  `entry` = 37626;
UPDATE `creature_template` SET `VehicleId` = 522 WHERE  `entry` = 37627;
UPDATE `creature_template` SET `VehicleId` = 648 WHERE  `entry` = 38974;
UPDATE `creature_template` SET `VehicleId` = 648 WHERE  `entry` = 38973;
UPDATE `creature_template` SET `VehicleId` = 648 WHERE  `entry` = 38975;
UPDATE `creature_template` SET `VehicleId` = 106 WHERE  `entry` = 35419;
UPDATE `creature_template` SET `VehicleId` = 106 WHERE  `entry` = 35421;
UPDATE `creature_template` SET `VehicleId` = 106 WHERE  `entry` = 35415;
UPDATE `creature_template` SET `VehicleId` = 436 WHERE  `entry` = 36358;
UPDATE `creature_template` SET `VehicleId` = 36  WHERE  `entry` = 35413;
UPDATE `creature_template` SET `VehicleId` = 452 WHERE  `entry` = 35410;
UPDATE `creature_template` SET `VehicleId` = 591 WHERE  `entry` = 38285;
UPDATE `creature_template` SET `VehicleId` = 718 WHERE  `entry` = 40081;
UPDATE `creature_template` SET `VehicleId` = 718 WHERE  `entry` = 40470;
UPDATE `creature_template` SET `VehicleId` = 718 WHERE  `entry` = 40471;
UPDATE `creature_template` SET `VehicleId` = 718 WHERE  `entry` = 40472;
UPDATE `creature_template` SET `VehicleId` = 79  WHERE  `entry` = 35427;
UPDATE `creature_template` SET `VehicleId` = 79  WHERE  `entry` = 35429;
UPDATE `creature_template` SET `VehicleId` = 591 WHERE  `entry` = 38788;
UPDATE `creature_template` SET `VehicleId` = 591 WHERE  `entry` = 38789;
UPDATE `creature_template` SET `VehicleId` = 591 WHERE  `entry` = 38790;
UPDATE `creature_template` SET `VehicleId` = 700 WHERE  `entry` = 39682;
UPDATE `creature_template` SET `VehicleId` = 745 WHERE  `entry` = 39713;
UPDATE `creature_template` SET `VehicleId` = 745 WHERE  `entry` = 39714;
UPDATE `creature_template` SET `VehicleId` = 753 WHERE  `entry` = 39759;
UPDATE `creature_template` SET `VehicleId` = 763 WHERE  `entry` = 39819;
UPDATE `creature_template` SET `VehicleId` = 711 WHERE  `entry` = 39860;
UPDATE `creature_template` SET `VehicleId` = 747 WHERE  `entry` = 40479;
-- YTDB updates 571-578
UPDATE `creature_template` SET `VehicleId` = 265 WHERE  `entry` = 31225;
UPDATE `creature_template` SET `VehicleId` = 224 WHERE  `entry` = 31748;
UPDATE `creature_template` SET `VehicleId` = 223 WHERE  `entry` = 31749;
UPDATE `creature_template` SET `VehicleId` = 220 WHERE  `entry` = 31752;

-- Ymirjar Skycaller true fix (delete hack from YTDB)
DELETE FROM `creature_template_addon` WHERE `entry` = 31260;
REPLACE INTO vehicle_accessory (entry,accessory_entry,description) VALUES (36891,31260,'Ymirjar Skycaller');

-- From zergtmn
/*
    Havenshire Stallion
    Havenshire Mare
    Havenshire Colt
*/
UPDATE creature_template SET
    spell1 = 52264,
    spell2 = 52268,
    spell3 = 0,
    spell4 = 0,
    spell5 = 0,
    spell6 = 0,
    VehicleId = 123
WHERE entry IN (28605, 28606, 28607);

DELETE FROM npc_spellclick_spells WHERE npc_entry IN (28605, 28606, 28607);
INSERT INTO npc_spellclick_spells VALUES
(28605, 52263, 12680, 1, 12680, 1),
(28606, 52263, 12680, 1, 12680, 1),
(28607, 52263, 12680, 1, 12680, 1);
INSERT IGNORE INTO spell_script_target VALUES (52264, 1, 28653);

-- From jahangames
-- Massacre at Light's point quest
UPDATE creature_template SET
spell1 = 52435,
spell2 = 52576,
spell3 = 0,
spell4 = 0,
spell5 = 52588,
spell6 = 0,
VehicleId = 139
WHERE entry IN (28833,28887);

INSERT INTO npc_spellclick_spells VALUES ('28833', '52447', '12701', '1', '12701', '1');
INSERT INTO npc_spellclick_spells VALUES ('28887', '52447', '12701', '1', '12701', '1');
INSERT IGNORE INTO spell_script_target VALUES (52576, 1, 28834);
INSERT IGNORE INTO spell_script_target VALUES (52576, 1, 28886);
INSERT IGNORE INTO spell_script_target VALUES (52576, 1, 28850);

-- From Lanc
-- quest 12953
UPDATE `creature_template` SET
    spell1 = 55812,
    spell2 = 0,
    spell3 = 0,
    spell4 = 0,
    spell5 = 0,
    spell6 = 0,
    VehicleId = 213
WHERE entry IN (30066);

DELETE FROM `npc_spellclick_spells` WHERE `npc_entry` IN (30066);
INSERT INTO `npc_spellclick_spells` VALUES
(30066, 44002, 12953, 1, 12953, 1);
INSERT IGNORE INTO `spell_script_target` VALUES (55812, 1, 30096);

-- From lanc
/* 7th Legion Chain Gun */
UPDATE creature_template SET
    IconName = 'Gunner',
    spell1 = 49190,
    spell2 = 49550,
    spell3 = 0,
    spell4 = 0,
    spell5 = 0,
    spell6 = 0,
    VehicleId = 68
WHERE entry IN (27714);

DELETE FROM npc_spellclick_spells WHERE npc_entry IN (27714);
INSERT INTO npc_spellclick_spells VALUES
(27714, 67373, 0, 0, 0, 1);

/* Broken-down Shredder */
UPDATE creature_template SET
    IconName = 'vehichleCursor',
    spell1 = 48558,
    spell2 = 48604,
    spell3 = 48548,
    spell4 = 0,
    spell5 = 48610,
    spell6 = 0,
    VehicleId = 49
WHERE entry IN (27354);

DELETE FROM npc_spellclick_spells WHERE npc_entry IN (27354);
INSERT INTO npc_spellclick_spells VALUES
(27354, 67373, 0, 0, 0, 1);
INSERT IGNORE INTO spell_script_target VALUES (48610, 1, 27396);

/* Forsaken Blight Spreader */
UPDATE creature_template SET
    IconName = 'vehichleCursor',
    spell1 = 48211,
    spell2 = 0,
    spell3 = 0,
    spell4 = 0,
    spell5 = 0,
    spell6 = 0,
    VehicleId = 36
WHERE entry IN (26523);

DELETE FROM npc_spellclick_spells WHERE npc_entry IN (26523);
INSERT INTO npc_spellclick_spells VALUES
(26523, 47961, 0, 0, 0, 1);

/* Argent Tournament mount */
UPDATE creature_template SET
    spell1 = 62544,
    spell2 = 62575,
    spell3 = 63010,
    spell4 = 62552,
    spell5 = 64077,
    spell6 = 62863,
    VehicleId = 349
WHERE entry IN (33844, 33845);
UPDATE creature_template SET KillCredit1 = 33340 WHERE entry IN (33272);
UPDATE creature_template SET KillCredit1 = 33339 WHERE entry IN (33243);

DELETE FROM npc_spellclick_spells WHERE npc_entry IN (33842, 33843);
INSERT INTO npc_spellclick_spells VALUES
(33842, 63791, 13829, 1, 0, 3),
(33842, 63791, 13839, 1, 0, 3),
(33842, 63791, 13838, 1, 0, 3),
(33843, 63792, 13828, 1, 0, 3),
(33843, 63792, 13837, 1, 0, 3),
(33843, 63792, 13835, 1, 0, 3);

DELETE FROM creature WHERE id IN (33844,33845);
UPDATE creature_template SET speed_run = '1.5', unit_flags = 8 WHERE entry IN (33844,33845);

-- Quest vehicles Support: Going Bearback (12851)
UPDATE `creature_template` SET
    spell1 = 54897,
    spell2 = 54907,
    spell3 = 0,
    spell4 = 0,
    spell5 = 0,
    spell6 = 0,
    VehicleId = 308
WHERE entry IN (29598);

DELETE FROM `npc_spellclick_spells` WHERE `npc_entry` IN (29598);
INSERT INTO `npc_spellclick_spells` VALUES
(29598, 54908, 12851, 1, 12851, 1);

INSERT IGNORE INTO `spell_script_target` VALUES (54897, 1, 29358);

/* Scourge Gryphon */
UPDATE creature_template SET
    spell1 = 0,
    spell2 = 0,
    spell3 = 0,
    spell4 = 0,
    spell5 = 0,
    spell6 = 0,
    VehicleId = 143
WHERE entry IN (28864);

/* Frostbrood Vanquisher */
UPDATE creature_template SET
    spell1 = 53114,
    spell2 = 53110,
    spell3 = 0,
    spell4 = 0,
    spell5 = 0,
    spell6 = 0,
    VehicleId = 156
WHERE entry IN (28670);

UPDATE creature_template SET maxhealth = 133525, minhealth = 133525, maxmana = 51360, minmana = 51360, InhabitType = 3 WHERE entry = 28670;

REPLACE INTO `creature_template_addon` (`entry`, `mount`, `bytes1`, `b2_0_sheath`, `emote`, `moveflags`, `auras`) VALUES
(28670, 0, 50331648, 1, 0, 1024, '53112 0 53112 1 53112 2');

-- from rsa
-- into realm of shadows
UPDATE `creature_template` SET `IconName` = 'vehichleCursor',
`unit_flags` = 0,
`spell1` = 52362
WHERE `entry` =28782;

UPDATE `quest_template` SET
`SrcSpell` = 52359,
`SpecialFlags` = 2,
`ReqCreatureOrGOId1` = 28768,
`ReqCreatureOrGOCount1` = 1,
`ReqSpellCast1` = 0,
`RewItemId1` = 39208,
`RewItemCount1` = 1 WHERE `entry` = 12687;

DELETE FROM `creature_involvedrelation` WHERE `quest` in (12687);
INSERT INTO `creature_involvedrelation` (`id`, `quest`) VALUES (28788, 12687);
UPDATE `creature_template` SET `npcflag` = 2 WHERE `entry` = 28788;

DELETE FROM `spell_script_target` WHERE `entry` = 52349;

UPDATE `creature_ai_scripts` SET
`action1_type`   = '11',
`action1_param1` = '52361',
`action1_param2` = '6',
`action1_param3` = '16',
`action2_type`   = '11',
-- `action2_param1` = '52357',
`action2_param1` = '52275',
`action2_param2` = '6',
`action2_param3` = '16',
`action3_type`   = '0'
WHERE `id` = 2876806;

DELETE FROM `creature` WHERE `id` = 28782;
DELETE FROM `creature_template_addon` WHERE `entry` = 28782;

DELETE FROM `npc_spellclick_spells` WHERE `npc_entry` IN (28782);
INSERT INTO `npc_spellclick_spells` VALUES
(28782, 46598, 0, 0, 0, 1);

-- from lanc
-- Infected Kodo fix quest (11690)
UPDATE `creature_template` SET
spell1 = 45877,
spell2 = 0,
spell3 = 0,
spell4 = 0,
spell5 = 0,
spell6 = 0,
VehicleId = 29
WHERE `entry` IN (25596);

INSERT IGNORE INTO `spell_script_target` VALUES (45877, 1, 25596);

-- Horde Siege Tank
UPDATE `creature_template` SET
spell1 = 50672,
spell2 = 45750,
spell3 = 50677,
spell4 = 47849,
spell5 = 47962,
spell6 = 0,
VehicleId = 26
WHERE `entry` IN (25334);

DELETE FROM `npc_spellclick_spells` WHERE `npc_entry` IN (25334, 27107);
INSERT INTO `npc_spellclick_spells` VALUES
(25334, 47917, 11652, 1, 11652, 1);

REPLACE INTO `spell_script_target` VALUES (47962, 1, 27107);

REPLACE INTO `spell_area` (`spell`, `area`, `quest_start`, `quest_start_active`, `quest_end`, `aura_spell`, `racemask`, `gender`, `autocast`)
VALUES ('47917','4027','11652','1','11652','0','0','2','0'), ('47917','4130','11652','1','11652','0','0','2','0');

-- from lanc
-- Xink's Shredder (quest 12050)
UPDATE `creature_template` SET
spell1 = 47939,
spell2 = 47921,
spell3 = 47966,
spell4 = 47938,
spell5 = 0,
spell6 = 0,
VehicleId = 300
WHERE `entry` IN (27061);

DELETE FROM `npc_spellclick_spells` WHERE npc_entry IN (27061);
INSERT INTO `npc_spellclick_spells` VALUES (27061, 47920, 0, 0, 0, 1);
REPLACE INTO `spell_script_target` VALUES (47939, 2, 188539);

-- Argent Cannon (quest 13086)
UPDATE `creature_template` SET
    spell1 = 57485,
    spell2 = 57412,
    spell3 = 0,
    spell4 = 0,
    spell5 = 0,
    spell6 = 0,
    VehicleId = 244
WHERE `entry` IN (30236);

DELETE FROM `npc_spellclick_spells` WHERE npc_entry IN (30236);
INSERT INTO `npc_spellclick_spells` VALUES
(30236, 57573, 13086, 1, 13086, 1);

-- Wyrmrest Vanquisher (quest 12498)
UPDATE `creature_template` SET
    spell1 = 55987,
    spell2 = 50348,
    spell3 = 50430,
    spell4 = 0,
    spell5 = 0,
    spell6 = 0,
    VehicleId = 99
WHERE `entry` IN (27996);

DELETE FROM `npc_spellclick_spells` WHERE npc_entry IN (27996);
INSERT INTO `npc_spellclick_spells` VALUES
(27996, 50343, 12498, 1, 12498, 1);

REPLACE INTO `creature_template_addon` (entry, auras) VALUES (27996, '53112 0 53112 1 53112 2');

-- from rsa
-- Quest Reclamation (12546)
UPDATE `creature_template` SET `spell1` = 50978,`spell2` = 50980,`spell3` = 50983,`spell4` = 50985,
`VehicleId` = 111
WHERE  `entry` = 28222;

-- from YTDB/TC 578
DELETE FROM `npc_spellclick_spells` WHERE `npc_entry` IN (27850,27881,28094,28312,28319,28670,32627,32629);
INSERT INTO `npc_spellclick_spells` (`npc_entry`, `spell_id`, `quest_start`, `quest_start_active`, `quest_end`, `cast_flags`) VALUES
(27850, 60968, 0, 0, 0, 1),
(27881, 60968, 0, 0, 0, 1),
(28094, 60968, 0, 0, 0, 1),
(28312, 60968, 0, 0, 0, 1),
(28319, 60968, 0, 0, 0, 1),
(28670, 52196, 0, 0, 0, 1),
(32627, 60968, 0, 0, 0, 1),
(32629, 60968, 0, 0, 0, 1);

-- Quest 12996
UPDATE `creature_template` SET `spell1` = 54459,`spell2` = 54458,`spell3` = 54460,`VehicleId` = 208 WHERE  `creature_template`.`entry` = 29918;

-- from lanc
UPDATE `creature_template` SET
    spell1 = 50232,
    spell2 = 50248,
    spell3 = 50240,
    spell4 = 50253
 WHERE `entry` IN (27756);

UPDATE `creature_template` SET
    spell1 = 49840,
    spell2 = 49838,
    spell3 = 49592
 WHERE `entry` IN (27755);

UPDATE `creature_template` SET
    spell1 = 50328,
    spell2 = 50341,
    spell3 = 50344
 WHERE `entry` IN (27692);

-- from rsa
DELETE FROM `spell_script_target` WHERE `entry` IN (49460, 49346, 49464);
INSERT INTO `spell_script_target` VALUES (49460, 1, 27755);
INSERT INTO `spell_script_target` VALUES (49346, 1, 27692);
INSERT INTO `spell_script_target` VALUES (49464, 1, 27756);

REPLACE INTO `creature_template_addon` (entry,auras) VALUES (27755,'57403 0 57403 1 57403 2');
REPLACE INTO `creature_template_addon` (entry,auras) VALUES (27756,'57403 0 57403 1 57403 2');
REPLACE INTO `creature_template_addon` (entry,auras) VALUES (27692,'57403 0 57403 1 57403 2');
UPDATE creature_template SET InhabitType = 3,VehicleId = 70,PowerType=3 WHERE entry IN (27755,27756,27692);

-- Ulduar Salvaged vehicles (unifieddb fix)
DELETE FROM creature_addon WHERE guid IN (SELECT guid FROM creature WHERE id IN (33060,33062,33109));
UPDATE creature_model_info SET bounding_radius = 1.5 WHERE modelid IN (25870,25871);