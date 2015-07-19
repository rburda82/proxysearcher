<?php

include_once "PhpLibraries/HttpProxyType.php";
include_once "PhpLibraries/ProxyTracker.php";

echo HttpProxyType::Get();
ProxyTracker::Track();

?>