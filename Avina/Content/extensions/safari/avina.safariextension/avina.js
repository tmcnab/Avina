(function () {
    $.ajaxSettings.beforeSend = function (xhr) {
        xhr.withCredentials = true;
    };
    $.ajax({
        type: 'POST',
        data: {
            url: document.URL,
            title: document.title,
            referrer: document.referrer
        },
        url: 'http://avina.apphb.com/Submit/'
    });
    //'http://localhost:28462/Submit/'
})();