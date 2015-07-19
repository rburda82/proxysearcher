<?php

include_once "PhpLibraries/DAL.php";
include_once "PhpLibraries/Log.php";
include_once "PhpLibraries/HttpProxyType.php";
include_once "PhpLibraries/ClientIP.php";

class ProxyTracker
{
    public static function Track()
    {
        $ip = $_REQUEST["ip"];
        $port = $_REQUEST["port"];
        $type =  $_REQUEST["type"];
        $myIP = $_REQUEST["myip"];

        if (self::CanTrackProxy($ip, $port, $type, $myIP))
        {
            $type = strtolower($type);
            $subType = $type == "http" ? HttpProxyType::Get() : null;

            DAL::InsertProxy($ip, $port, $type, $subType, $myIP);
            DAL::CleanupOldProxiesIfNeeded();
        }
    }

     private static function CanTrackProxy($ip, $port, $type)
     {
         if ($ip == null || $port == null || $type == null)
         {
             return false;
         }

         if (!ip2long($ip))
         {
             Log::Add("Ip is not valid: ".$ip);
             return false;
         }

         if (!is_numeric($port))
         {
             Log::Add("Port is not numeric: ".$port);
             return false;
         }

         $portInt = intval($port);

         if ($portInt < 0 || $portInt > 65535)
         {
             Log::Add("Wrong port value has been passed: ".$portInt);
             return false;
         }

         if (!DAL::IsProxyTypeExists(strtolower($type)))
         {
             Log::Add("Wrong proxy type has been passed: ".$type);
             return false;
         }

         $clientIP = ClientIP::Get();
         $containsIP = ClientIP::Contains($ip);

         if ($clientIP != $ip && !$containsIP)
         {
             Log::Add("Client ip ".$clientIP." is not equal to ip from parameters ".$ip.". From that reason that proxy was not tracked. Parameters: ".$ip.":".$port."(".$type.", ".HttpProxyType::Get().")");
             //return false;
         }

         return true;
    }
}

?>