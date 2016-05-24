<%@ Page Title="Home Page" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebViewingDemo._Default" %>
<%@ Register TagPrefix="wdv" TagName="WebViewingDemo" Src="~/WebViewingDemoControl.ascx" %>

<!DOCTYPE />

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <wdv:WebViewingDemo runat="server" />
    </div>
    </form>
</body>
</html>
