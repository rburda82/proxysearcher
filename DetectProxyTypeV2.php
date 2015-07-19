<?php

include_once "PhpLibraries/HttpProxyType.php";
include_once "PhpLibraries/ProxyTracker.php";
include_once "PhpLibraries/ClientIP.php";

echo HttpProxyType::Get();
echo ",";
echo ClientIP::Get();
ProxyTracker::Track();

?>