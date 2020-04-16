var _viewer;
var _thumbs;
var _scanPage = 1;
var _serverUrl = "Handlers/WebDocumentViewerHandler.ashx";
var _docUrl = "~/WebViewingDemoResources/startup.pdf";
var _savePath = "~/WebViewingDemoResources/Saved/";
var _thumbsShowing = true;

var _initialViewerWidth;
var _nothumbsViewerWidth;
var _testing;

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

        AddFileToolbar();

    } //End Try
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
        //documenturl: _docUrl,
        //savepath: _savePath,
        allowannotations: true,
        showbuttontext: false,
        allowtext: true,
        mousetool: {
            text: {
                hookcopy: true
            }
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
        "documentsaved": onDocumentSaved,
        "documentloaded": onDocumentLoaded
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

    function onDocumentSaved(e) {
    }

    function onDocumentLoaded(e) {
    }

    _viewer.annotations.setDefaults([
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
    ]);

    _viewer.annotations.setStamps([
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
    ]);

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

function AddFileToolbar() {


    $("." + _viewer.domclasses.atala_toolbar).prepend(AddFileUploadButton());
}

function AddFileUploadButton() {

	var uploadButton = $("<button id='undefined_wdv1_toolbar_Button_Upload' title='Upload File' class='ui-button ui-widget ui-state-default ui-corner-all ui-button-icon-only atala-ui-button  atala-upload-button' role='utton'>Upload File</button>");

    uploadButton.click(ShowFileUpload);

    uploadButton.button({
        icons: { primary: "atala-ui-icon atala-ui-icon-upload" }, text: false
    });
    
    return uploadButton;
}

function AddFileSaveButton() {

	var saveButton = $("<button id='undefined_wdv1_toolbar_Button_SaveFile' title='Save Document' class='ui-button ui-widget ui-state-default ui-corner-all ui-button-icon-only atala-ui-button atala-save-document-button' role='button'>Save Document</button>");

    saveButton.click(SaveFile);

    saveButton.button({
        icons: { primary: "atala-ui-icon atala-ui-icon-save-document" }, text: false
    });
    
    return saveButton;
}

function ShowFileUpload() {
    try {
        $("#resultsText").html("<div>Upload a file to have it displayed in the viewer.</div><br /><div><input id='fileUpload' type='file' name='file' class='input'/></div>");

        $("#results").dialog({
            title: "File Upload",
            minWidth: 500,
            height: 300,
            buttons: [{
                text: "Submit",
                id: "submitButton",
                disabled: true,
                click: function () { $(this).dialog("close"); }
            }, {
                text: "Cancel",
                click: function () { $(this).dialog("close"); }
            }],
            resizable: true
        });

        var gif = $(".loadingGif");
        var dim = $(".dimwrapper");
        $("#fileUpload").fileupload({
            replaceFileInput: false,
            dataType: "json",
            url:"Handlers/UploadHandler.ashx",
            add: function (e, data) {
                var btn = $("#submitButton");
                btn.button("enable");
                btn.button().click(function() {
                    ShowLoadingGif(gif);
                    data.submit();
                });
            },
            done: function (e, data) {
                dim.hide();
                gif.hide();
                if (data.result.success) {
                    _docUrl = data.result.file;
                    _thumbs.OpenUrl(_docUrl, "");

                } else {
                    ShowError();
                }
            },
            error: function (data, status, e) {
                dim.hide();
                gif.hide();
                alert(status + " " + data.error + " " + e);
            }
        });

    } catch (e) {
        alert(e);
    }
	
	return false;
}

function ShowError(){

        $("#resultsText").html("<div>We could not open your document.</div><br /><div>See our <a href='http://www.atalasoft.com/products/dotimage/feature-matrix' target='_blank'>feature matrix</a> for a complete list of supported file types.</div>");
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

function ShowLoadingGif(gif) {

    var viewer = $(".atala-document-viewer");
    var pos = viewer.position();
    var w = viewer.width();
    var h = viewer.height();

    var dim = $(".dimwrapper");
    dim.show();
    dim.css("left", (pos.left - 7));
    dim.css("top", (pos.top + 1));
    dim.css("height", h);
    dim.css("width", w);
    dim.css("background-color", "#424242");
    dim.css("opacity", 0.75);

    gif.show();
    gif.css("left", ((pos.left + w / 2) - 55));
    gif.css("top", ((pos.top + h / 2) - 55));
    gif.css("height", h);
    gif.css("width", w);
}

function SaveFile(){

    _viewer.save(null, function() {
        window.open("Handlers/ProcessingHandler.svc/MakePrintPdf?document=" + _docUrl + "&annotationFolder=" + _savePath); 
    });

    return false;
}

function AppendStatus(error){
    console.log(error);
    alert(error);
}
