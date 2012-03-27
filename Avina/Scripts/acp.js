/// <reference path="jquery-1.7.1-vsdoc.js" />

$('#removeListingForm').submit(function (e) {
    e.preventDefault();
    $.post('/ACP/DeleteUrl', $('#removeListingUrl').val(), function (stat) {
        if (stat) {
            $('#removeListingUrl').attr('placeholder', 'Url Deleted').val('');
        }
        else {
            $('#removeListingUrl').attr('placeholder', 'Error Deleting Url').val();
        }
    });
});

$('#FilterPurge_Button').click(function () {
    $.post('/ACP/PurgeWithFilters', null, function (result) {
        if (result) $('#PurgeFilters_Button').text('Purged').delay(2500).text('Purge');
        else        $('#PurgeFilters_Button').text('Error').delay(2500).text('Purge');
    });
});

$('#RebuildIndex_Button').click(function () {
    $.post('/ACP/RebuildIndex', null, function (result) {
        if (result) $('#RebuildIndex_Button').text('Rebuilding').addClass('disabled');
    });
});

$().ready(function () {
    setInterval(IndexStatus, 5000);
    setInterval(FilterStatus, 5000);
    IndexStatus();
    FilterStatus();
});

function IndexStatus() {
    $.getJSON('/ACP/IndexStatus', function (result) {
        if (!result.isRebuilding) {
            $('#RebuildIndex_Button').text('Rebuild Index')
                                            .removeClass('disabled');
        }
        else {
            var percent = (parseFloat(result.currentItems) / parseFloat(result.totalItems)) * 100.0;
            $('#PB_IndexStatus').css('width', percent + '%');
        }
        $('#T_IndexStatus > tbody > tr').remove();

        $('#T_IndexStatus').append('<tr><td>Total Keywords   </td><td>' + result.totalEntries + '</td></tr>')
                                .append('<tr><td>Total Records    </td><td>' + result.totalItems + '</td></tr>')
                                .append('<tr><td>Records Processed</td><td>' + result.currentItems + '</td></tr>')
                                .append('<tr><td>Average Query Time (ms)</td><td>' + result.avgQueryTime + '</td></tr>');
    });
}

function FilterStatus() {
    $.getJSON('/ACP/FiltersGet/', null, function (model) {
        $('#T_ActiveFilters > tbody > tr').remove();
        var table = $('#T_ActiveFilters');
        $.each(model, function (i, item) {
            table.append('<tr><td><code>' + item + '</code></td><td><a class="close">&times;</a></td></tr>');
        });
    });
}