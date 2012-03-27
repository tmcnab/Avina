/// <reference path="_references.js" />

$().ready(function () {

    $.getScript('/Scripts/jquery.hotkeys.min.js', function () {
        $(document).bind('keydown', 'ctrl+space', function () {
            $('input[type="text"][aria-controls="T_Results"]').focus();
        });
        $(document).bind('keydown', 'tab', function (e) {
            if ($('input[type="text"][aria-controls="T_Results"]').is(':focus')) {
                e.stopPropagation();
                console.log('fire');
                $('input[type="text"][aria-controls="T_Results"]').blur();
                $("#T_Results tr:first").focus();
                return false;
            }
        });
    });

    $('#T_Results > tbody > tr').remove();
    $('#T_Results').dataTable({
        "sDom": "<'row'<'span6'l><'span6'f>r>t<'row'<'span6'i><'span6'p>>",
        "sPaginationType": "bootstrap",
        "bProcessing": true,
        "bServerSide": true,
        "sAjaxSource": "/Home/DataTables",
        "bDeferRender": true,
        "bLengthChange": false,
        "iDisplayLength": 10,
        "aoColumns": [
                {
                    "bSortable": false,
                    "fnRender": function (obj) {
                        return '<h3><a onclick="$.ajax({async:false,type:\'POST\',data:\'' + (obj.aData[0])[0] + '\',url:\'/submit/click\'});" data-purpose="external-link" href="' + (obj.aData[0])[0] + '">' + (obj.aData[0])[1] + '</a><h3>' +
                               '<h5>' + (obj.aData[0])[2] + '</h5>';
                    }
                },
                {
                    "bSearchable": false,
                    "sWidth": "55px",
                    "fnRender": function (obj) {
                        return '<span class="badge badge-success">' + obj.aData[1] + '</span>';
                    }
                },
                {
                    "bSearchable": false,
                    "sWidth": "70px",
                    "fnRender": function (obj) {
                        return '<span class="badge badge-info">' + obj.aData[2] + '</span>';
                    }
                }
            ]
    }).fnSetFilteringDelay(300);


    var searchBox = $('input[type="text"][aria-controls="T_Results"]')
                                .detach()
                                .addClass('span7')
                                .attr('x-webkit-speech', 'x-webkit-speech')
                                .attr('placeholder', 'Search Avina')
                                .css('margin-bottom', 0);
    $('div.dataTables_filter').remove();
    $('form.navbar-search').append(searchBox);

    $('div[role="grid"]').children().first().remove();

    $('#T_Results').find('th').attr('tabindex', '-1');
});