
var microsoftTeams;
var tenantId;

// Set up the tab and stuff.
microsoftTeams.initialize();

//Get context for Microsoft Teams tab. like context about user, team or company.
microsoftTeams.getContext(function (context) {
    if (context != null) {
        userObjectId = context.userObjectId;
    }
});

// Get selected teams.
function getSelectedTeams() {
    var teams = [];
    $("#eventTeam :selected").each(function () {
        teams.push({
            Id: $(this).val()
        });
    });
    return teams;
}

// submit the data to task module opener.
function submitForm(action) {
    var eventInfo = {
        Id: eventId,
        Type: $('#eventType :selected').val(),
        Title: $('#title').val(),
        Message: $('#message').val(),
        Date: $('#eventDate').val(),
        ImageURL: $(".carousel-item.active > img").attr("src"),
        Teams: getSelectedTeams(),
        OwnerAadObjectId: userObjectId,
        TimeZoneId: $('#timezonelist :selected').val(),
    };
    if (IsValidDate($('#eventDate').val())) {
        $("#invalidDate").hide();
    } else {
        $("#invalidDate").show();
        return false;
    }

    eventData = {
        Action: action,
        EventInfo: eventInfo
    };

    if (eventId) {
        microsoftTeams.tasks.submitTask(eventData);
        return true;
    }
    else {
        var requestUrl = "/Tabs/GetTotalEventCountOfUser?userObjectId=" + userObjectId;
        $.get(requestUrl, function (data) {
            var count = Number(data);
            if (count < 5) {
                microsoftTeams.tasks.submitTask(eventData);
                return true;
            } else {
                $("#errorAlert span").text("I’ve reached the maximum number of events I can keep track of right now. If you want to add something new, delete an older one to make room.");
                $("#errorAlert").removeClass('hide').addClass('show');
            }
        });
    }
    return false;
}

function IsValidDate(date) {
    var separator = date.match(/[.\/\-\s].*?/),
        parts = date.split(/\W+/);
    if (!separator || !parts || parts.length < 3) {
        return false;
    }
    if (isNaN(Date.parse(date))) {
        return false;
    }

    return true;
}


function closeTaskModule() {
    microsoftTeams.tasks.submitTask();
}