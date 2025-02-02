using UnityEngine;
using System.Collections;
using System.Text;
using System;

public class PeriPageBLE : MonoBehaviour
{
    private const string ServiceUUID = "6e400001-b5a3-f393-e0a9-e50e24dcca9e"; // PeriPage BLE Service
    private const string WriteUUID = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";   // Write Characteristic
    private string targetDeviceName = "PeriPage_A6"; // Change this to your printer model
    private string connectedDeviceID = "";
    private bool isConnected = false;

    void Start()
    {
        // Request Bluetooth permissions on Android
        if (Application.platform == RuntimePlatform.Android)
        {
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN"))
                UnityEngine.Android.Permission.RequestUserPermission("android.permission.BLUETOOTH_SCAN");

            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
                UnityEngine.Android.Permission.RequestUserPermission("android.permission.BLUETOOTH_CONNECT");
        }

        // Initialize Bluetooth
        BluetoothLEHardwareInterface.Initialize(true, false, () =>
        {
            Debug.Log("BLE Initialized.");
            ScanForPrinters();
        },
        (error) =>
        {
            Debug.LogError("BLE Error: " + error);
        });
    }

    private void ScanForPrinters()
    {
        Debug.Log("Scanning for PeriPage printer...");
        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (deviceID, deviceName) =>
        {
            if (deviceName.Contains(targetDeviceName))
            {
                Debug.Log("Found PeriPage Printer: " + deviceName);
                BluetoothLEHardwareInterface.StopScan();
                ConnectToPrinter(deviceID);
            }
        });
    }

    private void ConnectToPrinter(string deviceID)
    {
        Debug.Log("Connecting to PeriPage Printer...");
        BluetoothLEHardwareInterface.ConnectToPeripheral(deviceID, (id) =>
        {
            connectedDeviceID = id;
            isConnected = true;
            Debug.Log("Connected to " + targetDeviceName);
            InitializePrinter();
        },
        null,
        (id, characteristicUUID, bytes) =>
        {
            Debug.Log("Data received: " + bytes);
        },
        (id) =>
        {
            Debug.Log("Disconnected.");
            isConnected = false;
        });
    }

    private void InitializePrinter()
    {
        byte[] initCommand = { 0x1F, 0x10, 0x04, 0x00 }; // PeriPage Initialization
        SendPrintData(initCommand);
    }

    public void PrintText(string text)
    {
        if (!isConnected) return;

        byte[] textData = Encoding.UTF8.GetBytes(text + "\n");
        SendPrintData(textData);
    }

    private void SendPrintData(byte[] data)
    {
        if (!isConnected) return;

        BluetoothLEHardwareInterface.WriteCharacteristic(connectedDeviceID, ServiceUUID, WriteUUID, data, data.Length, true, (characteristicUUID) =>
        {
            Debug.Log("Sent data to printer.");
        });
    }

    public void PrintImage(Texture2D image)
    {
        byte[] imageData = ImageConverter.ConvertToPeriPageFormat(image);
        SendPrintData(imageData);
    }

    public void SendChunkedData(byte[] data)
    {
        int chunkSize = 244;
        for (int i = 0; i < data.Length; i += chunkSize)
        {
            int remaining = Mathf.Min(chunkSize, data.Length - i);
            byte[] chunk = new byte[remaining];
            System.Array.Copy(data, i, chunk, 0, remaining);
            SendPrintData(chunk);
        }
    }
}
