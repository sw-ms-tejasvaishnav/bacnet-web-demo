var totalFloor = null;
$(document).ready(function () {

    StartBacknetProtocol();

    RangeSlider();

    //Dropdown change event.
    $('#ddlfloor').on('change', function () {

        var selectedfloor = $("#ddlfloor").val();
        if (selectedfloor != 0) {
            $("#floorSlidar").removeAttr("disabled", false);
            GetsLutronLightLevel(selectedfloor);
        }
        else {
            $("#floorSlidar").attr("disabled", true);
            SetSlidarValue(0, selectedfloor);
        }
    })


    $("#btnOnLight").click(function () {
        //var isChecked = RadioBtnCheckedOrNot();
        //if (isChecked == true) {
        //    var deviceId = GetDeviceIdByRbnBtnSetColor();
        //    SetLightsSimulator(deviceId);
        //}
        //else {
        //    alert("Please select light.")
        //}
    });

    $('#ddlDevice').on('change', function () {
        var selectedValue = $('#ddlDevice').val();
        if (selectedValue != 0) {
            BindScheduleObject(selectedValue);
        } else {
            $('#ddlObject').html("");
            var ddlObject = $('#ddlObject');
            ddlObject.append('<option value=""> Please Select Object </option>');
        }
    });


    var frmSchedule = $('#frmSchedule').validate(
        {
            rules: {
                ddlDay: {
                    required: true
                },
                time: {
                    required: true
                },
                ddlDevice: {
                    required: true
                },
                ddlObject: {
                    required: true
                },
                presentValue: {
                    required: true
                }
            },
            messages: {
                ddlDay: {
                    required: "Please select Schedule Day."
                },
                time: {
                    required: "Please enter Schedule Time."
                },
                ddlDevice: {
                    required: "Please select Schedule Device."
                },
                ddlObject: {
                    required: "Please select Schedule Object."
                },
                presentValue: {
                    required: "Please enter Schedule Present Value."
                }
            },
            submitHandler: function (form) {
                //
                SaveSchedule();
            }
        });

    $('#btnSchedule').on('click', function () {


    });

});

//Start backnet service.
function StartBacknetProtocol() {
    $.post("api/LutronLightFloor/StartBackNetProtocol", function () {

    }).success(function () {

        BindAllDropDownList();
    });
}

///Fills all dropdown and floor list method.
function BindAllDropDownList() {
    $.when(BindFloorDropDown(), BindWeekDropDown(), BindDevices()).then(function (floorLst, weekLst, devicesLst) {

        var ddlFloor = $('#ddlfloor');
        ddlFloor.append('<option value="' + 0 + '"> Please Select Floor </option>');
        $.each(floorLst[0], function (key, value) {
            ddlFloor.append('<option value="' + value.FloorId + '">' + value.FloorName + '</option>');
        });


        var ddlDay = $('#ddlDay');
        ddlDay.append('<option value=""> Please Select Day </option>');
        $.each(weekLst[0], function (key, value) {
            ddlDay.append('<option value="' + value.DayId + '">' + value.DayName + '</option>');
        });

        var ddlDevices = $('#ddlDevice');
        ddlDevices.append('<option value=""> Please Select Device </option>');
        $.each(devicesLst[0], function (key, value) {
            ddlDevices.append('<option value="' + value.DeviceId + '">' + value.DeviceName + '</option>');
        });

        var ddlObject = $('#ddlObject');
        ddlObject.append('<option value=""> Please Select Object </option>');


        var floorhtml = "";
        var floorLightHtml = "";
        var floorDetail = floorLst[0];
        totalFloor = floorDetail;
        var binaryInput = 1;
        for (var i = 0; i < floorDetail.length; i++) {
            if (i > 0) {
                floorhtml += "&nbsp;";
                floorLightHtml += "&nbsp;";
            }
            floorhtml += "<div class='panel panel-warning col-sm-3 margin-right5'><h4 class='panel-heading bccolorwhite'>"
                + floorDetail[i].FloorName + "</h4>";

            floorLightHtml += "<div class='panel panel-warning col-sm-4 margin-right5'><h4 class='panel-heading bccolorwhite'>"
                + floorDetail[i].FloorName + "</h4>";

            var buttonInstanceHtml = "";
            var instance = 1;
            for (var j = 0; j < floorDetail[i].NoOfInstance; j++) {

                buttonInstanceHtml += "<button type='button' id='btninstance' class='floorLable pointernone wd31px basecolor"
                    + floorDetail[i].FloorId + " m-r-3'>" + instance + "</button>";
                instance++;
            }
            if (buttonInstanceHtml != "") {
                floorhtml += "<div class='panel-body wd142px'>" + buttonInstanceHtml + "</div>";
            }
            if (floorhtml != "") {
                floorhtml += "</div>";
            }

            var lightFloorWiseHtml = "";
            var lightInstance = 1;
            for (var k = 0; k < floorDetail[i].BinaryDetails.length; k++) {
                var currentStatus = floorDetail[i].BinaryDetails[k].LightStatus;
                var deviceId = floorDetail[i].FloorId;
                var instanceId = floorDetail[i].BinaryDetails[k].InstanceId;
                var id = deviceId +""+ instanceId;

                if (currentStatus == false) {
                    lightFloorWiseHtml += "<span><i class='fa fa-lightbulb-o fa-2x clrBlack'  aria-hidden='true' onclick='SetLightsSimulator(" +
                        deviceId + "," + instanceId + ")' id='floorIcon" + id + "'></i></span> ";
                    
                }
                else
                {
                    lightFloorWiseHtml += "<span><i class='fa fa-lightbulb-o fa-2x clryellow'  aria-hidden='true' onclick='SetLightsSimulator(" +
                        deviceId + "," + instanceId + ")' id='floorIcon" + id + "'></i></span> ";
                }
                lightInstance++;
            }
            if (lightFloorWiseHtml != "") {
                floorLightHtml += "<div class='panel-body wd120px'>" + lightFloorWiseHtml + "</div></div>";
            }
            
        }
        $("#floorHtml").html(floorhtml);
        $("#floorlight").html(floorLightHtml);

      //  BindLightsBaseOnFloor();

    });
}

