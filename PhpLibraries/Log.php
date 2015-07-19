<?php

class Log
{
    function Add($line)
    {
	 $line = date('H:i:s d-m-Y').": ".$line;
        $path = "/home/project-web/proxysearcher/htdocs/Logs/current.txt";
        $sizeLimit = 1048576;

        if (filesize($path) > $sizeLimit)
        {
            file_put_contents($path, $line."\r\n", LOCK_EX);
        }
        else
        {
            file_put_contents($path, $line."\r\n", FILE_APPEND | LOCK_EX);
        }
    }
}

?>