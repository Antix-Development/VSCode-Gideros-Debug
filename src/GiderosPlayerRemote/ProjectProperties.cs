using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiderosPlayerRemote
{
    class ProjectProperties
    {
        public ProjectProperties()
        {
            Clear();
        }

        public void Clear()
        {
            // graphics options
            scaleMode = 0;
            logicalWidth = 320;
            logicalHeight = 480;
            windowWidth = 0;
            windowHeight = 0;
            imageScales = new List<KeyValuePair<string, double>>();
            orientation = 0;
            fps = 60;

            // iOS options
            retinaDisplay = 0;
            autorotation = 0;

            // input options
            mouseToTouch = true;
            touchToMouse = true;
            mouseTouchOrder = 0;

            // export options
            architecture = 0;
            //android_template = 0;
            exportMode = 0;
            iosDevice = 0;
            //version = "1.0";
            //version_code = 1;
            //ios_bundle = "com.yourdomain.";
            packageName = "com.yourdomain.yourapp";
            //osx_org = "GiderosMobile";
            //osx_domain = "giderosmobile.com";
            //osx_bundle = "com.yourdomain.";
            //osx_category = 5;
            //win_org = "GiderosMobile";
            //win_domain = "giderosmobile.com";
            //winrt_org = "GiderosMobile";
            //winrt_package = "com.yourdomain.yourapp";
            //html5_host = "";
            //html5_mem = 256;
            //encryptCode = false;
            //encryptAssets = false;
            //app_icon = "";
            //tv_icon = "";
            //splash_h_image = "";
            //splash_v_image = "";
            //disableSplash = false;
            //backgroundColor = "#ffffff";
        }

        // graphics options
        public int scaleMode;
        public int logicalWidth;
        public int logicalHeight;
        public int windowWidth;
        public int windowHeight;
        public List<KeyValuePair<string, double>> imageScales;
        public int orientation;
        public int fps;

        // iOS options
        public int retinaDisplay;
        public int autorotation;

        // input options
        public bool mouseToTouch;
        public bool touchToMouse;
        public int mouseTouchOrder;

        // export options
        public int architecture;
        //int android_template;
        public int exportMode;
        public int iosDevice;
        //int version_code;
        //string version;
        //string ios_bundle;
        public string packageName;
        //string osx_org;
        //string osx_domain;
        //string osx_bundle;
        //int osx_category;
        //string win_org;
        //string win_domain;
        //string winrt_org;
        //string winrt_package;
        //string html5_host;
        //int html5_mem;
        //string app_icon;
        //string tv_icon;
        //string splash_h_image;
        //string splash_v_image;
        //bool disableSplash;
        //string backgroundColor;
        //bool encryptCode;
        //bool encryptAssets;
    };
}