//Gets floor detail with all device instance.
function BindFloorDropDown() {
    return $.get("api/LutronLightFloor/Floor");
}

//Gets week day name.
function BindWeekDropDown() {
    return $.get("api/LutronLightFloor/Weeks");
}

//Gets device list of simulator.
function BindDevices() {
    return $.get("api/LutronLightFloor/Devices");
}

///Gets and set selected range value.
var RangeSlider = function () {
    var slider = $('.range-slider'),
        range = $('.range-slider__range'),
        value = $('.range-slider__value');

    slider.each(function () {

        value.each(function () {
            var value = $(this).prev().attr('value');
            $(this).html(value);

        });

        range.on('input', function () {
            $(this).next(value).html(this.value);
            var selectedfloor = $("#ddlfloor").val();
            $(".basecolor" + selectedfloor).css({ "background-color": "hsl(0," + this.value + "%," + "88%)" })
        });

        range.on("change", function (event, ui) {
            var selectedfloor = $("#ddlfloor").val();
            var selectedValue = this.value;
            SaveBackNetPresantValue(selectedValue, selectedfloor);
        });
    });
};

//Saves Presant value of analog instance presant value of bacnet device according to floor.
function SaveBackNetPresantValue(bacnetvalue, floorId) {
    $.post("api/LutronLightFloor/SaveBackNetPresantValue/" + bacnetvalue + "/" + floorId, function () {

    }).success(function (data) {

    })

}

//Gets analog light level base on floor.
function GetsLutronLightLevel(floorId) {
    $.get("api/LutronLightFloor/GetsLutronLightLevelByFloor/" + floorId, function (lightLevel) {

        var cLightLevel = lightLevel;


    }).success(function (cLightLevel) {


        SetSlidarValue(cLightLevel, floorId);
    })
}

//Sets slidar property according to light level.
function SetSlidarValue(cLightLevel, floorId) {
    if (cLightLevel != 0) {
        $(".basecolor" + floorId).css({ "background-color": "hsl(0," + cLightLevel + "%," + "88%)" })
    }
    else {
        var cFloor = 1;
        for (var i = 0; i < totalFloor.length; i++) {
            $(".basecolor" + cFloor).css({ "background-color": "lightgray" })
            cFloor++;
        }

    }
    $('input[type="range"]').val(cLightLevel).change();
    $(".range-slider__value").html(cLightLevel);
}

//set radiobutton of light according to floor.
function BindLightsBaseOnFloor() {
    if (totalFloor != null) {
        var rbnLightHtml = "";
        for (var i = 0; i < totalFloor.length; i++) {
            if (i > 0) {
                rbnLightHtml += "<br/>";
            }
            rbnLightHtml += "<input type='radio' name='rbnLight' id=rbnLight" + totalFloor[i].FloorId +
                ">&nbsp;&nbsp;&nbsp;Light " + totalFloor[i].FloorId;
        }
        $("#rbnLightHtml").html(rbnLightHtml);

        $('input[type=radio][name=rbnLight]').change(function () {
            var deviceId = GetDeviceIdByRbnBtn();
            GetsCurrentStatusOfLight(deviceId);
        });
    }
}

