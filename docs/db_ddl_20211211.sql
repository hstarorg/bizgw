CREATE TABLE `cluster` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `cluster_code` varchar(254) NOT NULL COMMENT '集群ID',
  `cluster_name` varchar(254) NOT NULL COMMENT '集群名称',
  `load_balancing_policy` varchar(254) NOT NULL COMMENT '负载均衡策略',
  `enabled_helth_check` tinyint(3) unsigned NOT NULL COMMENT '是否启用负载均衡',
  `helth_check_interval` int(11) NOT NULL COMMENT '健康检查间隔秒数',
  `helth_check_timeout` int(11) NOT NULL COMMENT '健康检查超时时间',
  `helth_check_policy` varchar(254) NOT NULL COMMENT '健康检查策略',
  `helth_check_path` varchar(254) NOT NULL COMMENT '健康检查path',
  `is_deleted` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT '是否删除',
  `remark` varchar(2000) NOT NULL COMMENT '备注信息',
  `create_date` bigint(20) NOT NULL COMMENT '创建时间',
  `creator_name` varchar(254) NOT NULL COMMENT '创建人',
  `modify_date` bigint(20) NOT NULL COMMENT '修改时间',
  `modifier_name` varchar(254) NOT NULL COMMENT '修改人',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4
GO

CREATE TABLE `destination` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `cluster_code` varchar(254) NOT NULL COMMENT '关联的集群',
  `address` varchar(254) NOT NULL COMMENT 'Host',
  `helth_check_path` varchar(254) NOT NULL COMMENT '健康检查地址',
  `is_deleted` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT '是否删除',
  `name` varchar(254) NOT NULL COMMENT '名称',
  `create_date` bigint(20) NOT NULL COMMENT '创建时间',
  `creator_name` varchar(254) NOT NULL COMMENT '创建人',
  `modify_date` bigint(20) NOT NULL COMMENT '修改时间',
  `modifier_name` varchar(254) NOT NULL COMMENT '修改人',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4
GO

CREATE TABLE `route` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `route_name` varchar(254) NOT NULL COMMENT '路由名称',
  `cluster_code` varchar(254) NOT NULL COMMENT '关联的目标集群',
  `match_path` varchar(500) NOT NULL COMMENT '要转发的路径',
  `match_methods` varchar(254) NOT NULL COMMENT '允许的 method',
  `is_deleted` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT '是否删除',
  `remark` varchar(2000) NOT NULL COMMENT '备注',
  `create_date` bigint(20) NOT NULL COMMENT '创建时间',
  `creator_name` varchar(254) NOT NULL COMMENT '创建人',
  `modify_date` bigint(20) NOT NULL COMMENT '修改时间',
  `modifier_name` varchar(254) NOT NULL COMMENT '修改人',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4