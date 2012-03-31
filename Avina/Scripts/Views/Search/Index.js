/// <reference path="../../jquery-1.7.1-vsdoc.js" />

$('#T_Results').dataTable({
    "sPaginationType": "bootstrap",
    "bLengthChange": false,
    "iDisplayLength": 10,
    "bFilter": false,
    "bSort": false,
    "sDom": "<'row'<'span4'l><'span4'f>r>t<'row datatable-footer'<'span5'p><'span3'i>>"
});
if (window.innerWidth >= 768) {
    $('#T_Results').parent().addClass('span9');
}


$('#T_Results_info').text($('#T_Results_info').text().replace('entries', 'Results'));

$('a[rel="popover"]').popover({
    placement: 'left'
});

$('button[data-toggle="dropdown"]').dropdown();

$('a[data-type="url"]').click(function (e) {
    e.preventDefault();
    $.ajax({
        url: '/submit/click',
        async: false,
        type: 'POST',
        data: {
            url: $(this).attr('href')
        }
    });
    window.location = $(this).attr('href');
});

$('a[data-type="url"]').on('focus', function (e) {
    $(this).parent().parent().addClass('highlighted');
});

$('a[data-type="url"]').on('blur', function (e) {
    $(this).parent().parent().removeClass('highlighted');
});

$(document).bind('keydown', 'esc', function () {
    $('input[type="search"]').focus();
});

$(document).bind('keydown', 'ctrl+space', function () {
    $('input[type="search"]').focus();
});

$(document).bind('keydown', 'up', function () {

});

$(document).bind('keydown', 'down', function () {

});

$(document).bind('keydown', 'left', function () {
    if (!$('input[type="search"]').is(':focus')) {
        if (!$('li.prev').hasClass('disabled')) {
            $('li.prev').children(':first').click();
        }
    }
});

$(document).bind('keydown', 'right', function () {
    if (!$('input[type="search"]').is(':focus')) {
        if (!$('li.next').hasClass('disabled')) {
            $('li.next').children(':first').click();
        }
    }
});