<?php
 include_once "PhpLibraries/DAL.php";

 class Cache
 {
    public static function GetProxiesDataTableOrNull($type, $filtered, $limit)
    {
        return DAL::GetProxiesDataTableOrNull($type, $filtered, $limit);
    }
 }

?>