/*
using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Text;

public class CatPrinterManager : MonoBehaviour
{
    // ---------------------------------------------------------------------
    // Service and Characteristic UUIDs (example from catprinter Python code).
    // The device may advertise AE30 or AF30 service. We'll look for both.
    // TX = AE01 or AF01, RX = AE02 or AF02.
    // ---------------------------------------------------------------------
    private static readonly string[] ServiceUUIDs =
    {
        "0000ae30-0000-1000-8000-00805f9b34fb", // AE30
        "0000af30-0000-1000-8000-00805f9b34fb"  // AF30
    };

    // We'll store possible TX/RX. This example uses AE01/AE02 if we detect AE30.
    // If you have an AF30 device, it might use AF01/AF02. Adjust as needed.
    private const string TX_CHARACTERISTIC_AE = "0000ae01-0000-1000-8000-00805f9b34fb";
    private const string RX_CHARACTERISTIC_AE = "0000ae02-0000-1000-8000-00805f9b34fb";
    private const string TX_CHARACTERISTIC_AF = "0000af01-0000-1000-8000-00805f9b34fb";
    private const string RX_CHARACTERISTIC_AF = "0000af02-0000-1000-8000-00805f9b34fb";

    // The "printer ready" notification from Python catprinter
    private static readonly byte[] PRINTER_READY_NOTIFICATION = new byte[]
    {
        0x51, 0x78, 0xae, 0x01, 0x01, 0x00, 0x00, 0x00, 0xff
    };

    // We'll guess 20 bytes if we haven't negotiated a larger MTU
    private const int DEFAULT_CHUNK_SIZE = 20;
    // Wait 20ms between chunks (Python code uses 0.02s)
    private const float WAIT_AFTER_EACH_CHUNK_S = 0.02f;
    // Wait up to 30s for the printer to send its done/ready notification
    private const float WAIT_FOR_PRINTER_DONE_TIMEOUT = 30f;

    // Scan timeout
    private const float SCAN_TIMEOUT_S = 10f;

    // Internal fields
    private bool _initialized = false;
    private bool _scanning = false;
    private bool _connected = false;

    private string _deviceAddress;
    private string _currentServiceUUID;        // which service (AE30 or AF30) we found
    private string _currentTXCharacteristic;   // AE01 or AF01
    private string _currentRXCharacteristic;   // AE02 or AF02

    // If we want to wait for the "done" notification
    private bool _printerReady = false;

    void Start()
    {
        // Initialize the plugin
        BluetoothLEHardwareInterface.Initialize(
            enableAdapterPopup: true,
            asCentral: false,
            onInitialized: () =>
            {
                Debug.Log("BLE Initialized successfully.");
                _initialized = true;

                // Optionally start scanning automatically here
                StartScan();
            },
            onError: (error) =>
            {
                Debug.LogError("BLE init error: " + error);
            }
        );
    }

    #region SCAN & PARSE
    public void StartScan()
    {
        if (!_initialized)
        {
            Debug.LogWarning("BLE is not initialized, cannot scan.");
            return;
        }

        if (_scanning)
        {
            Debug.LogWarning("Already scanning.");
            return;
        }

        Debug.Log("Starting BLE scan with no filter; we'll parse advData ourselves...");
        _scanning = true;

        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
            null, 
            (deviceAddress, deviceName) =>
            {
                // Basic callback
                Debug.Log($"[Basic Callback] Found device: {deviceName} @ {deviceAddress}");
            },
            (deviceAddress, deviceName, rssi, advData) =>
            {
                // Extended callback with raw advertisement
                ParseAdvertisement(deviceAddress, deviceName, advData);
            },
            isRssiOnly: false
        );

        // Optionally stop scanning after SCAN_TIMEOUT_S
        StartCoroutine(StopScanAfterTime(SCAN_TIMEOUT_S));
    }

    private IEnumerator StopScanAfterTime(float secs)
    {
        yield return new WaitForSeconds(secs);
        if (_scanning)
        {
            Debug.LogWarning("Scan timed out. Stopping scan.");
            BluetoothLEHardwareInterface.StopScan();
            _scanning = false;
        }
    }

    private void ParseAdvertisement(string address, string name, byte[] advData)
    {
        if (advData == null || advData.Length == 0) return;

        int index = 0;
        while (index < advData.Length)
        {
            byte length = advData[index];
            if (length == 0) break; // no more structures
            index += 1;
            if (index + length > advData.Length) break;

            byte adType = advData[index];
            index += 1;

            byte[] data = new byte[length - 1];
            Array.Copy(advData, index, data, 0, length - 1);
            index += (length - 1);

            // Check for 16-bit or 128-bit service UUID AD types
            if (adType == 0x02 || adType == 0x03) // 16-bit services
            {
                for (int i = 0; i < data.Length; i += 2)
                {
                    if (i + 1 >= data.Length) break;
                    ushort uuid16 = (ushort)(data[i] | (data[i + 1] << 8));
                    // e.g. AE30 => 0xAE30
                    string hex16 = uuid16.ToString("x4"); 
                    // Build the full 128-bit
                    string full128 = "0000" + hex16 + "-0000-1000-8000-00805f9b34fb";
                    full128 = full128.ToLower();

                    if (ServiceUUIDs.Contains(full128))
                    {
                        OnFoundCatPrinter(address, name, full128);
                        return;
                    }
                }
            }
            else if (adType == 0x06 || adType == 0x07) // 128-bit services
            {
                for (int i = 0; i < data.Length; i += 16)
                {
                    if (i + 15 >= data.Length) break;
                    byte[] uuidBytes = new byte[16];
                    Array.Copy(data, i, uuidBytes, 0, 16);

                    // reverse from little-endian
                    Array.Reverse(uuidBytes);
                    string uuidStr = BitConverter.ToString(uuidBytes).Replace("-", "").ToLower();

                    // insert dashes
                    string formatted = uuidStr.Substring(0,8) + "-" +
                                       uuidStr.Substring(8,4) + "-" +
                                       uuidStr.Substring(12,4) + "-" +
                                       uuidStr.Substring(16,4) + "-" +
                                       uuidStr.Substring(20);

                    if (ServiceUUIDs.Contains(formatted))
                    {
                        OnFoundCatPrinter(address, name, formatted);
                        return;
                    }
                }
            }
        }
    }

    private void OnFoundCatPrinter(string address, string deviceName, string serviceUUID)
    {
        Debug.Log($"Found catprinter {deviceName} ({serviceUUID}) at {address}. Stopping scan...");
        if (_scanning)
        {
            BluetoothLEHardwareInterface.StopScan();
            _scanning = false;
        }

        _currentServiceUUID = serviceUUID; 
        Connect(address);
    }
    #endregion

    #region CONNECT & SUBSCRIBE
    private void Connect(string address)
    {
        Debug.Log("Connecting to: " + address);
        _deviceAddress = address;

        BluetoothLEHardwareInterface.ConnectToPeripheral(
            address,
            (connectedAddr) => {
                Debug.Log($"Connected to {connectedAddr}");
                _connected = true;
            },
            (connectedAddr, serviceUUID) => {
                Debug.Log($"Service discovered: {serviceUUID}");
            },
            (connectedAddr, serviceUUID, characteristicUUID) => {
                Debug.Log($"Characteristic discovered: {characteristicUUID}");

                // If we detect it's the RX char, subscribe for notifications
                if (IsRXCharacteristic(serviceUUID, characteristicUUID))
                {
                    BluetoothLEHardwareInterface.SubscribeCharacteristic(
                        _deviceAddress,
                        serviceUUID,
                        characteristicUUID,
                        (notifyAddr, notifyChar) => {
                            Debug.Log("Subscribed to RX notifications.");
                        },
                        (notifyAddr, notifyChar, bytes) => {
                            OnPrinterNotification(bytes);
                        }
                    );
                }
            },
            (disconnectedAddr) => {
                Debug.LogWarning($"Disconnected from {disconnectedAddr}");
                _connected = false;
            }
        );
    }

    private bool IsRXCharacteristic(string serviceUUID, string characteristicUUID)
    {
        // Compare lowercased strings
        string s = serviceUUID.ToLower();
        string c = characteristicUUID.ToLower();

        // If we discovered AF30 service, we expect AF02 for RX. If AE30, AE02 for RX.
        // Or you can store them in _currentServiceUUID and match accordingly.
        if (s == "0000af30-0000-1000-8000-00805f9b34fb" && c == TX_CHARACTERISTIC_AF.Replace("af01", "af02"))
        {
            return true;
        }
        if (s == "0000ae30-0000-1000-8000-00805f9b34fb" && c == RX_CHARACTERISTIC_AE)
        {
            return true;
        }
        // Adjust logic if your device uses different combos
        return false;
    }

    private bool IsTXCharacteristicAvailable()
    {
        // We'll set it up once we know which service we found
        return !string.IsNullOrEmpty(_currentTXCharacteristic);
    }

    private string GetTXCharacteristicFromService()
    {
        // If we found AE30, we use AE01 for TX
        // If we found AF30, we use AF01 for TX
        // Adjust if your device is different
        string s = _currentServiceUUID?.ToLower();
        if (s == "0000ae30-0000-1000-8000-00805f9b34fb") return TX_CHARACTERISTIC_AE;
        if (s == "0000af30-0000-1000-8000-00805f9b34fb") return TX_CHARACTERISTIC_AF;
        return "";
    }

    private void OnPrinterNotification(byte[] data)
    {
        Debug.Log($"RX Notification: {BitConverter.ToString(data)}");
        // Compare with PRINTER_READY_NOTIFICATION
        if (data.Length == PRINTER_READY_NOTIFICATION.Length)
        {
            bool match = true;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != PRINTER_READY_NOTIFICATION[i])
                {
                    match = false;
                    break;
                }
            }
            if (match)
            {
                Debug.Log("Printer signaled done/ready!");
                _printerReady = true;
            }
        }
    }
    #endregion

    #region PUBLIC PRINT FUNCTIONS

    /// <summary>
    /// Public method to print text. Internally calls PrintData with ASCII bytes + newline.
    /// </summary>
    public void PrintText(string text)
    {
        if (!_connected)
        {
            Debug.LogWarning("Not connected to catprinter. Please scan/connect first.");
            return;
        }

        // Append a newline if desired
        byte[] data = Encoding.ASCII.GetBytes(text + "\n");
        StartCoroutine(PrintDataCoroutine(data));
    }

    /// <summary>
    /// Public method to send arbitrary byte data in chunks, then wait up to WAIT_FOR_PRINTER_DONE_TIMEOUT for a "ready" notification.
    /// </summary>
    public void PrintData(byte[] data)
    {
        if (!_connected)
        {
            Debug.LogWarning("Not connected to catprinter.");
            return;
        }

        // Start a coroutine for chunked writes + wait
        StartCoroutine(PrintDataCoroutine(data));
    }

    private IEnumerator PrintDataCoroutine(byte[] data)
    {
        // We'll reset the printerReady flag each time we start a print job
        _printerReady = false;

        if (string.IsNullOrEmpty(_currentServiceUUID))
        {
            Debug.LogError("No known service for TX. Can't print.");
            yield break;
        }

        // Check or retrieve the TX characteristic
        string txChar = GetTXCharacteristicFromService();
        if (string.IsNullOrEmpty(txChar))
        {
            Debug.LogError("Could not determine TX characteristic. Can't send data.");
            yield break;
        }

        Debug.Log($"Sending {data.Length} bytes in chunked form to {txChar}...");

        // We'll assume 20-byte chunk size unless you negotiated a bigger MTU.
        int chunkSize = DEFAULT_CHUNK_SIZE;

        for (int i = 0; i < data.Length; i += chunkSize)
        {
            int length = Mathf.Min(chunkSize, data.Length - i);
            byte[] chunk = new byte[length];
            Array.Copy(data, i, chunk, 0, length);

            // Write chunk
            BluetoothLEHardwareInterface.WriteCharacteristic(
                _deviceAddress,
                _currentServiceUUID,
                txChar,
                chunk,
                length,
                withResponse: false,
                (charUUID) => {
                    // local success callback
                }
            );

            yield return new WaitForSeconds(WAIT_AFTER_EACH_CHUNK_S);
        }

        // Now wait up to WAIT_FOR_PRINTER_DONE_TIMEOUT for the "ready" notification
        float startTime = Time.time;
        Debug.Log("Waiting for printer to signal done...");
        while (!_printerReady && Time.time - startTime < WAIT_FOR_PRINTER_DONE_TIMEOUT)
        {
            yield return null;
        }

        if (_printerReady)
        {
            Debug.Log("Printer signaled ready. Done printing!");
        }
        else
        {
            Debug.LogWarning("Timed out waiting for printer-ready notification.");
        }

        // If you want to disconnect automatically, you can do it here
        // Disconnect();
    }

    #endregion

    // Optional: a Disconnect method
    private void Disconnect()
    {
        if (!string.IsNullOrEmpty(_deviceAddress))
        {
            BluetoothLEHardwareInterface.DisconnectPeripheral(_deviceAddress, (disconnectedAddress) =>
            {
                Debug.Log("Disconnected from " + disconnectedAddress);
                _connected = false;
                _deviceAddress = null;
                _currentServiceUUID = null;
                _printerReady = false;
            });
        }
    }
}
*/