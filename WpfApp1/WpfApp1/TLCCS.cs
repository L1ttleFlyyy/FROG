namespace Thorlabs.ccs.interop64
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public class TLCCS : IDisposable
    {
        private bool _disposed;
        private HandleRef _handle;

        public TLCCS(IntPtr Instrument_Handle)
        {
            this._disposed = true;
            this._handle = new HandleRef(this, Instrument_Handle);
            this._disposed = false;
        }

        public TLCCS(string Resource_Name, bool ID_Query, bool Reset_Device)
        {
            IntPtr ptr;
            this._disposed = true;
            int status = PInvoke.init(Resource_Name, Convert.ToUInt16(ID_Query), Convert.ToUInt16(Reset_Device), out ptr);
            this._handle = new HandleRef(this, ptr);
            PInvoke.TestForError(this._handle, status);
            this._disposed = false;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                PInvoke.close(this._handle);
                this._handle = new HandleRef(null, IntPtr.Zero);
            }
            this._disposed = true;
        }

        public int errorQuery(out int Error_Number, StringBuilder Error_Message)
        {
            int status = PInvoke.errorQuery(this._handle, out Error_Number, Error_Message);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        ~TLCCS()
        {
            this.Dispose(false);
        }

        public int getAmplitudeData(double[] Amplitude_Correction_Factors, int Buffer_Start, int Buffer_Length, int Mode)
        {
            int status = PInvoke.getAmplitudeData(this._handle, Amplitude_Correction_Factors, Buffer_Start, Buffer_Length, Mode);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int getDeviceStatus(out int Device_Status)
        {
            int status = PInvoke.getDeviceStatus(this._handle, out Device_Status);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int getIntegrationTime(out double Integration_Time__s_)
        {
            int status = PInvoke.getIntegrationTime(this._handle, out Integration_Time__s_);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int getRawScanData(int[] Scan_Data_Array)
        {
            int status = PInvoke.getRawScanData(this._handle, Scan_Data_Array);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int getScanData(double[] Data)
        {
            int status = PInvoke.getScanData(this._handle, Data);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int getUserCalibrationPoints(int[] Pixel_Data_Array, double[] Wavelength_Data_Array, out int Buffer_Length)
        {
            int status = PInvoke.getUserCalibrationPoints(this._handle, Pixel_Data_Array, Wavelength_Data_Array, out Buffer_Length);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int getUserText(StringBuilder User_Text)
        {
            int status = PInvoke.getUserText(this._handle, User_Text);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int getWavelengthData(short Data_Set, double[] Wavelength_Data_Array, out double Minimum_Wavelength, out double Maximum_Wavelength)
        {
            int status = PInvoke.getWavelengthData(this._handle, Data_Set, Wavelength_Data_Array, out Minimum_Wavelength, out Maximum_Wavelength);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int identificationQuery(StringBuilder Manufacturer_Name, StringBuilder Device_Name, StringBuilder Serial_Number, StringBuilder Firmware_Revision, StringBuilder Instrument_Driver_Revision)
        {
            int status = PInvoke.identificationQuery(this._handle, Manufacturer_Name, Device_Name, Serial_Number, Firmware_Revision, Instrument_Driver_Revision);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int reset()
        {
            int status = PInvoke.reset(this._handle);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int revisionQuery(StringBuilder Instrument_Driver_Revision, StringBuilder Firmware_Revision)
        {
            int status = PInvoke.revisionQuery(this._handle, Instrument_Driver_Revision, Firmware_Revision);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int selfTest(out short Self_Test_Result, StringBuilder Self_Test_Message)
        {
            int status = PInvoke.selfTest(this._handle, out Self_Test_Result, Self_Test_Message);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int setAmplitudeData(double[] Amplitude_Correction_Factors, int Buffer_Length, int Buffer_Start, int Mode)
        {
            int status = PInvoke.setAmplitudeData(this._handle, Amplitude_Correction_Factors, Buffer_Length, Buffer_Start, Mode);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int setIntegrationTime(double Integration_Time__s_)
        {
            int status = PInvoke.setIntegrationTime(this._handle, Integration_Time__s_);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int setUserText(string User_Text)
        {
            int status = PInvoke.setUserText(this._handle, User_Text);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int setWavelengthData(int[] Pixel_Data_Array, double[] Wavelength_Data_Array, int Buffer_Length)
        {
            int status = PInvoke.setWavelengthData(this._handle, Pixel_Data_Array, Wavelength_Data_Array, Buffer_Length);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int startScan()
        {
            int status = PInvoke.startScan(this._handle);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int startScanCont()
        {
            int status = PInvoke.startScanCont(this._handle);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int startScanContExtTrg()
        {
            int status = PInvoke.startScanContExtTrg(this._handle);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public int startScanExtTrg()
        {
            int status = PInvoke.startScanExtTrg(this._handle);
            PInvoke.TestForError(this._handle, status);
            return status;
        }

        public IntPtr Handle =>
            this._handle.Handle;

        private class PInvoke
        {
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_close", CallingConvention=CallingConvention.StdCall)]
            public static extern int close(HandleRef Instrument_Handle);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_error_message", CallingConvention=CallingConvention.StdCall)]
            public static extern int errorMessage(HandleRef Instrument_Handle, int Status_Code, StringBuilder Description);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_errorQuery", CallingConvention=CallingConvention.StdCall)]
            public static extern int errorQuery(HandleRef Instrument_Handle, out int Error_Number, StringBuilder Error_Message);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_getAmplitudeData", CallingConvention=CallingConvention.StdCall)]
            public static extern int getAmplitudeData(HandleRef Instrument_Handle, [In, Out] double[] Amplitude_Correction_Factors, int Buffer_Start, int Buffer_Length, int Mode);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_getAttribute", CallingConvention=CallingConvention.StdCall)]
            public static extern int getAttribute(HandleRef Instrument_Handle, int Attribute, out int Value);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_getDeviceStatus", CallingConvention=CallingConvention.StdCall)]
            public static extern int getDeviceStatus(HandleRef Instrument_Handle, out int Device_Status);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_getIntegrationTime", CallingConvention=CallingConvention.StdCall)]
            public static extern int getIntegrationTime(HandleRef Instrument_Handle, out double Integration_Time__s_);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_getRawScanData", CallingConvention=CallingConvention.StdCall)]
            public static extern int getRawScanData(HandleRef Instrument_Handle, [In, Out] int[] Scan_Data_Array);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_getScanData", CallingConvention=CallingConvention.StdCall)]
            public static extern int getScanData(HandleRef Instrument_Handle, [In, Out] double[] Data);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_getUserCalibrationPoints", CallingConvention=CallingConvention.StdCall)]
            public static extern int getUserCalibrationPoints(HandleRef Instrument_Handle, [In, Out] int[] Pixel_Data_Array, [In, Out] double[] Wavelength_Data_Array, out int Buffer_Length);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_getUserText", CallingConvention=CallingConvention.StdCall)]
            public static extern int getUserText(HandleRef Instrument_Handle, StringBuilder User_Text);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_getWavelengthData", CallingConvention=CallingConvention.StdCall)]
            public static extern int getWavelengthData(HandleRef Instrument_Handle, short Data_Set, [In, Out] double[] Wavelength_Data_Array, out double Minimum_Wavelength, out double Maximum_Wavelength);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_identificationQuery", CallingConvention=CallingConvention.StdCall)]
            public static extern int identificationQuery(HandleRef Instrument_Handle, StringBuilder Manufacturer_Name, StringBuilder Device_Name, StringBuilder Serial_Number, StringBuilder Firmware_Revision, StringBuilder Instrument_Driver_Revision);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_init", CallingConvention=CallingConvention.StdCall)]
            public static extern int init(string Resource_Name, ushort ID_Query, ushort Reset_Device, out IntPtr Instrument_Handle);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_reset", CallingConvention=CallingConvention.StdCall)]
            public static extern int reset(HandleRef Instrument_Handle);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_revisionQuery", CallingConvention=CallingConvention.StdCall)]
            public static extern int revisionQuery(HandleRef Instrument_Handle, StringBuilder Instrument_Driver_Revision, StringBuilder Firmware_Revision);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_selfTest", CallingConvention=CallingConvention.StdCall)]
            public static extern int selfTest(HandleRef Instrument_Handle, out short Self_Test_Result, StringBuilder Self_Test_Message);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_setAmplitudeData", CallingConvention=CallingConvention.StdCall)]
            public static extern int setAmplitudeData(HandleRef Instrument_Handle, [In, Out] double[] Amplitude_Correction_Factors, int Buffer_Length, int Buffer_Start, int Mode);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_setAttribute", CallingConvention=CallingConvention.StdCall)]
            public static extern int setAttribute(HandleRef Instrument_Handle, int Attribute, int Value);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_setIntegrationTime", CallingConvention=CallingConvention.StdCall)]
            public static extern int setIntegrationTime(HandleRef Instrument_Handle, double Integration_Time__s_);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_setUserText", CallingConvention=CallingConvention.StdCall)]
            public static extern int setUserText(HandleRef Instrument_Handle, string User_Text);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_setWavelengthData", CallingConvention=CallingConvention.StdCall)]
            public static extern int setWavelengthData(HandleRef Instrument_Handle, [In, Out] int[] Pixel_Data_Array, [In, Out] double[] Wavelength_Data_Array, int Buffer_Length);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_startScan", CallingConvention=CallingConvention.StdCall)]
            public static extern int startScan(HandleRef Instrument_Handle);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_startScanCont", CallingConvention=CallingConvention.StdCall)]
            public static extern int startScanCont(HandleRef Instrument_Handle);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_startScanContExtTrg", CallingConvention=CallingConvention.StdCall)]
            public static extern int startScanContExtTrg(HandleRef Instrument_Handle);
            [DllImport("TLCCS_64.dll", EntryPoint="tlccs_startScanExtTrg", CallingConvention=CallingConvention.StdCall)]
            public static extern int startScanExtTrg(HandleRef Instrument_Handle);
            public static int TestForError(HandleRef handle, int status)
            {
                if (status < 0)
                {
                    ThrowError(handle, status);
                }
                return status;
            }

            public static int ThrowError(HandleRef handle, int code)
            {
                StringBuilder description = new StringBuilder(0x100);
                errorMessage(handle, code, description);
                throw new ExternalException(description.ToString(), code);
            }
        }
    }
}

