<?php

include_once "PhpLibraries/HttpProxyType.php";

function Get()
    {
        $headers = getallheaders();

        if($headers["X-Forwarded-For"] && $headers["X-Forwarded-For"] != $headers["X-Remote-Addr"])
            return "Transparent";

        if ($headers["Via"])
        {
            return "Anonymous";
        }

        return "HighAnonymous";
    }


$headers = getallheaders();

foreach (getallheaders() as $name => $value) {
    echo "$name: $value\n"."<br />";
}


echo "<br>";

foreach ($_SERVER as $name => $value) {
    echo "$name: $value\n"."<br />";
}

echo "<br>";

echo Get();

echo "<br>";

echo date('H:i:s d-m-Y');

?>