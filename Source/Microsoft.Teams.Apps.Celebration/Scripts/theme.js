var DARK = "dark";
var CONTRAST = "contrast";

// To set the theme to the page
function setTheme(theme, id) {
    switch (theme) {
        case DARK:           
            document.getElementById(id).className = "theme-dark";
            break;
        case CONTRAST:
            document.getElementById(id).className = "theme-highContrast";
            break;
        default:
            document.getElementById(id).className = "theme-default";
            break;
    }
}

// To get the query string key and values
function getQueryParameters(delimeter) {
    let queryParams = {};
    location.search.substr(1).split(delimeter).forEach(function (item) {
        let s = item.split("="),
            k = s[0],
            v = s[1] && decodeURIComponent(s[1]);
        queryParams[k] = v;
    });
    return queryParams;
}