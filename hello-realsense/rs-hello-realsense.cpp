/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2017 Intel Corporation. All Rights Reserved.

*******************************************************************************/

#include "DWS_sdk.h"
#include <iostream>
#include <mutex>

void* FrameBuffers[3]; // 0 - depth, 1 - IR, 2 - RGB
void* local_depth = new unsigned char[640 * 480 * 2];
void* local_ir = new unsigned char[640 * 480 * 1];
void* local_colour = new unsigned char[1280 * 720 * 3];
bool keep_alive = true;
bool image_ready = false;
std::mutex mutex_display;
DWS_Box boxes[DWS_MAX_NUM_OF_BOXES];
int numOfDetectedBoxes = 0;
long im_id = -1;
int handler = -1;

void* _callback(const void* depth_buffer, int d_w, int d_h, int d_bpp, const void* ir_buffer, int i_w, int i_h, int i_bpp, const void* colour_buffer, int c_w, int c_h, int c_bpp, int frameId)
{
	// ###############################
	// #### CRITICAL ####
	// COPY FRAMES TO LOCAL BUFFER!!!!
	// ###############################
	{
		std::lock_guard<std::mutex> lck(mutex_display);
		memcpy(local_depth, depth_buffer, (d_w * d_h * d_bpp));
		memcpy(local_ir, ir_buffer, (i_w * i_h * i_bpp));
		if (colour_buffer != nullptr)
			memcpy(local_colour, colour_buffer, (c_w * c_h * c_bpp));
		im_id = frameId;
		image_ready = true;
	}

	return nullptr;
}


void* _dim_callback(int frameId, int box_count, const DWS_Box* box_buffer)
{
	numOfDetectedBoxes = box_count;
	return nullptr;
}

void* _status_callback(DWS_Severity severity, DWS_MessageCode code, const char* msg)
{
	std::cout << "Severity: " << severity << ", Code: " << code << ", " << msg << std::endl;

	switch (code) {
	case DWS_MessageCode::CALIBRATION_FAILED:	// Couldn't complete base surface detection
	case DWS_MessageCode::BAD_USER_INPUT:		// Bad input to dws_create
	case DWS_MessageCode::CAMERA_DISCONNECTED:	// Detected camera disconnection
	case DWS_MessageCode::INVALID_LICENSE:		// License file missing, expired, or doesn't match camera SN
	case DWS_MessageCode::MISSING_CONFIGS:		// Couldn't find the /Configuration/ folder
	case DWS_MessageCode::BAD_API_CALL:			// API handle called during an active earlier one
	case DWS_MessageCode::UNKNOWN_ERROR:			// Internal, exception, msg might contain more details
		dws_stop(handler);
		keep_alive = false;
	default:
		break;
	}

	if (code == DWS_MessageCode::CALIBRATION_SUCCESSFUL)
	{
		if (dws_start(handler, _dim_callback) != DWS_Status::SUCCESS)
			dws_stop(handler);
	}
	return nullptr;
}

int main(int argc, char* argv[])
{
	int imageNum = 0;
	auto status = dws_create(DWS_Mode::STATIC_MODE, false, _callback, _status_callback, &boxes[0], handler);
	if (status != DWS_Status::SUCCESS)
	{
		return -1;
	}
	std::string _ver = dws_version(handler);
	dws_autoCalibrate(handler, false);
	while (keep_alive) {
		if (image_ready) {
			std::lock_guard<std::mutex> lck(mutex_display);
			std::cout << "new image received. ID = " << im_id << std::endl;
			image_ready = false;
		}
	}

	return EXIT_SUCCESS;
}