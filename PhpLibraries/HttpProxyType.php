<?php

include_once "PhpLibraries/Log.php";

class HttpProxyType
{
    public static function Get()
    {
        $headers = getallheaders();

        if($headers["X-Forwarded-For"])
        {
             //Log::Add("Logging X-Forwarded-For header: ".$headers["X-Forwarded-For"]);
           
             $array = split(", ", $headers["X-Forwarded-For"]);
             $myIP = $_REQUEST["myip"];

             if ($myIP)
             {
                if (in_array($myIP, $array))
                {
                    return "Transparent";
                }

                return "Anonymous";
             }
             
             return "Transparent";
        }

        return "HighAnonymous";
    }
}

?>