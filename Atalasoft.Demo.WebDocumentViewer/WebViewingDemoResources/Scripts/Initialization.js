var _viewer;
var _thumbs;
var _scanPage = 1;
var _serverUrl = "Handlers/WebDocumentViewerHandler.ashx";
var _docUrl = "~/WebViewingDemoResources/startup.pdf";
var _uploadPath = "~/WebViewingDemoResources/TempSession/";
var _thumbsShowing = true;

var _initialViewerWidth;
var _nothumbsViewerWidth;
var _testing;
var _lastUploadedFile;

$(function() {

    try {
        InitializeViewers();

        window.onresize = function() {
            $(".atala-document-viewer").height($(".inner-viewer").height());
            $(".atala-document-thumbs").height($(".inner-viewer").height());
            $(".clickmeleft").height($(".inner-viewer").height());
            $(".clickmeright").height($(".inner-viewer").height());
        };

        SetupClickBar();
    } 
    catch (error) {
        console.log(error);
        AppendStatus(error);
    }
});

function InitializeViewers() {
    _viewer = new Atalasoft.Controls.WebDocumentViewer({
        parent: $(".atala-document-viewer"),
        toolbarparent: $(".atala-document-toolbar"),
        serverurl: _serverUrl,
        allowannotations: true,
        showbuttontext: false,
        allowtext: true,
        mousetool: {
            text: {
                hookcopy: true
            }
        },
        upload: {
            enabled: true,
            uploadpath: _uploadPath,
            allowedfiletypes: '.jpg,.pdf,.png,.jpeg,image/tiff,.dwg,.doc,.docx,.raw,.orf,.raf,.cr3,.crw,.jbig2,.xps',
            allowedmaxfilesize: 1 * 1024 * 1024,
            allowmultiplefiles: false,
            allowdragdrop: true,
        },
        annotations: {
            defaults: [
                {
                    type: "line",
                    outline: { color: "#f00", opacity: 0.80, width: 15, endcap: { width: "wide", height: "long", style: "block" } }
                },
                {
                    type: "freehand",
                    outline: { color: "#00f", opacity: 0.80, width: 15 }
                },
                {
                    type: "text",
                    text: { value: "Double-click to change text", align: "left", font: { color: "#009", family: "Times New Roman", size: 36 } },
                    outline: { color: "#00a", opacity: 0.80, width: 1 },
                    fill: { color: "#ff9", opacity: 1 }
                },
                {
                    type: "rectangle",
                    fill: { color: "black", opacity: 1 }
                }
            ],
            stamps: [
                {
                    "name": "Approved",
                    "fill": {
                        "color": "white",
                        "opacity": 0.50
                    },
                    "outline": {
                        "color": "green",
                        "width": 15
                    },
                    "text": {
                        "value": "APPROVED",
                        "align": "center",
                        "font": {
                            "bold": false,
                            "color": "green",
                            "family": "Georgia",
                            "size": 64
                        }
                    }
                },
                {
                    "name": "Rejected",
                    "fill": {
                        "color": "white",
                        "opacity": 0.50
                    },
                    "outline": {
                        "color": "red",
                        "width": 15
                    },
                    "text": {
                        "value": "REJECTED",
                        "align": "center",
                        "font": {
                            "bold": false,
                            "color": "red",
                            "family": "Georgia",
                            "size": 64
                        }
                    }
                }
            ]
        }
    });

    _thumbs = new Atalasoft.Controls.WebDocumentThumbnailer({
        parent: $(".atala-document-thumbs"),
        serverurl: _serverUrl, // server handler url to send image requests to
        documenturl: _docUrl, // + _docFile, 	// document url relative to the server handler url
        allowannotations: true,
        allowdragdrop: true,
        dragdelay: Atalasoft.Utils.Browser.Mobile.Any() ? 750 : 250,
        viewer: _viewer
    });

    _viewer.bind({
        "error": onError,
        'fileaddedtoupload': onFileAdded,
        'fileuploadfinished': onFileUploadFinished,
        'uploadfinished': onUploadFinished,
    });

    $("body").bind("beforeunload", function() {
        _viewer.zoom(1);
        _thumbs.zoom(1);
    });

    function onError(e) {
        _testing = e.message;
        if (e.name != "ResumePageRequestsError")
            alert("Error: " + e.name + "\n" + e.message);
    }


    function onFileAdded(eventObj) {
        if (!eventObj.success) {
            switch (eventObj.reason) {
            case 1:
                ShowError("The size of file exceeds 1 Mb permitted.");
                break;
            case 2:
                ShowError("Prohibited file type.");
                break;
            case 3:
                ShowError("File with same name is already added to upload. ");
                break;
            }

        }

    }

    function onFileUploadFinished(eventObj) {
        _lastUploadedFile = eventObj.filepath;
    }

    function onUploadFinished(e) {
        if (_lastUploadedFile)
            _thumbs.OpenUrl(_lastUploadedFile, '');
    }

    //Don"t show the ellipse annotation.
    $(".atala-ui-icon-ellipse").parent().css("display", "none");

    //Don"t show the multi-lines annotations.
    $(".atala-ui-icon-lines").parent().css("display", "none");

    //Don"t show the save button. We"ll call it programmatically.
    $(".atala-ui-icon-save").parent().css("display", "none");

    //Hide entries on the context menu when not appropriate.
    _viewer.bind("contextmenu", function(event, anno, menu) {
        if (anno.type === "stamp")
            delete menu["Properties"];
    });


    SetViewerWidth();
}

function SetupClickBar() {

    var clickBar = $(".clickmeleft");

    clickBar.click(function(event) {

        var mainThumbs = $(".atala-document-thumbs");

        if (_thumbsShowing) {
            mainThumbs.animate({ width: "0px" }, 500, function() {
                mainThumbs.hide();
                $(".atala-document-viewer").animate({ width: _nothumbsViewerWidth }, 250, function() {});
            });
            _thumbsShowing = false;
            clickBar.removeClass("clickmeleft");
            clickBar.removeClass("clickmehoverleft");
            clickBar.addClass("clickmeright");
        } else {
            mainThumbs.show();
            mainThumbs.animate({ width: "200px" }, 500);
            _thumbsShowing = true;
            $(".atala-document-viewer").css("width", _initialViewerWidth);
            clickBar.removeClass("clickmeright");
            clickBar.removeClass("clickmehoverright");
            clickBar.addClass("clickmeleft");
        }
    });

    clickBar.hover(
        function() {
            if (_thumbsShowing)
                clickBar.addClass("clickmehoverleft");
            else
                clickBar.addClass("clickmehoverright");
        },
        function() {
            if (_thumbsShowing)
                clickBar.removeClass("clickmehoverleft");
            else
                clickBar.removeClass("clickmehoverright");
        }
    );
}

function SetViewerWidth(){

    var totalWidth = parseInt($(".main-viewer").css("width"),10);
    var thumbsWidth = parseInt($(".atala-document-thumbs").css("width"),10);
    var clickWidth =  parseInt($(".clickmeleft").css("width"),10);

    _initialViewerWidth = totalWidth - (thumbsWidth + clickWidth + 3);
    _nothumbsViewerWidth = totalWidth - clickWidth - 2;
    $(".atala-document-viewer").css("width", _initialViewerWidth);

}



function ShowError(msg){

    $("#resultsText").html(msg);
        $("#results").dialog({
            title: "File Upload",
            minWidth: 500,
            height: 300,
            buttons: [{
                text: "Close",
                click: function () { $(this).dialog("close"); }
            }],
            resizable: true
        });
}

function AppendStatus(error){
    console.log(error);
    alert(error);
}
