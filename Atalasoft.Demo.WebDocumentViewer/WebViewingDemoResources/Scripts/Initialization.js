var _viewer;
var _thumbs;
var _scanPage = 1;
var _serverUrl = "Handlers/WebDocumentViewerHandler.ashx";
var _docUrl = "~/WebViewingDemoResources/startup.pdf";
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
        __CSearchBox($(".atala-document-toolbar"), 100, true);
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
        allowannotations: true,
        showbuttontext: false,
        allowtext: true,
        mousetool: {
            text: {
                hookcopy: true,
                allowsearch: false
            }
        }
    });

    _thumbs = new Atalasoft.Controls.WebDocumentThumbnailer({
        parent: $(".atala-document-thumbs"),
        serverurl: _serverUrl, // server handler url to send image requests to
        documenturl: _docUrl, // + _docFile, 	// document url relative to the server handler url
        allowannotations: true,
        allowdragdrop: true,
        viewer: _viewer
    });

    _viewer.bind("error", onError);

    $("body").bind("beforeunload", function() {
        _viewer.zoom(1);
        _thumbs.zoom(1);
    });

    function onError(e) {
        if(console && console.log)
            console.log(e);

        if (e.name != "ResumePageRequestsError")
            alert("Error: " + e.name + "\n" + e.message);
    }

    _viewer.annotations.setDefaults([
        {
            type: "line",
            outline: { color: "#f00", opacity: 0.80, width: 15, endcap: { width: "wide", height: "long", style: "classic" } }
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
    
    var toolbar = $("<div />");
    toolbar.addClass("UploadToolbar");
    toolbar.append(AddFileUploadButton());

    $(".atala-document-toolbar").prepend(toolbar);
}

function AddFileToolbar() {

    var toolbar = $("<div />");
    toolbar.addClass("UploadToolbar");
    toolbar.append(AddFileUploadButton());

    $(".atala-document-toolbar").prepend(toolbar);
}

function AddFileUploadButton() {

	var uploadButton = $("<button id='UploadButton' title='Upload File' class='ui-button ui-widget ui-state-default ui-corner-all ui-button-icon-only atala-ui-button  atala-upload-button' role='utton'>Upload File</button>");

    uploadButton.click(ShowFileUpload);

    uploadButton.button({
        icons: { primary: "atala-ui-icon atala-ui-icon-upload" }, text: false
    });
    
    return uploadButton;
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
                if (data.result.success === "true") {
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

function AppendStatus(error){
    console.log(error);
    alert(error);
}

function __CSearchBox(toolbarParent, searchDelay, wrapSearch) {
    var styles = {
        clearable: 'atala_search_input_clearable',
        onclear: 'atala_search_input_clear_hover',
        inputEmpty: 'atala_search_input_empty',
        loading: 'atala_search_input_loading'
    };

    var parent = toolbarParent;
    var delay = searchDelay || 100;
    var wrap = wrapSearch ? true : false;
    var iterator = null;
    var searchTimeout = null; // timeout to trigger search when user finished typing in search box.
    var container,
        inputBox;

    __initDom();

    function __initDom() {
        container = $('<div />').addClass(_viewer.domclasses.atala_search_container);

        inputBox = $('<input type="text" placeholder="Search..." />').addClass(_viewer.domclasses.atala_search_input).addClass('ui-widget').css('height',  '24px');
        inputBox.bind({
            keydown: __onInputKeyUp,
            'input propertychange': __onIputChange
        });

        var inputSpan = $('<span />').appendTo(container).css({ width: '100%' });
        inputSpan.append(inputBox);

        var buttonsSpan = $('<span />').appendTo(container);
        $('<button />')
            .prop('title', 'Next')
            .click(__makeButtonHandler(__onSearchNext))
            .button({
                icons: { primary: 'atala-ui-icon atala-ui-icon-search-next' },
                text: false
            })
            .addClass('atala-ui-button')
            .css({ 'float': 'right', 'height': '20px' })
            .appendTo(buttonsSpan);

        $('<button />')
            .prop('title', 'Previous')
            .click(__makeButtonHandler(__onSearchPrev))
            .button({
                icons: { primary: 'atala-ui-icon atala-ui-icon-search-prev' },
                text: false
            })
            .addClass('atala-ui-button')
            .css({ 'float': 'right', 'height': '20px'  })
            .appendTo(buttonsSpan);

        container.on('mousemove', '.' + styles.clearable, __togglePointer)
                 .on('touchstart click', '.' + styles.onclear, __onClearClick);

        parent.append(container);
        parent.append($('<div style="clear:both;"></div>'));
    }

    this.dispose = __disposeSearchBox;
    function __disposeSearchBox() {
        inputBox.unbind({
            keypress: __onInputKeyUp,
            'input propertychange': __onIputChange
        });

        container.off('mousemove', '.' + styles.clearable, __togglePointer)
            .off('touchstart click', '.' + styles.onclear, __onClearClick);

        if (iterator) {
            iterator.dispose();
        }

        container.remove();
    }

    function __onIputChange() {

        var text = inputBox.val();

        if (text) {
            inputBox.addClass(styles.clearable);
        } else {
            inputBox.removeClass(styles.clearable);
        }

        if (text && iterator && iterator.isValid() && text === iterator.getQuery()) {
            return true;
        }

        clearTimeout(searchTimeout);
        iterator = null;

        if (text && text.length >= 3) {
            __updateMatchIndicator(true);
            searchTimeout = setTimeout(function () {
                iterator = _viewer.text.search(text, _viewer.getCurrentPageIndex(), __onNextMatch);
                __suspendUI(true);
            }, delay);

            return false;
        } else {
            __onClearSearch();
        }
    }

    function __onInputKeyUp(e) {
        var text = inputBox.val();
        if (e.keyCode === 13 && iterator && text && iterator.isValid() && iterator.getQuery() === text) {
            if (!e.shiftKey) {
                __onSearchNext();
            } else {
                __onSearchPrev();
            }
            return false;
        } else if (e.keyCode === 13 && (!iterator || !iterator.isValid())) {
            __onIputChange();
            return false;
        }
        else if (e.keyCode === 27) {
            __onClearSearch();
            __onClearClick();
            return false;
        } else if (Atalasoft.Utils.Browser.Explorer && Atalasoft.Utils.Browser.Version <= 9 && (e.keyCode === 8 || e.keyCode === 46)) {
            // old ie incorrectly handles delete/backspace keys: they are not throwing oninput. so workaround it here.
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                __onIputChange();
            }, delay);
        }
    }

    function __onSearchNext() {
        if (iterator) {
            __suspendUI(true);
            iterator.next(__onNextMatch);
        }
    }

    function __onSearchPrev() {
        if (iterator) {
            __suspendUI(true);
            iterator.prev(__onNextMatch);
        }
    }

    function __onClearSearch() {
        iterator = null;
        _viewer.text.search('');
        __suspendUI(false);
        __updateMatchIndicator(true);
    }

    function __onNextMatch(iterator, match) {
        if (iterator.isValid()) {
            __suspendUI(false);
            iterator.wrap = wrap;
            if (!match) {
                __updateMatchIndicator(match);
            }
        }
    }

    function __suspendUI(suspended) {
        __toggleStyle(styles.loading, suspended);
    }

    function __updateMatchIndicator(match) {
        __toggleStyle(styles.inputEmpty, !match);
    }

    function __makeButtonHandler(fn) {
        return function (e) {
            e.preventDefault();
            if (fn) {
                fn();
            }
        };
    }

    function __toggleStyle(style, enabled) {
        if (enabled) {
            inputBox.addClass(style);
        } else {
            inputBox.removeClass(style);
        }
    }

    function __togglePointer(e) {
        __toggleStyle(styles.onclear, this.offsetWidth - 18 < e.clientX - this.getBoundingClientRect().left);
    }

    function __onClearClick(e) {
        if (e) {
            e.preventDefault();
        }
        inputBox.removeClass(styles.clearable).removeClass(styles.onclear).val('').change();
        __onIputChange();
    }
}