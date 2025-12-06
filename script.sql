-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               10.4.32-MariaDB - mariadb.org binary distribution
-- Server OS:                    Win64
-- HeidiSQL Version:             12.3.0.6589
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Dumping database structure for crimson_knight
CREATE DATABASE IF NOT EXISTS `crimson_knight` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci */;
USE `crimson_knight`;

-- Dumping structure for table crimson_knight.player
CREATE TABLE IF NOT EXISTS `player` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `username` varchar(100) DEFAULT NULL,
  `password` varchar(100) DEFAULT NULL,
  `name` varchar(100) DEFAULT NULL,
  `mapid` smallint(6) NOT NULL DEFAULT 0,
  `x` smallint(6) NOT NULL DEFAULT 744,
  `y` smallint(6) NOT NULL DEFAULT 486,
  `stats` varchar(2000) NOT NULL DEFAULT '{"HP":{"Id":0,"Value":100},"MP":{"Id":1,"Value":50},"ATK":{"Id":2,"Value":20},"DEF":{"Id":3,"Value":15}}',
  `classtype` tinyint(4) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Dumping data for table crimson_knight.player: ~3 rows (approximately)
INSERT INTO `player` (`id`, `username`, `password`, `name`, `mapid`, `x`, `y`, `stats`, `classtype`) VALUES
	(1, 'player1', '1', 'Xin chào 1', 0, 744, 486, '{"HP":{"Id":0,"Value":100},"MP":{"Id":1,"Value":50},"ATK":{"Id":2,"Value":20},"DEF":{"Id":3,"Value":15}}', 0),
	(2, 'player2', '1', 'Xin chào 2', 0, 744, 486, '{"HP":{"Id":0,"Value":100},"MP":{"Id":1,"Value":50},"ATK":{"Id":2,"Value":20},"DEF":{"Id":3,"Value":15}}', 0),
	(3, 'player3', '1', 'Xin chào 3', 0, 744, 486, '{"HP":{"Id":0,"Value":100},"MP":{"Id":1,"Value":50},"ATK":{"Id":2,"Value":20},"DEF":{"Id":3,"Value":15}}', 0);

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
