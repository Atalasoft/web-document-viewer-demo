<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WebViewingDemoControl.ascx.cs" Inherits="WebViewingDemoControl" %>

    <!--[if lte IE 8]><script src="../Scripts/html5.js" type="text/javascript"></script><![endif]-->
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />

    <script src="Scripts/jquery-1.7.1.min.js" type="text/javascript"></script>
    <script src="Scripts/jquery.easing.1.3.js" type="text/javascript"></script>
    <link href="Content/themes/base/minified/jquery.ui.all.min.css" rel="stylesheet" type="text/css" />

    <script src="Scripts/jquery-ui-1.8.14.min.js" type="text/javascript"></script>
    <script src="Scripts/raphael-min.js" type="text/javascript"></script>
    <script src="Scripts/atalaWebDocumentViewer.js" type="text/javascript"></script>
    <link href="Icons/atalaWebDocumentViewer24.css" rel="stylesheet" type="text/css" />

    <link href="WebViewingDemoResources/Scripts/Demo.css" rel="stylesheet" type="text/css" />
    <script src="WebViewingDemoResources/Scripts/ajaxfileupload.js" type="text/javascript"></script>
    <script src="WebViewingDemoResources/Scripts/Initialization.js" type="text/javascript"></script>

<div class="main-viewer">
    <div class="atala-document-toolbar">
    </div>
    <div class="inner-viewer">
        <div class="atala-document-thumbs">
        </div>
        <div class="clickmeleft">
        </div>
        <div class="atala-document-viewer">
        </div>
    </div>
</div>

<div id="LoadingGif" class="loadingGif" style="display: none;">
</div>
<div id="results" title="Results" style="display: none; overflow: auto;">
    <div id="resultsText" style="overflow: auto;"></div>
</div>