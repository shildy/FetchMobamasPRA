--
-- テーブルの構造 `members`
--

CREATE TABLE IF NOT EXISTS `members` (
  `id` int(11) NOT NULL,
  `username` varchar(50) NOT NULL,
  `password` varchar(255) NOT NULL DEFAULT 'password',
  `role` varchar(20) NOT NULL DEFAULT 'guest',
  `producer_name` varchar(50) NOT NULL,
  `producer_rank` varchar(3) NOT NULL DEFAULT 'X',
  `fan05` int(11) NOT NULL DEFAULT '0',
  `fan04` int(11) NOT NULL DEFAULT '0',
  `fan03` int(11) NOT NULL DEFAULT '0',
  `fan02` int(11) NOT NULL DEFAULT '0',
  `fan01` int(11) NOT NULL DEFAULT '0',
  `latest` date DEFAULT NULL,
  `fan00` int(11) NOT NULL DEFAULT '0',
  `fan00_diff` int(11) NOT NULL DEFAULT '0',
  `producer_type` int(11) NOT NULL DEFAULT '0',
  `created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `modified` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB AUTO_INCREMENT=45 DEFAULT CHARSET=utf8mb4;

--
-- Indexes for table `members`
--
ALTER TABLE `members`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT for table `members`
--
ALTER TABLE `members`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=1;
