(function () {
    $.ajaxSettings.beforeSend = function (xhr) {
        xhr.withCredentials = false;
    };
    try {
        $.ajax({
            type: 'POST',
            crossDomain: true,
            data: {
                url: document.URL,
                referrer: document.referrer
            },
            url: 'http://avina.apphb.com/Submit/',
            //url: 'http://localhost:28462/submit/',
            error: function () { }
        });
    }
    catch (e) { }
})();