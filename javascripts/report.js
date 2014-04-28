(function () {
    var tincan = new TinCan (
        {
            recordStores: [
                {
                    endpoint: "https://cloud.scorm.com/tc/public/",
                    username: "test",
                    password: "pass"
                }
            ]
        }
    );
    tincan.sendStatement(
        {
            actor: {
                mbox: "mailto:tincan.net-github@tincanapi.com"
            },
            verb: {
                id: "http://adlnet.gov/expapi/verbs/experienced"
            },
            target: {
                id: "http://rusticisoftware.github.com/TinCan.NET"
            }
        },
        function () {}
    );
}());
