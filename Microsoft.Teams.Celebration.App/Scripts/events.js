
var microsoftTeams;
var tenantId;

// Set up the tab and stuff.
microsoftTeams.initialize();

microsoftTeams.getContext(function (context) {
    if (context != null) {
        userObjectId = context.userObjectId;
    }
});

//Start : Manage Events

function getSelected() {
    var teams = [];
    //ToDo: Push selected teams in array.
    return teams;
}

function submitForm() {
    var eventInfo = {
        Id:"", // TODO - assign eventId when task module is open in edit mode.
        Type: $('#eventType :selected').val(),
        Title: $('#title').val(),
        Header: $('#header').val(),
        Message: $('#message').val(),
        Date: $('#eventDate').val(),
        ImageURL: $(".carousel-item.active > img").attr("src"),
        Teams: getSelected(),
        OwnerAadObjectId: userObjectId,
    };
    microsoftTeams.tasks.submitTask(eventInfo);
    return true;
}
//End : Manage Events