//Set radio button color base on binary presant value.
function GetDeviceIdByRbnBtnSetColor() {
    var deviceId = null;
    for (var i = 0; i < totalFloor.length; i++) {
        if ($('#rbnLight' + totalFloor[i].FloorId).is(':checked')) {
            $("#floorIcon" + totalFloor[i].FloorId).css('color', 'yellow');
            $("#floorLight" + totalFloor[i].FloorId).css('background-color', 'darkgreen');
            return deviceId = totalFloor[i].FloorId;
        }
        //else {
        //    $("#floorLight" + totalFloor[i].FloorId).css('background-color', 'gray');
        //    $("#floorIcon" + totalFloor[i].FloorId).css('color', 'white');
        //}
    }
    return deviceId;
}

//gets device id base on check radio button.
function GetDeviceIdByRbnBtn() {
    var deviceId = null;
    for (var i = 0; i < totalFloor.length; i++) {
        if ($('#rbnLight' + totalFloor[i].FloorId).is(':checked')) {
            return deviceId = totalFloor[i].FloorId;
        }
    }
    return deviceId;
}

//Check radio button is checked or not.
function RadioBtnCheckedOrNot() {
    for (var i = 0; i < totalFloor.length; i++) {
        if ($('#rbnLight' + totalFloor[i].FloorId).is(':checked')) {
            return true;
        }
    }
    return false;
}

//Sets binary presant value in simulator.
function SetLightsSimulator(deviceId, instanceId) {
    $.post("api/LutronLightFloor/SetLightsSimulator/" + deviceId + "/" + instanceId, function (data) {
        var isTrue = data;
    }).success(function (isTrue) {
        SetOnOffTextHtml(isTrue,deviceId, instanceId);
    });
}


//Gets current presant value of simulator base on device id.
function GetsCurrentStatusOfLight(deviceId) {

    $.get("api/LutronLightFloor/CurrentBinaryPresantValue/" + deviceId, function (data) {

        var isTrue = data;

    }).success(function (isTrue) {
        SetOnOffTextHtml(isTrue, deviceId);
    })
}

//sets on and off base on simulator presant value status.
function SetOnOffTextHtml(isTrue, deviceId, instanceId) {
    var id = deviceId + "" + instanceId;
    if (isTrue == true) {
        $("#floorIcon" + id).removeClass('clrBlack');
        $("#floorIcon" + id).addClass('clryellow');

        //$("#floorLight" + instanceId).css('background-color', 'darkgreen');
     //   $("#btnOnLight").html("Off");
    }
    else if (isTrue == false) {
        //    $("#floorLight" + deviceId).css('background-color', 'gray');
        $("#floorIcon" + id).removeClass('clryellow');
       // $("#floorIcon" + id).addClass('clrBlack');
      
      //  $("#btnOnLight").html("On");
    }
}

//Gets chedule object type base on selected device.
function BindScheduleObject(deviceId) {
    $.get("api/LutronLightFloor/ScheduleObjects/" + deviceId, function (data) {
        var scheduleObjList = data;
    }).success(function (scheduleObjList) {
        $('#ddlObject').html("");

        var ddlObject = $('#ddlObject');
        ddlObject.append('<option value=""> Please Select Object </option>');
        $.each(scheduleObjList, function (key, value) {
            ddlObject.append('<option value="' + value.ObjectInstance + '">' + value.ObjectName + '</option>');
        });
    });
}

//Save schedule detail.
function SaveSchedule() {
    var selectedDay = $("#ddlDay").val();
    var selectedTime = $("#textTime").val();
    var selectedDevice = $("#ddlDevice").val();
    var selectedScheduleObj = $("#ddlObject").val();
    var presentValue = $("#presentValue").val();

    var objSchedule = {
        SelectedDay: selectedDay,
        SelectedTime: selectedTime,
        DeviceId: selectedDevice,
        ScheduleObjectid: selectedScheduleObj,
        SchedulePresantValue: presentValue
    }

    $.post("api/LutronLightFloor/SaveSchedule", objSchedule, function (data) {

    }).success(function () {
        ClearScheduleForm();
    });
}

function ClearScheduleForm() {
    $("#ddlDay").val("");
    $("#textTime").val("");
    $("#ddlDevice").val("");
    $("#ddlObject").val("");
    $("#presentValue").val("");
}