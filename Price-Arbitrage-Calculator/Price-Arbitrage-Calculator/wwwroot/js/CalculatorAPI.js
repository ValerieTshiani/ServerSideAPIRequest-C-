"use strict";

var Calculator = {
    GetValues: function () {
        document.getElementById("bitcoin").innerHTML = "<div class='loader'></div>";
        document.getElementById("xrp").innerHTML = "<div class='loader'></div>";

        $.ajax({
            url: "/Home/GetValues",
            type: "POST",
            data: "",//No data being sent 
            dataType: "json"
        })
            .done(function (data) {
                return Calculator.DisplayValues(data);
            })
            .fail(function (xhr, textStatus, errorThrown) {
                console.log("xhr object: ", xhr);
                console.log(textStatus);
                console.log(errorThrown);
                let data = { err: errorThrown, textStatus: textStatus, xhr: xhr };
                return Calculator.DisplayValues(data);
            });
    },
    DisplayValues: function (data) {

        if (data[0].error == null) 
            document.getElementById("bitcoin").innerHTML = data[0].data.bitCoinArbitrageValue;
        else
            document.getElementById("bitcoin").innerHTML = data[0].error;

        if (data[1].error == null)
            document.getElementById("xrp").innerHTML = data[1].data.xrpArbitrageValue;
        else
            document.getElementById("xrp").innerHTML =  data[1].error ;

    }
}