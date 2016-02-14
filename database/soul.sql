/*
Navicat MySQL Data Transfer

Source Server         : local_my
Source Server Version : 50067
Source Host           : localhost:3306
Source Database       : soul

Target Server Type    : MYSQL
Target Server Version : 50067
File Encoding         : 936

Date: 2016-02-14 10:29:58
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `account`
-- ----------------------------
DROP TABLE IF EXISTS `account`;
CREATE TABLE `account` (
  `id` int(11) NOT NULL auto_increment,
  `account` varchar(32) default NULL,
  `password` varchar(32) default NULL,
  `vip` tinyint(4) default NULL,
  `serverindex` int(11) default '-1',
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of account
-- ----------------------------
INSERT INTO `account` VALUES ('1', 'fucknd', '123456', '1', '-1');
INSERT INTO `account` VALUES ('2', '123456', '113', '1', '-1');
INSERT INTO `account` VALUES ('3', 'ceshi', '123', '1', '-1');

-- ----------------------------
-- Table structure for `cq_eudemon`
-- ----------------------------
DROP TABLE IF EXISTS `cq_eudemon`;
CREATE TABLE `cq_eudemon` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `itemid` int(11) default '0',
  `ownerid` int(11) default '0',
  `name` varchar(255) default '',
  `phyatk_grow_rate` int(11) default '0',
  `phyatk_grow_rate_max` int(11) default '0',
  `magicatk_grow_rate` int(11) default '0',
  `magicatk_grow_rate_max` int(11) default '0',
  `life_grow_rate` int(11) default '0',
  `defense_grow_rate` int(11) default '0',
  `magicdef_grow_rate` int(11) default '0',
  `life` int(11) default '0',
  `atk_min` int(11) default '0',
  `atk_max` int(11) default '0',
  `magicatk_min` int(11) default '0',
  `magicatk_max` int(11) default '0',
  `defense` int(11) default '0',
  `magicdef` int(11) default '0',
  `luck` int(11) default '0',
  `intimacy` int(11) default '0',
  `level` smallint(4) default '1',
  `card` int(11) default '0',
  `exp` int(11) default '0',
  `quality` int(11) default '0',
  `wuxing` int(11) default '0',
  `recall_count` int(11) default '0',
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of cq_eudemon
-- ----------------------------
INSERT INTO `cq_eudemon` VALUES ('1', '7', '1', '法师攻防型', '2', '5', '4', '23', '34', '7', '9', '70', '28', '19', '60', '56', '53', '88', '4', '150', '1', '0', '0', '0', '2', '0');
INSERT INTO `cq_eudemon` VALUES ('2', '8', '1', '法师攻防型', '2', '4', '18', '27', '5', '22', '2', '17', '23', '11', '79', '57', '28', '87', '86', '150', '1', '0', '0', '0', '1', '0');
INSERT INTO `cq_eudemon` VALUES ('3', '26', '2', '战士攻防型', '7', '38', '0', '0', '31', '16', '1', '78', '22', '24', '0', '39', '77', '33', '23', '150', '51', '366885', '689', '1000', '2', '0');
INSERT INTO `cq_eudemon` VALUES ('4', '17', '2', '战士攻防型', '5', '30', '0', '0', '34', '3', '10', '50', '73', '54', '0', '20', '82', '47', '87', '150', '79', '347245051', '120450', '1000', '3', '0');
INSERT INTO `cq_eudemon` VALUES ('5', '32', '2', '奇异兽O型', '6', '20', '3', '14', '9', '16', '1', '270', '52', '10', '22', '84', '52', '38', '9', '150', '80', '0', '84', '600', '3', '0');
INSERT INTO `cq_eudemon` VALUES ('6', '34', '2', 'XO型幻兽', '2', '22', '4', '13', '21', '12', '8', '232', '86', '14', '64', '43', '89', '27', '64', '150', '80', '0', '0', '1200', '1', '0');
INSERT INTO `cq_eudemon` VALUES ('7', '55', '2', 'XO型幻兽', '4', '23', '7', '15', '6', '16', '5', '242', '93', '80', '10', '70', '14', '62', '44', '150', '80', '0', '0', '1900', '1', '0');
INSERT INTO `cq_eudemon` VALUES ('8', '55', '2', 'XO型幻兽', '4', '23', '7', '15', '6', '16', '5', '242', '93', '80', '10', '70', '14', '62', '44', '150', '80', '0', '0', '1900', '1', '0');
INSERT INTO `cq_eudemon` VALUES ('9', '64', '2', '神影兽XO型', '13', '20', '4', '15', '49', '6', '1', '104', '26', '59', '79', '29', '61', '40', '28', '150', '55', '0', '339', '900', '2', '0');
INSERT INTO `cq_eudemon` VALUES ('10', '67', '2', '神影兽XO型', '11', '24', '10', '15', '39', '11', '5', '135', '10', '69', '74', '83', '42', '29', '59', '150', '80', '0', '490137', '2500', '3', '0');
INSERT INTO `cq_eudemon` VALUES ('11', '69', '2', 'XO型幻兽', '16', '20', '6', '16', '31', '6', '5', '19', '86', '75', '20', '95', '48', '60', '29', '150', '80', '0', '1433', '2500', '4', '0');
INSERT INTO `cq_eudemon` VALUES ('12', '69', '2', 'XO型幻兽', '16', '20', '6', '16', '31', '6', '5', '19', '86', '75', '20', '95', '48', '60', '29', '150', '80', '0', '1476', '2500', '4', '0');
INSERT INTO `cq_eudemon` VALUES ('13', '69', '2', 'XO型幻兽', '16', '20', '6', '16', '31', '6', '5', '19', '86', '75', '20', '95', '48', '60', '29', '150', '80', '0', '1476', '2500', '4', '0');
INSERT INTO `cq_eudemon` VALUES ('14', '69', '2', 'XO型幻兽', '16', '20', '6', '16', '31', '6', '5', '19', '86', '75', '20', '95', '48', '60', '29', '150', '80', '0', '1476', '2500', '4', '0');
INSERT INTO `cq_eudemon` VALUES ('15', '77', '3', '法师攻防型', '1', '4', '15', '25', '27', '16', '9', '207', '18', '20', '74', '40', '10', '46', '9', '150', '1', '0', '0', '0', '1', '0');
INSERT INTO `cq_eudemon` VALUES ('16', '78', '3', '法师攻防型', '2', '3', '10', '26', '52', '21', '3', '106', '29', '11', '42', '15', '72', '58', '68', '150', '1', '0', '0', '0', '2', '0');
INSERT INTO `cq_eudemon` VALUES ('17', '80', '4', '法师攻防型', '1', '4', '4', '25', '41', '12', '11', '60', '17', '26', '45', '45', '19', '30', '33', '150', '1', '0', '0', '0', '4', '0');
INSERT INTO `cq_eudemon` VALUES ('18', '81', '4', '法师攻防型', '2', '5', '13', '20', '36', '18', '8', '126', '14', '25', '81', '21', '88', '49', '86', '150', '1', '0', '0', '0', '2', '0');
INSERT INTO `cq_eudemon` VALUES ('19', '83', '5', '法师攻防型', '2', '5', '14', '24', '29', '21', '6', '172', '14', '27', '36', '39', '52', '33', '15', '150', '1', '0', '0', '0', '3', '0');
INSERT INTO `cq_eudemon` VALUES ('20', '84', '5', '法师攻防型', '1', '4', '20', '22', '10', '4', '8', '231', '28', '24', '69', '47', '45', '41', '76', '150', '1', '0', '0', '0', '3', '0');
INSERT INTO `cq_eudemon` VALUES ('21', '133', '6', '龙魂近卫.赛弗', '22', '34', '0', '0', '49', '7', '9', '211', '37', '50', '0', '0', '41', '52', '97', '150', '1', '0', '0', '0', '1', '0');
INSERT INTO `cq_eudemon` VALUES ('22', '134', '6', '龙魂近卫.赛弗', '18', '35', '0', '0', '25', '18', '3', '40', '53', '56', '0', '0', '41', '41', '41', '150', '1', '0', '0', '0', '1', '0');
INSERT INTO `cq_eudemon` VALUES ('23', '138', '6', '奇异兽O型', '16', '19', '8', '16', '33', '8', '10', '152', '96', '24', '26', '60', '38', '46', '3', '150', '1', '0', '0', '0', '3', '0');
INSERT INTO `cq_eudemon` VALUES ('24', '138', '6', '奇异兽O型', '16', '19', '8', '16', '33', '8', '10', '152', '96', '24', '26', '60', '38', '46', '3', '150', '1', '0', '0', '0', '3', '0');
INSERT INTO `cq_eudemon` VALUES ('25', '146', '2', '吉祥噜噜', '16', '37', '0', '0', '63', '7', '12', '185', '57', '17', '0', '0', '75', '72', '52', '150', '51', '0', '0', '307', '3', '0');
INSERT INTO `cq_eudemon` VALUES ('26', '148', '2', '神影兽XO型', '1', '19', '8', '15', '61', '13', '14', '260', '75', '65', '33', '87', '24', '61', '56', '150', '51', '0', '0', '664', '4', '0');

-- ----------------------------
-- Table structure for `cq_eudemon_magic`
-- ----------------------------
DROP TABLE IF EXISTS `cq_eudemon_magic`;
CREATE TABLE `cq_eudemon_magic` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `ownerid` int(11) default NULL,
  `magicid` int(11) default NULL,
  `level` tinyint(4) default NULL,
  `exp` int(11) default NULL,
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=gb2312;

-- ----------------------------
-- Records of cq_eudemon_magic
-- ----------------------------

-- ----------------------------
-- Table structure for `cq_friend`
-- ----------------------------
DROP TABLE IF EXISTS `cq_friend`;
CREATE TABLE `cq_friend` (
  `id` int(11) NOT NULL auto_increment,
  `userid` int(11) default '0',
  `friendtype` tinyint(4) default '0',
  `friendid` int(11) default '0',
  `friendname` varchar(255) default '',
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of cq_friend
-- ----------------------------

-- ----------------------------
-- Table structure for `cq_item`
-- ----------------------------
DROP TABLE IF EXISTS `cq_item`;
CREATE TABLE `cq_item` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `playerid` int(11) NOT NULL,
  `itemid` int(11) NOT NULL,
  `postion` tinyint(4) default '0',
  `stronglv` tinyint(4) unsigned default '0',
  `gemcount` tinyint(6) default '0',
  `gem1` tinyint(11) unsigned default '0',
  `gem2` tinyint(11) unsigned default '0',
  `forgename` varchar(32) default '',
  `amount` int(11) default '0',
  `war_ghost_exp` int(11) default '0',
  `di_attack` tinyint(4) unsigned default '0',
  `shui_attack` tinyint(4) unsigned default '0',
  `huo_attack` tinyint(4) unsigned default '0',
  `feng_attack` tinyint(4) unsigned default '0',
  `property` int(11) default '0',
  `gem3` tinyint(4) unsigned default '0',
  `god_exp` int(11) default '0',
  `god_strong` int(11) default '0',
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=149 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of cq_item
-- ----------------------------
INSERT INTO `cq_item` VALUES ('1', '1', '115041', '1', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('2', '1', '125041', '2', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('3', '1', '135041', '3', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('4', '1', '440101', '4', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('5', '1', '145041', '7', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('6', '1', '165031', '8', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('7', '1', '1071220', '53', '0', '0', '0', '0', '法师攻防型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('8', '1', '1071220', '53', '0', '0', '0', '0', '法师攻防型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('9', '2', '111041', '50', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('10', '2', '121041', '2', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('11', '2', '131041', '3', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('13', '2', '141041', '7', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('14', '2', '161091', '8', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('15', '2', '420103', '50', '9', '0', '255', '255', '', '10000', '0', '9', '9', '9', '9', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('17', '2', '1071022', '53', '0', '0', '0', '0', '战士攻防型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('18', '2', '1038170', '50', '0', '0', '54', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('19', '2', '1038200', '50', '0', '0', '50', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('20', '2', '1038230', '50', '0', '0', '9', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('21', '2', '1038260', '50', '0', '0', '63', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('22', '2', '1038290', '50', '0', '0', '52', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('26', '2', '1071022', '53', '0', '0', '0', '0', '战士攻防型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('36', '2', '813001', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('37', '2', '813003', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('39', '2', '813003', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('40', '2', '813004', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('41', '2', '813005', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('46', '2', '743494', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('48', '2', '1021070', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('49', '2', '1021080', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('64', '2', '1071961', '53', '0', '0', '0', '0', '神影兽XO型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('67', '2', '1071962', '53', '0', '0', '0', '0', '神影兽XO型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('69', '2', '1071982', '53', '0', '0', '0', '0', 'XO型幻兽', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('70', '2', '415070', '49', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('71', '3', '115041', '50', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('72', '3', '125041', '50', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('73', '3', '135041', '50', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('74', '3', '440101', '50', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('75', '3', '145041', '50', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('76', '3', '165031', '50', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('77', '3', '1071220', '53', '0', '0', '0', '0', '法师攻防型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('78', '3', '1071220', '53', '0', '0', '0', '0', '法师攻防型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('79', '4', '729000', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('80', '4', '1071220', '53', '0', '0', '0', '0', '法师攻防型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('81', '4', '1071220', '53', '0', '0', '0', '0', '法师攻防型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('82', '5', '729000', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('83', '5', '1071220', '53', '0', '0', '0', '0', '法师攻防型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('84', '5', '1071220', '53', '0', '0', '0', '0', '法师攻防型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('85', '5', '440000', '4', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('86', '5', '135300', '3', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('87', '2', '813001', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('88', '2', '745960', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('89', '2', '779001', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('90', '2', '729032', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('91', '2', '729032', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('94', '2', '729032', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('95', '2', '813001', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('97', '2', '1021080', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('98', '2', '728077', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('99', '2', '728077', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('100', '2', '728075', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('101', '2', '748385', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('102', '2', '729032', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('103', '2', '729032', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('105', '2', '813001', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('108', '2', '111041', '1', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('120', '2', '410101', '4', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('132', '6', '729000', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('133', '6', '1072560', '53', '0', '0', '0', '0', '龙魂近卫.赛弗', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('134', '6', '1072560', '53', '0', '0', '0', '0', '龙魂近卫.赛弗', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('135', '6', '480000', '4', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('136', '6', '132300', '3', '0', '0', '0', '0', '', '10000', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('138', '6', '1071990', '53', '0', '0', '0', '0', '奇异兽O型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('144', '2', '724024', '50', '0', '0', '0', '0', '', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('146', '2', '1071421', '53', '0', '0', '0', '0', '吉祥噜噜', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `cq_item` VALUES ('148', '2', '1071961', '53', '0', '0', '0', '0', '神影兽XO型', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0');

-- ----------------------------
-- Table structure for `cq_legion`
-- ----------------------------
DROP TABLE IF EXISTS `cq_legion`;
CREATE TABLE `cq_legion` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `name` varchar(32) default NULL,
  `member_title` tinyint(4) default NULL,
  `leader_id` int(11) default NULL,
  `leader_name` varchar(32) default NULL,
  `money` bigint(20) default NULL,
  `notice` varchar(64) default NULL,
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of cq_legion
-- ----------------------------

-- ----------------------------
-- Table structure for `cq_legion_members`
-- ----------------------------
DROP TABLE IF EXISTS `cq_legion_members`;
CREATE TABLE `cq_legion_members` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `legion_id` int(11) default NULL,
  `members_name` varchar(32) default NULL,
  `money` bigint(20) default NULL,
  `rank` int(11) default NULL,
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of cq_legion_members
-- ----------------------------

-- ----------------------------
-- Table structure for `cq_magic`
-- ----------------------------
DROP TABLE IF EXISTS `cq_magic`;
CREATE TABLE `cq_magic` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `ownerid` int(11) default '0',
  `magicid` int(11) default '0',
  `level` tinyint(4) default NULL COMMENT '0',
  `exp` int(11) default NULL COMMENT '0',
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=43 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of cq_magic
-- ----------------------------
INSERT INTO `cq_magic` VALUES ('1', '1', '3000', '0', '0');
INSERT INTO `cq_magic` VALUES ('2', '1', '3001', '0', '0');
INSERT INTO `cq_magic` VALUES ('3', '1', '3002', '0', '0');
INSERT INTO `cq_magic` VALUES ('4', '1', '3003', '0', '0');
INSERT INTO `cq_magic` VALUES ('5', '1', '3004', '0', '0');
INSERT INTO `cq_magic` VALUES ('6', '1', '3005', '0', '0');
INSERT INTO `cq_magic` VALUES ('7', '1', '3006', '0', '0');
INSERT INTO `cq_magic` VALUES ('8', '1', '3009', '0', '0');
INSERT INTO `cq_magic` VALUES ('9', '1', '3011', '0', '0');
INSERT INTO `cq_magic` VALUES ('10', '1', '3010', '0', '0');
INSERT INTO `cq_magic` VALUES ('11', '1', '5300', '0', '0');
INSERT INTO `cq_magic` VALUES ('12', '1', '5301', '0', '0');
INSERT INTO `cq_magic` VALUES ('13', '1', '5302', '0', '0');
INSERT INTO `cq_magic` VALUES ('14', '1', '5309', '0', '0');
INSERT INTO `cq_magic` VALUES ('15', '1', '5310', '0', '0');
INSERT INTO `cq_magic` VALUES ('16', '2', '1007', '0', '0');
INSERT INTO `cq_magic` VALUES ('17', '2', '1009', '0', '0');
INSERT INTO `cq_magic` VALUES ('18', '2', '1007', '0', '3');
INSERT INTO `cq_magic` VALUES ('19', '2', '1009', '0', '0');
INSERT INTO `cq_magic` VALUES ('20', '2', '1007', '0', '31');
INSERT INTO `cq_magic` VALUES ('21', '2', '1009', '0', '6');
INSERT INTO `cq_magic` VALUES ('22', '3', '3000', '0', '0');
INSERT INTO `cq_magic` VALUES ('23', '3', '3001', '0', '0');
INSERT INTO `cq_magic` VALUES ('24', '3', '3002', '0', '0');
INSERT INTO `cq_magic` VALUES ('25', '3', '3003', '0', '0');
INSERT INTO `cq_magic` VALUES ('26', '3', '3004', '0', '0');
INSERT INTO `cq_magic` VALUES ('27', '3', '3005', '0', '0');
INSERT INTO `cq_magic` VALUES ('28', '3', '3006', '0', '0');
INSERT INTO `cq_magic` VALUES ('29', '3', '3009', '0', '0');
INSERT INTO `cq_magic` VALUES ('30', '3', '3011', '0', '0');
INSERT INTO `cq_magic` VALUES ('31', '3', '3010', '0', '0');
INSERT INTO `cq_magic` VALUES ('32', '3', '5300', '0', '0');
INSERT INTO `cq_magic` VALUES ('33', '3', '5301', '0', '0');
INSERT INTO `cq_magic` VALUES ('34', '3', '5302', '0', '0');
INSERT INTO `cq_magic` VALUES ('35', '3', '5309', '0', '0');
INSERT INTO `cq_magic` VALUES ('36', '3', '5310', '0', '0');
INSERT INTO `cq_magic` VALUES ('37', '2', '1010', '0', '0');
INSERT INTO `cq_magic` VALUES ('38', '2', '1010', '0', '0');
INSERT INTO `cq_magic` VALUES ('39', '2', '1010', '0', '0');
INSERT INTO `cq_magic` VALUES ('40', '2', '1010', '0', '0');
INSERT INTO `cq_magic` VALUES ('41', '2', '1010', '0', '0');
INSERT INTO `cq_magic` VALUES ('42', '2', '1010', '0', '0');

-- ----------------------------
-- Table structure for `cq_payrec`
-- ----------------------------
DROP TABLE IF EXISTS `cq_payrec`;
CREATE TABLE `cq_payrec` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `order` varchar(255) default NULL,
  `account` varchar(255) default NULL,
  `money` int(11) default NULL,
  `state` tinyint(4) default NULL,
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=gb2312;

-- ----------------------------
-- Records of cq_payrec
-- ----------------------------

-- ----------------------------
-- Table structure for `cq_user`
-- ----------------------------
DROP TABLE IF EXISTS `cq_user`;
CREATE TABLE `cq_user` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `accountid` int(10) unsigned NOT NULL,
  `name` varchar(32) default NULL,
  `lookface` int(11) default '0',
  `hair` int(11) default '0',
  `level` tinyint(4) unsigned default '1',
  `exp` int(11) default '0',
  `life` int(11) default '0',
  `mana` int(11) default '0',
  `profession` tinyint(4) default '0',
  `pk` int(11) default '0',
  `gold` int(10) unsigned default '0',
  `gamegold` int(10) unsigned default '0',
  `stronggold` int(11) default '0',
  `mapid` int(11) default '0',
  `record_x` int(11) default '0',
  `record_y` int(11) default '0',
  `hotkey` varchar(255) default '',
  `guanjue` bigint(20) unsigned default '0',
  `godlevel` tinyint(4) unsigned default '0',
  `maxeudemon` tinyint(4) default '2',
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of cq_user
-- ----------------------------
INSERT INTO `cq_user` VALUES ('1', '1', 'aaa[PM]', '150001', '0', '1', '0', '50', '60', '10', '0', '0', '0', '0', '1000', '312', '461', '', '0', '0', '2');
INSERT INTO `cq_user` VALUES ('2', '2', '我是战士[PM]', '170001', '119', '74', '2710', '3020', '0', '20', '0', '100000119', '971133', '0', '1000', '376', '258', '2|0|1|0|2|1007|0,2|8|9|0|2|1010|0,', '0', '0', '3');
INSERT INTO `cq_user` VALUES ('6', '3', '我是暗暗[PM]', '610001', '0', '1', '123', '100', '0', '70', '0', '0', '972', '0', '1000', '431', '498', '', '0', '0', '2');
