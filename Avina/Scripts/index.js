/// <reference path="_references.js" />

$().ready(function () {
    $('#T_Results').dataTable({
        "sDom": "<'row'<'span6'l><'span6'f>r>t<'row'<'span6'i><'span6'p>>",
        "aaSorting": [[1, "desc"], [2, "desc"]],
        "sPaginationType": "bootstrap"
    });

    var searchBox = $('input[type="text"][aria-controls="T_Results"]')
                                .detach()
                                .addClass('span6')
                                .attr('x-webkit-speech', 'x-webkit-speech')
                                .attr('placeholder', 'Search Avina')
                                .css('margin-bottom', 0);
    $('div.dataTables_filter').remove();
    $('form.navbar-search').append(searchBox);

    var nResultsBox = $('select[name="T_Results_length"][aria-controls="T_Results"]')
                                .detach()
                                .addClass('span1')
                                .css('margin-left', '5px')
                                .css('margin-top', '6px')
                                .css('margin-bottom', '0px');
    $('#T_Results_wrapper > div.row').first().remove();
    $('ul.nav-select').append($('<li>').append(nResultsBox));

    $('tr td a').on('click', function (e) {
        if ($(this).attr('data-purpose') === 'external-click') {
            //e.preventDefault();
            $.ajax({
                async: false,
                type: 'POST',
                data: $(this).attr('href'),
                url: '/submit/click'
            });
        }
    });
});