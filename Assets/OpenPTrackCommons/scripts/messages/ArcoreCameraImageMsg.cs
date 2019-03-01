using Newtonsoft.Json;
using UnityEngine;

/// <author>
/// Carlo Rizzardo
/// </author>
/// <summary>
/// ROS message used to transfer the camera image from ARCore devices
/// together with the pose estimated by ARcore and the camera intrinsic
/// parameters.
/// </summary>
namespace OpenPTrack
{
    public class ArcoreCameraImageMsg : RosSharp.RosBridgeClient.Message
    {
        [JsonIgnore]
        public const string RosMessageName = "opt_msgs/ArcoreCameraImage";
        public TimedHeaderMsg header;

        /// The pose of the camera estimated by ARcore
        public RosSharp.RosBridgeClient.Messages.Geometry.Pose mobileFramePose;

        /// The focal length in pixels
        public double focal_length_x_px;
        public double focal_length_y_px;

        /// The image size in pixels
        public int image_width_px;
        public int image_height_px;

        /// The position of th principal point, in pixels
        public double principal_point_x_px;
        public double principal_point_y_px;


        /// The pixel size in millimiters
        public double pixel_size_x_mm;
        public double pixel_size_y_mm;

        // The camera image
        public RosSharp.RosBridgeClient.Messages.Sensor.CompressedImage image;

        public ArcoreCameraImageMsg()
        {
            header = new TimedHeaderMsg();
            mobileFramePose = new RosSharp.RosBridgeClient.Messages.Geometry.Pose();
            image = new RosSharp.RosBridgeClient.Messages.Sensor.CompressedImage();
            focal_length_x_px = 0;
            focal_length_y_px = 0;
            image_width_px = 0;
            image_height_px = 0;
            principal_point_x_px = 0;
            principal_point_y_px = 0;
        }

        public ArcoreCameraImageMsg(Texture2D texture, Pose cameraPose, GoogleARCore.CameraIntrinsics cameraIntrinsics, Vector2 pixelSize, string frameId) : this()
        {
            //OptarLogger.info("starting sendMessage...");
            TimedHeaderMsg tHeader = new TimedHeaderMsg();
            tHeader.Update();
            header = tHeader;//update header
            header.frame_id = frameId;
            image.header = tHeader;//to avoid any confusion
            image.data = ImageConversion.EncodeToPNG(texture);
            image.format = "png";

            mobileFramePose.position.x = cameraPose.position.x;
            mobileFramePose.position.y = cameraPose.position.y;
            mobileFramePose.position.z = cameraPose.position.z;

            mobileFramePose.orientation.x = cameraPose.rotation.x;
            mobileFramePose.orientation.y = cameraPose.rotation.y;
            mobileFramePose.orientation.z = cameraPose.rotation.z;
            mobileFramePose.orientation.w = cameraPose.rotation.w;

            focal_length_x_px = cameraIntrinsics.FocalLength.x;
            focal_length_y_px = cameraIntrinsics.FocalLength.y;
            image_width_px = cameraIntrinsics.ImageDimensions.x;
            image_height_px = cameraIntrinsics.ImageDimensions.y;
            principal_point_x_px = cameraIntrinsics.PrincipalPoint.x;
            principal_point_y_px = cameraIntrinsics.PrincipalPoint.y;

            pixel_size_x_mm = pixelSize.x;
            pixel_size_y_mm = pixelSize.y;

        }
    }
}