<!DOCTYPE html>
<html>
    <head>
        <meta http-equiv="X-UA-Compatible" content="IE=edge">
        <meta http-equiv="Pragma" content="no-cache">
        <meta http-equiv="Expires" content="-1">
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
        <link rel="shortcut icon" href="/Images/favicon.ico">

        <title><?php 
                      include_once "PhpLibraries/DAL.php";
                      $type = $_REQUEST["type"];
                      if (!DAL::IsProxyTypeExists(strtolower($type)))
                      {
                           $type="http";
                      }

                      echo strtoupper($type); 
               ?> Proxy List</title>
        <link href="CSS/style.css" rel="stylesheet" type="text/css">
        <link href="/Libraries/jquery.tablesorter/themes/general/style.css" rel="stylesheet" type="text/css">
      
        <script type="text/javascript">
            var _gaq = _gaq || [];
            _gaq.push(['_setAccount', 'UA-39209364-1']);
            _gaq.push(['_trackPageview']);

            (function () {
                var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
                ga.src = ('https:' == document.location.protocol ? 'https://' : 'http://') + 'stats.g.doubleclick.net/dc.js';
                var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
             })();
         </script>
         <script src="Javascript/jquery-1.9.1.min.js"></script>
         <script src="Libraries/jquery.tablesorter/jquery.tablesorter.min.js"></script>
         <script src="Javascript/proxy-list.js"></script>

    </head>
    <body>
       <div class="head-element">
         <div class="head-content">
            <h2 class="looking-for-proxies-easily">Looking for proxies easily
            </h2>
            <h2 class="find-avaliable-proxies-in-one-click">Find available proxies in one click
            </h2>
            <h2 class="hide-your-ip-from-everyone">Hide your IP from everyone
            </h2>
            <h2 class="do-it-for-free">Use Proxy Searcher for free
            </h2>
         </div>
       </div>

       <div class="body-element">
           <a href="/"><img src="Images/world64x64.jpg" class="program-logo" /></a>
           <h1 class="body-header">Burd's <?php 
                                               include_once "PhpLibraries/DAL.php";
                      			     $type = $_REQUEST["type"];
                      			     if (!DAL::IsProxyTypeExists(strtolower($type)))
                      			     {
                           			        $type="http";
                      			     }

                                               echo strtoupper($type);
                                           ?> Proxy List</h1>
            
           <div class="separator"></div>

           <h3>This list is generated automatically based on <a href="/">Proxy Searcher</a> queries. </h3>
           <h4>Average proxy alive time: <?php include_once "PhpLibraries/DAL.php";
                                                echo DAL::GetAverageProxyAliveTimeInDays(); ?> day(s)</h4>

           <div class="normal-text">
               <?php
		     include_once "PhpLibraries/DAL.php";
                   $type = strtolower($_REQUEST["type"]);
                   if (!DAL::IsProxyTypeExists(strtolower($type)))
                   {
                       $type="http";
                   }

                   $anotherProtocol = $type == "http"  ? "socks" : "http";
                   $filteredPath = strtolower($_REQUEST["filtered"]) == "true"? "&filtered=true" : "";               
                   echo "Switch to <a href=\"/Proxy List.php?type=".$anotherProtocol.$filteredPath."\">".strtoupper($anotherProtocol)." Proxy List.</a>"
               ?>		
           </div>
           
           <div class="proxy-list">
               <div class="table-element">
                   <table border="1px" cellpadding="2px" id="proxyTable" class="tablesorter">
                       <thead>
                           <tr>
                               <th>#</th>
                               <th>Ip:Port</th>
                               <?php 
                                  if (strtolower($_REQUEST["type"]) == "http")
                                  {
                                      echo "<th>Type</th>";
                                  }
                               ?>
                               <th style="width:100px;">Alive time</th>
                               <th style="width:100px;">Last check</th>
                            </tr>
                       </thead>
                       <tbody>
                       <?php
                           include_once "PhpLibraries/DAL.php";
                           include_once "PhpLibraries/Cache.php";
                           $limit = '';

                           $type = strtolower($_REQUEST["type"]);
                           $filtered = strtolower($_REQUEST["filtered"]) === "true";
                           if (is_numeric($_REQUEST["limit"]))
                           {
                              $limit = intval($_REQUEST["limit"]);
                           }

                           $isProxyExists = false;

                           if (!DAL::IsProxyTypeExists(strtolower($type)))
                           {
                               $type="http";
                           }

                           $data = Cache::GetProxiesDataTableOrNull($type, $filtered, $limit);
                           $index = 0;

                           if ($data != null)
                           {
                               while ($row = mysql_fetch_assoc($data)) 
                               {   
                                   $index++;
                                   
                                   $seconds = strtotime($row["CurrentDate"]) - strtotime($row["InsertTime"]);
                                   $aliveSeconds = strtotime($row["MaxInsertTime"]) - strtotime($row["MinInsertTime"]);
                                          
                                   $timeToShow = ($seconds >= 86400)? "> 1 day": date("H:i:s",  $seconds);                                       
                                   $aliveDaysToShow = floor($aliveSeconds / 86400);                                       
                                   $aliveTimeToShow = date("H:i:s",  $aliveSeconds);

                                   if ($aliveDaysToShow != 0)
                                   {
                                      $aliveTimeToShow = $aliveDaysToShow." ".$aliveTimeToShow;
                                   }
   
                                   $redColor = round(0xFF * $seconds / 3600);
                                   $greenColor = round(0xFF * (3600 - $seconds) / 3600);
                                   $color = $seconds > 3600? "FF0000" : sprintf("%02x%02x%02x", $redColor, $greenColor, 0);

                                   if ($type == "http")
                                   {                                  
                                       echo "<tr><td>".$index."</td><td>".$row["Ip"].":".$row["Port"]."</td><td>".$row["SubType"]."</td><td>".$aliveTimeToShow."</td><td style=\"color:#".$color.";\">".$timeToShow."</td></tr>";
                                   }
                                   else
                                   {
                                       echo "<tr><td>".$index."</td><td>".$row["Ip"].":".$row["Port"]."</td><td>".$aliveTimeToShow."</td><td style=\"color:#".$color.";\">".$timeToShow."</td></tr>";
                                   }
                                   $isProxyExists = true;
                               }
                           }


                           if (!$isProxyExists)
                           {
                               if ($type == "http")
                               {
                                   echo "<tr><td colspan=\"4\">No one proxy of this type was found</td></tr>";
                               }
                               else
                               {
                                   echo "<tr><td colspan=\"3\">No one proxy of this type was found</td></tr>";
                               }
                           }
                        ?>
                       </tbody>
                   </table>       
                </div>
            </div>
         </div>
         <div class="right-panel-advertising">
              <!-- Begin BidVertiser code -->
			<SCRIPT LANGUAGE="JavaScript1.1" SRC="http://bdv.bidvertiser.com/BidVertiser.dbm?pid=638376&bid=1591769" type="text/javascript"></SCRIPT>
			<noscript><a href="http://www.bidvertiser.com/bdv/BidVertiser/bdv_xml_feed.dbm">search feed</a></noscript>
		<!-- End BidVertiser code --> 		   
         </div>
         <div class="right-panel-advertising">
			<div>
				Copy proxies here
			</div>
			<?php
                           include_once "PhpLibraries/DAL.php";
                           include_once "PhpLibraries/Cache.php";
                           
                           if (!DAL::IsProxyTypeExists(strtolower($type)))
                           {
                               $type = "http";
                           }

                           $data = Cache::GetProxiesDataTableOrNull($type, $filtered, $limit);
                           $index = 0;

                           if ($data != null)
                           {
                               while ($row = mysql_fetch_assoc($data)) 
                               {   
                                   echo "<div>".$row["Ip"].":".$row["Port"]."</div>";                                       
                                   $isProxyExists = true;
                               }
                           }
                            

                           if (!$isProxyExists)
                           {
                               if ($type == "http")
                               {
                                   echo "<tr><td colspan=\"4\">No one proxy of this type was found</td></tr>";
                               }
                               else
                               {
                                   echo "<tr><td colspan=\"3\">No one proxy of this type was found</td></tr>";
                               }
                           }
                        ?>
		</div>
         <div class="clear"></div>
         <div class="separator"></div>
     </body>
</html>


