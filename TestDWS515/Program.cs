using System;

namespace TestDWS515
{
    static class Program
    {
        static byte[][] FrameBuffers = new byte[3][]; // 0 - depth, 1 - IR, 2 - RGB
        static byte[] local_depth = new byte[640 * 480 * 2];
        static byte[] local_ir = new byte[640 * 480 * 1];
        static byte[] local_colour = new byte[1280 * 720 * 3];
        static bool keep_alive = true;
        static bool image_ready = false;
        //std::mutex mutex_display;
        static DWS_Box[] boxes = new DWS_Box[DWSAPI.DWS_MAX_NUM_OF_BOXES];
        static int numOfDetectedBoxes = 0;
        static long im_id = -1;
        static int handler = -1;

        static void _callback(byte[] depth_buffer, int d_w, int d_h, int d_bpp, byte[] ir_buffer, int i_w, int i_h, int i_bpp, byte[] colour_buffer, int c_w, int c_h, int c_bpp, int frameId)
        {
            // ###############################
            // #### CRITICAL ####
            // COPY FRAMES TO LOCAL BUFFER!!!!
            // ###############################
            //std::lock_guard<std::mutex> lck(mutex_display);
            //memcpy(local_depth, depth_buffer, (d_w * d_h * d_bpp));
            //memcpy(local_ir, ir_buffer, (i_w * i_h * i_bpp));
            //if (colour_buffer != null)
            //    memcpy(local_colour, colour_buffer, (c_w * c_h * c_bpp));
            im_id = frameId;
            image_ready = true;
        }

        static void _dim_callback(int frameId, int box_count, DWS_Box[] box_buffer)
        {
            numOfDetectedBoxes = box_count;
        }

        static void _status_callback(DWS_Severity severity, DWS_MessageCode code, string msg)
        {
            //std::cout << "Severity: " << severity << ", Code: " << code << ", " << msg << std::endl;
            switch (code)
            {
                case DWS_MessageCode.CALIBRATION_FAILED: // Couldn't complete base surface detection
                    DWSAPI.dws_stop(handler);
                    keep_alive = false;
                    break;
                case DWS_MessageCode.BAD_USER_INPUT: // Bad input to dws_create
                    DWSAPI.dws_stop(handler);
                    keep_alive = false;
                    break;
                case DWS_MessageCode.CAMERA_DISCONNECTED: // Detected camera disconnection
                    DWSAPI.dws_stop(handler);
                    keep_alive = false;
                    break;
                case DWS_MessageCode.INVALID_LICENSE: // License file missing, expired, or doesn't match camera SN
                    DWSAPI.dws_stop(handler);
                    keep_alive = false;
                    break;
                case DWS_MessageCode.MISSING_CONFIGS: // Couldn't find the /Configuration/ folder
                    DWSAPI.dws_stop(handler);
                    keep_alive = false;
                    break;
                case DWS_MessageCode.BAD_API_CALL: // API handle called during an active earlier one
                    DWSAPI.dws_stop(handler);
                    keep_alive = false;
                    break;
                case DWS_MessageCode.UNKNOWN_ERROR: // Internal, exception, msg might contain more details
                    DWSAPI.dws_stop(handler);
                    keep_alive = false;
                    break;
                case DWS_MessageCode.CALIBRATION_SUCCESSFUL:
                    if (DWSAPI.dws_start(handler, _dim_callback) != DWS_Status.SUCCESS)
                    {
                        DWSAPI.dws_stop(handler);
                    }
                    break;
                default:
                    break;
            }
        }

        static int Main()
        {
            var status = DWSAPI.dws_create(DWS_Mode.STATIC_MODE, false, _callback, _status_callback, boxes[0], out handler);
            if (status != DWS_Status.SUCCESS)
            {
                return -1;
            }
            string _ver = DWSAPI.dws_version(handler);
            DWSAPI.dws_autoCalibrate(handler, false);
            while (keep_alive)
            {
                if (image_ready)
                {
                    //std::lock_guard<std::mutex> lck(mutex_display);
                    Console.WriteLine($"{im_id}");
                    image_ready = false;
                }
            }
            return 0;
        }

    }
}