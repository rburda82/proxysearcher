<?php 
  $redirectUrl = str_replace("ProxyList.php","Proxy List.php",$_SERVER[REQUEST_URI]);
  header("Location: ".$redirectUrl);
  die();
?>