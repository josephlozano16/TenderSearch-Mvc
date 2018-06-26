var currentMethod = "";
var CLIENT = $(function () {

    //#region utility methods

    var removePager = function (newHtml) {

        const positionStart = 0;
        const positionEnd = newHtml.indexOf('<div class="pagedList"'); ///todo use jquery
        const noPager = newHtml.substring(positionStart, positionEnd);
        return noPager;
    };
    var getPager = function (newHtml) {
        const positionStart = newHtml.indexOf('<div class="pagedList"');///todo use jquery
        const positionEnd = newHtml.length;
        const pager = newHtml.substring(positionStart, positionEnd);
        return pager;
    };
    var showErrorMessage = function (currentModal, errMsg) {
        try {
            const errorMessageModalId = "#errorMessageModal";
            $(errorMessageModalId).draggable({
                handle: ".modal-header"
            });
            if (currentModal) {
                $(currentModal).modal("hide");
                $(currentModal).find("#modalbody").html(""); // clear contents
            }
            let newMsg = errMsg;
            ///check for destructive msgs 
            if (newMsg.indexOf("<html>") > 0) {
                const searchStringStart = "<title>";
                const searchStringEnd = "</title>";
                const positionStart = newMsg.indexOf(searchStringStart);///todo use jquery 
                const positionEnd = newMsg.indexOf(searchStringEnd);
                newMsg = newMsg.substring(positionStart + searchStringStart.length, positionEnd);


            }

            $(errorMessageModalId).find(".modal-body").first().html(newMsg);
            $(errorMessageModalId).modal("show");
        } catch (e) {

            console.warn(`Error in method: showErrorMessage - ${e.message}\n\n\n\nOriginal error:\n${errMsg}`);
            alert(`Error in method: showErrorMessage - ${e.message}`);
        }
    }
    var showPleaseWaitModal = function () {
        const modalId = "#pleaseWaitModal";
        $(modalId).modal("show");
    }

    var disableSortButtons = function (targetTbody) {
        const $sortButtons = targetTbody.parents("[data-qpse-tablecontainer='True']").parent("td").siblings().find("[data-qpse-sorttarget]");
        $sortButtons.addClass("disabled");
        $sortButtons.find("i").removeClass("animated pulse infinite");
    }
    var updatePageNumber = function (targetTbody, pageNumber) {
        targetTbody.parents("[data-qpse-tablecontainer='True']").parent("td").siblings().find("form[data-qpse-sortorderconfirmation='true']").find("#page").val(pageNumber);
    }


    var selectRow = function (td) {
        const selectioncolor = "warning";
        td.closest("tbody").find("tr").removeClass(selectioncolor);
        td.closest("tr").addClass(selectioncolor);
        const $sortButtons = td.parents("[data-qpse-tablecontainer='True']").parent("td").siblings().find("[data-qpse-sortdirection]");

        $sortButtons.removeClass("disabled");
        $sortButtons.find("i").addClass("animated pulse");
    }
    var rowSelectHandler = function () {
        selectRow($(this));
    }

    var setBusyIndicatorBig = function (modalId, targetElement) {
        if (!targetElement) targetElement = "#modalbody";
        const $busyIndicator = $("#busyIndicator");
        const $modalbody = $(modalId).find(targetElement);
        $modalbody.html($busyIndicator.html());
    }

    var updateRowCounter = function (item) {
        const rowLabel = " row(s) found.";
        //   $(item).parents().eq(11).css("border", "2px red solid");

        var $rowCounter = $(item).parents().eq(11).find(`span:contains('${rowLabel}')`);
        if ($rowCounter.length > 0) {
            $rowCounter = $rowCounter.first();
            $rowCounter.removeClass("animated rubberBand  ");

            const rowCounterHtml = $rowCounter.html();
            let counter = rowCounterHtml.replace(rowLabel, "").trim();
            counter = parseFloat(counter);
            counter--;
            $rowCounter.html(counter + rowLabel);

            $rowCounter.addClass("animated rubberBand  ");

        } else console.warn("=====$rowCounter not updated... ");
    }
    //#endregion // utility methods


    //#region Copy
    var ajaxCopy = function () {
        const $form = $(this);
        if (!$form.valid()) return false;
        var modalId = "#copyItemModal";

        const url = $form.attr("action");
        const options = {
            url: url,
            type: $form.attr("method"),
            data: $form.serialize() //important!!! this will prevent error: -> The required anti-forgery form field "__RequestVerificationToken" is not present
        };
        $("#busyindicatorCopy").show();
        $.ajax(options)
            .done(function (data) {
                $(modalId).modal("hide");
                window.location = data;
            })
            .fail(function (err, status) {
                showErrorMessage(modalId, err.responseText);
            });
        return false;
    }
    var copyLinkHandler = function () {
        var modalId = "#copyItemModal";
        $(modalId).draggable({
            handle: ".modal-header"
        });


        selectRow($(this));

        const copyLinkName = $(this).attr("data-qpse-copyname");
        $(modalId).find("#modaltitle").html(copyLinkName);

        const copyLinkObj = $(this);
        const url = copyLinkObj[0].href;

        setBusyIndicatorBig(modalId);
        $(modalId).modal("show"); //show busy indicator

        //Ajaxify here
        const options = {
            url: url,
            data: $("form").serialize(),
            type: "get"
        };


        $.ajax(options).done(function (data) {
            const $target = $(modalId).find("#modalbody");
            const $newHtml = $(data);
            $newHtml.find(".form-group").addClass("animated fadeIn");
            $target.html($newHtml);
            //TODO use Json response to insert the new row 
            //   $("form[data-qpse-createconfirmation='true']").validationEngine();
            $.validator.unobtrusive.parse("form[data-qpse-copyconfirmation='true']"); //refresh jqueryval
            $("form[data-qpse-copyconfirmation='true']").submit(ajaxCopy); //refresh event hook-up

        })
            .fail(function (err, status) {
                showErrorMessage(modalId, err.responseText);
            });

        return false;
    }
    //#endregion // Copy

    //#region Edit
    var ajaxEdit = function () {

        const $form = $(this);
        if (!$form.valid()) return false;

        const url = $form.attr("action");
        const options = {
            url: url,
            type: $form.attr("method"),
            data: $form.serialize() //important!!! this will prevent error: -> The required anti-forgery form field "__RequestVerificationToken" is not present
        };

        $("#busyindicatorEdit").show();
        var modalId = "#editItemModal";

        $.ajax(options)
            .done(function (data) {
                $(modalId).modal("hide");
                $(modalId).find("#modalbody").html("");
                window.location = data;
                $("[data-qpse-editlink='True']").click(editLinkHandler);
            })
            .fail(function (err, status) {
                showErrorMessage(modalId, err.responseText);
            });
        return false;
    }

    var editLinkHandler = function () {
        var modalId = "#editItemModal";
        $(modalId).draggable({
            handle: ".modal-header"
        });
        selectRow($(this));
        const editLinkName = $(this).attr("data-qpse-editname");
        $(modalId).find("#modaltitle").html(editLinkName);

        const editLinkObj = $(this);
        const url = editLinkObj[0].href;

        setBusyIndicatorBig(modalId);
        $(modalId).modal("show"); //show busy indicator

        //Ajaxify here
        const options = {
            url: url,
            data: $("form").serialize(),
            type: "get"
        };


        $.ajax(options)
            .done(function (data) {
                const $target = $(modalId).find("#modalbody");
                const $newHtml = $(data);
                //animated fadeInDown
                $newHtml.find(".form-horizontal").addClass("animated fadeIn");
                $target.html($newHtml);
                $.validator.unobtrusive.parse("form[data-qpse-editconfirmation='true']"); //refresh jqueryval
                $("form[data-qpse-editconfirmation='true']").submit(ajaxEdit); //refresh event hook-up
                setDatePickerHandler();

            })
            .fail(function (err, status) {
                showErrorMessage(modalId, err.responseText);
            });

        return false; // prevent default behaviour
    }
    //#endregion // Edit

    //#region Create
    var ajaxCreate = function () {
        var modalId = "#createItemModal";
        const $form = $(this);
        if (!$form.valid()) return false;

        const url = $form.attr("action");
        const options = {
            url: url,
            type: $form.attr("method"),
            data: $form.serialize() //important!!! this will prevent error: -> The required anti-forgery form field "__RequestVerificationToken" is not present
        };

        $("#busyindicatorCreate").show();


        $.ajax(options)
            .done(function (data) {
                $(modalId).modal("hide");
                $(modalId).find("#modalbody").html(""); // clear contents
                window.location = data;
                $("[data-qpse-createlink='true']").click(createLinkHandler);
            })
            .fail(function (err, status) {
                showErrorMessage(modalId, err.responseText);
            });
        return false;
    }

    var createLinkHandler = function () {
        var modalId = "#createItemModal";
        $(modalId).draggable({
            handle: ".modal-header"
        });
        const createLinkName = $(this).attr("data-qpse-createname");
        $(modalId).find("#modaltitle").html(createLinkName);

        const createLinkObj = $(this);
        const url = createLinkObj[0].href;

        setBusyIndicatorBig(modalId);
        $(modalId).modal("show"); //show busy indicator

        //Ajaxify here
        const options = {
            url: url,
            data: $("form").serialize(),
            type: "get"
        };


        $.ajax(options)
            .done(function (data) {
                const $target = $(modalId).find("#modalbody");
                const $newHtml = $(data);
                $newHtml.find(".form-horizontal").addClass("animated fadeIn");
                $target.html($newHtml);
                $.validator.unobtrusive.parse("form[data-qpse-createconfirmation='true']"); //refresh jqueryval
                $("form[data-qpse-createconfirmation='true']").submit(ajaxCreate); //refresh event hook-up
                setDatePickerHandler();
            })
            .fail(function (err, status) {
                showErrorMessage(modalId, err.responseText);
            });

        return false; // prevent default behaviour
    }
    //#endregion // Create

    //#region Delete

    var deleteLinkObj; //global variable used to visually delete the selected row
    // var replacePager = false;
    var ajaxDelete = function () {
        var modalId = "#deleteConfirmationModal";
        const $form = $(this);
        const url = $form.attr("action");
        const options = {
            url: url,
            type: $form.attr("method"),
            data: $form.serialize() //important!!! this will prevent error: -> The required anti-forgery form field "__RequestVerificationToken" is not present
        };

        $.ajax(options)
            .done(function (data) {
                $(modalId).modal("hide");
                deleteLinkObj.closest("tr").hide("fast"); //Hide Row
                disableSortButtons(deleteLinkObj);
                updateRowCounter(deleteLinkObj);
            })
            .fail(function (err, status) {
                showErrorMessage(modalId, err.responseText);
            });
        return false;
    };
    var deleteLinkHandler = function () {
        var modalId = "#deleteConfirmationModal";
        $(modalId).draggable({
            handle: ".modal-header"
        });
        selectRow($(this));

        deleteLinkObj = $(this);  //for ajaxDelete use. See ajaxDelete
        const deleteLinkName = $(this).attr("data-qpse-deletename");
        $("#data-qpse-deletename").html(deleteLinkName);//update message box

        const url = deleteLinkObj[0].href;

        setBusyIndicatorBig(modalId, ".modal-footer");
        $(modalId).modal("show");
        $("#data-qpse-deletename").addClass("animated swing");
        //Ajaxify here
        const options = {
            url: url,
            data: $("form").serialize(),
            type: "get"
        };

        $.ajax(options)
            .done(function (data) {

                const $target = $(modalId).find(".modal-footer");
                const $newHtml = $(data);
                $newHtml.find(".form-group, button").addClass("animated fadeIn");
                $target.html($newHtml);
                $("form[data-qpse-deleteconfirmation='true']").submit(ajaxDelete); //refresh event hook-up

            })
            .fail(function (err, status) {
                showErrorMessage(modalId, err.responseText);
            });

        return false; // prevent default behaviour
    }
    //#endregion // Delete

    //#region Unlock
    var unlockLinkObj; //global variable used to visually unlock the selected row
    // var replacePager = false;
    var ajaxUnlock = function () {
        var modalId = "#unlockConfirmationModal";
        const $form = $(this);
        const url = $form.attr("action");

        const options = {
            url: url,
            type: $form.attr("method"),
            data: $form.serialize() //important!!! this will prevent error: -> The required anti-forgery form field "__RequestVerificationToken" is not present
        };
        // var $target =  $form.attr("[data-valmsg-for='DeletionReason']") ;
        //$("[data-valmsg-for='DeletionReason']").cl() ;
        //return false;
        $("#busyindicatorunlock").show();

        $.ajax(options)
            .done(function (data) {

                $(modalId).modal("hide");
                window.location = data;
                $("[data-qpse-unlocklink='true']").click(unlockLinkHandler);
            })
            .fail(function (err, status) {
                showErrorMessage(modalId, err.responseText);
            });
        return false;
    };
    var unlockLinkHandler = function () {
        var modalId = "#unlockConfirmationModal";
        $(modalId).draggable({
            handle: ".modal-header"
        });

        unlockLinkObj = $(this);  //for ajaxUnlock use. See ajaxUnlock
        const unlockLinkName = $(this).attr("data-qpse-unlockname");
        $("#data-qpse-unlockname").html(unlockLinkName);//update message box


        const url = unlockLinkObj[0].href;

        setBusyIndicatorBig(modalId, ".modal-footer");
        $(modalId).modal("show");
        $("#data-qpse-unlockname").addClass("animated swing");
        //Ajaxify here
        const options = {
            url: url,
            data: $("form").serialize(),
            type: "get"
        };

        $.ajax(options).done(function (data) {
            const $target = $(modalId).find(".modal-footer");
            const $newHtml = $(data);
            $newHtml.find("button").addClass("animated fadeIn");
            $target.html($newHtml);

            $("form[data-qpse-unlockconfirmation='true']").submit(ajaxUnlock); //refresh event hook-up

        })
            .fail(function (err, status) {
                showErrorMessage(modalId, err.responseText);
            });

        return false; // prevent default behaviour
    }

    //#endregion // Unlock

    //#region Email
    var ajaxEmail = function () {
        var modalId = "#emailItemModal";
        const $form = $(this);
        if (!$form.valid()) return false;

        const url = $form.attr("action");
        const options = {
            url: url,
            type: $form.attr("method"),
            data: $form.serialize()
        };

        $("#busyindicatorEmail").show();


        $.ajax(options)
            .done(function (data) {
                $(modalId).modal("hide");
                $(modalId).find("#modalbody").html(""); // clear contents
                // window.location = data;
                $("[data-qpse-emaillink='true']").click(emailLinkHandler);
            })
            .fail(function (err, status) {

                showErrorMessage(modalId, err.responseText);
            });
        return false;
    }

    var emailLinkHandler = function () {
        var modalId = "#emailItemModal";
        $(modalId).draggable({
            handle: ".modal-header"
        });

        const emailLinkObj = $(this);
        const url = emailLinkObj[0].href;

        setBusyIndicatorBig(modalId);
        $(modalId).modal("show"); //show busy indicator

        //Ajaxify here
        const options = {
            url: url,
            data: $("form").serialize(),
            type: "get"
        };

        $.ajax(options)
            .done(function (data) {
                const $target = $(modalId).find("#modalbody");
                const $newHtml = $(data);
                $newHtml.find(".form-horizontal").addClass("animated fadeIn");
                $target.html($newHtml);
                $.validator.unobtrusive.parse("form[data-qpse-emailconfirmation='true']"); //refresh jqueryval
                $("form[data-qpse-emailconfirmation='true']").submit(ajaxEmail); //refresh event hook-up

            })
            .fail(function (err, status) {
                showErrorMessage(modalId, err.responseText);
            });

        return false; // prevent default behaviour
    }
    //#endregion // Email


    //#region Search

    var ajaxSearch = function (url, targettablebody) { ///Will return the pager html

        const $form = $("form[data-qpse-ajax='true']").filter(`[data-qpse-targettablebody='${targettablebody}']`);
        if ($form.length === 0) {
            console.warn("ajaxSearch - Unable to find: form[data-qpse-ajax='true'].");
            return false;
        }

        showBusySearch();

        const options = {
            url: url,
            data: $form.serialize(),
            type: $form.attr("method")
        };

        $.ajax(options)
            .done(function (newHtml) {
                const $target = $(targettablebody);
                var pagerHtml = getPager(newHtml);
                const noPagerTableHtml = removePager(newHtml);

                $target.html(noPagerTableHtml); //replace rows

                $target.find("[data-qpse-deletelink='true']").click(deleteLinkHandler);
                $target.find("tr td").click(rowSelectHandler);
                $target.find("[data-qpse-createlink='true']").click(createLinkHandler);
                $target.find("[data-qpse-editlink='True']").click(editLinkHandler);
                $target.find("[data-qpse-copylink='true']").click(copyLinkHandler);
                $target.find("[data-qpse-unlocklink='true']").click(unlockLinkHandler);

                $target.effect("highlight");
                const $pagedList = $target.parents().find("div .pagedList").filter(`[data-qpse-targettablebody='${targettablebody}']`);

                if (!$pagedList || $pagedList.length === 0) {
                    //console.warn("targettablebody: " + targettablebody);
                    alert("Unable to find pagedList");
                } else {
                    if (pagerHtml == null) {
                        pagerHtml = ""; //default to empty string
                        alert("pagerHtml is empty");
                    }
                    else {
                        $pagedList.html($(pagerHtml).html());
                        $pagedList.find("a").filter("[href]").click(pageClickHandler);
                    }
                    disableSortButtons($target);
                }

                const $pageNum = $pagedList.find(".active a, .active span");
                if ($pageNum) {
                    if ($pageNum.length > 0 && url) {
                        $pageNum.addClass("animated bounce");
                        const matches = url.match(/page=([^&]+)/);
                        if (matches && matches.length > 0) {
                            const page = url.match(/page=([^&]+)/)[1]; //get page number
                            updatePageNumber($target, page);
                        }
                    }
                }

                hideBusySearch();
                return pagerHtml;
            })
            .fail(function (err, status) {
                hideBusySearch();

                console.warn(err);
                showErrorMessage(null, err.responseText);
                pagerHtml = "";
                window.location = url; //redirect to non-ajax page
                return false;
            }
            );

    }

    const ajaxSearchSubmit = function () {
        const $form = $(this);
        const url = $form.attr("action");
        const targettablebody = $form.attr("data-qpse-targettablebody");
        const pager = ajaxSearch(url, targettablebody);

        return false;
    };

    var ajaxSubmitAutoCompleteSearch = function (event, ui) {
        const $input = $(this);
        $input.val(ui.item.label);

        const $form = $input.parent("form :first");
        $form.submit();
    };
    const createSearchAutoComplete = function () {
        var $input = $(this);

        const options = {
            source: function (request, response) {
                getIntellisense(request, response, $input);
            },
            select: ajaxSubmitAutoCompleteSearch,
            open: function (event, ui) {
                hideBusySearch();
            }
        };
        $input.autocomplete(options);
    };

    var showBusySearch = function () {
        //fa fa-spinner fa-pulse
        const $searchButton = $("i[data-qpse-searchbutton='true']");
        $searchButton.removeClass("fa-search-plus");
        $searchButton.addClass("fa-spinner");
        $searchButton.addClass("fa-pulse");
    }
    var hideBusySearch = function () {
        const $searchButton = $("i[data-qpse-searchbutton='true']");
        $searchButton.removeClass("fa-spinner");
        $searchButton.removeClass("fa-pulse");
        $searchButton.addClass("fa-search-plus");
    }
    var getIntellisense = function (request, callback, input) {
        // var $input = $(input); $form.serialize()
        const $form = input.parent("form :first");
        const url = input.attr("data-qpse-autocomplete");
        const options = {
            url: url,
            data: { term: request.term },
            type: $form.attr("method")
        };
        showBusySearch();
        $.ajax(options)
            .done(function (data) {
                var suggestions = [];

                $.each(data, function (i, val) {
                    suggestions.push(val.label);
                });

                hideBusySearch();
                callback(suggestions);
            })
            .fail(function (err, status) {
                hideBusySearch();
                showErrorMessage(null, err.responseText);
            });
    }
    //#endregion // Search

    //#region Pager Handlers

    var pageClickHandler = function () {
        const $a = $(this);
        const url = $a.attr("href");
        const busyindicatorPaging = $("#busyindicatorPaging");

        if (busyindicatorPaging) $a.prepend(busyindicatorPaging.html());
        const targettablebody = $a.parents(".pagedList").first().attr("data-qpse-targettablebody");
        ajaxSearch(url, targettablebody);

        return false;
    };
    //#endregion // Pager Handlers

    //#region sortMove Handlers
    const sortMoveHandler = function () {
        const $sortButton = $(this);
        const targetTableBody = $sortButton.attr("data-qpse-sorttarget");
        const sortDirection = $sortButton.attr("data-qpse-sortdirection");
        const pageNumber = $(targetTableBody).attr("data-qpse-pagenumber");
        const $selectedRow = $(targetTableBody).find("tr").filter(".warning"); //get selected row
        const fromPosition = $selectedRow.attr("data-qpse-positionfrom");
        var toPositon = $selectedRow.attr("data-qpse-positionto");

        if (!toPositon) toPositon = fromPosition;
        var canUpdate = false;
        var isTargetRowVisible = true;
        if (sortDirection === "up") {
            if ($selectedRow.prev().find("th").length === 0) {
                const prevRow = $selectedRow.prev();
                $selectedRow.prev().attr("data-qpse-positionto", toPositon);
                isTargetRowVisible = prevRow.is(":visible");


                toPositon--;
                $selectedRow.insertBefore(prevRow);
                if (!isTargetRowVisible) {
                    prevRow.show();
                    $selectedRow.hide();
                }
                canUpdate = true;
            }
        }
        if (sortDirection === "down") {
            if ($selectedRow.next().html()) {
                const nextRow = $selectedRow.next();
                nextRow.attr("data-qpse-positionto", toPositon);
                isTargetRowVisible = nextRow.is(":visible");

                toPositon++;
                $selectedRow.insertAfter(nextRow);
                if (!isTargetRowVisible) {
                    nextRow.show();
                    $selectedRow.hide();
                }
                canUpdate = true;
            }
        }
        if (canUpdate) {
            $selectedRow.attr("data-qpse-positionto", toPositon);
            var $sortRoles = $sortButton.closest("td").find("[data-qpse-sortrole]");
            if ($sortRoles && $sortRoles.length > 0) {
                setTimeout(function () {
                    if ($sortRoles.hasClass("disabled")) $sortRoles.removeClass("disabled");
                    $sortRoles.find("i").addClass("animated pulse infinite");
                }, 100);
            }
        }
    };
    //#endregion // sortMove Handlers  

    //#region sortRole Handlers
    var getSortItems = function (rows) {
        var sortItems = [];
        $(rows).each(function (index, value) {
            const id = $(this).attr("id");
            const positionTo = $(this).attr("data-qpse-positionto");
            const item = { 'ID': id, "Order": positionTo };
            sortItems.push(item);
        });
        return JSON.stringify(sortItems);
    }

    const sortRoleHandler = function () {
        const $button = $(this).find("[data-qpse-sortrole]");
        const role = $button.attr("data-qpse-sortrole");
        if (role) {
            $button.addClass("disabled");
            $button.find("i").removeClass("animated pulse infinite");

        }
        // data - qpse - sorttarget
        var modalId = "#pleaseWaitModal";

        if (role === "save") {
            $(modalId).modal("show"); //
            const $form = $(this);
            const rows = $($button.attr("data-qpse-sorttarget")).find("[data-qpse-positionto]");
            const sortItems = getSortItems(rows);
            $form.find("#sortItems").val(sortItems);


            const url = $form.attr("action");
            const options = {
                url: url,
                type: $form.attr("method"),
                data: $form.serialize()
            };

            $.ajax(options)
                .done(function (data) {

                    //clear all data-qpse-positionto values
                    $(modalId).modal("hide");
                })
                .fail(function (err, status) {
                    showErrorMessage(modalId, err.responseText);
                });
        }
        return false;
    };
    //#endregion // sortRole Handlers

    //#region downloadReport

    const downloadReport = function () {
        var modalId = "#pleaseWaitModal";
        showPleaseWaitModal();
        const $form = $(this);
        const url = $form.attr("action");
        const options = {
            url: url,
            type: $form.attr("method"),
            data: $form.serialize()
        };
        //  var deferred = $q.defer();
        $.ajax(options)
            .done(function (data) {
                $(modalId).modal("hide");
                window.location = data;
                //  deferred.resolve(data);
                //  alert("downloadReport touchdown test!");
            })
            .fail(function (err, status) {
                showErrorMessage(modalId, err.responseText);
            });
        return false;
    };

    const downloadReportHandler = function (reportItemId, reportKey) {
        const $form = $("form[data-qpse-downloadreportconfirmation='true']").first();
        $form.find("#reportItemId").val(reportItemId);
        $form.find("#reportKey").val(reportKey);
        $form.submit();
    };

    //#endregion // downloadReport

    //#region menuclick
    const menucClickHandler = function () {
        $("ul.nav > li").removeClass("active");
        $(this).addClass("active");
    };
    //#endregion // menuclick


    //#region datepicker
    const setDatePickerHandler = function () {
        $("input[type=datetime]").datepicker({
            todayBtn: "linked",
            clearBtn: true,
            forceParse: false,
            autoclose: true,
            todayHighlight: true,
            dateFormat: "dd/mm/yy"
        });
    };
    setDatePickerHandler();

    $.validator.addMethod("date",
        function (value, element) {
            $.culture = Globalize.culture("en-AU");
            const date = Globalize.parseDate(value, "dd/MM/yyyy", "en-AU");
            return this.optional(element) ||
                !/Invalid|NaN/.test(new Date(date).toString());
        });
    //#endregion // datepicker

    //#region EVENT HANDLERS
    $(".pagedList a").filter("[href]").click(pageClickHandler);
    $("[data-qpse-deletelink='true']").click(deleteLinkHandler);
    $("form[data-qpse-ajax='true']").submit(ajaxSearchSubmit);
    $("input[data-qpse-autocomplete]").each(createSearchAutoComplete);
    $("[data-qpse-tablecontainer='True'] table tbody tr td").click(rowSelectHandler);
    $("[data-qpse-createlink='true']").click(createLinkHandler);
    $("[data-qpse-editlink='True']").click(editLinkHandler);
    $("[data-qpse-copylink='true']").click(copyLinkHandler);
    $("[data-qpse-unlocklink='true']").click(unlockLinkHandler);
    $("[data-qpse-emaillink='true']").click(emailLinkHandler);
    $("[data-qpse-sortdirection]").click(sortMoveHandler);
    $("form[data-qpse-sortorderconfirmation='true']").submit(sortRoleHandler);
    $("form[data-qpse-downloadreportconfirmation='true']").submit(downloadReport);
    $("ul.nav > li, a.navbar-brand").click(menucClickHandler);


    //#endregion // EVENT HANDLERS 


    //#region public methods
    this.downloadReportHandler = downloadReportHandler;  //used by durandal

    //#endregion // public methods

})[0];