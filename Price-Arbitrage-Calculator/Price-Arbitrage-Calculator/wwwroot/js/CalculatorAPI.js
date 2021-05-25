"use strict";

var Calculator = {
    GetValues: function () {
        console.log("Step1");
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
        console.log(data);
      
        document.getElementById("bitcoin").innerHTML = data.bitCoinArbitrageValue;
        document.getElementById("xrp").innerHTML = data.xrpArbitrageValue;
    }
}