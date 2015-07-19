<?php

class ClientIP
{
    static function GetHeaderName()
    {
        $headers = getallheaders();
        $headerName = "X-Remote-Addr";

        if ($headers["X-Forwarded-For"] && $headers["X-Forwarded-For"] != "127.0.0.1")
        {
            $headerName = "X-Forwarded-For";
        }

        return $headerName;
    }

    public static function Contains($ip)
    {
        $headers = getallheaders();
        $headerName = ClientIP::GetHeaderName();
    
        $array = split(", ", $headers[$headerName]);
        return in_array($array, $ip);
    }

    public static function Get()
    {
        $headers = getallheaders();
        $headerName = ClientIP::GetHeaderName();
    
        $array = split(", ", $headers[$headerName]);
        return end($array);
    }
}

?> 