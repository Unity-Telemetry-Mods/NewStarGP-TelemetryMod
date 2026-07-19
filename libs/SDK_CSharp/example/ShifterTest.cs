/*************************************************************************************
 * This file is an example code on how to use MOZA Shifter device-related interfaces with MOZA SDK C#
 *
 * For other APIs in MOZA SDK, please refer to the example code file `sdk_api_test.cc` in the MOZA SDK C++ version.
 * You can also refer to the API documentation of the C++ version, located at `docsEng/index.html`.
 * The API in the MOZA SDK C# version is almost identical to the C++ version in terms of function names and input parameters.
*************************************************************************************/

using mozaAPI;
using static mozaAPI.mozaAPI;

MozaShifterTest();
Console.WriteLine("Program finished.");
return;

void MozaShifterTest()
{
    var devices = EnumShifterDevices(out var error);
    if (error != ERRORCODE.NORMAL || devices.Count == 0)
    {
        Console.WriteLine($"No MOZA Shifter device found, error = {error}");
        return;
    }

    var device = devices[0];
    if (!device.Open())
    {
        Console.WriteLine("Device open failed.");
        return;
    }

    Console.WriteLine($"MOZA Shifter device '{device.Path}' is opened.");

    var gear = 0;
    while (device.IsConnected)
    {
        // The `GetCurrentGear` function waits for the HID report while reading data, until valid data is received or an error occurs.
        // This means the execution time of this function may be relatively long, and it is not recommended to call it in the main thread.
        var currentGear = device.GetCurrentGear();
        if (gear == currentGear) continue;
        Console.WriteLine($"The gear has been switched from {gear} to {currentGear}");
        gear = currentGear;
    }

    Console.WriteLine("The device has been disconnected.");
}