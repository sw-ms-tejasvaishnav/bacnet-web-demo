var totalFloor = null;
var gscheduleId = null;
var schedulObjId = 0;
var sInstanceId = null;
var oTable = null;
var scheduleDTable = null;
var isModalPopUp = false;
var currentDeviceIdForWeeklySchedule = 0;
var frmNewSchedule;
var tblCall = 0;
$(document).ready(function () {
    $('#scheduleStartDate').datetimepicker({
        format: 'MM-DD-YYYY'
    });
    $('#scheduleEndDate').datetimepicker({
        format: 'MM-DD-YYYY'
    });



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
            submitHandler: function (form) {               //

                SaveSchedule();

            }
        });

    $('#scheduleStartDate').on('change', function (e) { console.log(e.date); })
    $('#btnReset').on('click', function () {

        schedulObjId = 0;
        sInstanceId = null;
        frmSchedule.resetForm();
    });

    $('#btnNewSchedule').on('click', function () {

        $('#frmNewSchedule').submit();
        return false;

    });

    $('#btnNewReset').on('click', function () {

        schedulObjId = 0;
        sInstanceId = null;
        frmNewSchedule.resetForm();
        $("#addNewSchedule").modal('hide');
        currentDeviceIdForWeeklySchedule = 0;
        isModalPopUp = false;
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
    $.when(BindFloorDropDown(), BindWeekDropDown(), BindDevices(), BindAllSchdule()).then(function (floorLst, weekLst, devicesLst, scheduleLst) {

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
                var id = deviceId + "" + instanceId;

                if (currentStatus == false) {
                    lightFloorWiseHtml += "<span><i class='fa fa-lightbulb-o fa-2x clrBlack'  aria-hidden='true' onclick='SetLightsSimulator(" +
                        deviceId + "," + instanceId + ")' id='floorIcon" + id + "'></i></span> ";

                }
                else {
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


        BindScheduleTable(scheduleLst[0]);
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

//bind all schedule of new added in simulator.
function BindAllSchdule() {
    return $.get("api/LutronLightFloor/GetScheduleList");
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
        SetOnOffTextHtml(isTrue, deviceId, instanceId);
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
        var ddlobject = "";
        if (isModalPopUp == false) {
            ddlObject = $("#ddlObject");
        } else {
            ddlObject = $("#ddlNewObject");
        }
        ddlObject.html("");

        // var ddlObject = $('#ddlObject');
        ddlObject.append('<option value=""> Please Select Object </option>');
        $.each(scheduleObjList, function (key, value) {
            ddlObject.append('<option value="' + value.ObjectInstance + '">' + value.ObjectName + '</option>');
        });

        if (schedulObjId != 0) {
            ddlObject.val(schedulObjId);
        }
    });
}

//Save schedule detail.
function SaveSchedule() {
    debugger;
    var selectedDay = $("#ddlDay").val();
    var selectedTime = $("#textTime").val();
    var selectedDevice = $("#ddlDevice").val();
    var selectedScheduleObj = $("#ddlObject").val();
    var presentValue = $("#presentValue").val();
    var scheduleStartDate = $("#scheduleStartDate").val();
    var scheduleEndDate = $("#scheduleEndDate").val();

    var objSchedule = {
        SelectedDayId: selectedDay,
        SelectedTime: selectedTime,
        DeviceId: selectedDevice,
        ScheduleId: selectedScheduleObj,
        PresentValue: presentValue,
        ScheduleDetailId: gscheduleId,
        InstanceId: sInstanceId,
        d1: scheduleStartDate,
        d2: scheduleEndDate
    }

    $.post("api/LutronLightFloor/SaveSchedule", objSchedule, function (data) {

    }).success(function () {
        ClearScheduleForm();
        BindSchduleLstSave();
    });
}

//Bind data table of schedule on add new schedule. 
function BindSchduleLstSave() {
    $.get("api/LutronLightFloor/GetScheduleList", function (scheduleLst) {
        var schedule = scheduleLst;
    }).success(function (schedule) {
        BindScheduleTable(schedule);
    });
}

//sclear schedule form.
function ClearScheduleForm() {
    $("#ddlDay").val("");
    $("#textTime").val("");
    $("#ddlDevice").val("");
    $("#ddlObject").val("");
    $("#presentValue").val("");
    $("#scheduleEndDate").val("");
    $("#scheduleStartDate").val("");
}

//Clear model popup for adding weekly schedule.
function ClearModalPopWeeklySchedule() {
    $("#ddlNewDay").val("");
    $("#textNewTime").val("");
    $("#ddlNewDevice").val("");
    $("#ddlNewObject").val("");
    $("#presentNewValue").val("");

    frmNewSchedule.resetForm();
}

//Data table for schedule.
function BindScheduleTable(scheduleLst) {

    if (scheduleLst.length > 0) {
        if (oTable != null) {
            oTable.clear().destroy();
        }
        $("#scheduleListShow").show();
        $("#norecords").hide();
        oTable = $('#scheduleRecords').DataTable({
            "data": scheduleLst,
            "columns": [
                {
                    "className": 'details-control', "orderable": false, "defaultContent": ''

                },
                {
                    "title": "Device", "data": "DeviceName", "sort": true, "Width": "5%"
                },
                {
                    "title": "Name", "data": "ScheduleName", "sort": true, "Width": "8%"
                },
                {
                    "title": "End Date", "data": "ScheduleStartDate", "sort": true, "Width": "8%", "render": function (data, type, row) {
                        return GetDay(data);
                    }
                },
                {
                    "title": "Start Date", "data": "ScheduleEndDate", "sort": true, "Width": "8%", "render": function (data, type, row) {
                        return GetDay(data);
                    }

                },
                {
                    "title": "Action", "data": "", "sort": true, "Width": "5%", "render": function (data, type, row) {

                        return '<button class="btn btn-sm" onclick=GetsScheduleInfo(' + row.DeviceId + "," + row.InstanceId + ')>Add Schedule</button>';

                    }
                }


                //{
                //    "title": "Time", "data": "SelectedTime", "sort": true, "width": "10% ", "render": function (date) {
                //        

                //        return GetTime(date);
                //    }
                //},
                //{
                //    "title": "Value", "data": "PresentValue", "width": "15%", "className": "txtalignright",
                //}
            ],
            "initComplete": function (settings, json) {

                $('#scheduleRecords tbody').on('click', 'td.details-control', function (event) {


                    var tr = $(this).closest('tr');
                    var row = oTable.row(tr);

                    if (row.child.isShown()) {
                        // This row is already open - close it
                        row.child.hide();
                        tr.removeClass('shown');
                        event.stopImmediatePropagation();
                    }
                    else {
                        // Open this row
                        var data = format(row.data(), row, tr);
                        row.child(data).show();
                        tr.addClass('shown');
                        event.stopImmediatePropagation();

                    }

                });

            }

        })
    } else {
        $("#scheduleListShow").hide();
        $("#norecords").show();
    }


}

//gets only time from date.
function GetTime(date) {
    var dt = new Date(date);
    var time = dt.getHours() + ":" + dt.getMinutes() + ":" + dt.getSeconds();
    return time;
}

//Gets date in format dd/mm/yyyy
function GetDay(date) {

    var dt = new Date(date);
    var date = dt.getDay() + "/" + dt.getMonth() + "/" + dt.getFullYear();
    return date;
}

//Gets schedule info base on device if and instance id.
function GetsScheduleInfo(deviceId, instanceId) {
    $.get("api/LutronLightFloor/GetsScheduleInfo/" + deviceId + "/" + instanceId, function (scheduleInfo) {

    }).success(function (scheduleInfo) {
        BindScheduleInfo(scheduleInfo);
    });
}

//Fills data in schedule form.
function BindScheduleInfo(scheduleInfo) {

    //$("#ddlDay").val(scheduleInfo.SelectedDay);
    //var dt = new Date(scheduleInfo.SelectedTime);
    //var time = dt.getHours() + ":" + (dt.getMinutes() < 10 ? ("0" + dt.getMinutes()) : dt.getMinutes()) + ":" +
    //    (dt.getSeconds() < 10 ? ("0" + dt.getSeconds()) : dt.getSeconds());
    //$("#textTime").val(time);
    //$("#ddlDevice").val(scheduleInfo.DeviceId);
    //$("#ddlDevice").change();
    //schedulObjId = scheduleInfo.ScheduleId;
    //$("#presentValue").val(scheduleInfo.PresentValue);

    BindModalPopUpDropDownList(scheduleInfo);
}

//gets weekly schedule near table row.
function format(data, row, tr) {
    // `d` is the original data object for the row
    var table = BindScheduleDaysTable(data.WeeklySchedule);

    return table
}

//Bind schedule days by schedule and append child table in perticular table row.
function BindScheduleDaysTable(scheduleDaysLst) {
    var dayhtml = "";
    if (scheduleDaysLst.length > 0) {
        dayhtml = '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">';
    }
    var currentOrder = 1;
    for (var i = 0; i < scheduleDaysLst.length; i++) {
        dayhtml +=
            '<tr><td>' + currentOrder + '</td>' +
            '<th>Day:</th>' + '<td>' + scheduleDaysLst[i].DayName + '</td>' +
            '<th>Time:</th>' + '<td>' + GetTime(scheduleDaysLst[i].SelectedTime) + '</td>' +
            '<th>Value:</th>' + '<td>' + scheduleDaysLst[i].PresentValue + '</td></tr>'
        currentOrder++;
    }
    if (dayhtml != "") {
        dayhtml += '</table>';
    }
    else {
        dayhtml = '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">' +
            '<tr><td>No schedule.</td></tr></table>'
    }
    return dayhtml;
}


//Validate modal pop up for weekly schedule.
function ValidateFormModelPopup() {
    frmNewSchedule = $('#frmNewSchedule').validate(
        {
            rules: {
                ddlNewDay: {
                    required: true
                },
                timeNew: {
                    required: true
                },
                ddlNewObject: {
                    required: true
                },
                presentNewValue: {
                    required: true
                },
                scheduleStartDate: {
                    required: true,
                    date: true
                },
                scheduleEndDate: {
                    required: true,
                    date: true
                }
            },
            messages: {
                ddlNewDay: {
                    required: "Please select Schedule Day."
                },
                timeNew: {
                    required: "Please enter Schedule Time."
                },
                ddlNewObject: {
                    required: "Please select Schedule Object."
                },
                presentNewValue: {
                    required: "Please enter Schedule Present Value."
                },
                scheduleStartDate: {
                    required: "Please enter Schedule Start Value."
                },
                scheduleEndDate: {
                    required: "Please enter Schedule End Value."
                }
            },
            submitHandler: function (form) {               //

                AddNewSchedule();

            }
        });
}

//Add new weekly schedule on existing schedule.
function AddNewSchedule() {
    var selectedDay = $("#ddlNewDay").val();
    var selectedTime = $("#textNewTime").val();
    var selectedScheduleObj = $("#ddlNewObject").val();
    var presentValue = $("#presentNewValue").val();

    var objSchedule = {
        SelectedDayId: selectedDay,
        SelectedTime: selectedTime,
        DeviceId: currentDeviceIdForWeeklySchedule,
        ScheduleId: selectedScheduleObj,
        PresentValue: presentValue,
        ScheduleDetailId: gscheduleId,
        InstanceId: sInstanceId
    }

    $.post("api/LutronLightFloor/SaveSchedule", objSchedule, function (data) {

    }).success(function () {
        ClearModalPopWeeklySchedule();
        isModalPopUp = false;
        gscheduleId = 0;
        sInstanceId = 0;
        $("#addNewSchedule").modal('hide');
        BindSchduleLstSave();
    });
}

//Binds all dropdown of weekly schedule modal.
function BindModalPopUpDropDownList(scheduleInfo) {
    $.when(BindWeekDropDown()).then(function (weekLst) {

        gscheduleId = scheduleInfo.ScheduleDetailId;
        sInstanceId = scheduleInfo.InstanceId;
        currentDeviceIdForWeeklySchedule = scheduleInfo.DeviceId;
        $('#ddlNewDay').empty();
        $('#ddlNewObject').empty();


        var ddlDay = $('#ddlNewDay');
        ddlDay.append('<option value=""> Please Select Day </option>');
        $.each(weekLst, function (key, value) {
            ddlDay.append('<option value="' + value.DayId + '">' + value.DayName + '</option>');
        });

        $("#addNewSchedule").modal('show');
        $('#addNewSchedule').on('shown.bs.modal', function (e) {

            ValidateFormModelPopup();
            ClearModalPopWeeklySchedule();
            ObjectDeviceWise(scheduleInfo.DeviceId);
            e.stopPropagation();

        })
        isModalPopUp = true;

    });
}


function ObjectDeviceWise(deviceId) {

    if (deviceId != 0) {
        BindScheduleObject(deviceId);
    } else {
        $('#ddlNewObject').html("");
        var ddlObject = $('#ddlNewObject');
        ddlObject.append('<option value=""> Please Select Object </option>');
    }
}