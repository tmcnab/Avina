(function () {
    $.ajaxSettings.beforeSend = function (xhr) {
        xhr.withCredentials = true;
    };
    try {
    $.ajax({
        type: 'POST',
        data: {
            url: document.URL,
            title: document.title,
            referrer: document.referrer
        },
        url: 'http://avina.apphb.com/Submit/',
        //url: 'http://localhost:28462/Submit/'
        error: function() {}
    });
    }
    catch(e) {}
})();