using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace TestDWS515
{
    internal enum DWS_Status
	{
		DERROR = -1,
		SUCCESS = 0,
		NO_BOXES = 1
	};

	internal enum DWS_Severity
	{
		S_INFO = 0,
		S_WARNING,
		S_ERROR
	};

	internal enum DWS_MessageCode
	{
		CALIBRATION_INVALIDATED = 0,
		NO_CALIBRATION_FOUND,
		CALIBRATION_FILE_FOUND,
		CALIBRATION_SUCCESSFUL,
		CALIBRATION_DONE_NO_MARKERS,
		CALIBRATION_FAILED,
		CAMERA_DISCONNECTED,
		INVALID_LICENSE,
		LICENSE_EXPIRES_SOON,
		MISSING_CONFIGS,
		UNEXPECTED_BOXES,
		BAD_BOX,
		BAD_USER_INPUT,
		BAD_API_CALL,
		BOX_DETECTED,
		NO_BOX_DETECTED,
		UNKNOWN_ERROR
	};

	internal enum DWS_Mode
	{
		STATIC_MODE = 0,
		MOBILE_MODE,
		OBJECT_MODE,
		DWS_MODE_COUNT
	};

	internal struct DWS_Point2d
	{
		int x, y;
		bool isVisible;
	};

	internal struct DWS_Point3d
	{
		float x, y, z;
		bool isVisible;
	};

	internal enum DWS_DimensionStatus
	{
		DIMENSION_STATUS_BAD = 0,
		DIMENSION_STATUS_MED,
		DIMENSION_STATUS_GOOD
	};

	internal struct DWS_Box
	{
        private void Init()
		{
			this.id = 0;
			this.center3d = new DWS_Point3d();
			this.center2d = new DWS_Point2d();
			this.boxOrigin = new float[3];
			this.height = 0;
			this.width = 0;
			this.length = 0;
			this.planeAngles = new float[3];
			this.planeDistance = new float[3];
			this.resolutionAtAxis = new float[3];
			this.measureType = 0;
			this.corners2d = new DWS_Point2d[8];
			this.corners3d = new DWS_Point3d[8];
			this.confidence = 10;
			this.dimensionStatus = new DWS_DimensionStatus[3];
		}

		int id; // Unique if tracking is enabled. Garbage otherwise.
		DWS_Point3d center3d;   //< box center in 3d world coordinate    
		DWS_Point2d center2d;   //< box center in 2d world coordinate        
		float[] boxOrigin;

		///// Box Size /////
		// will be double for size in meters, int if in milimeters.
		float height;
		float width;
		float length;

		// Order is 0,1,2 -> H,L,W
		float[] planeAngles;
		float[] planeDistance;
		float[] resolutionAtAxis;
		int measureType;

		DWS_Point2d[] corners2d;  // array of points denoting corners on a 2d image. 
		DWS_Point3d[] corners3d; // array of points denoting corners on a 3d image

		int confidence; // 0-10, 0 is invalid, 10 is 100% confidence
		DWS_DimensionStatus[] dimensionStatus;
	};

	[SuppressUnmanagedCodeSecurity]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	internal static class DWSAPI {


		/* Callback structure:	[0] pointer to DEPTH frame data (byte buffer)				*/
		/*						[1] depth width												*/
		/*						[2] depth height											*/
		/*						[3] depth bytes per pixel									*/
		/*						[4] pointer to IR frame data (byte buffer)					*/
		/*						[5] IR width												*/
		/*						[6] IR height												*/
		/*						[7] IR bytes per pixel										*/
		/*						[8] pointer to RGB frame data	(byte buffer)				*/
		/*						[9] RGB width												*/
		/*						[10] RGB height												*/
		/*						[11] RGB bytes per pixel									*/
		/*						[12] Framse set identifier									*/
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
		internal delegate void dws_frameCallback(byte[] depthFrameData, int depthWidth, int depthHeight, int depthBytesPerPixel, byte[] irFrameData, int irWidth, int irHeight, int irBytePerPixel, byte[] rgbFrameData, int rgbWidth, int rgbHeight, int rgbBytePerPixel, int framesetId);

		/*						[0] matching frame id 										*/
		/*						[1] Num of boxes in array (DWS_MAX_NUM_OF_BOXES = 10)		*/
		/*						[2] Pointer to boxes array									*/
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void dws_measureCallback(int frameId, int numOfBoxes, DWS_Box[] boxes);

		/*						[0] Message severity										*/
		/*						[1] Message code											*/
		/*						[2] detailed message										*/
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void dws_statusCallback(DWS_Severity severity, DWS_MessageCode code, string message);

		[DllImport("dwssdk", CallingConvention = CallingConvention.Cdecl)]
		internal static extern DWS_Status dws_create(DWS_Mode mode, bool multiple_boxes, dws_frameCallback usercallback, dws_statusCallback errorcallback, DWS_Box box_buffer, out int handler); // int& outHandler
		[DllImport("dwssdk", CallingConvention = CallingConvention.Cdecl)]
		internal static extern DWS_Status dws_calibrate(int handler, int x, int y);
		[DllImport("dwssdk", CallingConvention = CallingConvention.Cdecl)]
		internal static extern DWS_Status dws_detect(int handler);
		[DllImport("dwssdk", CallingConvention = CallingConvention.Cdecl)]
		internal static extern DWS_Status dws_autoCalibrate(int handler, bool use_calibration_target);
		[DllImport("dwssdk", CallingConvention = CallingConvention.Cdecl)]
		internal static extern DWS_Status dws_start(int handler, dws_measureCallback usercallback);
		[DllImport("dwssdk", CallingConvention = CallingConvention.Cdecl)]
		internal static extern DWS_Status dws_predictBoxByCalibration(int handler, float box_height, DWS_Point2d polygon);
		[DllImport("dwssdk", CallingConvention = CallingConvention.Cdecl)]
		internal static extern DWS_Status dws_pause(int handler);
		[DllImport("dwssdk", CallingConvention = CallingConvention.Cdecl)]
		internal static extern DWS_Status dws_stop(int handler);
		[DllImport("dwssdk", CallingConvention = CallingConvention.Cdecl)]
		internal static extern string dws_version(int handler);

		internal const int DWS_MAX_NUM_OF_BOXES = 10;
	}
}
