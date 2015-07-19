<?php

include_once "Log.php";

class DAL
{
    public static function InsertProxy($ip, $port, $type, $subType, $myIp)
    {
        $typeData = mysql_query("SELECT Id FROM ProxyType WHERE Type = '".$type."'", self::getReadWriteConnection());

        if (!$typeData) 
        {
            Log::Add("Cannot get id of proxy type ".$type);
            return;
        }

        if (mysql_num_rows($typeData) != 1)
        {
            Log::Add("Cannot find single proxy type ".$type);
            return;            
        }

        $row = mysql_fetch_assoc($typeData);
        $typeId = $row["Id"];
       
        $result = mysql_query("INSERT INTO Proxy(Ip, Port, Type, InsertTime, SubType, ReportedBy) VALUES('".$ip."', ".$port.", ".$typeId .", '".date("Y-m-d H:i:s")."', '".$subType."', '".$myIp."');", self::getReadWriteConnection());
    
        if (!$result)
        {
            Log::Add("Cannot insert proxy ".$ip.":".$port);
        }
    }

    public static function CleanupOldProxiesIfNeeded()
    {
        /*$cleanupPeriodInSeconds = 31536000; // 1 year
        $data = mysql_query("SELECT LastCleanupTime FROM Setting Limit 1", self::getReadWriteConnection());
        
        if (!$data) 
        {
            Log::Add("Cannot get last cleuanup time. Error: ".mysql_error());
            return;
        }
        
        $row = mysql_fetch_assoc($data);
        $lastCleanupTime = $row["LastCleanupTime"];

        $secondsPassed = strtotime(date("Y-m-d H:i:s")) - $lastCleanupTime;

        if ($secondsPassed > $cleanupPeriodInSeconds)
        {
            $deleteTime = date('Y/m/d H:i:s', strtotime(date("Y-m-d H:i:s")) - $cleanupPeriodInSeconds);
            mysql_query("DELETE FROM Proxy WHERE InsertTime <='".$deleteTime."'", self::getReadWriteConnection());
            mysql_query("UPDATE Setting SET LastCleanupTime = '".date("Y-m-d H:i:s")."'", self::getReadWriteConnection());
        }*/

    }

    public static function IsProxyTypeExists($type)
    {
        if (!in_array(strtolower($type), self::GetProxyTypes()))
        {
            Log::Add("Passed unrecognized proxy type: ".$type." Request string: ".$_SERVER['REQUEST_URI']);
            return false;
        }

        return true;
    }

    public static function GetProxiesDataTableOrNull($type, $filtered, $limit)
    {
        if (!self::IsProxyTypeExists($type))
        {
            return null;
        }

        $query = "SELECT p.Ip, p.Port, MAX(p.InsertTime) AS InsertTime, p.SubType, NOW() as CurrentDate, Max(InsertTime) as MaxInsertTime, Min(InsertTime) AS MinInsertTime FROM Proxy as p".
                 " INNER JOIN ProxyType pt ON p.Type = pt.Id".
                 " WHERE pt.Type='".$type."' AND p.InsertTime > DATE_SUB(NOW(), INTERVAL 1 DAY)";
        
        if ($filtered)
            $query = $query." AND p.SubType <> 'Transparent'";

        $query = $query." GROUP BY p.Ip, p.Port, p.SubType".                        
                        " ORDER BY InsertTime DESC";

        if (isset($limit) && is_numeric($limit))
            $query = $query." LIMIT ".$limit;

        $data = mysql_query($query, self::getReadOnlyConnection());

        if (!$data) 
        {
            Log::Add("Cannot get proxy list. Error: ".mysql_error());
            return null;
        }

        return $data;
    }

    public static function GetAverageProxyAliveTimeInDays()
    {
	$query = "SELECT AVG(p.aliveTime) As avgAliveTime FROM (SELECT DateDIFF(Max(InsertTime),Min(InsertTime)) AS aliveTime FROM `Proxy`".
		  " Group By Ip, Port".
                " HAVING Max(InsertTime) > (NOW() - INTERVAL 1 DAY)".
                " ORDER By aliveTime DESC) AS p";

       $row = mysql_fetch_assoc(mysql_query($query, self::getReadOnlyConnection()));

       return $row['avgAliveTime'];
    }

    private static function ConnectToDB($userName, $password)
    {
        if (!$result = mysql_pconnect("mysql-p", $userName, $password)) 
        {
            Log::Add("Cannot connect to database");
            return null;
        }

        if (!mysql_select_db("p1268553_ProxySearcher", $result))
        {
            Log::Add("Cannot select database");
            return null;
        }

        return $result;
    }    

    private static function getReadWriteConnection()
    {
        return self::ConnectToDB("p1268553rw", "rtOu65C3Edsaz");
    }

    private static function getReadOnlyConnection()
    {
        return self::ConnectToDB("p1268553ro", "aWq1dgrde4swW");
    }

    private static function GetProxyTypes()
    {
        $data = mysql_query("SELECT Type FROM ProxyType", self::getReadWriteConnection());

        if (!$data) 
        {
            Log::Add("Cannot execute query in DAL::GetProxyTypes function: ".mysql_error());
            return array();
        }

        $result = array();

        while ($row = mysql_fetch_assoc($data)) 
        {  
            array_push($result, $row["Type"]);
        }

        return $result;
    }
}

?>